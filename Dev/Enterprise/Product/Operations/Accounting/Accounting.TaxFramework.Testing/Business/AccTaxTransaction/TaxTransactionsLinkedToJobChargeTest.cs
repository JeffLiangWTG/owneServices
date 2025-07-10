using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(TaxTransactionsLinkedToJobCharge))]
	public class TaxTransactionsLinkedToJobChargeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTaxTransactionPropertyMatching()
		{
			var accTaxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accTaxTransaction.ATT_A9_TaxMessage = accInvMsg.PK;
			accTaxTransaction.ATT_TaxAuthorityServiceCode = "Other";
			accTaxTransaction.ATT_RealisationDate = ZDate.Today;

			var pivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_LocalTaxAmount = 2m;

			var taxTransactionsLinkedToJobCharge = new TaxTransactionsLinkedToJobCharge(accTaxTransaction, pivot);
			AssertEquals(accTaxTransaction.TaxID.AT_Code, taxTransactionsLinkedToJobCharge.ATT_AT_TaxID);
			AssertEquals(accTaxTransaction.TaxMessage.A9_Code, taxTransactionsLinkedToJobCharge.ATT_A9_TaxMessage);
			AssertEquals(accTaxTransaction.ATT_Rate, taxTransactionsLinkedToJobCharge.ATT_Rate);
			AssertEquals(accTaxTransaction.ATT_RX_NKOSTaxCurrency, taxTransactionsLinkedToJobCharge.ATT_RX_NKOSTaxCurrency);
			AssertEquals(accTaxTransaction.ATT_OSTaxBaseAmount, taxTransactionsLinkedToJobCharge.ATT_OSTaxBaseAmount);
			AssertEquals(accTaxTransaction.ATT_OSTaxAmount, taxTransactionsLinkedToJobCharge.ATT_OSTaxAmount);
			AssertEquals(accTaxTransaction.ATT_TaxAuthorityServiceCode, taxTransactionsLinkedToJobCharge.ATT_TaxAuthorityServiceCode);
			AssertEquals(accTaxTransaction.ATT_TaxSystemCode, taxTransactionsLinkedToJobCharge.ATT_TaxSystemCode);
			AssertEquals(accTaxTransaction.ATT_LocalTaxBaseAmount, taxTransactionsLinkedToJobCharge.ATT_LocalTaxBaseAmount);
			AssertEquals(accTaxTransaction.ATT_LocalTaxAmount, taxTransactionsLinkedToJobCharge.ATT_LocalTaxAmount);
			AssertEquals(accTaxTransaction.ATT_Basis, taxTransactionsLinkedToJobCharge.ATT_Basis);
			AssertEquals(accTaxTransaction.ATT_AffectsSourceTransactionTotal, taxTransactionsLinkedToJobCharge.ATT_AffectsSourceTransactionTotal);
			AssertEquals(accTaxTransaction.ATT_TaxDate, taxTransactionsLinkedToJobCharge.ATT_TaxDate);
			AssertEquals(accTaxTransaction.ATT_PostDate, taxTransactionsLinkedToJobCharge.ATT_PostDate);
			AssertEquals(accTaxTransaction.ATT_Ledger, taxTransactionsLinkedToJobCharge.ATT_Ledger);
			AssertEquals(accTaxTransaction.ATT_TaxSuperType, taxTransactionsLinkedToJobCharge.ATT_TaxSuperType);
			AssertEquals(accTaxTransaction.ATT_RealisationDate, taxTransactionsLinkedToJobCharge.ATT_RealisationDate);
			AssertEquals(accTaxTransaction.ATT_AH_MatchTransaction_ForBinding, taxTransactionsLinkedToJobCharge.ATT_AH_MatchTransaction_ForBinding);
			AssertEquals(accTaxTransaction.TaxAuthorityCode, taxTransactionsLinkedToJobCharge.TaxAuthorityCode);
			AssertEquals(accTaxTransaction.ATT_TaxAuthorityServiceCodeDescription, taxTransactionsLinkedToJobCharge.ATT_TaxAuthorityServiceCodeDescription);
			AssertEquals(pivot.ATP_LocalTaxAmount, taxTransactionsLinkedToJobCharge.ATP_LocalTaxAmount);
		}
	}
}
