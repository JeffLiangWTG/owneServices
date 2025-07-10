using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(OverrideReceiptPaymentCashFlowCategoryHelper))]
	public class OverrideReceiptPaymentCashFlowCategoryHelperTest : OverrideReceiptPaymentDetailsHelperTest
	{
		protected override void AssertBusinessContext(ReceiptPaymentBase receiptPayment)
		{
			Assert("ReceiptPayment is in correct context", receiptPayment.HasContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory));
		}

		protected override void AssertWritableColumns(ReceiptPaymentBase receiptPayment)
		{
			AssertEquals("receiptPayment.DisplayCashFlowCategoryOverrideInfo.ReadOnly", false, receiptPayment.DisplayCashFlowCategoryOverrideInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ReceiptPaymentPKs != null ? new OverrideReceiptPaymentCashFlowCategoryHelper(Factory, ReceiptPaymentPKs) : new OverrideReceiptPaymentCashFlowCategoryHelper(Factory, Factory.NewWithValidTestData<ARReceipt>().PK);
		}
	}
}
