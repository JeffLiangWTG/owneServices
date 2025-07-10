using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// In which we are using the continuation message passing style in order to have fine-grained control over operation order.
	/// </summary>
	/// <typeparam name="T">Type of bizobj to process</typeparam>
	public abstract class ManagedBatchProcessor<T> : IProcessor
	{
		#region API

		#region Real API

		public void Process(INotifications notifications, CancellationToken token = default) => ProcessCore(notifications, token);

		#endregion

		#region Testing API

		public void ProcessRow(INotifications notifications, CancellationToken token, T row, BusinessObjectFactory factory) => ProcessRow(notifications, token, row, factory, (1, 1));
		public void ProcessRow(INotifications notifications, CancellationToken token, T row, BusinessObjectFactory factory, (int index, int count) position) => ProcessRowCore(notifications, token, row, factory, position);
		public void MarkRowAsBad(INotifications notifications, T row) => MarkRowAsBadInNewFactory(notifications, row);
		public IList<T> LoadBatch(BusinessObjectFactory factory) => LoadBatchInternal(factory);

		#endregion

		#endregion

		#region SuppressResourceStringsCheckRegion

		void ProcessCore(INotifications notifications, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			// We use a stack rather than a queue, because we want to retry things that failed before we do other things that are queued.
			var stack = new Stack<BatchAction>();
			void LoadAndAddNewBatches() => LoadFreshBatches(notifications, token).ForEach(b => stack.Push(b));
			LoadAndAddNewBatches();

			if (!stack.Any())
			{
				notifications.AddVerboseInfo(() => "Nothing to process");
			}

			while (stack.Any())
			{
				token.ThrowIfCancellationRequested();
				try
				{
					foreach (var action in stack.Pop().Do(notifications))
					{
						stack.Push(action);
					}
					if (!stack.Any())
					{
						LoadAndAddNewBatches();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("CriticalErrorInBatch", ex);
					notifications.AddError(string.Format(CultureInfo.InvariantCulture, "Error in batching occurred. [{0}]", ex.Message));
				}
			}
		}

		IList<BatchAction> LoadFreshBatches(INotifications notifications, CancellationToken token)
		{
			var factory = GetNewFactory();
			var batch = LoadBatch(factory);
			if (batch.Count > 0)
			{
				notifications.AddInfo(string.Format(CultureInfo.InvariantCulture, "Loaded batch of size [{0}]", batch.Count));

				return GetGrouper()
				.GetGroups(factory, batch)
				.Select(g => new BatchAction(ProcessBatch(token, g.groupedBatchFactory, g.groupedBatch, g.groupKey)))
				.ToList();
			}
			return Array.Empty<BatchAction>();
		}

		Func<INotifications, IList<BatchAction>> ProcessBatch(CancellationToken token, BusinessObjectFactory factory, IList<T> batch, object groupKey)
		{
			return (notifications) =>
			{
				var attemptNumber = 1;

				if (batch.Count > 0)
				{
					try
					{
						using (WithBatch(factory, batch, groupKey))
						using (factory.AddDisposableService())
						{
							var index = 0;
							foreach (var row in batch)
							{
								ProcessRowCore(notifications, token, row, factory, (++index, batch.Count));
							}

							notifications.AddVerboseInfo(() => string.Format(CultureInfo.InvariantCulture, "Processed batch of size [{0}]", batch.Count));
							factory.Save();
							notifications.AddInfo(string.Format(CultureInfo.InvariantCulture, "Finished batch of size [{0}]", batch.Count));
						}
						return Array.Empty<BatchAction>();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						LogWarning(notifications, batch, ex, attemptNumber);
						notifications.AddWarning("Will now process one at a time");
						return batch.Select(s => MakeSingleRowBatchAction(token, s, attemptNumber + 1, groupKey)).ToList();
					}
					finally
					{
						OnAfterBatch(batch);
					}
				}
				else
				{
					OnAfterBatch(batch);
				}

				return Array.Empty<BatchAction>();
			};
		}

		BatchAction MakeSingleRowBatchAction(CancellationToken token, T sourceRow, int attemptNumber, object groupKey)
		{
			var query = GetSingularQuery(sourceRow);
			return new BatchAction((notifications) =>
			{
				var factory = GetNewFactory();
				var rows = LoadBatchCore(factory, query);
				using (WithBatch(factory, rows, groupKey))
				using (factory.AddDisposableService())
				{
					try
					{
						if (rows.Count == 0)
						{
							notifications.AddWarning("Item not found");
						}
						else if (rows.Count > 1)
						{
							notifications.AddWarning("Logic error: Retry action loaded multiple rows");
							ErrorReporter.ReportOnce("MultipleRowsFoundWhereOnlyOneExpected", string.Format(CultureInfo.InvariantCulture, "Expected only one row here, but there were {0}", rows.Count));
						}
						else
						{
							var row = rows[0];
							ProcessRow(notifications, token, row, factory);
							factory.Save();
							notifications.AddInfo(string.Format(CultureInfo.InvariantCulture, "Processed {0}", GetPrettyPrinter().PrettyPrint(row)));
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						LogWarning(notifications, rows, ex, attemptNumber);
						if (attemptNumber < MaxAttempts)
						{
							return new[] { MakeSingleRowBatchAction(token, sourceRow, attemptNumber + 1, groupKey) };
						}
						else
						{
							ErrorReporter.ReportOnce("UnhandledExceptionDuringRetry", "Unhandled error", ex);
							MarkRowAsBad(notifications, sourceRow);
						}
					}
					finally
					{
						OnAfterBatch(rows);
					}
				}

				return Array.Empty<BatchAction>();
			});
		}

		void MarkRowAsBadInNewFactory(INotifications notifications, T row)
		{
			var factory = GetNewDefaultFactory("BadnessMarker");
			var batch = LoadBatchCore(factory, GetSingularQuery(row));
			try
			{
				foreach (var rowInNewFactory in batch)
				{
					MarkRowAsBadCore(notifications, rowInNewFactory, factory);
				}
				factory.Save();
			}
			finally
			{
				OnAfterBatch(batch);
			}
		}

		void LogWarning(INotifications notifications, IList<T> rows, Exception ex, int attemptNumber)
		{
			var printer = GetPrettyPrinter();
			var rowsMessage = string.Join(", ", (rows ?? Enumerable.Empty<T>()).Select(r => printer.PrettyPrint(r)));
			notifications.AddWarning(string.Format(CultureInfo.InvariantCulture, "Failure in processing [{0}], on attempt [{1}] due to exception [{2}] ", rowsMessage, attemptNumber, ex.Message));
		}

		BusinessObjectFactory GetNewDefaultFactory(string name)
		{
			return new BusinessObjectFactory
			{
				NameForDebugging = FormattableString.Invariant($"{GetType().Name}.{name}"),
				RefreshEnabled = false
			};
		}

		IList<T> LoadBatchInternal(BusinessObjectFactory factory)
		{
			var query = GetQuery();
			if (query.IsNoResultQuery)
			{
				return Array.Empty<T>();
			}
			else
			{
				query.MaximumRows = BatchSize;
				return LoadBatchCore(factory, query);
			}
		}

		#endregion SuppressResourceStringsCheckRegion

		#region Abstract

		protected virtual int MaxAttempts => 3;
		public virtual int BatchSize => 40;
		protected abstract ZQuery GetQuery();
		protected abstract ZQuery GetSingularQuery(T row);
		protected abstract void ProcessRowCore(INotifications notifications, CancellationToken token, T row, BusinessObjectFactory factory, (int index, int count) batchPosition);
		protected abstract void MarkRowAsBadCore(INotifications notifications, T row, BusinessObjectFactory factory);
		protected virtual IPrettyPrinter GetPrettyPrinter() => new DefaultPrettyPrinter();
		protected virtual IBatchGrouper GetGrouper() => DefaultBatchGrouper.Instance;
		protected abstract IList<T> LoadBatchCore(BusinessObjectFactory factory, ZQuery query);
		protected virtual void OnAfterBatch(IList<T> batch) { }
		protected virtual BusinessObjectFactory GetNewFactory() => GetNewDefaultFactory((NoResString)"Batch");
		protected virtual IDisposable WithBatch(BusinessObjectFactory factory, IList<T> batch, object groupKey) => null;

		#endregion

		#region Sub types

		class BatchAction
		{
			public BatchAction(Func<INotifications, IList<BatchAction>> func)
			{
				this.func = func;
			}

			readonly Func<INotifications, IList<BatchAction>> func;

			public IList<BatchAction> Do(INotifications notifications)
			{
				return func(notifications);
			}
		}

		public interface IPrettyPrinter
		{
			string PrettyPrint(T row);
		}

		class DefaultPrettyPrinter : IPrettyPrinter
		{
			public string PrettyPrint(T row) => (row as BusinessObject)?.HumanReadableName;
		}

		public interface IBatchGrouper
		{
			IEnumerable<(BusinessObjectFactory groupedBatchFactory, IList<T> groupedBatch, object groupKey)> GetGroups(BusinessObjectFactory factory, IList<T> batch);
		}

		[Immutable]
		class DefaultBatchGrouper : IBatchGrouper
		{
			internal static DefaultBatchGrouper Instance { get; } = new DefaultBatchGrouper();

			public IEnumerable<(BusinessObjectFactory groupedBatchFactory, IList<T> groupedBatch, object groupKey)> GetGroups(BusinessObjectFactory factory, IList<T> batch)
			{
				yield return (factory, batch, null);
			}
		}

		#endregion
	}
}
