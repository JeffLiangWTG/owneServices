using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class UpdateUS_SupplementalTariffWithOnlyNumbers : DataTransformation
	{
		public override string UserDescription => "Update US_SupplementalTariff Of CusClassPartPivot With Only Numbers";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.CusClassPartPivot
SET CI_SupplementalTariff = REPLACE(REPLACE(CI_SupplementalTariff, '.', ''), ' ', ''), CI_SystemLastEditTimeUtc = GETUTCDATE(), CI_SystemLastEditUser = '~BP'
WHERE CI_SupplementalTariff<> '' and CI_SupplementalTariff LIKE '%[. ]%'
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
