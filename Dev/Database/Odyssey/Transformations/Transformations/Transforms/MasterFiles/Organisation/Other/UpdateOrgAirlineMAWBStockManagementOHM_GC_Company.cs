using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles.Organisation.Other
{
	public class UpdateOrgAirlineMAWBStockManagementOHM_GC_Company : DataTransformation
	{
		public override string UserDescription => "Update existing records of table [OrgAirlineMAWBStockManagement] to set value of new columns [OHM_GC_Company]";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE [dbo].[OrgAirlineMAWBStockManagement]
SET
	[OHM_GC_Company] = (SELECT TOP 1 [GB_GC] FROM [GlbBranch] WHERE [GlbBranch].[GB_PK] = [OrgAirlineMAWBStockManagement].[OHM_GB_Branch])
	,[OHM_SystemLastEditTimeUtc] = GetUTCDate()
	,[OHM_SystemLastEditUser] = '~BP'
WHERE [OHM_GC_Company] IS NULL AND [OHM_GB_Branch] IS NOT NULL;
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
