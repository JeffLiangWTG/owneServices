using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class OverrideReceiptPaymentCashFlowCategoryHelper : OverrideReceiptPaymentDetailsHelper
	{
		public OverrideReceiptPaymentCashFlowCategoryHelper(BusinessObjectFactory factory, params ZGuid[] receiptPaymentPKs)
			: base(factory, receiptPaymentPKs)
		{
		}

		protected override string[] ColumnsToOverride()
		{
			return new string[]
			{
				"DisplayCashFlowCategoryOverride"
			};
		}

		protected override void SetBusinessContext(ReceiptPaymentBase receiptPayment)
		{
			receiptPayment.SetContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory);
		}
	}
}
