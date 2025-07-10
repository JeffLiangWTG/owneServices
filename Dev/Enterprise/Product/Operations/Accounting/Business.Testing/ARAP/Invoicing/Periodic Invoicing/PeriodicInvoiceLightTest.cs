using CargoWise.Types;
using static Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBulkPoster;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class PeriodicInvoiceLightTest : PeriodicInvoiceTest
	{
		protected override void SelectJobPks(ZGuid debtorPK, ZString invoiceType, ZGuid[] jobPKsToSelect, ZGuid[] chargeCodeToSelect)
		{
			var invoiceInfo = new InvoiceInfo();
			invoiceInfo.DebtorPK = debtorPK;
			invoiceInfo.InvoiceType = invoiceType;
			invoiceInfo.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			invoiceInfo.InvoiceDate = ZDateTime.Today;
			invoiceInfo.PostDate = ZDateTime.Today;
			invoiceInfo.SelectedJobs = jobPKsToSelect;
			invoiceInfo.Charges = chargeCodeToSelect;
			invoiceInfo.LocalExTaxAmount = 150M;
			invoiceInfo.LocalTaxAmount = 10M;
			invoiceInfo.MiscInvoices = System.Array.Empty<ZGuid>();
			Initialize(invoiceInfo);
		}

		protected InvoiceInfo PrepareInvoiceInfo(PeriodicInvoiceBase invoice, ZGuid[] jobPks, ZGuid[] chargePks, ZGuid[] invoicePks)
		{
			var invoiceInfo = new InvoiceInfo();
			invoiceInfo.DebtorPK = ((PeriodicInvoice)invoice).DebtorPK;
			invoiceInfo.InvoiceType = ((PeriodicInvoice)invoice).InvoiceType;
			invoiceInfo.CurrencyNK = invoice.CurrencyNK;
			invoiceInfo.InvoiceDate = invoice.InvoiceDate;
			invoiceInfo.PostDate = invoice.PostDate;
			invoiceInfo.SelectedJobs = System.Array.Empty<ZGuid>();
			if (jobPks != null)
			{
				invoiceInfo.SelectedJobs = jobPks;
			}
			invoiceInfo.Charges = System.Array.Empty<ZGuid>();
			if (chargePks != null)
			{
				invoiceInfo.Charges = chargePks;
			}
			invoiceInfo.MiscInvoices = System.Array.Empty<ZGuid>();
			if (invoicePks != null)
			{
				invoiceInfo.MiscInvoices = invoicePks;
			}
			invoiceInfo.LocalExTaxAmount = invoice.LocalExTaxAmount;
			invoiceInfo.LocalTaxAmount = invoice.LocalTaxAmount;
			return invoiceInfo;
		}

		protected abstract void Initialize(InvoiceInfo invoiceInfo);

		public new void TestJobValidationOnDebtor()
		{
			Assert("This test is not suitable here as RunPresaveValidation is overridden.", true);
		}

		public new void TestValidateBranchDepartmentCombination()
		{
			Assert("This test is not suitable here as RunPresaveValidation is overridden.", true);
		}

		public new void TestLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled()
		{
			Assert("This test is not suitable here as LoadJobsCore and ReloadChargesCore are overridden.", true);
		}

		public new void TestPreSaveValidationDoesNotLeaveErrorsOnUntickedJobs()
		{
			Assert("This test is not suitable here as RunPresaveValidation is overridden.", true);
		}

		public new void TestValidatePostDate()
		{
			Assert("This test is not suitable here as RunPresaveValidation is overridden.", true);
		}

		public new void TestCalculateTotalsWhenPostingSellInvoiceCurrencyChargesOnLocalCurrencyInvoice()
		{
			Assert("This test is not suitable here as LoadJobsCore and ReloadChargesCore are overridden.", true);
		}

		public new void TestPeriodicInvoiceOnHold()
		{
			Assert("This test is not suitable here. It is only intended to test PeriodicInvoice.", true);
		}

		public override void TestDontRetrievePeriodicInvoicesAsMiscInvoices()
		{
			Assert("Periodic Invoice Light loads invoices and charges in a completely separate way, so this test isn't required here", true);
		}

		public new void TestSetTerms()
		{
			Assert("This test is not suitable here. It is only intended to test PeriodicInvoice.", true);
		}
	}
}