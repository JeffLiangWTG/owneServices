using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class PopulateAllocationRouteWizardTemplateRowLinkScheduleType : DataTransformation
	{
		public override string UserDescription => "Populate ARR_LinkScheduleType from ARR_HasLinkedSchedule for all AllocationRouteWizardTemplateRows.";

		protected override void OfflinePreUpgradeTransform()
		{
			var allocationRouteWizardTemplateRowTableName = "AllocationRouteWizardTemplateRow";
			if (!DbObjectCreator.ColumnExists(Db.Connection, allocationRouteWizardTemplateRowTableName, "ARR_HasLinkedSchedule"))
			{
				return;
			}

			if (DbObjectCreator.TableExists(Db.Connection, allocationRouteWizardTemplateRowTableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, allocationRouteWizardTemplateRowTableName, "ARR_LinkScheduleType", "CHAR(3)", "'DNL'"))
			{
				var setLinkScheduleTypeSql = @"
					UPDATE dbo.[AllocationRouteWizardTemplateRow]
					SET [ARR_LinkScheduleType] = (CASE WHEN [ARR_HasLinkedSchedule] = 1 THEN 'ANY' ELSE 'DNL' END),
						[ARR_SystemLastEditTimeUtc] = GETUTCDATE(),
						[ARR_SystemLastEditUser] = '~BP'
					";
				Db.Connection.ExecuteNonQuery(setLinkScheduleTypeSql);
			}
		}
	}
}
