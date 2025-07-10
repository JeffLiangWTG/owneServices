using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
{
	public void TestDeclarantTypeList_ExportUCC6()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
			var declarantTypeList = lookups.DeclarantTypeList;

			AssertEquals("When declaration is UCC6, SEL is not expected", false, declarantTypeList.ContainsCode("SEL"));
		}
	}

	public void TestDeclarantTypeListExportNonUCC6()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
		var declarantTypeList = lookups.DeclarantTypeList;

		AssertEquals("When declaration is non UCC6, SEL is expected", true, declarantTypeList.ContainsCode("SEL"));
	}

	public void TestDeclarantTypeListImport()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
		var declarantTypeList = lookups.DeclarantTypeList;

		AssertEquals("When declaration is Import, SEL is not expected", false, declarantTypeList.ContainsCode("SEL"));
	}

	public void TestMessageTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var lookups = new JobDeclarationLookups(declaration);
		AssertEquals("MessageTypeList contains only core declaration types while excluding codes for Declaration Lock functionality", "EXP, IMP, MSC", lookups.MessageTypeList.CodesAsString);
	}

	public void TestSubLocationOfGoodsList()
	{
		var loader = new RefCountry.Loader(Factory);
		var italy = loader.LoadForCountry("IT");
		var france = loader.LoadForCountry("FR");
		var australia = loader.LoadForCountry("AU");

		var declaration = Factory.New<JobDeclaration>();
		var countries = declaration.Lookups.SubLocationOfGoodsList;
		CombineAssertions(() =>
		{
			AssertEquals("Italy is in the list", true, countries.Contains(italy));
			AssertEquals("France is in the list", true, countries.Contains(france));
			AssertEquals("Australia is not in the list", false, countries.Contains(australia));
		});
	}

	public void TestFinalDestinations_Import()
	{
		var loader = new RefUNLOCO.Loader(Factory);
		var sanMarino = loader.Load("SMAQ8");
		var rotterdam = loader.Load("NLRTM");
		var milano = loader.Load("ITMIL");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			var finalDestinations = declaration.Lookups.FinalDestinations;
			AssertEquals("For Import FinalDestinations contains eu ports", true, finalDestinations.Contains(rotterdam));
			AssertEquals("For Import FinalDestinations contains San Marino", true, finalDestinations.Contains(sanMarino));
			AssertEquals("For Import FinalDestinations contains Milano", true, finalDestinations.Contains(milano));
		});
	}

	public void TestLocations()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1", "MYLOC1 RULE DESCRIPTION");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC2", "MYLOC2 RULE DESCRIPTION");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, "UND", "UNDEFINED");

		var declaration = Factory.New<JobDeclaration>();
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertType<GoodsLocationList>("When AuthorisationNumber is empty, GoodsLocationList is returned", declaration.Lookups.Locations);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertEquals("Invalid authorisation number selected", 0, declaration.Lookups.Locations.Count);

		declaration.ZG_AuthorisationNumber = "999999";
		AssertEquals("Valid authorisation number, but message type is empty", 0, declaration.Lookups.Locations.Count);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = orgHeader.PK;
		AssertLookup("Valid locations are loaded for EXP message type", (CodeDescriptionPairList)declaration.Lookups.Locations, 2, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" } });

		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC3", "MYLOC3 RULE DESCRIPTION");
		AssertLookup("Result has been cached and new authorisations are not loaded", (CodeDescriptionPairList)declaration.Lookups.Locations, 2, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" } });

		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisationHeader.PK}|{declaration.JE_CustomsOffice}");
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = orgHeader.PK;
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport;
		AssertEquals(3, declaration.Lookups.Locations.Count);
		AssertLookup("Valid locations are loaded for IMP message type", (CodeDescriptionPairList)declaration.Lookups.Locations, 3, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" }, { "MYLOC3", "MYLOC3 RULE DESCRIPTION" } });
	}

	public void TestLocationsFilteredByPresentationCustomsOffice()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var authorisationRule1 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456A");
		var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
		linkedRule1.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule1.CPR_ValueFrom = "IT137100";
		var authorisationRule2 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456B");
		var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
		linkedRule2.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule2.CPR_ValueFrom = "IT137100";
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456C");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = orgHeader.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.ZG_AuthorisationNumber = "999999";
		declaration.JE_CustomsOffice = ZString.Empty;
		AssertEquals("All locations are listed", 3, declaration.Lookups.Locations.Count);
		AssertEquals("123456A, 123456B, 123456C", ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString);

		declaration.JE_CustomsOffice = "IT137100";
		AssertEquals("Filtered locations are listed", 2, declaration.Lookups.Locations.Count);
		AssertEquals("123456A, 123456B", ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString);
	}

	public void TestCustomsOfficeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
		var italianCustomsOfficePK = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Off1", "TestDescription", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1)).PK;
		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
		{
			var declaration = Factory.New<JobDeclaration>();
			var customsOffices = declaration.Lookups.CustomsOffices;
			customsOffices.Load();
			AssertEquals(1, customsOffices.Count);
			Assert(customsOffices.Contains(italianCustomsOfficePK));
		}
	}

	public void TestNodes_WhenDeclarationIsNotUCC6()
	{
		var accountsSetupAction = new Action(() =>
		{
			new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
				.AppendAccount("11111111111-001", "1234")
				.AppendAccountDetail("INTCODE1", "AA")
				.AppendAccountDetail("INTCODE1REP1", "REP1")
				.AppendAccount("22222222222-001", "5678")
				.AppendAccountDetail("INTCODE2", "BB")
				.AppendAccountDetail("INTCODE2REP1", "REP1")
				.Build();
		});

		AssertCustomsProfileList(accountsSetupAction, isUcc6: false);
	}

	public void TestNodes_WhenDeclarationIsUCC6()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var accountsSetupAction = new Action(() =>
			{
				var currentCompany = GlbCompany.CurrentCompany;
				var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
				CreateAccountDetail(companyWrapper, "INTCODE1", "AA", "11111111111-001");
				CreateAccountDetail(companyWrapper, "INTCODE1REP1", "REP1", "11111111111-001");
				CreateAccountDetail(companyWrapper, "INTCODE2", "BB", "22222222222-001");
				CreateAccountDetail(companyWrapper, "INTCODE2REP1", "REP1", "22222222222-001");
				currentCompany.Factory.Save();
			});

			AssertCustomsProfileList(accountsSetupAction, isUcc6: true);

			GlbMauExternalPassword CreateAccountDetail(GlbCompanyWrapper companyWrapper, string internalCode, string declarantCode, string authorizedUser)
			{
				var accountDetail = companyWrapper.PasswordCollection.AddNew();
				accountDetail.GP_UserID = internalCode;
				accountDetail.GP_Name = declarantCode;
				accountDetail.GP_MailBoxID = authorizedUser;
				return accountDetail;
			}
		}
	}

	void AssertCustomsProfileList(Action accountsSetupAction, bool isUcc6)
	{
		var declarantAddressAA = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddressAA.Header.OH_Code = "AA";
		var declarantAddressBB = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddressBB.Header.OH_Code = "BB";
		var representative = Factory.New<OrgHeader>();
		representative.OH_Code = "REP1";
		Factory.Save();

		accountsSetupAction();

		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertContainsInternalCode("When Declarant and Representative are not selected", 4, "INTCODE1", "INTCODE1REP1", "INTCODE2", "INTCODE2REP1");

			declaration.JE_OA_DeclarantAddress = declarantAddressAA.PK;
			AssertContainsInternalCode("When 'AA' declarant selected", 1, "INTCODE1");

			declaration.JE_OA_DeclarantAddress = declarantAddressBB.PK;
			AssertContainsInternalCode("When 'BB' declarant selected", 1, "INTCODE2");

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			AssertContainsInternalCode("When REP representative is selected", 2, "INTCODE1REP1", "INTCODE2REP1");

			declaration.JE_OA_DeclarantAddress = declarantAddressBB.PK;
			AssertContainsInternalCode("When BB declarant and REP representative are selected", 3, "INTCODE1REP1", "INTCODE2", "INTCODE2REP1");

			void AssertContainsInternalCode(string assertionMessage, int expectedNodesCount, params string[] expectedInternalCodes)
			{
				var nodes = declaration.Lookups.ProfileList;
				AssertEquals("Available nodes count", expectedNodesCount, nodes.Count);
				CombineAssertions(assertionMessage, () =>
				{
					foreach (var internalCode in expectedInternalCodes)
					{
						Assert($"{internalCode} Internal Code is available", nodes.ContainsCode(internalCode));
					}
				});
			}
		}
	}

	public void TestCusAgents()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "STF";
		staff.GS_FullName = "STAFF FULL NAME";
		var staff2Wrapper = GlbStaffWrapper.Get(staff);
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-DEC1", "DEC1")
			.Build();

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var declarationLookups = declaration.Lookups;
		declaration.JE_CustomsProfile = "";
		AssertEquals(0, declarationLookups.CusAgents.Count);

		declaration.JE_CustomsProfile = "9999";
		AssertEquals(0, declarationLookups.CusAgents.Count);

		declaration.JE_CustomsProfile = "1234-DEC1";
		AssertArrayEqualsByElements(new GlbStaff[] { staff }, declarationLookups.CusAgents.ToArray());

		declaration.JE_CustomsProfile = "5678-DEC1";
		AssertArrayEqualsByElements(new GlbStaff[] { staff }, declarationLookups.CusAgents.ToArray());
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsForConsignee()
	{
		var declaration = CreateDeclarationAndImporter("IMP1", string.Empty, string.Empty);
		var availableDefermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("No available deferment approval numbers", 0, availableDefermentApprovalNumbers.Count);

		declaration = CreateDeclarationAndImporter("IMP2", "1000", string.Empty);
		var availableDefermentApprovalNumbers2 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers2.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers2.ContainsCode("1000"));

		declaration = CreateDeclarationAndImporter("IMP3", string.Empty, "1001");
		var availableDefermentApprovalNumbers3 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers3.Count);
		Assert("1001 is available deferment approval number", availableDefermentApprovalNumbers3.ContainsCode("1001"));

		declaration = CreateDeclarationAndImporter("IMP4", "1000", "1001");
		var availableDefermentApprovalNumbers4 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("2 available deferment approval numbers", 2, availableDefermentApprovalNumbers4.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers4.ContainsCode("1000"));
		Assert("1001 is available deferment approval number", availableDefermentApprovalNumbers4.ContainsCode("1001"));
	}

	public void TestDefermentApprovalNumberListWhenDefermentMethodIsForDeclarant()
	{
		var declaration = CreateDeclarationAndDeclarant("DEC1", string.Empty, string.Empty);
		var availableDefermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("No available deferment approval numbers", 0, availableDefermentApprovalNumbers.Count);

		declaration = CreateDeclarationAndDeclarant("DEC2", "1000", string.Empty);
		var availableDefermentApprovalNumbers2 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers2.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers2.ContainsCode("1000"));

		declaration = CreateDeclarationAndDeclarant("DEC3", string.Empty, "1001");
		var availableDefermentApprovalNumbers3 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("Only one available deferment approval number", 1, availableDefermentApprovalNumbers3.Count);
		Assert("1001 is available deferment approval number", availableDefermentApprovalNumbers3.ContainsCode("1001"));

		declaration = CreateDeclarationAndDeclarant("DEC4", "1000", "1001");
		var availableDefermentApprovalNumbers4 = declaration.Lookups.DefermentApprovalNumberList;
		AssertEquals("2 available deferment approval numbers", 2, availableDefermentApprovalNumbers4.Count);
		Assert("1000 is available deferment approval number", availableDefermentApprovalNumbers4.ContainsCode("1000"));
		Assert("1001 is available deferment approval number", availableDefermentApprovalNumbers4.ContainsCode("1001"));
	}

	public void TestPaymentPartyList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var lookup = declaration.Lookups;
		var expectedCommonDeferTypes = ConvertToDictionary(new DefermentMethodList());

		var expectedImportDeferTypes = ConvertToDictionary(new ImportDefermentMethodList());
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertLookup("IMP Declaration", lookup.PaymentPartyList, 8, expectedImportDeferTypes);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertLookup("EXP Declaration", lookup.PaymentPartyList, 4, expectedCommonDeferTypes);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var expectedUcc6ExportDeferTypes = ConvertToDictionary(new Ucc6ExportDefermentMethodList());
			AssertLookup("UCC6 EXP Declaration", lookup.PaymentPartyList, 9, expectedUcc6ExportDeferTypes);
		}

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertLookup("MISC Declaration", lookup.PaymentPartyList, 4, expectedCommonDeferTypes);

		Dictionary<ZString, ZString> ConvertToDictionary(CodeDescriptionPairList pairList)
			=> pairList.ToArray()
				.ToDictionary(c => new ZString(c.Code), v => new ZString(v.Description));
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartyOne()
	{
		var declaration = Factory.New<JobDeclaration>();
		SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(targetPaymentMethod: Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions,
			paymentMethodForNegativeScenario: Ucc6ExportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions,
			orgHeaderPropertyInfo: declaration.JE_OA_DeclarantAddressInfo,
			declaration: declaration);
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartyTwo()
	{
		var declaration = Factory.New<JobDeclaration>();
		SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(targetPaymentMethod: Ucc6ExportDefermentMethodList.Codes.ConsigneesAccountFromCustomsDecisions,
			paymentMethodForNegativeScenario: Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions,
			orgHeaderPropertyInfo: declaration.JE_OH_ImporterInfo,
			declaration: declaration,
			useHeaderAddressPk: false);
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartyThree()
	{
		var declaration = Factory.New<JobDeclaration>();
		SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(targetPaymentMethod: Ucc6ExportDefermentMethodList.Codes.ForwardersAccountFromCustomsDecisions,
			paymentMethodForNegativeScenario: Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions,
			orgHeaderPropertyInfo: declaration.JE_OH_ForwarderInfo,
			declaration: declaration,
			useHeaderAddressPk: false);
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartyFour()
	{
		var declaration = Factory.New<JobDeclaration>();
		SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(targetPaymentMethod: Ucc6ExportDefermentMethodList.Codes.SuppliersAccountFromCustomsDecisions,
			paymentMethodForNegativeScenario: Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions,
			orgHeaderPropertyInfo: declaration.JE_OH_SupplierInfo,
			declaration: declaration,
			useHeaderAddressPk: false);
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartyFive()
	{
		var declaration = Factory.New<JobDeclaration>();
		SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(targetPaymentMethod: Ucc6ExportDefermentMethodList.Codes.ExportersAccountFromCustomsDecisions,
			paymentMethodForNegativeScenario: Ucc6ExportDefermentMethodList.Codes.DeclarantsAccountFromCustomsDecisions,
			orgHeaderPropertyInfo: declaration.ExporterDocAddress.OrganisationPKInfo,
			declaration: declaration,
			useHeaderAddressPk: false);
	}

	public void TestDefermentApprovalNumberListForUcc6ExportPaymentPartABCD()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "CODE1";
		declarant.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "DAT123", jobDeclaration.CountryCode);

		var importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = "IMPC1";
		importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "IMP123", jobDeclaration.CountryCode);
		Factory.Save();

		var importerExpectedApprovalNumber = new ZString("IMP123");
		var declarantApprovalNumber = new ZString("DAT123");

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
			AssertLookup("DefermentApprovalNumberList for Payment Method: A", declaration.Lookups.DefermentApprovalNumberList, 1, new Dictionary<ZString, ZString> { { declarantApprovalNumber, ZString.Empty } });

			var paymentMethodCodes = new[] { DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority, DefermentMethodList.Codes.ConsigneesAccountStandingAuthority, DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration };
			foreach (var paymentMethod in paymentMethodCodes)
			{
				declaration.JE_PaymentMethod = paymentMethod;
				AssertLookup($"DefermentApprovalNumberList for Payment Method:{paymentMethod}", declaration.Lookups.DefermentApprovalNumberList, 1, new Dictionary<ZString, ZString> { { importerExpectedApprovalNumber, ZString.Empty } });
			}
		}
	}

	public void TestDefermentApprovalNumberListTruncation()
	{
		var declaration = CreateDeclarationAndDeclarant(
			shortCode: "OrgIT",
			danNumber: "0987654321098765432109876543210987654321",
			datNumber: "1234567890123456789012345678901234567890");

		declaration.JE_PaymentMethod = "A";
		var defermentApprovalNumberList = declaration.Lookups.DefermentApprovalNumberList;

		AssertEquals("Count", 2, defermentApprovalNumberList.Count);
		AssertContainsExactElementsInAnyOrder(
			"Truncated Deferment Approval Numbers",
			new[] { "12345678901234567890123456789012345", "09876543210987654321098765432109876" },
			defermentApprovalNumberList.ToArray().Select(x => x.Code));
	}

	public void TestLocationQualifierList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var lookups = declaration.Lookups;
		declaration.JE_MessageType = "EXP";
		AssertEquals("When MessageType is EXP, LocationQualifierList is empty", 0, lookups.LocationQualifierList.Count);

		declaration.JE_MessageType = "IMP";
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertType<GoodsLocationQualifierList>("When AuthorisationNumber is empty, LocationQualifierList is returned", lookups.LocationQualifierList);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertEquals("When AuthorisationNumber is filled, LocationQualifierList has two items", 2, lookups.LocationQualifierList.Count);
		AssertType<ImportGoodsLocationQualifierWithAuthorisationList>("When AuthorisationNumber is not empty, ImportGoodsLocationQualifierWithAuthorisationList is returned", lookups.LocationQualifierList);

		AssertContainsExactElementsInAnyOrder("When Authorisation is not empty, LB and LC must be present", new[] { "LB", "LC" }, lookups.LocationQualifierList.GetAllCodes());
	}

	public void TestBuyers()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<OrgHeaderCollection>("Buyers Type", declaration.Lookups.Buyers);
	}

	public void TestMessageVersionList()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<MessageVersionList>("Message Version type", declaration.Lookups.MessageVersionList);
		AssertEquals("Code As String", "TXT, XML", declaration.Lookups.MessageVersionList.CodesAsString);
	}

	public void TestTransportMeansListForUcc6Export()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "AIR", expectedCodesAsString: string.Empty, expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "RAI", expectedCodesAsString: string.Empty, expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "ROA", expectedCodesAsString: string.Empty, expectedDefaultCode: null);

			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "FIX", expectedCodesAsString: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "IWT", expectedCodesAsString: "80, 81", expectedDefaultCode: "81");
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "OWN", expectedCodesAsString: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "MAI", expectedCodesAsString: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "SEA", expectedCodesAsString: "10, 11", expectedDefaultCode: "11");
		}
	}

	public void TestTransportMeansListForImport()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "AIR", expectedCodesAsString: "40, 41", expectedDefaultCode: "40");
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "RAI", expectedCodesAsString: "20, 21", expectedDefaultCode: "20");
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "ROA", expectedCodesAsString: "30, 31", expectedDefaultCode: "30");

			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "FIX", expectedCodesAsString: "10, 11, 20, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "IWT", expectedCodesAsString: "10, 11, 80, 81", expectedDefaultCode: "81");
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "OWN", expectedCodesAsString: "10, 11, 20, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "MAI", expectedCodesAsString: "10, 11, 20, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertTransportMeansListWithTransportModeInland(declaration, transportModeInland: "SEA", expectedCodesAsString: "10, 11, 80, 81", expectedDefaultCode: "11");
		}
	}

	void AssertTransportMeansListWithTransportModeInland(JobDeclaration declaration, string transportModeInland, string expectedCodesAsString, string expectedDefaultCode)
	{
		CombineAssertions($"For {transportModeInland}", () =>
		{
			declaration.JE_TransportModeInland = transportModeInland;
			AssertEquals("CodesAsString", expectedCodesAsString, declaration.Lookups.TransportMeansList.CodesAsString);
			AssertEquals("DefaultCode", expectedDefaultCode, declaration.Lookups.TransportMeansList.DefaultCode);
			AssertSame("Cache", declaration.Lookups.TransportMeansList, declaration.Lookups.TransportMeansList);
		});
	}

	public override void TestEntryStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<ITEntryStatusList>("Entry Status List", declaration.Lookups.EntryStatusList);
	}

	void SetupDataAndTestDermentApprovalNumberListForPaymentUnderUcc6Export(string targetPaymentMethod, string paymentMethodForNegativeScenario, ZPropertyInfo orgHeaderPropertyInfo, JobDeclaration declaration, bool useHeaderAddressPk = true)
	{
		var orgHeader = SetupOrgHeaderWithAuthHeaders(new[] { "124433", "DP8273221" }, declaration.CountryCode, "C23298382");
		Factory.Save();

		SetOrgHeaderPropertyValue(orgHeader);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = targetPaymentMethod;
			var defermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
			AssertNotNull(defermentApprovalNumbers);
			AssertEquals("Count", 2, defermentApprovalNumbers.Count);
			Assert("124433 is available", defermentApprovalNumbers.ContainsCode("124433"));
			Assert("DP8273221 is available", defermentApprovalNumbers.ContainsCode("DP8273221"));
			Assert("C23298382 is not available", !defermentApprovalNumbers.ContainsCode("C23298382"));

			orgHeaderPropertyInfo.Value = ZGuid.Empty;
			defermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
			AssertNotNull(defermentApprovalNumbers);
			AssertEquals("Count", 0, defermentApprovalNumbers.Count);

			var orgHeaderWithoutDopAuthHeaders = SetupOrgHeaderWithAuthHeaders(null, declaration.CountryCode, "CW122212");
			SetOrgHeaderPropertyValue(orgHeaderWithoutDopAuthHeaders);
			defermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
			AssertNotNull(defermentApprovalNumbers);
			AssertEquals("Count", 0, defermentApprovalNumbers.Count);

			declaration.JE_PaymentMethod = paymentMethodForNegativeScenario;
			defermentApprovalNumbers = declaration.Lookups.DefermentApprovalNumberList;
			AssertNotNull(defermentApprovalNumbers);
			AssertEquals("Count", 0, defermentApprovalNumbers.Count);
		}

		void SetOrgHeaderPropertyValue(OrgHeader header)
		{
			orgHeaderPropertyInfo.Value = useHeaderAddressPk ? header.MainAddress.PK : header.PK;
		}
	}

	OrgHeader SetupOrgHeaderWithAuthHeaders(string[] dopAuthNumbers, string countryCode, string cw1AuthNumber = null)
	{
		var owner = Factory.NewWithValidTestData<OrgHeader>();
		owner.CustomsCodes.AddNew("DAN", "1000", countryCode);

		if (dopAuthNumbers?.Any() ?? false)
		{
			foreach (var dopAuthNumber in dopAuthNumbers)
			{
				var authHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.DeferredPayment, owner.PK, dopAuthNumber);
			}
		}

		if (!string.IsNullOrEmpty(cw1AuthNumber))
		{
			var authHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, owner.PK, cw1AuthNumber);
		}

		return owner;
	}

	void AssertLookup(ZString combineAssertionMessage, CodeDescriptionPairList lookup, int count, Dictionary<ZString, ZString> codeDescriptions)
	{
		CombineAssertions(combineAssertionMessage, () =>
		{
			AssertEquals("Lookup count", count, lookup.Count);
			foreach (var pair in codeDescriptions)
			{
				AssertEquals(pair.Value, lookup.GetDescriptionFromCode(pair.Key));
			}
		});
	}

	JobDeclaration CreateDeclarationAndImporter(string shortCode, string danNumber, string datNumber)
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		jobDeclaration.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = shortCode;
		if (!danNumber.IsNullOrEmpty())
		{
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, danNumber, jobDeclaration.CountryCode);
		}
		if (!datNumber.IsNullOrEmpty())
		{
			importer.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, datNumber, jobDeclaration.CountryCode);
		}
		var euOrgImpAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, jobDeclaration.CountryCode);
		euOrgImpAddInfo.Deserialise();
		euOrgImpAddInfo.ZO_OtherDeferType = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		Factory.Save();

		jobDeclaration.JE_OH_Importer = importer.PK;
		return jobDeclaration;
	}

	JobDeclaration CreateDeclarationAndDeclarant(string shortCode, string danNumber, string datNumber)
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		jobDeclaration.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = shortCode;
		if (!danNumber.IsNullOrEmpty())
		{
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, danNumber, jobDeclaration.CountryCode);
		}
		if (!datNumber.IsNullOrEmpty())
		{
			declarant.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, datNumber, jobDeclaration.CountryCode);
		}
		Factory.Save();

		jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		return jobDeclaration;
	}
}
