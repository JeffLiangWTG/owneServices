using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogAddedToQueueOnlyScopeManager : IAfterOnSavingBOProcessingService
	{
		public StmALogAddedToQueueOnlyScopeManager(BusinessObjectFactory factory)
		{
			factory.Saved += new BusinessObjectFactory.SavedEventHandler(ClearServiceState);
		}

		readonly Dictionary<StmALogAddedToQueueOnly, DataRow> logQueuedRecordPairs = new Dictionary<StmALogAddedToQueueOnly, DataRow>();

		public void AddStmALogQueueRowToScope(StmALogAddedToQueueOnly log)
		{
			if (!logQueuedRecordPairs.ContainsKey(log))
			{
				var row = ((IBusinessObjectFactoryInternals)log.Factory).RowFactory.New(StmALogQueueSchema.Constants.TableName);
				logQueuedRecordPairs.Add(log, row);
			}
		}

		public void RemoveStmALogQueueRowFromScope(StmALogAddedToQueueOnly log)
		{
			if (logQueuedRecordPairs.TryGetValue(log, out var row))
			{
				row.Delete();
				logQueuedRecordPairs.Remove(log);
			}
		}

		void PopulateQueuedRowAndAddToTable()
		{
			foreach (var log in logQueuedRecordPairs.Keys)
			{
				var row = logQueuedRecordPairs[log];
				if (!log.IsDeleted && row.RowState == DataRowState.Detached)
				{
					row[StmALogQueueSchema.Constants.PK] = Guid.NewGuid();
					row[StmALogQueueSchema.Constants.SLQ_ParentTableName] = log.SL_Table;
					row[StmALogQueueSchema.Constants.SLQ_ParentID] = log.SL_Parent.IsValid ? log.SL_Parent.ToGuid() : Guid.Empty;
					row[StmALogQueueSchema.Constants.SLQ_GS_NKUser] = log.SL_GS_NKUser;
					row[StmALogQueueSchema.Constants.SLQ_GB_NKBranch] = log.SL_GB_NKBranch;
					row[StmALogQueueSchema.Constants.SLQ_GE_NKDepartment] = log.SL_GE_NKDepartment;
					row[StmALogQueueSchema.Constants.SLQ_FireWorkflow] = log.SL_FireWorkflow.GetValueForLogicalDataLayer(false);
					row[StmALogQueueSchema.Constants.SLQ_SE_NKEvent] = log.SL_SE_NKEvent;
					row[StmALogQueueSchema.Constants.SLQ_Reference] = log.SL_Reference;
					row[StmALogQueueSchema.Constants.SLQ_EventTime] = log.SL_EventTime.ToDateTime();
					row[StmALogQueueSchema.Constants.SLQ_EventTimeUtc] = log.SL_EventTimeUtc.ToDateTime();
					row[StmALogQueueSchema.Constants.SLQ_IsCancelled] = log.SL_IsCancelled.GetValueForLogicalDataLayer(false);
					row[StmALogQueueSchema.Constants.SLQ_IsEstimate] = log.SL_IsEstimate.GetValueForLogicalDataLayer(false);
					row[StmALogQueueSchema.Constants.SLQ_ALogReference] = Guid.Empty;
					row[StmALogQueueSchema.Constants.SLQ_PostedTimeUtc] = ZDateTime.UtcNow.ToDateTime();
					row.Table.Rows.Add(row);
				}
			}
		}

		void ClearServiceState(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				logQueuedRecordPairs.Clear();
			}
		}

		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			PopulateQueuedRowAndAddToTable();
		}
	}
}
