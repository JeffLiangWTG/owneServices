using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocARInvoiceDataProviderTest : TestCaseWithFactory
	{
		public void TestGetRecipientTaxIDNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoicingBase = Factory.New<ARInvoice>();

				var docAddress = invoicingBase.DocAddresses.AddNew(DocAddressType.DebtorAddress);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = GlbCompany.CurrentCompany.CompanyName;
				docAddress.E2_GovRegNum = "Debtor Address GovRegNum";

				var docARInvoiceDataProvider = new DocARInvoiceDataProvider();

				AssertEquals("Debtor Address GovRegNum", docARInvoiceDataProvider.GetRecipientTaxIDNumber(invoicingBase, Factory));
			}
		}

		public void TestGetTaxId()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoicingBase = Factory.New<ARInvoice>();

				var docAddress = invoicingBase.DocAddresses.AddNew(DocAddressType.BranchOrCompanyProxyARAdress);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_CompanyName = GlbCompany.CurrentCompany.CompanyName;
				docAddress.E2_GovRegNum = "Branch Or Company Proxy AR Adress";

				var docARInvoiceDataProvider = new DocARInvoiceDataProvider();

				AssertEquals("Branch Or Company Proxy AR Adress", docARInvoiceDataProvider.GetTaxId(invoicingBase, Factory));
			}
		}
	}
}
