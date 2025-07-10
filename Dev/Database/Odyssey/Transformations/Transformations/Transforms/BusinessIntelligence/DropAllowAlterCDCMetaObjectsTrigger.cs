using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.BusinessIntelligence
{
	class DropAllowAlterCDCMetaObjectsTrigger : DataTransformation
	{
		public override string UserDescription => "Drop TG_AllowAlterCDCMetaObjects database trigger if disabled";

		const string TriggerName = "TG_AllowAlterCDCMetaObjects";

		protected override void OnlinePreUpgradeTransform()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'{0}' and is_disabled = 1)
				BEGIN
					DROP TRIGGER " + TriggerName + @" ON DATABASE;
				END",
				TriggerName
			);

			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
