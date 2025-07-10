using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.QRCodeData.Testing
{
	public class TransactionQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestIsUsedBy()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertType<TransactionQRCodeDataProvider>(ObjectFactory.Get<IAccountingDependencyFactory>().GetTransactionQRCodeDataProvider(invoice));
		}

		public void TestGetQRCodeDataDataProvider()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "XYZ";
			var company_GC_Code = branch.Company.GC_Code;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ORG001";

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionType = "INV";
			invoice.AH_InvoiceDate = new ZDateTime(2021, 06, 14);
			invoice.AH_TransactionReference = "0000500001234";
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ExchangeRate = 2m;
			invoice.AH_OSTotal = 110m;
			invoice.AH_OSTaxAmount = 10m;
			invoice.AH_GC = branch.Company.PK;
			invoice.AH_OH = organization.PK;
			invoice.AH_GB = branch.PK;

			invoice.Lines.AddNew();
			invoice.Lines[0].AL_LocalExTaxAmount = 100m;
			invoice.Lines[0].AL_LocalTaxAmount = 10m;

			var lines = invoice as ITransactionHeaderWithLines;

			var actualTransactionQRCodeData = new TransactionQRCodeDataProvider(invoice) as ITransactionQRCodeDataProvider;

			AssertEquals(nameof(actualTransactionQRCodeData.ComplianceSubType), "TXI", actualTransactionQRCodeData.ComplianceSubType);
			AssertEquals(nameof(actualTransactionQRCodeData.TransactionType), TransactionTypes.Invoice, actualTransactionQRCodeData.TransactionType);
			AssertEquals(nameof(actualTransactionQRCodeData.InvoiceDate), new ZDateTime(2021, 06, 14), actualTransactionQRCodeData.InvoiceDate);
			AssertEquals(nameof(actualTransactionQRCodeData.TransactionReference), "0000500001234", actualTransactionQRCodeData.TransactionReference);
			AssertEquals(nameof(actualTransactionQRCodeData.TransactionCurrency), CurrencyCodes.Australia, actualTransactionQRCodeData.TransactionCurrency);
			AssertEquals(nameof(actualTransactionQRCodeData.ExchangeRate), 2m, actualTransactionQRCodeData.ExchangeRate);
			AssertEquals(nameof(actualTransactionQRCodeData.OSInvoiceTotal), 110m, actualTransactionQRCodeData.OSInvoiceTotal);
			AssertEquals(nameof(actualTransactionQRCodeData.Company.GC_Code), company_GC_Code, actualTransactionQRCodeData.Company.GC_Code);
			AssertEquals(nameof(actualTransactionQRCodeData.Branch.GB_Code), "XYZ", actualTransactionQRCodeData.Branch.GB_Code);
			AssertEquals(nameof(actualTransactionQRCodeData.GSTTotal), 5m, actualTransactionQRCodeData.GSTTotal);
			AssertEquals(nameof(actualTransactionQRCodeData.LocalTaxTotal), 5m, actualTransactionQRCodeData.LocalTaxTotal);
			AssertEquals(nameof(actualTransactionQRCodeData.OrgHeader.OH_Code), "ORG001", actualTransactionQRCodeData.OrgHeader.OH_Code);
			AssertEquals(nameof(actualTransactionQRCodeData.EInvoicingGovernmentAllocatedNumber), ZString.Empty, actualTransactionQRCodeData.EInvoicingGovernmentAllocatedNumber);
			AssertEquals("Lines", lines, actualTransactionQRCodeData.GetTransactionDataForPortugalOnly_Obsolete_MustBeRefactoredToUseMembersOfThisInterface());
		}
	}
}
