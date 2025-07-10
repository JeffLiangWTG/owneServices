using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.CDS.Messaging.Wrappers;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GbCDSImportEntryHeaderWrapperTests : TestCaseWithFactory
	{
		public void TestGetArrivalTransportMeansIdentificationTypeCode_RORO()
		{
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			entry.Declaration.ZG_Box18TransportID = "TxId03";
			var provider = new GbCDSImportEntryHeaderWrapper(entry);
			CombineAssertions(() =>
			{
				AssertEquals("RORO transport should have type code 30", "30", provider.GetArrivalTransportMeansIdentificationTypeCode(GBTransportTypeList.Codes.ROR));
			});
		}

		public void TestImporter()
		{
			const string countryCodeGb = Core.Constants.CountryCodes.UnitedKingdom;
			const string countryCodeFR = Core.Constants.CountryCodes.France;
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeGb);
			entry.Declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var provider = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is present so Name should be empty", ZString.Empty, provider.Importer.Name);
				AssertNull("ID is present so Address should be null", provider.Importer.Address);
				AssertEquals("ID is present so ID should be populated", "GB123", provider.Importer.ID);
			});

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			provider = new GbCDSImportEntryHeaderWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is NOT present so Name should not be empty", "Company Name 1", provider.Importer.Name);
				AssertNotNull("ID is NOT present so Address should not be null", provider.Importer.Address);
				AssertEquals("ID NOT is present so ID should not be populated", ZString.Empty, provider.Importer.ID);
			});

			orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeFR);
			entry.Declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.SendForeignEoriToCds, countryCodeGb, ZDate.Today, false))
			{
				provider = new GbCDSImportEntryHeaderWrapper(entry);
				CombineAssertions("Foreign organisation, SendForeignEoriToCds = false", () =>
				{
					AssertEquals("Name should not be empty", "Company Name 1", provider.Importer.Name);
					AssertNotNull("Address should not be null", provider.Importer.Address);
					AssertEquals("ID should not be populated", ZString.Empty, provider.Importer.ID);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.SendForeignEoriToCds, countryCodeGb, ZDate.Today, true))
			{
				provider = new GbCDSImportEntryHeaderWrapper(entry);
				CombineAssertions("Foreign organisation, SendForeignEoriToCds = true", () =>
				{
					AssertEquals("Name should be empty", ZString.Empty, provider.Importer.Name);
					AssertNull("Address should be null", provider.Importer.Address);
					AssertEquals("ID should be populated", "FR123", provider.Importer.ID);
				});
			}
		}

		public void TestImporterUnmatchedOrganisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Declaration.ImporterDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchOrg(Factory).PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION NO ADDRESS SPECIFIED PLEASE SEE ATTACHED NOTE NA NSW AUSTRALIA", entry.Declaration.ImporterDocumentaryAddress.AddressAsASingleLine);
			var provider = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Importer is Unmatched Organisation so should return null", null, provider.Importer);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.ImporterDocumentaryAddress.E2_CompanyName = "Importer";
			provider = new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Importer not null", "Importer", provider.Importer.Name);
		}

		public void TestSellerUnmatchedOrganisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Declaration.JE_OA_SellerAddress = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION NO ADDRESS SPECIFIED PLEASE SEE ATTACHED NOTE NA NSW AUSTRALIA", entry.Declaration.SellerAddress.AddressAsASingleLine);
			var provider = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Seller is Unmatched Organisation so should return null", null, provider.Seller);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.CreateValidOrgAddress();
			orgAddress.CompanyName = "Seller";
			orgAddress.OA_OH = orgHeader.PK;
			entry.Declaration.JE_OA_SellerAddress = orgAddress.PK;

			provider = new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Seller not null", "Seller", provider.Seller.Name);
		}

		public void TestBuyerUnmatchedOrganisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Declaration.JE_OH_Buyer = OrgHeader.UnmatchOrg(Factory).PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION", entry.Declaration.Buyer.OH_FullName);
			var provider = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Buyer is Unmatched Organisation so should return null", null, provider.Buyer);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.CreateValidOrgAddress();
			orgAddress.CompanyName = "Buyer";
			orgAddress.OA_OH = orgHeader.PK;
			entry.Declaration.BuyerDocAddress.E2_OA_Address = orgAddress.PK;
			entry.Declaration.JE_OH_Buyer = orgHeader.PK;

			provider = new GbCDSImportEntryHeaderWrapper(entry);
			AssertEquals("Buyer not null", "Buyer", provider.Buyer.Name);
		}

		public void TestBuyerFromShipment()
		{
			var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "EORI123", Core.Constants.CountryCodes.UnitedKingdom);
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_JS = shipment.PK;
			shipment.BuyerDocAddress.OrganisationPK = orgHeader.PK;

			var wrapper = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entryHeader);

			AssertNotNull("Buyer", wrapper.Buyer);
			AssertEquals("Buyer.ID", "GBEORI123", wrapper.Buyer.ID);
		}
	}
}
