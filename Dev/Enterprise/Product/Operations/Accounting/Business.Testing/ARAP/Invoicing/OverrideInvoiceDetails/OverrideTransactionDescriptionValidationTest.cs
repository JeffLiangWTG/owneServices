using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideTransactionDescriptionValidationTest : OverrideInvoiceDetailValidationTest
	{
		public void TestCheckAH_Desc()
		{
			InvoicingBase header = (InvoicingBase)Factory.New(InvoiceType);

			SetBusinessContext(header);
			OverrideTransactionDescriptionValidation testValidation = (OverrideTransactionDescriptionValidation)header.Validation;
			header.AH_Desc = "";
			AssertHasErrors(header.AH_DescInfo);

			header.AH_Desc = "Desc";
			AssertNoErrors(header.AH_DescInfo);
		}

		protected override OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as OverrideTransactionDescriptionValidation;
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARInvoice);
			}
		}

		protected override void SetBusinessContext(InvoicingBase invoice)
		{
			invoice.SetContext(BusinessContext.OverrideTransactionDescription);
		}
	}
}