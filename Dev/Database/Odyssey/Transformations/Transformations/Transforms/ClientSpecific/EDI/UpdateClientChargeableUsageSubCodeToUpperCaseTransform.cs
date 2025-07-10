using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class UpdateClientChargeableUsageSubCodeToUpperCaseTransform : DataTransformation
	{
		public override string UserDescription => "Update ClientChargeableUsage SubCode To UpperCase";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "ClientChargeableUsage"))
			{
				var sql = $@"
UPDATE dbo.ClientChargeableUsage SET U1_SubCode = UPPER(U1_SubCode)
WHERE U1_PeriodStart >= '2024-07-01' AND
LEN(U1_SubCode) <= 5 AND
BINARY_CHECKSUM(upper(U1_SubCode)) <> BINARY_CHECKSUM(U1_SubCode);
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
