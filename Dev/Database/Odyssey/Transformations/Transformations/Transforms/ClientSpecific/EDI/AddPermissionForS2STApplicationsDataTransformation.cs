using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class AddPermissionForS2STApplicationsDataTransformation : DataTransformation
	{
		public override string UserDescription => "Set Permission for each S2ST Application.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplicationPermission") && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplication"))
			{
				var sql = @"INSERT INTO dbo.EdiIdentityApplicationPermission (IAP_PK, IAP_IDA, IAP_Scope, IAP_SystemCreateUser, IAP_SystemCreateTimeUtc, IAP_SystemLastEditUser, IAP_SystemLastEditTimeUtc) 
SELECT NEWID(), IDA_PK, '*', '~BP', GETUTCDATE(), '~BP', GETUTCDATE() 
FROM 
dbo.EdiIdentityApplication 
WHERE IDA_IDT IS NOT NULL;";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
