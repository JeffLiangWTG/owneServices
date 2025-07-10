using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	abstract class TermsAndDueDateCalculationProviderTest : TestCaseWithFactory
	{
		public void TestCalculateDueDate()
		{
			ZDateTime currentDate = ZDateTime.Now;
			Invoice.AH_InvoiceDate = currentDate;
			Invoice.AH_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			Invoice.AH_InvoiceTermDays = 0;
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date should be today", currentDate.Date, Invoice.AH_DueDate.Date);
		}

		public void TestGetInvoiceDate()
		{
			var currentDate = ZDateTime.Now;
			Invoice.AH_InvoiceDate = currentDate;
			Invoice.AH_DocumentReceivedDate = currentDate.AddDays(1);
			Invoice.AH_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			Invoice.AH_InvoiceTermDays = 0;
			CalculationProvider.CalculateDueDate();
			AssertEquals("Due Date should be today based on Invoice Date", currentDate.Date, Invoice.AH_DueDate.Date);

			using (AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				if (Invoice is APInvoice)
				{
					CalculationProvider.CalculateDueDate();
					AssertEquals("Due Date should be tomorrow based on Document Received Date", currentDate.Date.AddDays(1), Invoice.AH_DueDate.Date);
				}
				else
				{
					CalculationProvider.CalculateDueDate();
					AssertEquals("Due Date should be today still based on Invoice Date", currentDate.Date, Invoice.AH_DueDate.Date);
				}
			}
		}

		public abstract void TestSetInvoiceTerms();

		public virtual void TestCanInvoiceTermBeSelectedError()
		{
			Assert("The test is now suitable only for AR Invoices", true);
		}

		#region Implementation

		protected Invoice Invoice;
		protected TermsAndDueDateCalculationProvider CalculationProvider;
		protected OrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
			Organisation.OH_Code = "ORG1";
			Invoice = GetInvoice();
			CalculationProvider = GetCalculationProvider(Invoice);
			SetupOrganisation();
			SetupInvoice();
		}

		protected virtual void SetupOrganisation()
		{
			Organisation.OH_IsDebtor = true;
			Organisation.OH_IsCreditor = true;
		}

		protected virtual void SetupInvoice()
		{
			Invoice.AH_OH = Organisation.PK;
		}

		protected abstract TermsAndDueDateCalculationProvider GetCalculationProvider(Invoice invoice);

		protected abstract Invoice GetInvoice();

		#endregion

	}
}