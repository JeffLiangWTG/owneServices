using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_CusPermitLineTransaction_TransactionStatus))]
	class TG_CusPermitLineTransaction_TransactionStatusTest : TG_CusPermitLineTransactionTest
	{
		[ExpectNoExceptions]
		public void TestUpdateValid()
		{
			InsertCusPermitLineTransaction(PermitHeaderPK, transactionStatus: "PND");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'CON', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{PermitHeaderPK}';");
			DeleteCusPermitLineTransaction(PermitHeaderPK);

			InsertCusPermitLineTransaction(PermitHeaderPK, transactionStatus: "PND");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'DEL', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{PermitHeaderPK}';
");
			DeleteCusPermitLineTransaction(PermitHeaderPK);

			InsertCusPermitLineTransaction(PermitHeaderPK, transactionStatus: "PND");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = '', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{PermitHeaderPK}';
");
			DeleteCusPermitLineTransaction(PermitHeaderPK);

			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "PND");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'CON', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
			DeleteCusPermitLineTransaction(GuaranteeHeaderPK);

			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "PND");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'DEL', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
			DeleteCusPermitLineTransaction(GuaranteeHeaderPK);
		}

		public void TestUpdateInvalidFromPendingToEmpty()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "PND");
			AssertExceptionMessage(UpdateFromPendingStatusErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = '', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
		}

		public void TestUpdateInvalidFromConfirmedToPending()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "CON");
			AssertExceptionMessage(UpdateStatusErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'PND', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
		}

		public void TestUpdateInvalidFromConfirmedToEmpty()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "CON");
			AssertExceptionMessage(UpdateStatusErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = '', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
		}

		public void TestUpdateInvalidFromDeletedToPending()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "DEL");
			AssertExceptionMessage(UpdateStatusErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = 'PND', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
		}

		public void TestUpdateInvalidFromDeletedToEmpty()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, transactionStatus: "DEL");
			AssertExceptionMessage(UpdateStatusErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionStatus = '', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';
");
		}

		const string UpdateFromPendingStatusErrorMessage = "Transaction (CPL_TransactionStatus) can only be changed from 'PND' to 'DEL' OR 'CON' for Guarantee (CPH_ApplicationCode = 'GUA')";
		const string UpdateStatusErrorMessage = "Transaction (CPL_TransactionStatus) cannot be changed from 'DEL' OR 'CON' to any other statuses for Guarantee (CPH_ApplicationCode = 'GUA')";
	}
}
