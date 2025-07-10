using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public sealed class UpdateStatusOnAccDraftInvoiceHeaderForPostedTransactions : DataTransformation
	{
		public override string UserDescription => @"Update AIH_Status to AFP (Approved For Posting) on AccDraftInvoiceHeader for the posted transactions.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.AccDraftInvoiceHeader
SET
	AIH_Status = 'AFP',
	AIH_SystemLastEditTimeUtc = GETDATE(),
	AIH_SystemLastEditUser = '~BP'
WHERE AIH_AH_PostedTransactionHeader IS NOT NULL";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
