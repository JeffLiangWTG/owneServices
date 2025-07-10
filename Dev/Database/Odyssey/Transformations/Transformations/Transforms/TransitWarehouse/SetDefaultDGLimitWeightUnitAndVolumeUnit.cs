using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class SetDefaultDGLimitWeightUnitAndVolumeUnit : DataTransformation
	{
		public override string UserDescription => "Set Default DG Limit Weight Unit ('KG') and Volume Unit ('M3')";

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsUNDGLimitSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, WhsUNDGLimitSchema.Constants.TableName, WhsUNDGLimitSchema.Constants.WWD_TotalWeightLimitUQ)
				&& DbObjectCreator.ColumnExists(Db.Connection, WhsUNDGLimitSchema.Constants.TableName, WhsUNDGLimitSchema.Constants.WWD_TotalVolumeLimitUQ))
			{
				var sqlCmd = @"
UPDATE dbo.WhsUNDGLimit
SET WWD_TotalWeightLimitUQ = 'KG'
	, WWD_SystemLastEditTimeUtc = GETUTCDATE()
	, WWD_SystemLastEditUser = '~BP'
WHERE
	WWD_TotalWeightLimitUQ = ''

UPDATE dbo.WhsUNDGLimit
SET WWD_TotalVolumeLimitUQ = 'M3'
	, WWD_SystemLastEditTimeUtc = GETUTCDATE()
	, WWD_SystemLastEditUser = '~BP'
WHERE
	WWD_TotalVolumeLimitUQ = ''";

				using (var cmd = Db.Connection.Command(sqlCmd))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
