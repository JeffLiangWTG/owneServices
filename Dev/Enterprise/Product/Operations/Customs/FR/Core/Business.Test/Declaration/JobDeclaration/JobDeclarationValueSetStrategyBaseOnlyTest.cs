using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class JobDeclarationValueSetStrategyBaseOnlyTest : TestCaseWithFactory
	{
		public void TestSetBox14RepresentationWhenImporterChanged()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();

			var addInfo = EUOrgImpAddInfo.Get(importer, declaration.CountryCode);
			addInfo.Deserialise();
			addInfo.ZO_Box14UseIndirectRepresentation = true;
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(EU.Business.RepresentationTypeList.Codes._3Indirect, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = "";
			addInfo.ZO_Box14UseIndirectRepresentation = false;
			Factory.Save();
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(EU.Business.RepresentationTypeList.Codes._2Direct, declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = "";
			addInfo.ZO_Box14UseIndirectRepresentation = true;
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("", declaration.JE_DeclarantType);
		}

		public void TestDefaultZG_AgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.ZG_AgreedPlaceCode = "PLACE";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Agreed place code should be reset when switching from DeltaG to Delta IE.", ZString.Empty, declaration.ZG_AgreedPlaceCode);
		}

		public void TestDefaultTHIChanged()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "035", "BORDEAUX BASSENS 1", "FRDKK", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "110", "BORDEAUX BASSENS 2", "FRDKK", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "123", "BORDEAUX BASSENS 3", "FRBOL", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "456", "BORDEAUX BASSENS 4", "FRBOL", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 6", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "034", "BORDEAUX BASSENS 7", "FRDKK", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "140", "BORDEAUX BASSENS 8", "FRDKK", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";
			declaration.JE_RL_NKPortOfLoading = "FRDKK";

			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "230", "395" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());

			declaration.JE_MessageType = "EXP";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "034", "140" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());

			declaration.JE_RL_NKPortOfLoading = "FRBOL";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "123", "456" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());

			declaration.JE_MessageType = "IMP";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "230", "395" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());

			declaration.JE_RL_NKPortOfArrival = "FRDKK";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "034", "140" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());

			declaration.JE_CustomsOffice = "FR000100";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "035", "110" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());
		}

		[TestDate(2024, 12, 18, 0, 0, 0)]
		public void TestDefaultJE_CustomsGuarantee_DeltaG()
		{
			OrgHeader importer, supplier, declarant, branchOrg, orgProxy;
			GenerateGuarantee(out importer, out supplier, out declarant, out branchOrg, out orgProxy);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var dummyOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Company.GC_OH_OrgProxy = orgProxy.PK;
			declaration.JE_OA_DeclarantAddress = dummyOrg.MainAddress.PK;
			AssertEquals("OGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.Branch.GB_Code = "PAR";
			declaration.Branch.GB_OH_OrgProxy = branchOrg.PK;
			declaration.JE_OA_DeclarantAddress = dummyOrg.PK;
			AssertEquals("BGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertEquals("DGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			AssertEquals("IUAB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_MessageType = "EXP";
			AssertEquals("SGUB", declaration.JE_CustomsGuaranteeNumber);
		}

		[TestDate(2024, 12, 18, 0, 0, 0)]
		public void TestJE_ApplicationCodeChange()
		{
			OrgHeader importer, supplier, declarant, branchOrg, orgProxy;
			GenerateGuarantee(out importer, out supplier, out declarant, out branchOrg, out orgProxy);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var dummyOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Company.GC_OH_OrgProxy = orgProxy.PK;
			declaration.JE_OA_DeclarantAddress = dummyOrg.MainAddress.PK;
			AssertEquals("OGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.Branch.GB_Code = "PAR";
			declaration.Branch.GB_OH_OrgProxy = branchOrg.PK;
			declaration.JE_OA_DeclarantAddress = dummyOrg.PK;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("BGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("DGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("IUAB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals("SGUB", declaration.JE_CustomsGuaranteeNumber);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals(ZString.Empty, declaration.JE_CustomsGuaranteeNumber);
		}

		void GenerateGuarantee(out OrgHeader importer, out OrgHeader supplier, out OrgHeader declarant, out OrgHeader branchOrg, out OrgHeader orgProxy)
		{
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.OH_RL_NKClosestPort = "FRPAR";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "IMPDG1", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IUAB", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);

			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			supplier.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "SUPDG1", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPP", Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUB", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);

			declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DECDGI", ZString.Empty, ZString.Empty, "B26F06FF");
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DECDGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);

			branchOrg = Factory.NewWithValidTestData<OrgHeader>();
			branchOrg.OH_Code = "BRANCORG";
			branchOrg.OH_FullName = "Branch Organisation";
			branchOrg.MainAddress.Address1 = "BranchOrgAddress";
			branchOrg.MainAddress.OA_Code = "BranchOrgAddress";
			branchOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "BRADGI", ZString.Empty, ZString.Empty, "B26F06FF");
			branchOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "BRADGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUA", branchOrg.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "BRAG", Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUB", branchOrg.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);

			orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "ORGPROXY";
			orgProxy.OH_FullName = "Organisation Proxy";
			orgProxy.MainAddress.Address1 = "OrgProxy";
			orgProxy.MainAddress.OA_Code = "OrgProxy";
			orgProxy.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "ORGDGI", ZString.Empty, ZString.Empty, "B26F06FF");
			orgProxy.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "ORGDGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "OGUA", orgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "OrgProxy", OrgCusAccountDeltaGTypeList.Codes.G1, "ORGP", Core.Constants.CountryCodes.France);
			TestDateAttribute.AddMinutes(1);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "OGUB", orgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "OrgProxy", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			Factory.Save();
		}

		public void TestPopulateChargesWhenImporterChanged()
		{
			var (declaration, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, declaration.TopGroupInvoice.Charges.Count);

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(0, declaration.TopGroupInvoice.Charges.Count);

			declaration.JE_OH_Importer = link.OL_OH_Buyer;
			AssertEquals(1, declaration.TopGroupInvoice.Charges.Count);
		}

		public void TestPopulateChargesWhenSupplierChanged()
		{
			var (declaration, link) = PrepareDataForPopulateCharges();
			AssertEquals(1, declaration.TopGroupInvoice.Charges.Count);

			declaration.Invoices.ForEach(x => x.JZ_OH_Supplier = ZGuid.Empty);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(0, declaration.TopGroupInvoice.Charges.Count);

			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			AssertEquals(1, declaration.TopGroupInvoice.Charges.Count);
		}

		(JobDeclaration, OrgSupplierBuyerLink) PrepareDataForPopulateCharges()
		{
			var link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			link.OL_InsuranceUplift = 10m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			declaration.JE_OH_Importer = link.OL_OH_Buyer;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Australia;
			var groupInvoiceHeader = declaration.TopGroupInvoice;

			var invoice1 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew() as JobComInvoiceHeader;
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			return (declaration, link);
		}

		public void TestDefaultJE_LocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.Declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL000000000000000000000000001");

			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.Importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL000000000000000000000000002");

			declaration.JE_CustomsProfile = "DIE001";
			AssertEquals("Location of goods should be defaulted with AUL type of authorisation linked to the Customs profile owner.", "AUL000000000000000000000000001", declaration.JE_LocationOfGoods);

			declaration.JE_CustomsProfile = "DIE002";
			AssertEquals("Location of goods should be defaulted again when Customs profile changes.", "AUL000000000000000000000000002", declaration.JE_LocationOfGoods);

			declaration.Declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL000000000000000000000000003");
			declaration.JE_CustomsProfile = "DIE001";
			AssertEquals("Location of goods should be empty when selected customs profile has more than one linked authorised location.", "", declaration.JE_LocationOfGoods);
		}

		public void TestDefaultJE_SubLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importer = declaration.SetupImporter();
			importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL002").SetupAuthorisationRule(CusAuthorisationRuleTypeList.Codes.SUB).WithValue("LOC002");
			declaration.JE_LocationOfGoods = "AUL002";
			AssertEquals("LOC002", declaration.JE_SubLocationOfGoods);
			importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL003").SetupAuthorisationRule(CusAuthorisationRuleTypeList.Codes.SUB).WithValue(new string('a', 35));
			declaration.JE_LocationOfGoods = "AUL003";
			AssertEquals("It should trim the value when defaulting.", new string('a', 35), declaration.JE_SubLocationOfGoods);
		}

		public void TestDefaultJE_CustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importer = declaration.SetupImporter();
			importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL001").SetupAuthorisationRule(CusAuthorisationRuleTypeList.Codes.OFC).WithValue("FR001");
			importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL002").SetupAuthorisationRule(CusAuthorisationRuleTypeList.Codes.OFC).WithValue("FR002");
			declaration.JE_LocationOfGoods = "AUL002";
			AssertEquals("FR002", declaration.JE_CustomsOffice);
			importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL003").SetupAuthorisationRule(CusAuthorisationRuleTypeList.Codes.OFC).WithValue(new string('a', 35));
			declaration.JE_LocationOfGoods = "AUL003";
			AssertEquals("It should trim the value when defaulting.", new string('a', 10), declaration.JE_CustomsOffice);
		}

		public void TestDefaultOfficeOfDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importer = declaration.SetupImporter();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI001", "FR001", ZString.Empty, "3869DBE3");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI002", "FR002", ZString.Empty, "ED4AF3D7");
			declaration.JE_CustomsProfile = "DGI002";
			AssertEquals("FR002", declaration.OfficeOfDeclaration);
		}

		public void TestOnOrganisationChangedCalledWhenJE_CustomsProfileIsChanged()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			((DummyVATDeferStrategy)declaration.VATDeferStrategy).OnOrganisationCalled = false;
			Assert(!((DummyVATDeferStrategy)declaration.VATDeferStrategy).OnOrganisationCalled);
			declaration.JE_CustomsProfile = "123";
			Assert(((DummyVATDeferStrategy)declaration.VATDeferStrategy).OnOrganisationCalled);
		}

		public void TestOnSubstyleWhenJE_ExportDateIsChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Export).WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G1);
			var testInstruction1 = declaration.CustomsEntryInstructions[0];
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			AssertEquals(testInstruction1.EntryHeader, null);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.A, testInstruction1.CEI_SubStyle);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G2);
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.C, testInstruction1.CEI_SubStyle);

			var testInvHeader = declaration.Invoices.AddNew();
			var testLine1 = testInvHeader.InvoiceLines.AddNew();
			testLine1.FillWithValidTestData();
			testLine1.JI_CEI = testInstruction1.PK;
			var testEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			testEntryHeader1.FillWithValidTestData();
			testEntryHeader1.EntryNumber = ZString.Empty;
			var testEntryLine1 = testEntryHeader1.AllEntryLines.AddNew();
			testEntryLine1.FillWithValidTestData();
			testLine1.JI_CL = testEntryLine1.PK;

			AssertEquals(testInstruction1.EntryHeader, testEntryHeader1);
			AssertEquals(declaration.CustomsEntryInstructions[0].EntryHeader, testEntryHeader1);

			Assert(testEntryHeader1.EntryNumber.IsEmpty);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G1);
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.A, testInstruction1.CEI_SubStyle);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G2);
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.C, testInstruction1.CEI_SubStyle);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G1);
			testInstruction1.CEI_SubStyle = EntrySubstyleCodePairList.Codes.A;
			AssertEquals(EntrySubstyleCodePairList.Codes.A, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.A, testInstruction1.CEI_SubStyle);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G2);
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.C;
			AssertEquals(EntrySubstyleCodePairList.Codes.C, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.C, testInstruction1.CEI_SubStyle);

			testInstruction1.CEI_SubStyle = EntrySubstyleCodePairList.Codes.Z;
			AssertEquals(EntrySubstyleCodePairList.Codes.Z, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.Z, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.Z, testInstruction1.CEI_SubStyle);

			testInstruction1.CEI_SubStyle = EntrySubstyleCodePairList.Codes.Y;
			AssertEquals(EntrySubstyleCodePairList.Codes.Y, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.Y, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.Y, testInstruction1.CEI_SubStyle);

			testInstruction1.EntryHeader.EntryNumber = "12345";

			AssertEquals("12345", declaration.CustomsEntryInstructions[0].EntryHeader.EntryNumber);
			AssertNotNull(testInstruction1.EntryHeader);
			AssertEquals("12345", testInstruction1.EntryHeader.EntryNumber);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G1);
			testInstruction1.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, testInstruction1.CEI_SubStyle);

			declaration.WithDeltaType(OrgCusAccountDeltaGTypeList.Codes.G2);
			declaration.CustomsEntryInstructions[0].CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, testInstruction1.CEI_SubStyle);
		}

		public void TestRegionIsUpdatedWhenMessageTypeChange()
		{
			SetUpRefUNLOCO();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "FRPAR";
			declaration.JE_RL_NKOrigin = "FR2AC";

			AssertEquals("Prerequisite : JE_RegionOrTerritoryOfDestination value is CONTI.", "CONTI", declaration.JE_RegionOrTerritoryOfDestination);

			declaration.JE_MessageType = "EXP";
			AssertEquals("MessageType's value has changed to EXP so the region should now be Corse.", "CORSE", declaration.JE_RegionOrTerritoryOfDestination);

			declaration.JE_MessageType = "IMP";
			AssertEquals("MessageType's value has changed back to IMP so the region should now be CONTI.", "CONTI", declaration.JE_RegionOrTerritoryOfDestination);
		}

		void SetUpRefUNLOCO()
		{
			var state1 = CreateNewOrGetExistingRefCountryStates("2A", "Corse", "FR");
			CreateNewOrGetExistingRefUNLOCO("FR2AC").RL_RW = state1.PK;
			CreateNewOrGetExistingRefUNLOCO("FRPAR");
		}

		RefCountryStates CreateNewOrGetExistingRefCountryStates(ZString code, ZString description, ZString countryCode)
		{
			var result = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode(code, countryCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefCountryStates>();
				result.RW_Code = code;
				result.RW_Description = description;
				result.RW_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code)
		{
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.New<RefUNLOCO>();
				result.RL_Code = code;
			}
			return result;
		}

		class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore() => vatDeferStrategy ?? (vatDeferStrategy = new DummyVATDeferStrategy(this));
			EU.Business.Declaration.VATDeferStrategy vatDeferStrategy;
		}

		class DummyVATDeferStrategy : VATDeferStrategy
		{
			public DummyVATDeferStrategy(JobDeclaration declaration) : base(declaration)
			{
			}

			public override void OnOrganisationChanged()
			{
				OnOrganisationCalled = true;
			}

			public bool OnOrganisationCalled;
		}
	}
}
