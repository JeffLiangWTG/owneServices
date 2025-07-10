using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_CusPermitLineTransaction_IsAggregated))]
	class TG_CusPermitLineTransaction_IsAggregatedTest : TG_CusPermitLineTransactionTest
	{
		[ExpectNoExceptions]
		public void TestUpdateValid()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, isAggregated: false);
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_IsAggregated = 0, 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';");
		}

		public void TestUpdateInvalid()
		{
			InsertCusPermitLineTransaction(GuaranteeHeaderPK, isAggregated: true);
			AssertExceptionMessage(UpdateErrorMessage, @$"
UPDATE dbo.CusPermitLineTransaction 
SET 
    CPL_IsAggregated = 0, 
    CPL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CPL_SystemLastEditUser = '~BP' 
WHERE 
    CPL_CPH_PermitHeader = '{GuaranteeHeaderPK}';");
		}

		const string UpdateErrorMessage = "Transaction (CPL_IsAggregated) cannot be updated from 1 to 0 for Guarantee (CPH_ApplicationCode = 'GUA')";
	}
}
