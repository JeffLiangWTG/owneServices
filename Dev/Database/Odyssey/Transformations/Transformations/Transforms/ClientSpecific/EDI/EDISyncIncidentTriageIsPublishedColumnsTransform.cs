using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class EDISyncIncidentTriageIsPublishedColumnsTransform : DataTransformation
	{
		public override string UserDescription => "Copy the value from IMT_IsPublished to IMT_IsPublishedToAssist";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentTriage", "dbo"))
			{
				var sql = @"UPDATE dbo.IncidentTriage SET 
IMT_IsPublishedToAssist = IMT_IsActive,
IMT_SystemLastEditUser = 'E',
IMT_SystemLastEditTimeUtc = GETUTCDATE();";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
