using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	partial class PerformanceStatisticsPersister : IPerformanceStatisticsPersister
	{
		public const int ElementsCountThreshold = 50;
		volatile ConcurrentQueue<IPerformanceStatisticCollectorToken> receivingQueue = new ConcurrentQueue<IPerformanceStatisticCollectorToken>();

		public void Record(IEnumerable<IPerformanceStatisticCollectorToken> elements, bool forceWrite = false)
		{
			foreach (var collectorToken in elements)
			{
				receivingQueue.Enqueue(collectorToken);
			}

			ProcessQueue(forceWrite);
		}

		void ProcessQueue(bool forceWrite)
		{
			if ((forceWrite || receivingQueue.Count > ElementsCountThreshold) && EnvProxy.Instance.Licence != null)
			{
				// should not proceed if saving is in progress to avoid:
				// 1. parallel writes as it is low priority
				// 2. worsening the situation if deadlock happens and in retry process
				if (IsSavingIsInProgress() && !forceWrite)
				{
					return;
				}

#pragma warning disable 420 // passing a volatile to Interlocked is safe according to: https://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx
				var persistingQueue = Interlocked.Exchange(ref receivingQueue, new ConcurrentQueue<IPerformanceStatisticCollectorToken>());
#pragma warning restore 420

				if (persistingQueue.Count > 0)
				{
					SaveInAnotherThread(this, persistingQueue);
				}
			}
		}

		partial void BeginSave();

		partial void EndSave(bool success);

		partial void ThrowExceptionForTest();

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We accept that performance statistics may fail and we do nothing but reporting the failure")]
		void SaveInAnotherThread(PerformanceStatisticsPersister persister, IEnumerable<IPerformanceStatisticCollectorToken> elements)
		{
			SetSavingStatus(true);

			persister.BeginSave();

			var task = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (EnvProxy.Instance.SuspendBranchAccessError())
				{
					var success = false;
					try
					{
						var factory = new BusinessObjectFactory(Db.Connection);
						factory.RefreshEnabled = false;
						factory.SuspendValidation();

						var usage = factory.New<StmUsage>();
						usage.XW_MachineName = System.Environment.MachineName;
						usage.XW_StartTimeUtc = elements.Min(x => x.TimeStartedUtc);
						usage.XW_EndTimeUtc = elements.Max(x => x.TimeEndedUtc);
						var firstElement = elements.First();
						if (firstElement.CompanyCode != null)
						{
							usage.XW_CompanyCode = firstElement.CompanyCode;
						}
						if (firstElement.BranchCode != null)
						{
							usage.XW_GB_NKBranchCode = firstElement.BranchCode;
						}
						if (firstElement.UserInitials != null)
						{
							usage.XW_GS_NKStaffCode = firstElement.UserInitials;
						}

						// Time can go backwards in the case where the delta expires in ZDateTime and gets recalculated with a different latency to the server
						// Alternately the server time may be adjusted..
						if (usage.XW_StartTimeUtc > usage.XW_EndTimeUtc)
						{
							usage.XW_StartTimeUtc = usage.XW_EndTimeUtc;
						}

						var usages = new Statistics.Xml.Usages();
						// recursive call
						WriteSubElements(usages, elements);
						usage.UsageActionsSettings = usages;

						persister.ThrowExceptionForTest();
						using (FactorySaveAlerterOverride.TemporarilyOverride(this))
						{
							factory.Save();
						}

						success = true;
					}
					catch (SqlException sqlEx) when (new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.TempdbIsOutOfSpace)
					{
						AddElementToReceivingQueue(elements);
					}
					catch (DatabaseUpgradeInProgressException)
					{
						AddElementToReceivingQueue(elements);
					}
					catch (Exception ex)
					{
						if (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce("PerformanceStatisticsPersister failed due to exception", ex);
							AddElementToReceivingQueue(elements);
						}
					}
					finally
					{
						SetSavingStatus(false);
						persister.EndSave(success);
					}
				}
			});

#if DEBUG
			ProcessingTask = task;
#endif
		}

		#region Save Retry Status

		void SetSavingStatus(bool isInProgress)
		{
			Interlocked.Exchange(ref savingSatus, isInProgress ? StateIsInProgress : StateNotInProgress);
		}

		bool IsSavingIsInProgress()
		{
			return Interlocked.Read(ref savingSatus) == StateIsInProgress;
		}

		long savingSatus;
		const long StateIsInProgress = 1;
		const long StateNotInProgress = 0;

		#endregion

		void AddElementToReceivingQueue(IEnumerable<IPerformanceStatisticCollectorToken> elements)
		{
			foreach (var element in elements)
			{
				receivingQueue.Enqueue(element);
			}
		}

		static void WriteSubElements(Statistics.Xml.Usages parent, IEnumerable<IPerformanceStatisticCollectorToken> childTokens)
		{
			foreach (var childToken in childTokens)
			{
				var childSettings = Statistics.Xml.Usage.New(childToken);
				parent.AddNode(childSettings);
				WriteSubElements(childSettings, childToken.Children);
			}
		}
	}

#if DEBUG
	partial class PerformanceStatisticsPersister : IPerformanceStatisticsPersister
	{
		int pendingSaves;
		int failedSaves;
		int successfullSaves;
		Action<bool> endSaveHook = (_) => { };

		public Exception ExceptionThrownForTest;
		public Task ProcessingTask;

		public int QueueLength => receivingQueue.Count;

		public void HookEndSave(Action<bool> endSaveHookFunc)
		{
			endSaveHook = endSaveHookFunc;
		}

		public int PendingSaves
		{
			get
			{
				return pendingSaves;
			}
		}

		public int SuccessfullSaves
		{
			get
			{
				return successfullSaves;
			}
		}

		public int FailedSaves
		{
			get
			{
				return failedSaves;
			}
		}

		partial void BeginSave()
		{
			++pendingSaves;
		}

		partial void EndSave(bool success)
		{
			endSaveHook(success);

			--pendingSaves;

			if (success)
			{
				++successfullSaves;
			}
			else
			{
				++failedSaves;
			}
		}

		partial void ThrowExceptionForTest()
		{
			if (ExceptionThrownForTest != null)
			{
				throw ExceptionThrownForTest;
			}
		}
	}
#endif
}
