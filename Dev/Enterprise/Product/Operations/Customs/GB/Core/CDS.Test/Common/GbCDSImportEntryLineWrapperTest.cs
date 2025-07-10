using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GbCDSImportEntryLineWrapperTest : TestCaseWithFactory
	{
		public void TestGbCDSImportEntryLineWrapperDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();

			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.FillWithValidTestData();

			line1.JI_Description = "LINE\r\n25";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Description", new ZString("LINE\r\n25"), entryLine.Description);

			var wrapper = new GbCDSImportEntryLineWrapper(entryLine);
			AssertEquals("Description", new ZString("LINE 25"), ((ICommodity)wrapper).Description);

			line1.JI_Description = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567" +
				"89012345678901234567890123456789012345678901234567890123";
			AssertEquals(513, line1.JI_Description.Length);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			wrapper = new GbCDSImportEntryLineWrapper(entryLine);
			AssertEquals(512, ((ICommodity)wrapper).Description.Length);
		}

		public void TestUnmatchedOrganisationForBuyerSellerAndConsignor()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.Invoices.DeleteAll();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.FillWithValidTestData();
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.FillWithValidTestData();
			invoice1.JZ_OA_SellerAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			invoice1.JZ_OA_BuyerAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			invoice1.JZ_OA_SupplierAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.FillWithValidTestData();
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.FillWithValidTestData();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = Factory.CreateValidOrgAddress();
			orgAddress2.CompanyName = "Company2";
			orgAddress2.OA_OH = orgHeader2.PK;
			invoice2.JZ_OA_SellerAddress = orgAddress2.PK;
			invoice2.JZ_OA_BuyerAddress = orgAddress2.PK;
			invoice2.JZ_OA_SupplierAddress = orgAddress2.PK;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.FillWithValidTestData();
			invoice3.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.FillWithValidTestData();
			invoice3.JZ_OA_SellerAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			invoice3.JZ_OA_BuyerAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			invoice3.JZ_OA_SupplierAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;

			var invoice4 = declaration.Invoices.AddNew();
			invoice4.FillWithValidTestData();
			invoice4.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line4 = invoice4.JobComInvoiceLines.AddNew();
			line4.FillWithValidTestData();
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress4 = Factory.CreateValidOrgAddress();
			orgAddress4.CompanyName = "Company4";
			orgAddress4.OA_OH = orgHeader4.PK;
			invoice4.JZ_OA_SellerAddress = orgAddress4.PK;
			invoice4.JZ_OA_BuyerAddress = orgAddress4.PK;
			invoice4.JZ_OA_SupplierAddress = orgAddress4.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var wrapperLine1 = ((IGovernmentAgencyGoodsItem)new GbCDSImportEntryLineWrapper(entryLine1));
			var seller1 = wrapperLine1.Seller;
			var buyer1 = wrapperLine1.Buyer;
			var consignor1 = wrapperLine1.Consignor;

			var entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[1];
			var wrapperLine2 = ((IGovernmentAgencyGoodsItem)new GbCDSImportEntryLineWrapper(entryLine2));
			var seller2 = wrapperLine2.Seller;
			var buyer2 = wrapperLine2.Buyer;
			var consignor2 = wrapperLine2.Consignor;

			var entryLine3 = declaration.CustomsEntryHeaders[0].MergedLines[2];
			var wrapperLine3 = ((IGovernmentAgencyGoodsItem)new GbCDSImportEntryLineWrapper(entryLine3));
			var seller3 = wrapperLine3.Seller;
			var buyer3 = wrapperLine3.Buyer;
			var consignor3 = wrapperLine3.Consignor;

			AssertContainsExactElementsInAnyOrder(new string[] { "Company2", "Company4", null }, new string[] { seller1?.Name, seller2?.Name, seller3?.Name });
			AssertContainsExactElementsInAnyOrder(new string[] { "Company2", "Company4", null }, new string[] { buyer1?.Name, buyer2?.Name, buyer3?.Name });
			AssertContainsExactElementsInAnyOrder(new string[] { "Company2", "Company4", null }, new string[] { consignor1?.Name, consignor2?.Name, consignor3?.Name });
		}
	}
}
