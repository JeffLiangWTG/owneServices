using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	public class CusGuaranteeHeaderExtensionTest : TestCaseWithFactory
	{
		public void TestGetCustomsGuaranteesForDeltaG()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUB", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPB", Core.Constants.CountryCodes.France, true);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECB", Core.Constants.CountryCodes.France, true);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Code = "BRANCORG";
			branchOrgProxy.OH_FullName = "Branch Organisation";
			branchOrgProxy.MainAddress.Address1 = "BranchOrgAddress";
			branchOrgProxy.MainAddress.OA_Code = "BranchOrgAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUA", branchOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "BRAG", Core.Constants.CountryCodes.France);

			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgProxy.OH_Code = "ORGPROXY";
			companyOrgProxy.OH_FullName = "Organisation Proxy";
			companyOrgProxy.MainAddress.Address1 = "OrgProxy";
			companyOrgProxy.MainAddress.OA_Code = "OrgProxy";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "OGUA", companyOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "OrgProxy", OrgCusAccountDeltaGTypeList.Codes.G1, "ORGP", Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantees irrespective of end date.", new string[] { "DGUA", "DGUB" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantees with valid end date.", new string[] { "DGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees irrespective of end date.", new string[] { "DGUA", "DGUB", "IGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees with valid end date.", new string[] { "DGUA", "IGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Supplier guarantees irrespective of end date.", new string[] { "DGUA", "DGUB", "SGUA", "SGUB" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Supplier guarantees with valid end date.", new string[] { "DGUA", "SGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OA_DeclarantAddress = supplier.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("Declarant now equals to Supplier, the list should not show duplicates irrespective of end date.", new string[] { "SGUA", "SGUB" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("Declarant now equals to Supplier, the list should not show duplicates with valid end date.", new string[] { "SGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier and Company OrgProxy guarantees irrespective of end date.", new string[] { "DGUA", "DGUB", "SGUA", "SGUB", "OGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier and Company OrgProxy guarantees with valid end date.", new string[] { "DGUA", "SGUA", "OGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.Branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier, Branch OrgProxy and Company OrgProxy guarantees irrespective of end date.", new string[] { "DGUA", "DGUB", "SGUA", "SGUB", "OGUA", "BGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier, Branch OrgProxy and Company OrgProxy guarantees with valid end date.", new string[] { "DGUA", "SGUA", "OGUA", "BGUA" }, Factory.GetCustomsGuarantees(declaration, false, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
		}

		public void TestGetCustomsGuaranteesForDeltaIE()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUB", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "sgua", Core.Constants.CountryCodes.France);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUB", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "ADD1", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUC", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G2, "ADD2", Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Code = "BRANCORG";
			branchOrgProxy.OH_FullName = "Branch Organisation";
			branchOrgProxy.MainAddress.Address1 = "BranchOrgAddress";
			branchOrgProxy.MainAddress.OA_Code = "BranchOrgAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUA", branchOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgProxy.OH_Code = "ORGPROXY";
			companyOrgProxy.OH_FullName = "Organisation Proxy";
			companyOrgProxy.MainAddress.Address1 = "OrgProxy";
			companyOrgProxy.MainAddress.OA_Code = "OrgProxy";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "OGUA", companyOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "OrgProxy", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			Factory.Save();

			Assert("Prerequisite: declaration is delta IE ", declaration.IsDeltaIE);

			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA", "IGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Supplier guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA", "SGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OA_DeclarantAddress = supplier.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("Declarant now equals to Supplier, the list should not show duplicates irrespective of end date where there is no additional reference.", new string[] { "SGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier and Company OrgProxy guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA", "SGUA", "OGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.Branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant, Supplier, Branch OrgProxy and Company OrgProxy guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA", "SGUA", "OGUA", "BGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
		}

		public void TestGetDefermentCustomsGuaranteesForDeltaIE()
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
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			Assert("Prerequisite: Declaration should be DeltaIE.", declaration.IsDeltaIE);
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees irrespective of end date where there is no additional reference.", new string[] { "DGUA", "IGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.DEF).Select(x => x.CPH_Number));
		}

		public void TestGetCustomsGuaranteesWithEntRuleCode()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France, entryTypeCode: GuaranteeEntryTypeList.Codes.IMP);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "EGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPA", Core.Constants.CountryCodes.France, entryTypeCode: GuaranteeEntryTypeList.Codes.EXP);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECA", Core.Constants.CountryCodes.France);

			var importerWithExpEnt = Factory.NewWithValidTestData<OrgHeader>();
			importerWithExpEnt.OH_Code = "IMPORTER2";
			importerWithExpEnt.OH_FullName = "The importer2";
			importerWithExpEnt.MainAddress.Address1 = "ImporterAddress2";
			importerWithExpEnt.MainAddress.OA_Code = "ImporterAddress2";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUB", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress2", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPB", Core.Constants.CountryCodes.France, entryTypeCode: GuaranteeEntryTypeList.Codes.EXP);

			var supplierWithImpEnt = Factory.NewWithValidTestData<OrgHeader>();
			supplierWithImpEnt.OH_Code = "SUPPLIER2";
			supplierWithImpEnt.OH_FullName = "The supplier2";
			supplierWithImpEnt.MainAddress.Address1 = "SupplierAddress2";
			supplierWithImpEnt.MainAddress.OA_Code = "SupplierAddress2";
			var guaranteeWithImpEnt = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "EGUB", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress2", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPB", Core.Constants.CountryCodes.France, entryTypeCode: GuaranteeEntryTypeList.Codes.IMP);

			var declarantWithoutEnt = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithoutEnt.OH_Code = "DECLARANT2";
			declarantWithoutEnt.OH_FullName = "The declarant2";
			declarantWithoutEnt.MainAddress.Address1 = "DeclarantAddress2";
			var guaranteeWithoutEnt = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress2", OrgCusAccountDeltaGTypeList.Codes.G1, "DECB", Core.Constants.CountryCodes.France);
			guaranteeWithoutEnt.CusGuaranteeRules.DeleteAll();
			var rule = guaranteeWithoutEnt.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "DeclarantAddress2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;

			var declarationWrongConfig = Factory.New<JobDeclaration>();
			declarationWrongConfig.JE_OH_Importer = importerWithExpEnt.PK;
			declarationWrongConfig.ImporterDocumentaryAddress.E2_OA_Address = importerWithExpEnt.MainAddress.PK;
			declarationWrongConfig.JE_OH_Supplier = supplierWithImpEnt.PK;
			declarationWrongConfig.SupplierDocumentaryAddress.E2_OA_Address = supplierWithImpEnt.MainAddress.PK;
			declarationWrongConfig.JE_OA_DeclarantAddress = declarantWithoutEnt.MainAddress.PK;
			declarationWrongConfig.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declarationWrongConfig.JE_MessageType = ZString.Empty;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantees irrespective of end date where ENT is BTH.", new string[] { "BGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees irrespective of end date where ENT is BTH or IMP.", new string[] { "BGUA", "IGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Supplier guarantees irrespective of end date where ENT is BTH or EXP.", new string[] { "BGUA", "EGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			AssertEquals("The list should be empty as the declarant has no guarantee with rule ENT and BTH and the message type is empty.", 0, Factory.GetCustomsGuarantees(declarationWrongConfig, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number).Count());
			declarationWrongConfig.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("The list should be empty as the importer has no guarantee with rule ENT/Import and the declarant has no guarantee with rule ENT.", 0, Factory.GetCustomsGuarantees(declarationWrongConfig, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number).Count());
			declarationWrongConfig.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("The list should be empty as there the exporter has no guarantee with rule ENT/Export and the declarant has no guarantee with rule ENT.", 0, Factory.GetCustomsGuarantees(declarationWrongConfig, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number).Count());
		}

		public void TestGetCustomsGuaranteesWithAddRuleCode()
		{
			var importerWithoutAdd = Factory.NewWithValidTestData<OrgHeader>();
			importerWithoutAdd.OH_Code = "IMPORTER";
			importerWithoutAdd.OH_FullName = "The importer";
			importerWithoutAdd.MainAddress.Address1 = "ImporterAddress";
			importerWithoutAdd.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importerWithoutAdd.PK, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var importerWithMatchingAdd = Factory.NewWithValidTestData<OrgHeader>();
			importerWithMatchingAdd.OH_Code = "IMPORTER2";
			importerWithMatchingAdd.OH_FullName = "The importer2";
			importerWithMatchingAdd.MainAddress.Address1 = "ImporterAddress2";
			importerWithMatchingAdd.MainAddress.OA_Code = "ImporterAddress2";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUB", importerWithMatchingAdd.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress2", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var importerWithNonMatchingAdd = Factory.NewWithValidTestData<OrgHeader>();
			importerWithNonMatchingAdd.OH_Code = "IMPORTER3";
			importerWithNonMatchingAdd.OH_FullName = "The importer3";
			importerWithNonMatchingAdd.MainAddress.Address1 = "ImporterAddress3";
			importerWithNonMatchingAdd.MainAddress.OA_Code = "ImporterAddress3";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUC", importerWithMatchingAdd.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "InvalidImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarantWithoutAdd = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithoutAdd.OH_Code = "DECLARANT";
			declarantWithoutAdd.OH_FullName = "The declarant";
			declarantWithoutAdd.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarantWithoutAdd.PK, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarantWithMatchingAdd = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithMatchingAdd.OH_Code = "DECLARANT2";
			declarantWithMatchingAdd.OH_FullName = "The declarant2";
			declarantWithMatchingAdd.MainAddress.Address1 = "DeclarantAddress2";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUB", declarantWithMatchingAdd.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress2", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarantWithNonMatchingAdd = Factory.NewWithValidTestData<OrgHeader>();
			declarantWithNonMatchingAdd.OH_Code = "DECLARANT3";
			declarantWithNonMatchingAdd.OH_FullName = "The declarant3";
			declarantWithNonMatchingAdd.MainAddress.Address1 = "DeclarantAddress3";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUC", declarantWithMatchingAdd.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "InvalidDeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_OH_Importer = importerWithoutAdd.PK;
			declaration.JE_OA_DeclarantAddress = declarantWithoutAdd.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees when ADD rule is not applied.", new string[] { "DGUA", "IGUA" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OH_Importer = importerWithMatchingAdd.PK;
			declaration.JE_OA_DeclarantAddress = declarantWithMatchingAdd.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant and Importer guarantees when ADD rule is applied with matching addresses.", new string[] { "DGUB", "IGUB" }, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number));

			declaration.JE_OH_Importer = importerWithNonMatchingAdd.PK;
			declaration.JE_OA_DeclarantAddress = declarantWithNonMatchingAdd.MainAddress.PK;
			AssertEquals("The list should be empty when ADD rule is applied with non-matching addresses.", 0, Factory.GetCustomsGuarantees(declaration, true, GuaranteeTypeList.Codes.COD).Select(x => x.CPH_Number).Count());
		}
	}
}
