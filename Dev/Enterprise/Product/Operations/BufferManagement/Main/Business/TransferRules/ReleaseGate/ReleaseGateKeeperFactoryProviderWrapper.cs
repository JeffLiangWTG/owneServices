using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateKeeperFactoryProviderWrapper : ServiceTaskFactoryProviderWrapper
	{
		public ReleaseGateKeeperFactoryProviderWrapper(ILogger logger, bool enableDataRefresh, string serviceTaskCode = null)
			: base(logger, serviceTaskCode)
		{
			this.enableDataRefresh = enableDataRefresh;
		}

		readonly bool enableDataRefresh;

		protected override BusinessObjectFactory CreateStartingFactory()
		{
			// We should never be able to load the parent jobs of a task or workflow in the Release Gate. All the context we need to make decisions about release lives here.
			// Any additional concerns that don't require saving can use a different factory, so time-consuming calls to OnFactorySaving are not made.

			return new RestrictedTableBusinessObjectFactory(allowedTableNamesToLoad: GetAllowedTableNamesToLoad())
			{
				NameForDebugging = "ReleaseGateKeeper",
				RefreshEnabled = enableDataRefresh,
			};
		}

		static string[] GetAllowedTableNamesToLoad()
		{
			var tableNames = new List<string>();
			tableNames.AddRange(new[]
			{
				ViewProcessHeaderSchema.Constants.TableName,
				ProcessHeaderLinkSchema.Constants.TableName,
				ViewProcessTaskSchema.Constants.TableName,

				BMComponentSchema.Constants.TableName,
				BMComponentResourceLinkSchema.Constants.TableName,
				BMComponentReleaseGroupLinkSchema.Constants.TableName,
				BMZoneCapacityMultiplierSchema.Constants.TableName,

				GlbDepartmentSchema.Constants.TableName,
				GlbStaffSchema.Constants.TableName,
				GlbGroupSchema.Constants.TableName,
				GlbGroupLinkSchema.Constants.TableName,
				GlbCapabilitySchema.Constants.TableName,
				GlbResourceCapabilityPivotSchema.Constants.TableName,
				GlbBranchSchema.Constants.TableName,
				GlbHolidaySchema.Constants.TableName,
				GlbStaffHolidaySchema.Constants.TableName,
				GlbWorkTimeSchema.Constants.TableName,

				RefTimeZoneSchema.Constants.TableName,
				RefTimeZoneRuleSchema.Constants.TableName,
				RefTimeZoneSetSchema.Constants.TableName,

				TagDefinitionSchema.Constants.TableName,
				TagMagnitudeSchema.Constants.TableName,
				TagLinkSchema.Constants.TableName,

				StmALogSchema.Constants.TableName,
				StmNoteSchema.Constants.TableName,
				StmUniversalCopySchema.Constants.TableName,
			});

			if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement)
			{
				tableNames.AddRange(new[]
				{
					BMNCNAttachmentSchema.Constants.TableName,
					BMNCNScheduleSchema.Constants.TableName,
					BMNCNShapeSchema.Constants.TableName,
				});
			}

			return tableNames.ToArray();
		}
	}
}
