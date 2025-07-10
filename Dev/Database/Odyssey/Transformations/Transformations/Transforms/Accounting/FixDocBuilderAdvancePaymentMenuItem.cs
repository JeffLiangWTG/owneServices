using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public sealed class FixDocBuilderAdvancePaymentMenuItem : DataTransformation
	{
		public override string UserDescription => "Fix a property on the DocBuilder Advance Payment Request document";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.StmMenuItem
SET SU_PreventAutoDelivery = 0,
	SU_SystemLastEditTimeUtc = GetUtcDate(),
	SU_SystemLastEditUser = '~BP'
WHERE SU_MenuName = 'DocBuilder Advance Payment Request'
	AND SU_IsSystemDefined = 1
	AND SU_BusinessContext = 'Shipment'
	AND SU_PreventAutoDelivery = 1";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
