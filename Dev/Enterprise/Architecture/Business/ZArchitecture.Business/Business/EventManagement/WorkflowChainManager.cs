using System;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public class WorkflowChainManager : IDisposable
	{
		public WorkflowChainManager(IQueuedLog queuedLog)
		{
			if (currentInstance != null)
			{
				currentInstance = null;
				throw new InvalidOperationException("Should not be starting a new Workflow Chain when one is still running. A chain should only be started by the Workflow Log Walker.");
			}

			currentInstance = this;
			this.queuedLog = queuedLog;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		readonly IQueuedLog queuedLog;

		WorkflowTriggerEventData _queuedLogTriggerEventData;
		WorkflowTriggerEventData QueuedLogTriggerEventData
		{
			get { return _queuedLogTriggerEventData ?? (_queuedLogTriggerEventData = queuedLog == null ? null : new WorkflowTriggerEventData(queuedLog)); }
		}

		[ThreadStatic]
		static WorkflowChainManager currentInstance;

		public static bool HasWTEEventFromCurrentChain(IBaseTrigger trigger)
		{
			return currentInstance != null && currentInstance.HasWTEEventFromCurrentChainCore(trigger);
		}

		bool HasWTEEventFromCurrentChainCore(IBaseTrigger trigger)
		{
			var queuedLogTriggerEventData = QueuedLogTriggerEventData;
			if (queuedLogTriggerEventData != null)
			{
				var chainID = queuedLogTriggerEventData.TriggerChainID;

				// Make output consistent 
				// Calling it before and after GetOrGenerateChainID via new WorkflowTriggerEventData(...), etc should return same result
				if (chainID.IsEmpty)
				{
					chainID = GetOrGenerateChainID();
				}

				if (!chainID.IsEmpty)
				{
					var logQuery = new ZQuery(StmALogSchema.SL_Parent, trigger.Identifier);
					logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
					logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, chainID.ToString());

					return queuedLog.Factory.LoadTop1<StmALog>(logQuery) != null;
				}
			}

			return false;
		}

		internal static ZGuid ChainID
		{
			get { return currentInstance != null ? currentInstance.GetOrGenerateChainID() : ZGuid.Empty; }
		}

		ZGuid GetOrGenerateChainID()
		{
			var queuedLogTriggerEventData = QueuedLogTriggerEventData;
			if (queuedLogTriggerEventData == null)
			{
				return ZGuid.Empty;
			}

			if (queuedLogTriggerEventData.TriggerChainID.IsEmpty)
			{
				queuedLogTriggerEventData.TriggerChainID = ZGuid.NewZGuid();
				var originalLogQuery = new ZQuery(StmALogSchema.SL_Parent, queuedLog.SJ_ParentID);
				originalLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				originalLogQuery.AddToFilter(StmALogSchema.PK, queuedLog.SJ_ALogReference);
				var originalStmALog = queuedLog.Factory.LoadTop1<StmALog>(originalLogQuery);
				if (originalStmALog != null)
				{
					using (originalStmALog.LockForUpdatingKeyFields(false))
					{
						originalStmALog.SL_Reference = queuedLogTriggerEventData.ToReference();
					}
				}
			}

			return queuedLogTriggerEventData.TriggerChainID;
		}

		void IDisposable.Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			currentInstance = null;
		}
	}
}
