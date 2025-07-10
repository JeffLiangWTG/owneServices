using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeltaModeListWhenJE_ApplicationCodeIsDeltaIE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", ZString.Empty, ZString.Empty, ZString.Empty, "5DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "B", ZString.Empty, ZString.Empty, ZString.Empty, "3E93054B");
			declaration.SetupSupplier().SetupAccount(OrgCusAccountCodeList.Codes.DGE, "C", ZString.Empty, ZString.Empty, ZString.Empty, "6DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGE, "D", ZString.Empty, ZString.Empty, ZString.Empty, "4E93054B");
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, ZString.Empty, ZString.Empty, ZString.Empty, "4DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, ZString.Empty, ZString.Empty, ZString.Empty, "2E93054B");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Mode list should always be empty for a Delta IE type of declaration.", 0, declaration.Lookups.DeltaModeList.Count);
		}

		public void TestDefermentAccountNumberList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertEquals("The deferment account number list should be empty when neither Importer nor Declarant exists.", 0, declaration.Lookups.DefermentAccountNumberList.Count);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertContainsExactElementsInExactOrder("The deferment account number list should only contain the Importer deferment account number when only the Importer exists.", new[] { "IGUA" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("The description for the Importer deferment account number should match.", "Importer, The importer", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("IGUA"));

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertContainsExactElementsInExactOrder("The deferment account number list should only contain the Declarant deferment account number when only the Declarant exists.", new[] { "DGUA" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("The description for the Declarant deferment account number should match.", "Declarant, The declarant", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("DGUA"));

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertContainsExactElementsInExactOrder("The deferment account number list should contain both the Importer and Declarant deferment account numbers when both exist.", new[] { "IGUA", "DGUA" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("The description for the Importer deferment account number should match.", "Importer, The importer", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("IGUA"));
			AssertEquals("The description for the Declarant deferment account number should match.", "Declarant, The declarant", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("DGUA"));
		}

		public void TestProfileList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Importer_Full_Name";
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC001", "CUSOF001", ZString.Empty, "604D8AFA");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DEC002", "CUSOF002", ZString.Empty, "604D8AFA");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.HDN, "DEC003", "CUSOF003", ZString.Empty, "604D8AFA");
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = "Declarant_Full_Name";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC004", "CUSOF004", ZString.Empty, "C447912A");
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.OH_FullName = "Representative_Full_Name";
			representative.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DEC005", "CUSOF005", ZString.Empty, "TEST1001");
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			declaration.SetupImporter(importer);
			declaration.SetupDeclarant(declarant.MainAddress);
			declaration.SetupRepresentative(representative.MainAddress);
			var list = declaration.Lookups.ProfileList;

			CombineAssertions(() =>
			{
				AssertEquals("List values", "DEC001, DEC002, DEC003, DEC004, DEC005", list.CodesAsString);
				AssertEquals("Importer Importer_Full_Name, Delta DCC, CUSOF001", list["DEC001"].Description);
				AssertEquals("Importer Importer_Full_Name, Delta DCN, CUSOF002", list["DEC002"].Description);
				AssertEquals("Importer Importer_Full_Name, Delta HDN, CUSOF003", list["DEC003"].Description);
				AssertEquals("Declarant Declarant_Full_Name, Delta DCC, CUSOF004", list["DEC004"].Description);
				AssertEquals("Representative Representative_Full_Name, Delta DCC, CUSOF005", list["DEC005"].Description);
				AssertSame("Cached", list, declaration.Lookups.ProfileList);
			});
		}

		public void TestCustomsGuaranteeNumberList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertEquals("The customs guarantee account number list should be empty when neither Importer nor Declarant exists.", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertContainsExactElementsInExactOrder("The customs guarantee account number list should only contain the Importer customs guarantee account number when only the Importer exists.", new[] { "IGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("The description for the Importer customs guarantee account number should match.", "Importer, The importer", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("IGUA"));

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertContainsExactElementsInExactOrder("The customs guarantee account number list should only contain the Declarant customs guarantee account number when only the Declarant exists.", new[] { "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("The description for the Declarant customs guarantee account number should match.", "Declarant, The declarant", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("DGUA"));

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertContainsExactElementsInExactOrder("The customs guarantee account number list should contain both the Importer and Declarant customs guarantee account numbers when both exist.", new[] { "IGUA", "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("The description for the Importer customs guarantee account number should match.", "Importer, The importer", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("IGUA"));
			AssertEquals("The description for the Declarant customs guarantee account number should match.", "Declarant, The declarant", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("DGUA"));
		}

		public void TestDeltaIEDataGroupingForVATCANA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var lookups = declaration.Lookups;
			AssertEquals("Data grouping for DeltaIE VAT CANA code should be DIE", "DIE", lookups.DataGroupingForVATCANA);
		}
	}
}
