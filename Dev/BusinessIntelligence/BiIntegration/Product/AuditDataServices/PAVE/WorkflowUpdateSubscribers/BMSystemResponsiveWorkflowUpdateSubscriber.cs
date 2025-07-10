using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
	public class BMSystemResponsiveWorkflowUpdateSubscriber : ActualDataChangesAuditSubscriber
	{
		#region Properties

		public override string Code => "BSW";

		public override string Description => nameof(BMSystemResponsiveWorkflowUpdateSubscriber);

		public override ITableSchema Table => BMSystemSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => [BMSystemSchema.FS_IsLive];

		public override Action<DataRow> CustomFilter => null;

		public override bool IsRequired() => true;

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		#endregion

		#region ProcessChange

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();
			var isEnabled = bmsRegistry.BufferManagementEnabled && bmsRegistry.EnableResponsivePAVEDataProcessing && bmsRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges;

			if (!isEnabled)
			{
				logger.Log(LogType.Information, $"{changeTable.Rows.Count} changes were skipped as the {Code} subscriber is disabled.");
				return;
			}

			var rows = changeTable.Rows.Cast<DataRow>();

			foreach (var row in rows)
			{
				var systemPK = (Guid)row[BMSystemSchema.Constants.PK];
				var isLive = (bool)row[BMSystemSchema.Constants.FS_IsLive];
				var name = (string)row[BMSystemSchema.Constants.FS_Name];
				ActionScheduleProvider.ChangeStatusByToken(systemPK.ToString(), isLive ? Constants.TimeActionScheduleStatus.Scheduled : Constants.TimeActionScheduleStatus.Suspended);
				logger.Log(LogType.Information, $"{(isLive ? "Activated" : "Suspended")} scheduled responsive workflow updates for system {name} (IsLive = {isLive}).");
			}
		}

		#endregion

		#region Implementation
		ActionScheduleProvider ActionScheduleProvider => actionScheduleProvider ?? (actionScheduleProvider = new ActionScheduleProvider());
		ActionScheduleProvider actionScheduleProvider;

		#endregion
	}
}
