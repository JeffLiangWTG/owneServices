using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class JobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
	{
		public void TestChargePaymentOrDestinationIDs()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "108", "BORDEAUX BASSENS 3", "FRNTE", "FR005340");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "230", "BORDEAUX BASSENS 4", "FRLEH", "FR004560");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 5", "FRLEH", "FR004560");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";

			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "230", "395" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());
		}

		public void TestCustomsGuaranteeNumberListSkipsGuaranteesNotMatchingDeltaMode()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee1 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);
			guarantee1.CPH_StartDate = ZDate.Today.AddDays(-1);
			var guarantee2 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G2, "DECL", Core.Constants.CountryCodes.France);
			guarantee2.CPH_StartDate = ZDate.Today.AddDays(-1);
			var guarantee3 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUC", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", "", "DECL", Core.Constants.CountryCodes.France);
			guarantee3.CPH_StartDate = ZDate.Today.AddDays(-1);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("The list should be made of guarantee that either match the delta mode or have no additional reference.", new string[] { "DGUA", "DGUC" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
		}

		public void TestCustomsGuaranteeNumberListFiltersOnGuaranteeNumber()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "0123", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "", Core.Constants.CountryCodes.France);
			guarantee.CPH_StartDate = ZDate.Today;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("The list should be empty because Declarant guarantee number is not suitable for declaration (should be 4 uppercase letters).", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);

			guarantee.CPH_Number = "akho";
			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertEquals("The list should be empty because Declarant guarantee number is not suitable for declaration (should be 4 uppercase letters).", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);

			guarantee.CPH_Number = "AKHOPMLGDHFT";
			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertEquals("The list should be empty because Declarant guarantee number is not suitable for declaration (should be 4 uppercase letters).", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);

			guarantee.CPH_Number = "AKHO";
			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantee because its number is made of 4 uppercase letters .", new string[] { "AKHO" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
		}

		public void TestCustomsGuaranteeNumberListSkipsOutdatedGuarantees()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);
			guarantee.CPH_StartDate = ZDate.Today.AddDays(1);
			guarantee.CPH_StartDate = ZDate.Today.AddDays(1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;

			AssertEquals("The list should be empty because Declarant guarantee is not valid yet (will be valid from tomorrow on).", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);

			guarantee.CPH_StartDate = ZDate.Today;
			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertContainsExactElementsInAnyOrder("The list should be made of Declarant guarantee because it is now valid for today.", new string[] { "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
		}

		public void TestCustomsGuaranteesSkipsInActive()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "AKHO", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			guarantee.CPH_StartDate = ZDate.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;

			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertContainsExactElementsInAnyOrder(new string[] { "AKHO" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());

			guarantee.CPH_IsActive = false;
			Factory.Save();
			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,,,DeclarantAddress,PST: 10 HUTCHESON STREET,PST: 10 HUTCHESON STREET");
			AssertEquals("The list should be empty because Declarant guarantee is not active.", 0, declaration.Lookups.CustomsGuaranteeNumberList.Count);
		}

		public void TestCustomsGuarantees()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "IMPDG1", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			supplier.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "SUPDG1", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPP", Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DECDGI", ZString.Empty, ZString.Empty, "B26F06FF");
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DECDGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);

			var branchOrg = Factory.NewWithValidTestData<OrgHeader>();
			branchOrg.OH_Code = "BRANCORG";
			branchOrg.OH_FullName = "Branch Organisation";
			branchOrg.MainAddress.Address1 = "BranchOrgAddress";
			branchOrg.MainAddress.OA_Code = "BranchOrgAddress";
			branchOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "BRADGI", ZString.Empty, ZString.Empty, "B26F06FF");
			branchOrg.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "BRADGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BGUA", branchOrg.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "BRAG", Core.Constants.CountryCodes.France);

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "ORGPROXY";
			orgProxy.OH_FullName = "Organisation Proxy";
			orgProxy.MainAddress.Address1 = "OrgProxy";
			orgProxy.MainAddress.OA_Code = "OrgProxy";
			orgProxy.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "ORGDGI", ZString.Empty, ZString.Empty, "B26F06FF");
			orgProxy.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "ORGDGE", ZString.Empty, ZString.Empty, "B26F06FF");
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "OGUA", orgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "OrgProxy", OrgCusAccountDeltaGTypeList.Codes.G1, "ORGP", Core.Constants.CountryCodes.France);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			declaration.Branch.GB_OH_OrgProxy = branchOrg.PK;
			declaration.Company.GC_OH_OrgProxy = orgProxy.PK;

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_MessageType = "IMP";
			AssertEquals("Count", 4, declaration.Lookups.CODCustomsGuarantees.Length);

			var guaranteeList = new List<ZString>();
			declaration.Lookups.CODCustomsGuarantees.ForEach(guarantee => guaranteeList.Add(guarantee.PermitHolder.OH_Code));
			Assert(guaranteeList.Contains("BRANCORG"));
			Assert(guaranteeList.Contains("ORGPROXY"));
			Assert(guaranteeList.Contains("DECLARANT"));
			Assert(guaranteeList.Contains("IMPORTER"));

			declaration.JE_MessageType = "EXP";
			guaranteeList.Clear();
			declaration.Lookups.CODCustomsGuarantees.ForEach(guarantee => guaranteeList.Add(guarantee.PermitHolder.OH_Code));

			Assert(guaranteeList.Contains("BRANCORG"));
			Assert(guaranteeList.Contains("ORGPROXY"));
			Assert(guaranteeList.Contains("DECLARANT"));
			Assert(guaranteeList.Contains("SUPPLIER"));
		}

		public void TestCustomsGuaranteesIncludingExpired()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			var guarantee1 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France, true);
			guarantee1.CPH_EndDate = ZDate.Today.AddDays(-2);
			Factory.Save();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			var guarantee2 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France, true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var guaranteeList = new List<ZString>();
			declaration.Lookups.CODCustomsGuaranteesIncludingExpired.ForEach(guarantee => guaranteeList.Add(guarantee.PermitHolder.OH_Code));

			AssertEquals("The guarantee list should contain two guarantees including expired ones.", 2, guaranteeList.Count);
			Assert("The guarantee list should contain an expired importer guarantee.", guaranteeList.Contains("IMPORTER"));
			Assert("The guarantee list should contain a non-expired declarant guarantee.", guaranteeList.Contains("DECLARANT"));
		}

		public void TestDefermentCustomsGuarantees()
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

			var guaranteeList = new List<ZString>();
			declaration.Lookups.DefermentCustomsGuarantees.ForEach(guarantee => guaranteeList.Add(guarantee.PermitHolder.OH_Code));

			AssertEquals("The guarantee list should contain two guarantees.", 2, guaranteeList.Count);
			Assert("The guarantee list should contain an importer guarantee.", guaranteeList.Contains("IMPORTER"));
			Assert("The guarantee list should contain a declarant guarantee.", guaranteeList.Contains("DECLARANT"));
		}

		public void TestCustomsGuaranteeNumberList()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUA", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "SGUB", supplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "SupplierAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "SUPB", Core.Constants.CountryCodes.France);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchOrgProxy.OH_Code = "BRANCHORGPRO";
			branchOrgProxy.OH_FullName = "Branch org proxy";
			branchOrgProxy.MainAddress.Address1 = "BranchOrgProxyAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "BOPA", branchOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "BranchOrgProxyAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "BRAN", Core.Constants.CountryCodes.France);

			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyOrgProxy.OH_Code = "COMPANYORGPR";
			companyOrgProxy.OH_FullName = "Company org proxy";
			companyOrgProxy.MainAddress.Address1 = "CompanyOrgProxyAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "COPA", companyOrgProxy.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "CompanyOrgProxyAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "COMP", Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.Branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			declaration.Company.GC_OH_OrgProxy = companyOrgProxy.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = ZString.Empty;
			Factory.Save();

			Factory.ClearCachedValue<CusGuaranteeHeader[]>("FR.JobDeclarationLookups.CustomsGuarantees,G1,,ImporterAddress,SupplierAddress,DeclarantAddress,BranchOrgProxyAddress,CompanyOrgProxyAddress");

			AssertContainsExactElementsInAnyOrder("The list should include Branch OrgProxy, Company OrgProxy guarantees and Declarant.", new[] { "BOPA", "COPA", "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("DGUA/DECL/DECLARANT, Declarant, The declarant", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("DGUA"));
			AssertEquals("BOPA/BRAN/BRANCHORGPRO, Branch, Branch org proxy", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("BOPA"));
			AssertEquals("COPA/COMP/COMPANYORGPR, Company, Company org proxy", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("COPA"));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("The list should include Importer, Branch OrgProxy, Company OrgProxy guarantees and Declarant.", new[] { "IGUA", "BOPA", "COPA", "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("IGUA should be the first item in the list.", "IGUA", declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes().FirstOrDefault());
			AssertEquals("IGUA/IMPO/IMPORTER, Importer, The importer", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("IGUA"));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("The list should include Supplier, Branch OrgProxy, Company OrgProxy guarantees and Declarant.", new[] { "SGUA", "SGUB", "BOPA", "COPA", "DGUA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
			AssertEquals("SGUA/SUPA/SUPPLIER, Supplier, The supplier", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("SGUA"));
			AssertEquals("SGUB/SUPB/SUPPLIER, Supplier, The supplier", declaration.Lookups.CustomsGuaranteeNumberList.GetDescriptionFromCode("SGUB"));

			declaration.JE_OA_DeclarantAddress = supplier.MainAddress.PK;
			AssertContainsExactElementsInAnyOrder("The list should not show duplicates when the supplier is the declarant.", new[] { "SGUA", "SGUB", "BOPA", "COPA" }, declaration.Lookups.CustomsGuaranteeNumberList.GetAllCodes());
		}

		public void TestDefermentAccountNumberList()
		{
			var france = Factory.Load<RefCountry>(Core.Constants.CountryGuids.France);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "Supplier";
			supplier.OH_FullName = "The supplier";
			supplier.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, france, "SUPDAN");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, france, "IMPDAN");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "The declarant";
			declarant.MainAddress.Address1 = "declarantAddress";
			declarant.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, france, "DECDAN");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.JE_MessageType = ZString.Empty;
			AssertContainsExactElementsInExactOrder("The list should be made of Declarant deferment account number.", new[] { "DECDAN" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("Declarant, The declarant", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("DECDAN"));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInExactOrder("The list should be made of Importer and Declarant deferment account number.", new[] { "IMPDAN", "DECDAN" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("Importer, The importer", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("IMPDAN"));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertContainsExactElementsInExactOrder("The list should be made of Supplier and Declarant deferment account number.", new[] { "SUPDAN", "DECDAN" }, declaration.Lookups.DefermentAccountNumberList.GetAllCodes());
			AssertEquals("Supplier, The supplier", declaration.Lookups.DefermentAccountNumberList.GetDescriptionFromCode("SUPDAN"));
		}

		public void TestAuthorizedLocations()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(importer, declaration.DefaultDeltaAccountOrgHeader);

			var authorizedLocation1 = importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			authorizedLocation1.CPH_Number = "AUTHLOC1";
			var authorizedLocation2 = importer.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			authorizedLocation2.CPH_Number = "AUTHLOC2";

			var authorizedLocationList = declaration.Lookups.AuthorizedLocations;
			AssertEquals("AuthorizedLocations should contain 2 elements.", 2, authorizedLocationList.Count());
			AssertEquals("AuthorizedLocations should contain 1 element with number set as 'AUTHLOC1'.", 1, authorizedLocationList.Count(x => x.CPH_Number == "AUTHLOC1"));
			AssertEquals("AuthorizedLocations should contain 1 element with number set as 'AUTHLOC2'.", 1, authorizedLocationList.Count(x => x.CPH_Number == "AUTHLOC2"));
		}

		public void TestLookupsShouldBeCached()
		{
			// it should return the same instance as we have it cached.
			Assert(ReferenceEquals(jobDeclaration.Lookups.AirRouteTypeList, jobDeclaration.Lookups.AirRouteTypeList));
			Assert(ReferenceEquals(lookups.DeclarantTypeList, lookups.DeclarantTypeList));
			Assert(ReferenceEquals(lookups.PaymentPartyList, lookups.PaymentPartyList));
			Assert(ReferenceEquals(lookups.SubGoodsLocations, lookups.SubGoodsLocations));
			Assert(ReferenceEquals(lookups.DefermentAccountNumberList, lookups.DefermentAccountNumberList));
		}

		public void TestGoodsLocations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "AUL Header1";
			orgHeader.MainAddress.OA_Address1 = "AUL Address1";
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "Test1", ZString.Empty, ZString.Empty, "4C6446F3");

			var aulAuthorisation1 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			aulAuthorisation1.CPH_Number = "AULNUM";

			var aulAuthorisation2 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer);
			aulAuthorisation2.CPH_Number = "AULNUM2";

			var aulAuthorisation3 = orgHeader.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			aulAuthorisation3.CPH_Number = "AULNUM3";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "AUL Header2";
			orgHeader2.MainAddress.OA_Address1 = "AUL Address1";
			orgHeader2.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "Test2", ZString.Empty, ZString.Empty, "4C6446F3");

			var aulAuthorisation4 = orgHeader2.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			aulAuthorisation4.CPH_Number = "AULNUM4";

			SetupDeclaration();
			jobDeclaration.JE_OH_Importer = orgHeader.PK;
			jobDeclaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			jobDeclaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			jobDeclaration.JE_CustomsProfile = "Test1";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AULNUM", "AULNUM3" }, lookups.GoodsLocations.Select(x => x.CPH_Number));

			jobDeclaration.JE_OH_Importer = orgHeader2.PK;
			jobDeclaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			jobDeclaration.JE_CustomsProfile = "Test2";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AULNUM4" }, lookups.GoodsLocations.Select(x => x.CPH_Number));

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_FullName = "AUL Header3";
			orgHeader3.MainAddress.OA_Address1 = "AUL Address3";
			orgHeader3.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "Test3", ZString.Empty, ZString.Empty, "4C6446F3");

			var aulAuthorisation5 = orgHeader3.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			aulAuthorisation5.CPH_Number = "AULNUM5";

			jobDeclaration.JE_OA_DeclarantAddress = orgHeader3.MainAddress.PK;
			jobDeclaration.JE_CustomsProfile = "Test3";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AULNUM5" }, lookups.GoodsLocations.Select(x => x.CPH_Number));
		}

		public void TestGoodsLocationsFilter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var declarant = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var orgCusAccount1 = declarant.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount1.CZ_Account = "12345678";
			orgCusAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount1.CZ_OH = declarant.PK;
			orgCusAccount1.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount1.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var lookups = declaration.Lookups;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "AUL Header1";
			orgHeader.MainAddress.OA_Address1 = "AUL Address1";
			orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "Test1", ZString.Empty, ZString.Empty, "4C6446F3");

			AssertEquals(Core.Constants.CountryCodes.France, lookups.GoodsLocations.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.Country + ":Property"].Value);
			AssertEquals(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation, lookups.GoodsLocations.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"].Value);
			AssertEquals(declarant.PK, lookups.GoodsLocations.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"].Value);
			var ruledetailFilter = lookups.GoodsLocations.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.RuleDetails + ":Property1"];
			AssertEquals(new ZString(CusAuthorisationRuleTypeList.Codes.SUB), ruledetailFilter.Value);
			AssertEquals(true, ruledetailFilter.IsRemovable);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionIsThrownWhenOrganisationIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = ZGuid.Empty;  // force Declaration to be null
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull(declaration.Importer);
			AssertNull(declaration.Supplier);
			AssertNull(declaration.Declarant);
			_ = declaration.Lookups.AirRouteTypeList;
			_ = declaration.Lookups.DeclarantTypeList;
			_ = declaration.Lookups.ProfileList;
			_ = declaration.Lookups.PaymentPartyList;
			_ = declaration.Lookups.SubGoodsLocations;
			_ = declaration.Lookups.GoodsLocations;
		}

		public void TestRepresentationTypeList()
		{
			var typeList = lookups.DeclarantTypeList;
			AssertEquals(typeof(RepresentationTypeList), typeList.GetType());
			AssertEquals(3, typeList.Count);
			Assert(typeList.ContainsCode(RepresentationTypeList.Codes.SEL));
			Assert(typeList.ContainsCode(RepresentationTypeList.Codes.DIR));
			Assert(typeList.ContainsCode(RepresentationTypeList.Codes.IND));
		}
		public void TestDeclaration()
		{
			AssertEquals(lookups.Declaration, jobDeclaration);
		}

		public void TestAirRouteTypeList()
		{
			AssertEquals(jobDeclaration.Lookups.AirRouteTypeList, Factory.GetCachedValue<AirRouteTypeList>());
			ZArchitecture.Core.UntranslatableCodeDescriptionPairList valuationBypassCodeList = Factory.GetCachedValue<AirRouteTypeList>();
			AssertEquals(jobDeclaration.Lookups.AirRouteTypeList, valuationBypassCodeList);
		}

		public void TestAirRouteTypeList2()
		{
			CombineAssertions(() =>
			{
				var airRouteTypeList = lookups.AirRouteTypeList;
				AssertEquals("Values", "1, 2, 3, 4, 5, 6, 7, 8", airRouteTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<AirRouteTypeList>(), airRouteTypeList);
			});
		}

		public void TestUntranslatableCodeDescriptionPairList()
		{
			ZArchitecture.Core.UntranslatableCodeDescriptionPairList valueCodeList = Factory.GetCachedValue<AirRouteTypeList>();
			AssertEquals(jobDeclaration.Lookups.AirRouteTypeList, valueCodeList);

			valueCodeList = Factory.GetCachedValue<MethodOfPaymentList>();
			AssertEquals(lookups.PaymentPartyList, valueCodeList);

			valueCodeList = Factory.GetCachedValue<ExportExitTypeList>();
			AssertEquals(jobDeclaration.Lookups.ExportExitTypeList, valueCodeList);

			valueCodeList = Factory.GetCachedValue<ValuationBypassCodeList>();
			AssertEquals(jobDeclaration.Lookups.ValuationBypassCodeList, valueCodeList);
		}

		public void TestValuationBypassCodeList()
		{
			AssertEquals("A, B, C, D, E, F, G, H, I, J, K, L, M", jobDeclaration.Lookups.ValuationBypassCodeList.CodesAsString);
		}

		public void TestExportExitTypeList()
		{
			AssertEquals("ECS, EMC, OTH, STC, TRA", jobDeclaration.Lookups.ExportExitTypeList.CodesAsString);
		}

		public void TestExportExitTypeList2()
		{
			CombineAssertions(() =>
			{
				var exportExitTypeList = lookups.ExportExitTypeList;
				AssertEquals("Values", "ECS, EMC, OTH, STC, TRA", exportExitTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ExportExitTypeList>(), exportExitTypeList);
			});
		}

		public void TestAuthorisedAddressList()
		{
			SetupExporterImporter();

			AssertEquals("Should contain 0 items", 0, lookups.GoodsLocations.Count);
		}

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

		public void TestDeltaModeListWhenJE_ApplicationCodeIsChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, ZString.Empty, ZString.Empty, ZString.Empty, "4DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, ZString.Empty, ZString.Empty, ZString.Empty, "2E93054B");
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", ZString.Empty, ZString.Empty, ZString.Empty, "5DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "B", ZString.Empty, ZString.Empty, ZString.Empty, "3E93054B");
			declaration.SetupSupplier().SetupAccount(OrgCusAccountCodeList.Codes.DGE, "C", ZString.Empty, ZString.Empty, ZString.Empty, "6DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGE, "D", ZString.Empty, ZString.Empty, ZString.Empty, "4E93054B");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Mode list should contain account types of Importer and Declarant DGI configuration when JE_ApplicationCode is changed to DeltaG and MessageType is import.", "A, B", declaration.Lookups.DeltaModeList.CodesAsString);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("Mode list should be empty when JE_ApplicationCode is changed to DeltaIE.", string.Empty, declaration.Lookups.DeltaModeList.CodesAsString);
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Export);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("Mode list should contain account types of Supplier and Declarant DGE configuration when JE_ApplicationCode is changed to DeltaG and MessageType is export.", "C, D", declaration.Lookups.DeltaModeList.CodesAsString);
		}

		public void TestDeltaModeList_Distinct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, ZString.Empty, ZString.Empty, "4DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, ZString.Empty, ZString.Empty, "2E93054B");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			AssertEquals("The delta mode list should be distinct.", "G2", declaration.Lookups.DeltaModeList.CodesAsString);
		}

		public void TestDeltaModeList_CacheAndOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, ZString.Empty, ZString.Empty, "E27C8987");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, ZString.Empty, ZString.Empty, "12565287");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			CombineAssertions(() =>
			{
				var list = declaration.Lookups.DeltaModeList;
				AssertEquals("The delta mode list should be ordered.", "G1, G2", declaration.Lookups.DeltaModeList.CodesAsString);
				AssertSame("Cached", list, declaration.Lookups.DeltaModeList);
			});
		}

		public void TestRegionOrTerritoryOfDestinationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.Lookups.RegionOrTerritoryOfDestinationList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CONTI", "CORSE", "GUADE", "GUYAN", "MARTI", "MAYOT", "REUNI" }, list.GetAllCodes());
		}

		public void TestRegionOrTerritoryOfDestinationList2()
		{
			CombineAssertions(() =>
			{
				var regionOrTerritoryOfDestinationList = lookups.RegionOrTerritoryOfDestinationList;
				AssertEquals("Values", "CONTI, CORSE, GUADE, GUYAN, MARTI, MAYOT, REUNI", regionOrTerritoryOfDestinationList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<FRDomesticOverseasTerritories>(), regionOrTerritoryOfDestinationList);
			});
		}

		void SetupExporterImporter()
		{
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Singapore Test Importer Pte. Ltd.";
			testImporter.MainAddress.OA_Address1 = "Changi Airport";
			testImporter.MainAddress.OA_Address2 = "Building 3C";

			var testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Paris Test Exporter DHL Ltd.";
			testExporter.MainAddress.OA_Address1 = "CDG Airport";
			testExporter.MainAddress.OA_Address2 = "Location 1.2";

			jobDeclaration.JE_OH_Importer = testImporter.PK;
			jobDeclaration.JE_OH_Exporter = testExporter.PK;
		}

		void SetupDeclaration()
		{
			SetupExporterImporter();

			var testDeclarant = Factory.New<OrgHeader>();
			testDeclarant.OH_FullName = "Monbazillac Cooperative des Vins ";
			testDeclarant.MainAddress.OA_Address1 = "Lieu dit: Le Terrible";
			testDeclarant.MainAddress.OA_Address2 = "Chemin long";

			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.OH_FullName = "London Test Supplier DHL Ltd.";
			testSupplier.MainAddress.OA_Address1 = "Heathrow Airport";
			testSupplier.MainAddress.OA_Address2 = "Building 1B";

			jobDeclaration.Declarant.OA_OH = testDeclarant.PK;
			jobDeclaration.JE_OH_Supplier = testSupplier.PK;

			AssertEquals("Should contain 0 items", 0, lookups.GoodsLocations.Count);
		}

		public void TestApplicationCodeList()
		{
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(true, EU.Business.MessageTypeList.Codes.Import, true, false);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(true, EU.Business.MessageTypeList.Codes.Import, true, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(false, EU.Business.MessageTypeList.Codes.Import, false, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(false, EU.Business.MessageTypeList.Codes.Import, false, false);

			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(true, EU.Business.MessageTypeList.Codes.Export, false, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(true, EU.Business.MessageTypeList.Codes.Export, true, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(false, EU.Business.MessageTypeList.Codes.Export, false, false);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(false, EU.Business.MessageTypeList.Codes.Export, true, false);
		}

		void AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistry(bool expectedDICode, ZString messageType, bool enableDeltaIEForImports, bool enableDeltaIEForExports)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForImports))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, enableDeltaIEForExports))
			{
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", true, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaG));
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", expectedDICode, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaIE));
			}
		}

		public void TestApplicationCodeListComparingLocalCountryCustomsInterface()
		{
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: false, messageType: "", iEEnable: true, expectedInterfacedValue: false, expectedDeltaGValue: true, expectedDeltaIEValue: true);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: false, messageType: "", iEEnable: false, expectedInterfacedValue: false, expectedDeltaGValue: true, expectedDeltaIEValue: false);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: Customs.Business.DeclarationApplicationCodeList.Codes.Builtin, iEEnable: true, expectedInterfacedValue: false, expectedDeltaGValue: true, expectedDeltaIEValue: true);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: Customs.Business.DeclarationApplicationCodeList.Codes.Builtin, iEEnable: false, expectedInterfacedValue: false, expectedDeltaGValue: true, expectedDeltaIEValue: false);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced, iEEnable: true, expectedInterfacedValue: true, expectedDeltaGValue: false, expectedDeltaIEValue: false);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced, iEEnable: false, expectedInterfacedValue: true, expectedDeltaGValue: false, expectedDeltaIEValue: false);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted, iEEnable: true, expectedInterfacedValue: true, expectedDeltaGValue: true, expectedDeltaIEValue: true);
			AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(overriden: true, messageType: DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted, iEEnable: false, expectedInterfacedValue: true, expectedDeltaGValue: true, expectedDeltaIEValue: false);
		}

		void AssertApplicationCodeListFunctionOfLocalCountryCustomsInterfaceRegistry(bool overriden, ZString messageType, bool iEEnable, bool expectedInterfacedValue, bool expectedDeltaGValue, bool expectedDeltaIEValue)
		{
			IDisposable setLocalCountryCustomsInterface = null;
			if (overriden)
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany, null, null);
				customsInterface.RecipientID = "123";
				customsInterface.SubmissionType = messageType;
				setLocalCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			}

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, iEEnable))
			{
				AssertEquals($"Registry is overriden: {overriden}, Submission message type: {messageType}, Value tested {DeclarationApplicationCodeList.Codes.DeltaG}, IE is enable : {iEEnable}", expectedDeltaGValue, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaG));
				AssertEquals($"Registry is overriden: {overriden}, Submission message type: {messageType}, Value tested {DeclarationApplicationCodeList.Codes.Interface}, IE is enable : {iEEnable}", expectedInterfacedValue, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.Interface));
				AssertEquals($"Registry is overriden: {overriden}, Submission message type: {messageType}, Value tested {DeclarationApplicationCodeList.Codes.DeltaIE}, IE is enable : {iEEnable}", expectedDeltaIEValue, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaIE));
			}

			setLocalCountryCustomsInterface?.Dispose();
		}

		public void TestApplicationCodeListCompanyLevel()
		{
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(true, EU.Business.MessageTypeList.Codes.Import, true, false);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(true, EU.Business.MessageTypeList.Codes.Import, true, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(false, EU.Business.MessageTypeList.Codes.Import, false, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(false, EU.Business.MessageTypeList.Codes.Import, false, false);

			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(true, EU.Business.MessageTypeList.Codes.Export, false, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(true, EU.Business.MessageTypeList.Codes.Export, true, true);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(false, EU.Business.MessageTypeList.Codes.Export, false, false);
			AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(false, EU.Business.MessageTypeList.Codes.Export, true, false);
		}

		void AssertApplicationCodeListFunctionOfTheEnableDeltaIERegistryCompanyLevel(bool expectedDICode, ZString messageType, bool enableDeltaIEForImports, bool enableDeltaIEForExports)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableDeltaIEForImports))
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableDeltaIEForExports))
			{
				AssertEquals(FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranchPK, Guid.Empty), enableDeltaIEForImports);
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", true, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaG));
				AssertEquals($"messageType: {messageType}, enableDeltaIEForImports: {enableDeltaIEForImports}, enableDeltaIEForExports: {enableDeltaIEForExports}", expectedDICode, declaration.Lookups.ApplicationCodeList.ContainsCode(DeclarationApplicationCodeList.Codes.DeltaIE));
			}
		}

		public void TestBuyersLookupType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<OrgHeaderCollection>("Buyers Type", declaration.Lookups.Buyers);
		}

		public void TestCustomsLanguageList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.Lookups.CustomsLanguageList;
			CombineAssertions("CustomsLanguageList properties", () =>
			{
				AssertEquals("Code list code content.", "FR", declaration.Lookups.CustomsLanguageList.CodesAsString);
				AssertSame("CustomsLanguageList should be Cached.", list, declaration.Lookups.CustomsLanguageList);
			});
		}

		public void TestAgreedPlaceCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<RefUNLOCOCollection>("AgreedPlaceCodeList Type for UCC6", declaration.AddInfoLookups.AgreedPlaceCodeList);
		}
	}
}
