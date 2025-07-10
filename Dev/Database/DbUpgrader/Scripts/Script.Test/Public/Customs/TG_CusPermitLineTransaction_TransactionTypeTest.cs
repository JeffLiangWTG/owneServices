using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_CusPermitLineTransaction_TransactionType))]
	class TG_CusPermitLineTransaction_TransactionTypeTest : TG_CusPermitLineTransactionTest
	{
		[ExpectNoExceptions]
		public void TestInsertValid()
		{
			InsertCusPermitLineTransaction(PermitHeaderPK, "OBL");
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, "OBA");
		}

		public void TestInsertInvalid()
		{
			AssertExceptionMessage(OpenBalanceAdjustmentErrorMessage, () => InsertCusPermitLineTransaction(PermitHeaderPK, "OBA"));
		}

		[ExpectNoExceptions]
		public void TestUpdateValid()
		{
			InsertCusPermitLineTransaction(PermitHeaderPK, "OBL");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionType = 'TRA', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{PermitHeaderPK}';");
		}

		public void TestUpdatePermitInvalid()
		{
			InsertCusPermitLineTransaction(PermitHeaderPK, "OBL");
			AssertExceptionMessage(OpenBalanceAdjustmentErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionType = 'OBA', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{PermitHeaderPK}';");
		}

		public void TestUpdateGuaranteeInvalid()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, "OBL");
			AssertExceptionMessage(UpdateErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_TransactionType = 'OBA', 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';");
		}

		const string OpenBalanceAdjustmentErrorMessage = "Transaction (CPL_TransactionType = 'OBA') can only be inserted/updated for Guarantee (CPH_ApplicationCode = 'GUA')";
		const string UpdateErrorMessage = "Transaction (CPL_TransactionType) cannot be updated for Guarantee (CPH_ApplicationCode = 'GUA')";
	}
}
