using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class OverrideReceiptPaymentCashFlowCategoryValidationTest : OverrideReceiptPaymentDetailValidationTest
	{
		public void TestCheckDisplayCashFlowCategoryOverride()
		{
			ReceiptPaymentBase header = (ReceiptPaymentBase)Factory.New(InvoiceType);
			header.AH_OH = Creator.ABIGAS.PK;

			header.SetContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory);
			OverrideReceiptPaymentCashFlowCategoryValidation testValidation = (OverrideReceiptPaymentCashFlowCategoryValidation)header.Validation;
			header.DisplayCashFlowCategoryOverride = "ZZZ";
			AssertHasErrors(header.DisplayCashFlowCategoryOverrideInfo);

			header.DisplayCashFlowCategoryOverride = "O01";
			AssertNoErrors(header.DisplayCashFlowCategoryOverrideInfo);
		}

		protected override OverrideReceiptPaymentDetailValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as OverrideReceiptPaymentCashFlowCategoryValidation;
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARReceipt);
			}
		}

		protected override void SetBusinessContext(ReceiptPaymentBase receipPayment)
		{
			receipPayment.SetContext(BusinessContext.OverrideReceiptPaymentCashFlowCategory);
		}

		TestObjectCreator fCreator;
		public TestObjectCreator Creator
		{
			get
			{
				return fCreator ?? (fCreator = new TestObjectCreator(Factory));
			}
		}
	}
}