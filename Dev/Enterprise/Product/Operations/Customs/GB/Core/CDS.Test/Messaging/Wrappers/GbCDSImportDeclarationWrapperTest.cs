using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.CDS.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using CusEntryInstruction = Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	public class GbCDSImportDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestSendForeignEoriToCdsForExporter()
		{
			const string functionalityUnderTest = Universal.Constants.FunctionalityTypes.SendForeignEoriToCds;
			const string countryCodeGb = Core.Constants.CountryCodes.UnitedKingdom;
			const string countryCodeFR = Core.Constants.CountryCodes.France;
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(functionalityUnderTest, countryCodeGb, ZDate.Today, false))
			{
				var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeGb);
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
				IDeclaration wrapper = new GbCDSImportDeclarationWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("GB123", orgHeader.GetEuIdentificationNumber());
					AssertEquals("GB123", wrapper.Exporter.ID);
					AssertEquals(ZString.Empty, wrapper.Exporter.Name);
					AssertNull(wrapper.Exporter.Address);
				});

				orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "XI123", countryCodeGb);
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
				wrapper = new GbCDSImportDeclarationWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("XI123", orgHeader.GetEuIdentificationNumber());
					AssertEquals("XI123", wrapper.Exporter.ID);
					AssertEquals(ZString.Empty, wrapper.Exporter.Name);
					AssertNull(wrapper.Exporter.Address);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(functionalityUnderTest, countryCodeGb, ZDate.Today, false))
			{
				var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeFR);
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
				IDeclaration wrapper = new GbCDSImportDeclarationWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("FR123", orgHeader.GetEuIdentificationNumber());
					AssertEquals(ZString.Empty, wrapper.Exporter.ID);
					AssertEquals("Company Name 1", wrapper.Exporter.Name);
					AssertNotNull(wrapper.Exporter.Address);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(functionalityUnderTest, countryCodeGb, ZDate.Today, true))
			{
				var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeFR);
				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
				IDeclaration wrapper = new GbCDSImportDeclarationWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("FR123", orgHeader.GetEuIdentificationNumber());
					AssertEquals("FR123", wrapper.Exporter.ID);
					AssertEquals(ZString.Empty, wrapper.Exporter.Name);
					AssertNull(wrapper.Exporter.Address);
				});
			}
		}

		public void TestExporter()
		{
			const string countryCodeGb = Core.Constants.CountryCodes.UnitedKingdom;
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);

			entry.Declaration.SupplierDocumentaryAddress.OrganisationPK = OrgHeader.UnmatchOrg(Factory).PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION NO ADDRESS SPECIFIED PLEASE SEE ATTACHED NOTE NA NSW AUSTRALIA", entry.Declaration.SupplierDocumentaryAddress.AddressAsASingleLine);
			var provider = (IImportDeclaration)new GbCDSExportDeclarationWrapper(entry);
			AssertEquals("Exporter is Unmatched Organisation so should return null", null, provider.Exporter);
			AssertEquals("ExporterNameAndAddress is Unmatched Organisation so should return null", null, provider.ExporterNameAndAddress);

			var orgHeader = OrganisationWrapperTest.CreateOrgHeaderWithEori(Factory, "123", countryCodeGb);
			entry.Declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			provider = new GbCDSExportDeclarationWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is present so Name should be empty", ZString.Empty, provider.Exporter.Name);
				AssertNull("ID is present so Address should be null", provider.Exporter.Address);
				AssertEquals("ID is present so ID should be populated", "GB123", provider.Exporter.ID);
			});

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			provider = new GbCDSExportDeclarationWrapper(entry);

			CombineAssertions(() =>
			{
				AssertEquals("ID is NOT present so Name should not be empty", "Company Name 1", provider.Exporter.Name);
				AssertNotNull("ID is NOT present so Address should not be null", provider.Exporter.Address);
				AssertEquals("ID is NOT present so ID should not be populated", ZString.Empty, provider.Exporter.ID);
			});

			AssertEquals("ExporterNameAndAddress", "Address1 1", provider.ExporterNameAndAddress.Address.Line);
		}

		public void TestDeclarantUnmatchedOrganisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Declaration.Declarant.OA_OH = OrgHeader.UnmatchOrg(Factory).PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION 10 HUTCHESON STREET ALBION QLD 4010 AUSTRALIA", entry.Declaration.DeclarantAddress.AddressAsASingleLine);
			var provider = (IImportDeclaration)new GbCDSExportDeclarationWrapper(entry);
			AssertEquals("Declarant is Unmatched Organisation so should return null", null, provider.Declarant);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.CreateValidOrgAddress();
			orgAddress.CompanyName = "Declarant";
			orgAddress.OA_OH = orgHeader.PK;
			entry.Declaration.Declarant.OA_OH = orgHeader.PK;
			AssertEquals("Declarant is not Unmatched Organisation so should have a value", "10 HUTCHESON STREET ALBION  QLD", provider.Declarant.Address.Line);
		}

		public void TestAgentUnmatchedOrganisation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Declaration.JE_OA_Representative = OrgHeader.UnmatchOrg(Factory).MainAddress.PK;
			AssertEquals("Pre-req", "UNMATCHED ORGANISATION NO ADDRESS SPECIFIED PLEASE SEE ATTACHED NOTE NA NSW AUSTRALIA", entry.RepresentativeOrganisation.MainAddress.AddressAsASingleLine);
			var provider = (IImportDeclaration)new GbCDSExportDeclarationWrapper(entry);
			AssertEquals("agent is Unmatched Organisation so should return null", null, provider.Agent.Agent);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.CreateValidOrgAddress();
			orgAddress.CompanyName = "Agent";

			orgAddress.OA_OH = orgHeader.PK;
			entry.Declaration.JE_OA_Representative = orgAddress.PK;
			AssertEquals("Agent is not Unmatched Organisation so should have a value", "Agent", provider.Agent.Agent.Name);
		}

		public void TestImporterPrivateIndividual()
		{
			var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			var individualOrg = OrganisationWrapperTest.CreatePrivateIndividualOrg(Factory);
			entry.Declaration.JE_OH_Importer = individualOrg.PK;
			var provider = (IGoodsShipment)new GbCDSImportEntryHeaderWrapper(entry);

			CombineAssertions(() =>
			{
				Assert(provider.Importer.IsPrivateIndividual);
				Assert(provider.Importer.ID.IsEmpty);
				AssertEquals("GB00500 John Smith", provider.Importer.Name);
			});
		}

		public void TestAuthorisationHolders()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure4071 = helper.CreateRefCusProcedure("CDS", "A", "40", "71", "000", "whatever", "IMP", group: "H1", outOfWarehouse: true);
			var procedure7100 = helper.CreateRefCusProcedure("CDS", "A", "71", "00", "000", "whatever", "IMP", group: "H2", intoWarehouse: true);
			Factory.Save();

			var warehouseINTO = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseINTO.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseOUTOF = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOUTOF.CompanyData.OB_IMUsedBondedWhs = true;
			warehouseINTO.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseOUTOF.MainAddress.OA_RN_NKCountryCode = "GB";
			warehouseINTO.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U1234567INN", "GB");
			warehouseOUTOF.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "U7654321OUT", "GB");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = "CDS";
			dec.JE_Calc_LocationOtherInformationCountry = "AA";
			dec.JE_Calc_LocationOtherInformationType = "BB";
			dec.JE_LocationQualifier = "CC";
			dec.JE_GoodsLocation = "DDDDDDDDD";

			var cei = (CusEntryInstruction)dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";
			cei.CEI_OA_Warehouse = warehouseOUTOF.MainAddress.PK;
			cei.CEI_OA_Warehouse2 = warehouseINTO.MainAddress.PK;

			var invoice = dec.Invoices.AddNew();
			var inLine4071 = invoice.InvoiceLines.AddNew();
			var inLine7100 = invoice.InvoiceLines.AddNew();
			inLine4071.JI_CEI = cei.PK;
			inLine7100.JI_CEI = cei.PK;
			inLine4071.JI_Procedure = procedure4071.FullCodeCurrentPlusPreviousPlusConcession;
			inLine7100.JI_Procedure = procedure7100.FullCodeCurrentPlusPreviousPlusConcession;

			//var cusProcedureAB11OutOfWhs = Factory.New<RefCusProcedure>();
			//cusProcedureAB11OutOfWhs.ZZ6_ProcedureCode = "AB";
			//cusProcedureAB11OutOfWhs.ZZ6_PreviousProcedureCode = "11";
			//cusProcedureAB11OutOfWhs.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			//cusProcedureAB11OutOfWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			//cusProcedureAB11OutOfWhs.ZZ6_Description = "AB DESC";
			//var cusProcedureAB22IntoWhs = Factory.New<RefCusProcedure>();
			//cusProcedureAB22IntoWhs.ZZ6_ProcedureCode = "AB";
			//cusProcedureAB22IntoWhs.ZZ6_PreviousProcedureCode = "22";
			//cusProcedureAB22IntoWhs.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			//cusProcedureAB22IntoWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			//cusProcedureAB22IntoWhs.ZZ6_Description = "AB DESC";
			////var cusProcedureAB33OutOfInwardWhs = Factory.New<RefCusProcedure>();
			////cusProcedureAB33OutOfInwardWhs.ZZ6_ProcedureCode = "AB";
			////cusProcedureAB33OutOfInwardWhs.ZZ6_PreviousProcedureCode = "33";
			////cusProcedureAB33OutOfInwardWhs.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB33OutOfInwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			////cusProcedureAB33OutOfInwardWhs.ZZ6_Description = "AB DESC";
			////var cusProcedureAB44IntoInwardWhs = Factory.New<RefCusProcedure>();
			////cusProcedureAB44IntoInwardWhs.ZZ6_ProcedureCode = "AB";
			////cusProcedureAB44IntoInwardWhs.ZZ6_PreviousProcedureCode = "44";
			////cusProcedureAB44IntoInwardWhs.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB44IntoInwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			////cusProcedureAB44IntoInwardWhs.ZZ6_Description = "AB DESC";
			////var cusProcedureAB55OutOfOutwardWhs = Factory.New<RefCusProcedure>();
			////cusProcedureAB55OutOfOutwardWhs.ZZ6_ProcedureCode = "AB";
			////cusProcedureAB55OutOfOutwardWhs.ZZ6_PreviousProcedureCode = "55";
			////cusProcedureAB55OutOfOutwardWhs.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB55OutOfOutwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			////cusProcedureAB55OutOfOutwardWhs.ZZ6_Description = "AB DESC";
			////var cusProcedureAB66IntoOutwardWhs = Factory.New<RefCusProcedure>();
			////cusProcedureAB66IntoOutwardWhs.ZZ6_ProcedureCode = "AB";
			////cusProcedureAB66IntoOutwardWhs.ZZ6_PreviousProcedureCode = "66";
			////cusProcedureAB66IntoOutwardWhs.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB66IntoOutwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			////cusProcedureAB66IntoOutwardWhs.ZZ6_Description = "AB DESC";
			////var cusProcedureAB88IntoAndOutOfWhs = Factory.New<RefCusProcedure>();
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_ProcedureCode = "AB";
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_PreviousProcedureCode = "88";
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			////cusProcedureAB88IntoAndOutOfWhs.ZZ6_Description = "AB DESC";
			//var helper = new WhsDataTestHelper(Factory);
			//helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			//helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			//helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			//helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = false;
			////var declaration = Factory.New<JobDeclaration>();
			//var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			//declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			//declaration.JE_OH_Importer = helper.Importer.PK;
			//var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew() as Customs.Business.CusEntryInstruction;
			////var instruction = declaration.CustomsEntryInstructions.AddNew();
			//instruction.CEI_Style = "AB";
			//instruction.CEI_OH_Owner = helper.Owner.PK;
			//instruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			//instruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
			//var invoiceHeader = (Customs.Business.InvoiceHeaderActiveCollection)declaration.Invoices.AddNew();
			//var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			//invoiceLine1.JI_CEI = instruction.PK;
			//var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			//invoiceLine2.JI_CEI = instruction.PK;

			//instruction.CEI_Style = "AB";
			//invoiceLine1.JI_Procedure = "AB11";
			//invoiceLine2.JI_Procedure = "AB22";

			AssertEquals("pre-req IsOutOfWarehouseWarehousing", true, cei.HasOutOfWarehouseProcedure);
			AssertEquals("pre-req IsIntoWarehouseWarehousing", true, cei.HasIntoWarehouseProcedure);
			AssertEquals("pre-req IsIntoWarehouseWarehousing", true, cei.HasAnyChangeOfOwnershipProcedure);

			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();

			//var entry = GbCDSExportEntryHeaderWrapperTests.CreateSampleEntryHeader(Factory);
			//var instruction = entry.EntryInstruction;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_MasterUCR = "MUCR";
			entry.CH_CEI_Instruction = cei.PK;

			var auth1 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "DPO";
			auth1.AGC_Number = "ABCDEFG";
			auth1.AGC_OH_Owner = owner1.PK;

			var auth2 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "XXX";
			auth2.AGC_Number = "XXXXXXX";
			auth2.AGC_OH_Owner = owner2.PK;

			var auth3 = entry.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth3.AGC_Code = "ZZZ";
			auth3.AGC_Number = "ZZZZZZZ";
			auth3.AGC_OH_Owner = owner3.PK;

			entry.Declaration.JE_OH_Importer = owner2.PK;
			cei.CEI_OH_Owner = owner2.PK;

			var provider = (IImportDeclaration)new GbCDSImportDeclarationWrapper(entry);

			AssertEquals("Owner2 authorisation should not be in AuthorisationHolders", 2, provider.AuthorisationHolders.Count());
			var authWrapper = provider.AuthorisationHolders.FirstOrDefault(x => x.CategoryCode == "DPO");
			AssertNotNull("AuthorisationHolder DPO should exist", authWrapper);
			AssertEquals("ABCDEFG", authWrapper.ID);
			authWrapper = provider.AuthorisationHolders.FirstOrDefault(x => x.CategoryCode == "ZZZ");
			AssertNotNull("AuthorisationHolder ZZZ should exist", authWrapper);
			AssertEquals("ZZZZZZZ", authWrapper.ID);
		}
	}
}
