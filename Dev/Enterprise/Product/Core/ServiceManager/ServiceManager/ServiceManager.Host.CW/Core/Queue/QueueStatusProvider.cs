using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ServiceManager.Host;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Shared.CW;

namespace ServiceManager.Host.CW
{
	class QueueStatusProvider : IQueueStatusProvider
	{
		public QueueStatusProvider(
			ITaskStatusProvider statusProvider,
			IMemoryCache memoryCache,
			IApplicationSchemaResolver schemaResolver,
			ICancellationTokenProvider cancellationTokenProvider,
			IErrorReporterProxy errorReporterProxy,
			IHostedServiceQueuesProvider hostedServiceQueuesProvider)
		{
			this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			this.schemaResolver = schemaResolver ?? throw new ArgumentNullException(nameof(schemaResolver));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.hostedServiceQueuesProvider = hostedServiceQueuesProvider ?? throw new ArgumentNullException(nameof(hostedServiceQueuesProvider));
		}

		public QueueListDTO GetQueueStatus()
		{
			return memoryCache.AddOrGetExisting(MemoryCacheKey, CalculateQueueList, TimeSpan.FromMinutes(1));
		}

		QueueListDTO CalculateQueueList()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var queueList = HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings
					.Where(x => !string.IsNullOrEmpty(x.QueueName))
					.Select(binding => GetQueueResultSafe(() => CreateQueueResultFromQuery(binding.Table, binding.Predicates), binding.QueueName, binding.ServiceTaskCode))
					.ToList();

				var queues = hostedServiceQueuesProvider.Queues;
				using (Db.Connection.TemporarySetDefaultCommandTimeOut(15))
				{
					queues.ForEach(queue => queueList.Add(GetQueueResultSafe(() => queue.QueueResult, queue.Name, queue.ServiceTaskCode)));
				}

				return new QueueListDTO(queueList);

				QueueDTO GetQueueResultSafe(Func<QueueResult> getQueueResultFunc, string queueName, string serviceTaskCode)
				{
					cancellationTokenProvider.Token.ThrowIfCancellationRequested();
					try
					{
						return QueueResultToQueueDTO(getQueueResultFunc(), queueName, serviceTaskCode, statusProvider);
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						errorReporterProxy.ReportOnce($"Exception occurred during queue size query for Queue [{queueName}], Service Task [{serviceTaskCode}].", exception);
						return QueueResultToQueueDTO(QueueResult.Error, queueName, serviceTaskCode, statusProvider);
					}
				}
			}

			static QueueDTO QueueResultToQueueDTO(QueueResult queueResult, string name, string code, ITaskStatusProvider statusProvider)
			{
				var taskStatus = statusProvider.GetTaskStatus(code);

				return taskStatus == null
					? new QueueDTO(name, code, false, queueResult.QueueSize, (int?)queueResult.MaximumItemAge.TotalSeconds, 0, 0)
					: new QueueDTO(name, code, taskStatus.IsActive, queueResult.QueueSize, (int?)queueResult.MaximumItemAge.TotalSeconds, taskStatus.RunningCount, taskStatus.ErrorCountLast24Hours);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		QueueResult CreateQueueResultFromQuery(string tableName, IEnumerable<string> predicatesList)
		{
			var predicateFactory = new PredicateFactory(new NudgingSchemaResolver(schemaResolver));
			var predicates = predicateFactory
				.GeneratePredicates(predicatesList, tableName);

			var predicateText = MakePredicateText(predicates, tableName);
			var lastEditColumn = GetLastEditColumn(tableName);
			var hasLastEditColumn = !string.IsNullOrEmpty(lastEditColumn);

			var ageSqlText = hasLastEditColumn ? $", IsNull(Max(DateDiff(SECOND, {lastEditColumn}, GetUtcDate())), 0)" : "";

			var sqlText = $@"SELECT COUNT(*){ageSqlText} FROM {tableName}{predicateText}";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				var allParams = predicates
					.SelectMany(x => x.Parameters)
					.ToArray();

				foreach (var param in allParams)
				{
					cmd.AddParameter(
						param.ParameterName,
						param.Type,
						param.Size,
						param.Value);
				}

				var queueLength = 0;
				var age = TimeSpan.Zero;
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						queueLength = reader.GetInt32(0);
						if (hasLastEditColumn && !reader.IsDBNull(1))
						{
							age = TimeSpan.FromSeconds(reader.GetInt32(1));
						}
					}
				}
				return new QueueResult(queueLength, age);
			}
		}

		public string MakePredicateText(IEnumerable<IPredicate> predicates, string tableName)
		{
			var parameterizedPredicateLiteralSqlQuerys = predicates
				.Select(p => p.ParameterizedSqlCondition);

			if (!parameterizedPredicateLiteralSqlQuerys.Any())
			{
				return string.Empty;
			}

			var predicateText = CombinePredicateStrings(parameterizedPredicateLiteralSqlQuerys);
			if (tableName == EDIMessageSchema.Constants.TableName)
			{
				predicateText += " AND (EM_HeldUntilDate IS NULL OR EM_HeldUntilDate < GETUTCDATE())";
			}

			return predicateText;
		}

		string GetLastEditColumn(string tableName)
		{
			var tableSchema = schemaResolver.GetTableSchema(tableName);
			return tableSchema?.All.Reverse().FirstOrDefault(column => column.Name.EndsWith(AuditDetailsColumns.SystemLastEditTimeUtc))?.Name;
		}

		static string CombinePredicateStrings(IEnumerable<string> predicates)
		{
			return " WHERE (" + string.Join(") AND (", predicates) + ")";
		}

		const string MemoryCacheKey = "QueueRequestHandler";

		readonly IMemoryCache memoryCache;
		readonly ITaskStatusProvider statusProvider;
		readonly IApplicationSchemaResolver schemaResolver;
		readonly ICancellationTokenProvider cancellationTokenProvider;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IHostedServiceQueuesProvider hostedServiceQueuesProvider;
	}
}
