using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class AddParentApplicationForEAdaptorApplicationDataTransformation : DataTransformation
	{
		public override string UserDescription => "Add ParentApplication to each eAdaptor Application.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplication"))
			{
				var sql = @"
With childApp AS ( 
SELECT child.IDA_IDA_ParentApplication, child.IDA_SystemLastEditTimeUtc, child.IDA_SystemLastEditUser, ld.LD_PK FROM dbo.EdiIdentityApplication AS child 
CROSS APPLY ( 
SELECT 
    PARSENAME(child.IDA_ApplicationName, 4) AS EnterpriseCode,
    PARSENAME(child.IDA_ApplicationName, 3) AS ServerCode) AS parsed 
INNER JOIN dbo.LicenceEnterprise AS le 
    ON le.LE_EnterpriseCode = parsed.EnterpriseCode 
INNER JOIN dbo.LicenceDatabase AS ld 
    ON ld.LD_ServerCode = parsed.ServerCode 
    AND ld.LD_LE = le.LE_PK 
WHERE child.IDA_ApplicationModule = 'eAdaptor' 
)

UPDATE child 
SET 
    child.IDA_IDA_ParentApplication = parent.IDA_PK,
    child.IDA_SystemLastEditTimeUtc = GETUTCDATE(), 
    child.IDA_SystemLastEditUser = '~BP' 
FROM 
    childApp AS child 
INNER JOIN dbo.EdiIdentityApplication AS parent 
    ON parent.IDA_LD = child.LD_PK;";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
