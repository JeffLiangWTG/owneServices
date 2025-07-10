using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	class UpdateEdiIdentityApplicationForNonExistingAzureApplication : DataTransformation
	{
		public override string UserDescription => "Update EdiIdentityApplication for non existing Azure Application.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplication"))
			{
				var sql = "UPDATE dbo.EdiIdentityApplication SET IDA_ClientID = '' WHERE IDA_ClientID = '644D4A36-07FD-43E0-A4F1-F0539E456F62'";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
