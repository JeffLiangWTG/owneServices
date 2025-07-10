
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
	public class BMComponentResponsiveWorkflowUpdateSubscriber : BMComponentRelatedResponsiveWorkflowUpdateSubscriberBase
	{
		#region Properties

		public override string Code => "CPW";

		public override string Description => nameof(BMComponentResponsiveWorkflowUpdateSubscriber);

		public override ITableSchema Table => BMComponentSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => [BMComponentSchema.FC_IsActive, BMComponentSchema.FC_Type, BMComponentSchema.FC_GB_AgingBranch, BMComponentSchema.FC_GE_AgingDepartment];

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

			var scheduleFactory = new BusinessObjectFactory();
			scheduleFactory.NameForDebugging = $"{nameof(BMComponentLinkResponsiveWorkflowUpdateSubscriber)}.{nameof(scheduleFactory)}";
			var readonlyFactory = new ReadOnlyBusinessObjectFactory();
			readonlyFactory.NameForDebugging = $"{nameof(BMComponentLinkResponsiveWorkflowUpdateSubscriber)}.{nameof(readonlyFactory)}";

			var rows = changeTable.Rows.Cast<DataRow>();

			foreach (var row in rows)
			{
				var isActiveChanged = HasCellChanged(row, changeTable.Columns[BMComponentSchema.Constants.FC_IsActive]);
				var typeChanged = HasCellChanged(row, changeTable.Columns[BMComponentSchema.Constants.FC_Type]);
				var agingBranchChanged = HasCellChanged(row, changeTable.Columns[BMComponentSchema.Constants.FC_GB_AgingBranch]);
				var agingDepartmentChanged = HasCellChanged(row, changeTable.Columns[BMComponentSchema.Constants.FC_GE_AgingDepartment]);

				var needToUpdateEffectiveBranchDepartmentOnModifiedComponent = typeChanged || agingBranchChanged || agingDepartmentChanged;

				if (isActiveChanged || typeChanged || agingBranchChanged || agingDepartmentChanged)
				{
					var operationTypeForModifiedComponent = isActiveChanged && needToUpdateEffectiveBranchDepartmentOnModifiedComponent
						? ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment
						: isActiveChanged
							? ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer
							: ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment;

					var operationTypeForPrecedingComponents = isActiveChanged || typeChanged
						? ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer
						: ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment;

					var isActive = (bool)row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current];
					var modifiedPropertyNamesAndConditions = new (string Name, bool Condition)[] {
						($"activity status ({(isActive ? "activated" : "deactivated")})", isActiveChanged),
						("type", typeChanged),
						("aging branch", agingBranchChanged),
						("aging department", agingDepartmentChanged),
					};
					var modifiedPropertyNames = modifiedPropertyNamesAndConditions.Where(pair => pair.Condition).Select(pair => pair.Name).ToArray();
					var lastPropertyName = modifiedPropertyNames.Last();
					string modificationDesc;

					if (modifiedPropertyNames.Length == 1)
					{
						modificationDesc = lastPropertyName;
					}
					else
					{
						var commaSeparatedPropertyNames = modifiedPropertyNames.Except(lastPropertyName);
						modificationDesc = string.Join(" and ", [string.Join(", ", commaSeparatedPropertyNames), lastPropertyName]);
					}
					modificationDesc = "changed its " + modificationDesc;

					var componentPK = (Guid)row[BMComponentSchema.Constants.PK, DataRowVersion.Current]; // we don't react on component deletion here, therefore we don't need original version - just current

					var incomingLinkQuery = new ZQuery(BMComponentLinkSchema.FL_FC_ComponentTo, componentPK);
					incomingLinkQuery.AddToFilter(BMComponentLinkSchema.FL_TransferRulesEnabled, true);
					var incomingLinks = readonlyFactory.Load<IBMComponentLink>(incomingLinkQuery);
					var precedingComponentPKs = incomingLinks.Select(l => l.FL_FC_ComponentFrom).ToArray();
					var allComponents = precedingComponentPKs.Append(componentPK).ToArray();

					foreach (var pk in allComponents)
					{
						readonlyFactory.AddFetchHint(BMComponentSchema.Constants.TableName, pk);
					}

					var component = LoadComponentOrLogComponentDeleted(readonlyFactory, componentPK, logger, $"Component {modificationDesc}");

					if (component == null)
					{
						return;
					}
					var componentName = GetComponentDisplayName(component);
					var componentType = (string)row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Current];

					if (needToUpdateEffectiveBranchDepartmentOnModifiedComponent && !typeChanged && !isActiveChanged && componentType != BMComponentTypeList.Codes.Buffer)
					{
						logger.Log(LogType.Debug, $"Component {componentName} {modificationDesc}: no need to schedule responsive update of effective branch/department on workflows as the component is not a buffer.");
						return;
					}

					foreach (var pk in allComponents)
					{
						scheduleFactory.AddFetchHint(TimeActionScheduleSchema.TAS_TargetPK, pk); // each time when we schedule we check if there is already a scheduled action in the db for the given component
					}

					MaybeScheduleWorkflowsResponsiveUpdate(logger, $"Component {componentName} {modificationDesc}", componentPK, operationTypeForModifiedComponent, scheduleFactory, readonlyFactory);

					if (operationTypeForPrecedingComponents != null)
					{
						var precedingComponentQuery = new ZQuery(BMComponentSchema.PK, precedingComponentPKs);
						precedingComponentQuery.AddToFilter(BMComponentSchema.FC_IsActive, true);
						var precedingComponents = readonlyFactory.Load<IBMComponent>(precedingComponentQuery).OrderBy(c => c.FC_DisplaySequence);

						foreach (var precedingComponent in precedingComponents)
						{
							MaybeScheduleWorkflowsResponsiveUpdate(logger, $"An active component preceding to the modified component via active links", precedingComponent.PK.ToGuid(), operationTypeForPrecedingComponents, scheduleFactory, readonlyFactory);
						}
					}
				}
				else
				{
					ErrorReporter.ReportOnce("Something is wrong: BMComponentResponsiveWorkflowUpdateSubscriber should react to changes in FC_IsActive, FC_Type, FC_GB_AgingBranch and/or FC_GE_AgingDepartment, but here is a link without such changes");
				}
			}
			scheduleFactory.Save();
		}

		#endregion
	}
}
