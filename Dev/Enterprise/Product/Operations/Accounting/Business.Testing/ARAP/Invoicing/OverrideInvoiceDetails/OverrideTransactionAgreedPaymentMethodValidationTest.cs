using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideTransactionAgreedPaymentMethodValidationTest : OverrideInvoiceDetailValidationTest
	{
		protected override Type InvoiceType => typeof(ARInvoice);

		protected override OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent)
		{
			if (parent.HasContext(BusinessContext.OverrideTransactionAgreedPaymentMethod))
			{
				return parent.Validation as OverrideTransactionAgreedPaymentMethodValidation;
			}
			else
			{
				return parent.Validation as OverrideInvoiceDetailValidation;
			}
		}

		protected override void SetBusinessContext(InvoicingBase invoice)
		{
			invoice.SetContext(BusinessContext.OverrideTransactionAgreedPaymentMethod);
		}

		public void TestCheckAH_DueDate()
		{
			var header = (InvoicingBase)Factory.New(InvoiceType);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var today = ZDateTime.Today;
			header.AH_InvoiceDate = today;
			header.AH_DueDate = today.AddDays(30);
			header.AH_OH = org.PK;
			Factory.Save();

			header.SetContext(BusinessContext.OverrideTransactionAgreedPaymentMethod);

			header.AH_DueDate = ZDateTime.Empty;
			AssertHasErrors(header.AH_DueDateInfo);

			header.AH_DueDate = today.AddDays(-10);
			AssertHasErrors(header.AH_DueDateInfo);

			header.AH_DueDate = today.AddDays(10);
			AssertNoErrors(header.AH_DueDateInfo);
		}
	}
}