using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.CommissionManagement.GUI.Testing;

public class CommissionFinalizerLineItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestStopApprovalRequest()
	{
		var commissionFinalizer = new CommissionFinalizer();
		var item = commissionFinalizer.CommissionFinalizerLineItemCollection.AddNew(Factory.New<ViewCommissionLine>());

		item.ViewCommissionLine.VCL_Ledger = "AR";
		item.ViewCommissionLine.VCL_TransactionFullyPaidDate = ZDateTime.Today;
		item.ViewCommissionLine.Validation.ValidateFullyPaymentOfARInvoices();
		AssertEquals(false, item.Validation.ShouldStopApprovalRequest);

		item.ViewCommissionLine.VCL_Ledger = "AR";
		item.ViewCommissionLine.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
		item.ViewCommissionLine.VCL_LocalToPreferredExchangeRate = 1.5m;
		item.ViewCommissionLine.Validation.ValidateAllPreferredAmounts();
		item.ViewCommissionLine.Validation.ValidateFullyPaymentOfARInvoices();
		AssertEquals(true, item.Validation.ShouldStopApprovalRequest);

		item.ViewCommissionLine.VCL_RX_NKLocalCurrency = "GBP";
		item.ViewCommissionLine.VCL_RX_NKPreferredPaymentCurrency = "USD";
		item.ViewCommissionLine.VCL_CommissionToLocalExchangeRate = 0.55m;
		item.ViewCommissionLine.Validation.ValidateAllPreferredAmounts();
		AssertEquals(false, item.Validation.ShouldStopApprovalRequest);

		item.ViewCommissionLine.VCL_CommissionDate = new ZDate(2002, 2, 2);
		item.ViewCommissionLine.VCL_RX_NKCommissionCurrency = "AUD";
		var preferredAmountPropertyInfos = new[]
			{
					item.ViewCommissionLine.VCL_TotalCommissionableAmountInPreferredCurrencyInfo,
					item.ViewCommissionLine.VCL_ShareCommissionAmountInPreferredCurrencyInfo,
					item.ViewCommissionLine.VCL_EntityCommissionAmountInPreferredCurrencyInfo
				};
		item.ViewCommissionLine.VCL_RX_NKPreferredPaymentCurrency = "";
		item.ViewCommissionLine.Validation.ValidateAllPreferredAmounts();
		item.ViewCommissionLine.Validation.ValidateFullyPaymentOfARInvoices();
		AssertEquals(true, item.Validation.ShouldStopApprovalRequest);

		item.ViewCommissionLine.VCL_RX_NKLocalCurrency = "GBP";
		item.ViewCommissionLine.VCL_RX_NKPreferredPaymentCurrency = "USD";
		item.ViewCommissionLine.Validation.ValidateAllPreferredAmounts();
		AssertEquals(true, item.Validation.ShouldStopApprovalRequest);
	}
}
