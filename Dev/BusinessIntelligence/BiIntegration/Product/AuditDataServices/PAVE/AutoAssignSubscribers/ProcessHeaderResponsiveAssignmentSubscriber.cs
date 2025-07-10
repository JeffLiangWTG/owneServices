using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.BufferManagement.Service.Client;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	public class ProcessHeaderResponsiveAssignmentSubscriber : PAVEServiceTaskNudgingSubscriber
	{
		#region Properties

		public override string Code => "PAH";

		public override ITableSchema Table => ProcessHeaderSchema.Instance;

		public override bool IsRequired() => true;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override string Description => nameof(ProcessHeaderResponsiveAssignmentSubscriber);

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[ProcessHeaderSchema.FH_FC_CurrentComponent.Name] == DBNull.Value
				|| row[ProcessHeaderSchema.FH_FH_ParentHeader.Name] == DBNull.Value
				|| (row[ProcessHeaderSchema.FH_P0_Template.Name] != DBNull.Value && Convert.ToString(row[ProcessHeaderSchema.FH_P0_Template.Name]).Length > 0))
			{
				row.Delete();
			}
		};

		#endregion

		#region ProcessChange

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var isEnabled = BMSRegistry.BufferManagementEnabled && BMSRegistry.IsBufferManagementWorkflowModeOrBetterEnabled && BMSRegistry.EnableResponsivePAVEDataProcessing && BMSRegistry.AutoAssignCapabilityTasksOnChanges;

			if (!isEnabled)
			{
				logger.Log(LogType.Debug, $"{changeTable.Rows.Count} changes were skipped as the {Code} subscriber is disabled.");
				return;
			}

			if (changeTable.Rows.Count > BMSRegistry.AutoAssignCapabilityTasksMaxNumberOfCDCChanges)
			{
				logger.Log(LogType.Warning, $"Number of changes: {changeTable.Rows.Count} greater than {nameof(BMSRegistry.AutoAssignCapabilityTasksMaxNumberOfCDCChanges)}: {BMSRegistry.AutoAssignCapabilityTasksMaxNumberOfCDCChanges}; Auto-Assignment will not be processed responsively and will be deferred to the Capability Task Auto-Assignment service task."); // Log Message

				MaybeNudgeServiceTask(logger,
					shouldNudge: BMSRegistry.EnableNudgingBMTServiceTaskByThePVEServiceTask,
					serviceTaskCode: "BMT",
					delay: BMSRegistry.DelayForNudgingTheBMTServiceTaskByThePVEServiceTask);
				return;
			}

			var rows = changeTable.Rows
				.Cast<DataRow>()
				.Where(ShouldProcessChange);

			var pks = rows
				.Select(row => (Guid)row[ProcessHeaderSchema.Constants.PK])
				.Distinct()
				.ToArray();
			var capabilityTaskAutoAssignmentServiceClient = ObjectFactory.Get<IPAVEServiceClientFactory>().GetCapabilityTaskAutoAssignmentServiceClient();
			const int processTransferRulesBatchSize = 256; //TODO: Move to registry?

			try
			{
				foreach (var batch in pks.Batch(processTransferRulesBatchSize))
				{
					capabilityTaskAutoAssignmentServiceClient.AutoAssignCapabilityTasks(batch, logger);
				}
			}
			catch (Exception e)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, "CDC updates failed for {0}, Error: {1})", Table.TableName, e.ToString()));
			}
		}

		#endregion

		#region Responsive Auto Assignment

		static bool ShouldProcessChange(DataRow row)
		{
			if (!(bool)row[ProcessHeaderSchema.Constants.FH_IsActive])
			{
				return false;
			}

			if (row.RowState != DataRowState.Modified)
			{
				return true;
			}

			var releaseDateTime = row[ProcessHeaderSchema.Constants.FH_ReleaseDateTime];

			if (releaseDateTime == DBNull.Value)
			{
				return false;
			}

			var allowTaskAutoAssignment = (bool)row[ProcessHeaderSchema.Constants.FH_AllowTaskAutoAssignment];

			if (!allowTaskAutoAssignment)
			{
				return false;
			}

			return true;
		}

		#endregion
	}
}
