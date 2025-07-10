using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	public class FixFDDServiceTaskConfigString : DataTransformation
	{
		public override string UserDescription => "Fix FTP documents delivery service task customized configuration";

		protected override void OfflinePostUpgradeTransform()
		{
			var sqlText = @"
UPDATE dbo.StmServiceTask
SET 
	SST_Configuration = cast(replace(cast(SST_Configuration as nvarchar(max)), '&amp;', '&') as xml), 
	SST_SystemLastEditTimeUtc = GETUTCDATE(), 
	SST_SystemLastEditUser = '~BP'
WHERE SST_ServiceTaskCode = 'FDD'
AND SST_Configuration.exist('/ScheduleConfig/ConfigString[contains(., ""&amp;"")]') = 1
";

			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
