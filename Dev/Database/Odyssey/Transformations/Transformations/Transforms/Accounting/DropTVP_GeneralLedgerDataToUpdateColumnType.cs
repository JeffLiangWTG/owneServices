using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class DropTVP_GeneralLedgerDataToUpdateColumnType : DataTransformation
	{
		public override string UserDescription => "Drop TVP_GeneralLedgerData to update GLD_JournalEntriesNumber column from varchar to nvarchar during the upgrade";

		protected override void OfflinePreUpgradeTransform()
		{
			var sqlText = @"
				IF (TYPE_ID(N'dbo.TVP_GeneralLedgerData') is NOT NULL)
				BEGIN
					DROP TYPE dbo.TVP_GeneralLedgerData;
				END
				";

			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
