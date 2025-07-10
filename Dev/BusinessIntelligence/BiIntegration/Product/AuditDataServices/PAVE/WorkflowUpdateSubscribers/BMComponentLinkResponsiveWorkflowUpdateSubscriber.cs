using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
	public class BMComponentLinkResponsiveWorkflowUpdateSubscriber : BMComponentRelatedResponsiveWorkflowUpdateSubscriberBase
	{
		#region Properties

		public override string Code => "CLW";

		public override string Description => nameof(BMComponentLinkResponsiveWorkflowUpdateSubscriber);

		public override ITableSchema Table => BMComponentLinkSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => [BMComponentLinkSchema.FL_TransferRulesEnabled, BMComponentLinkSchema.FL_FC_ComponentTo, BMComponentLinkSchema.FL_IsReleaseGateRuleApplied];

		public override Action<DataRow> CustomFilter => null;

		public override bool IsRequired() => true;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

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

			var scheduleFactory = new BusinessObjectFactory();
			scheduleFactory.NameForDebugging = $"{nameof(BMComponentLinkResponsiveWorkflowUpdateSubscriber)}.{nameof(scheduleFactory)}";
			var readonlyFactory = new ReadOnlyBusinessObjectFactory();
			readonlyFactory.NameForDebugging = $"{nameof(BMComponentLinkResponsiveWorkflowUpdateSubscriber)}.{nameof(readonlyFactory)}";

			var rows = changeTable.Rows.Cast<DataRow>();

			foreach (var row in rows)
			{
				var linkPK = row.RowState != DataRowState.Deleted
					? (Guid)row[BMComponentLinkSchema.Constants.PK, DataRowVersion.Current]
					: (Guid)row[BMComponentLinkSchema.Constants.PK, DataRowVersion.Original];
				var linkName = GetLinkName(readonlyFactory, linkPK);

				if (row.RowState == DataRowState.Added)
				{
					if ((bool)row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Current])
					{
						MaybeScheduleWorkflowsResponsiveUpdate(logger, $"A new component link {linkName} was added",
							(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
					}
				}
				else if (row.RowState == DataRowState.Modified)
				{
					var transferEnabled = (bool)row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Current];

					if (HasCellChanged(row, changeTable.Columns[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled]))
					{
						if (transferEnabled)
						{
							MaybeScheduleWorkflowsResponsiveUpdate(logger, $"The {linkName} component link was enabled",
								(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
						}
						else
						{
							MaybeScheduleWorkflowsResponsiveUpdate(logger, $"The {linkName} component link was disabled",
								(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
						}
					}
					else if (transferEnabled && HasCellChanged(row, changeTable.Columns[BMComponentLinkSchema.Constants.FL_FC_ComponentTo]))
					{
						MaybeScheduleWorkflowsResponsiveUpdate(logger, $"The {linkName} component link changed ComponentTo",
							(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
					}
					else if (transferEnabled && HasCellChanged(row, changeTable.Columns[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied]))
					{
						if ((bool)row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Current])
						{
							MaybeScheduleWorkflowsResponsiveUpdate(logger, $"Release gate on the {linkName} component link was enabled",
								(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
						}
						else
						{
							MaybeScheduleWorkflowsResponsiveUpdate(logger, $"Release gate on the {linkName} component link was disabled",
								(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
						}
					}
				}
				else if (row.RowState == DataRowState.Deleted)
				{
					if ((bool)row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Original])
					{
						MaybeScheduleWorkflowsResponsiveUpdate(logger, $"The {linkName} component link was deleted",
							(Guid)row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original], ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, scheduleFactory, readonlyFactory);
					}
				}
			}
			scheduleFactory.Save();
		}

		#endregion

		#region Implementation

		string GetLinkName(ReadOnlyBusinessObjectFactory readonlyFactory, Guid linkPK)
		{
			var link = readonlyFactory.Load<IBMComponentLink>(linkPK);
			var name = link != null ? link.DisplayText.ToString() : "<deleted>";
			return $"{name} (PK = {linkPK})";
		}

		#endregion
	}
}
