using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideInvoiceReferenceValidationTest : InvoiceBaseValidationTest
	{
		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			if (parent.HasContext(BusinessContext.OverrideInvoiceReference))
			{
				return parent.Validation as OverrideInvoiceReferenceValidation;
			}
			else
			{
				return parent.Validation as InvoiceBaseValidation;
			}
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(APInvoice);
			}
		}

		public override void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_OSTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public override void TestCheckAH_OSTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is already covered in APCreditNoteValidationTest", true);
		}

		public void TestCheckAH_TransactionNum()
		{
			var header = (InvoicingBase)Factory.New(InvoiceType);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			header.AH_TransactionNum = "TN001";
			header.AH_OH = org.PK;
			Factory.Save();

			header.SetContext(BusinessContext.OverrideInvoiceReference);
			header.AH_TransactionNum = "TN001";

			AssertNoErrors(header.AH_TransactionNumInfo);
		}
	}
}