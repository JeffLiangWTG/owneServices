using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBH_CommunicationLanguage()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Factory.New<NctsHeader>().BH_CommunicationLanguageInfo, "XX", string.Empty);
		}

		public void TestGetRuleExplanation() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("RuleNumber is Empty", () => NctsHeaderValidationHelper.GetRuleExplanation(ZString.Empty, "explanation"));
			AssertExceptionThrown<ArgumentException>("explanation is Empty", () => NctsHeaderValidationHelper.GetRuleExplanation("CXXX", ZString.Empty));
			AssertEquals("Both not empty", "explanation(CXXX)", NctsHeaderValidationHelper.GetRuleExplanation("CXXX", "explanation"));
		});

		public void TestCheckBH_RL_NKImportLoadPort_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			ValidationTestHelper.AssertInvalidCodeMessageError(header.BH_RL_NKImportLoadPortInfo, "X~", Core.Constants.CountryCodes.Germany);
		}

		public void TestCheckBH_RL_NKImportLoadPort_ListC0009()
		{
			const string errorMessage = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodItem = header.Bills.AddNew().GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				header.BH_RL_NKImportLoadPort = "XX";
				AssertHasMessageErrorContaining("XX is not in List 9", header.BH_RL_NKImportLoadPortInfo, errorMessage);

				header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Germany;
				AssertNoMessageErrorContaining("DE is in List 9", header.BH_RL_NKImportLoadPortInfo, errorMessage);

				header.BH_RL_NKImportLoadPort = "";
				AssertNoMessageErrorContaining("Empty and not read only", header.BH_RL_NKImportLoadPortInfo, errorMessage);

				goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
				header.Validation.ValidateBH_RL_NKImportLoadPort();
				AssertNoMessageErrorContaining("Validate it only if not read only", header.BH_RL_NKImportLoadPortInfo, errorMessage);
			});
		}

		public void TestCheckBH_RL_NKImportLoadPort_ListC0009_GB()
		{
			const string errorMessage = "For T2 Declarations [15A] and/or [17A] must be a country from Code List 9 (Countries if European Union + NCTS Contracting Parties).";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry("GB");
			var xxxBranch = Factory.New<GlbBranch>();
			xxxBranch.GB_RL_NKHomePort = "GBNRW";
			xxxBranch.GB_Code = "XXX";
			xxxBranch.GB_GC = company.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(xxxBranch.PK.ToGuid()))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
				helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
				Factory.Save();

				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				CombineAssertions(() =>
				{
					header.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
					header.BH_RL_NKImportLoadPort = "XX";
					AssertHasMessageErrorContaining("Not List 9", header.BH_RL_NKImportLoadPortInfo, errorMessage);

					header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;
					AssertNoMessageErrorContaining("In List 9", header.BH_RL_NKImportLoadPortInfo, errorMessage);
				});
			}
		}

		public void TestCheckConsigneeNR0068()
		{
			const string messageError = "[NR0068] Consignor must have EORI number if additional reference code Y022 is used.";
			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				_ = deciderTestContext.ClearCachedValidationDecider(nctsHeader);
				deciderTestContext.EnableRule(c => c.IsRuleNR0068Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
				var consignor = nctsHeader.Consignor;
				var propertyInfo = consignor.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					consignor.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Y022, No Consignor", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Y022, Has Consignor (no eori)", propertyInfo, messageError);

					var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
					additionalReference.CSI_SubType = "REF";
					additionalReference.CSI_Code = "Y022";
					consignor.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("Has Y022, No Consignor", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageError("Has Y022, Has Consignor (no eori)", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Y022, Has Consignor (with eori)", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0068Active);
					consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Y022, Has Consignor (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestCheckBH_RL_NKImportLoadPort_MustBeFiled()
		{
			const string errorMessageRequiredDispatchInfo = "Either Dispatch Country in Goods tab or Country of Dispatch in Declaration must be filled.";
			const string errorMessageExclusiveDispatchInfo = "Dispatch Country must be filled in Goods tab or Country of Dispatch must be filled in Declaration tab but not both.";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			header.BH_RL_NKImportLoadPort = "";
			AssertHasMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			var goodItem = header.MovementHeader.GoodsItems.AddNew();
			goodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
			header.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertHasMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_RL_NKImportLoadPort = "";
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			header.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);

			header.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.China;
			AssertNoMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageRequiredDispatchInfo);
			AssertHasMessageError(header.BH_RL_NKImportLoadPortInfo, errorMessageExclusiveDispatchInfo);
		}

		public void TestCheckBMPlaceOfUnloading__PlaceOfUnloadingCode()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.PlaceOfUnloadingCode = "GBLHR";
			AssertNoMessageErrorContaining(header.PlaceOfUnloadingCodeInfo, "list");
			header.PlaceOfUnloadingCode = "";
			AssertNoMessageErrorContaining(header.PlaceOfUnloadingCodeInfo, "list");
			header.PlaceOfUnloadingCode = "X~";
			AssertHasMessageErrorContaining(header.PlaceOfUnloadingCodeInfo, "list");
		}

		public void TestDestinationCustomsOfficeCodeForDeparture()
		{
			CreateCustomsOfficeForTest("GB000011", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			AssertCustomsOfficeValidations(header.DestinationCustomsOfficeCodeForDepartureInfo, value => header.DestinationCustomsOfficeCodeForDeparture = value, "GB000011");
		}

		void CreateCustomsOfficeForTest(string officeCode, string role)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, Core.Constants.CountryCodes.UnitedKingdom, $"Office Description {officeCode}", new ZString[] { role });
			Factory.Save();
		}

		void AssertCustomsOfficeValidations(ZPropertyInfo propertyInfo, Action<string> propertySetter, string validOfficeCode)
		{
			AssertNoMessageErrorContaining(propertyInfo, "is not a valid office");
			propertySetter("xxxx");
			AssertHasMessageErrorContaining(propertyInfo, "prefix and then contain a further 6 characters");
			propertySetter("GB999999");
			AssertHasMessageErrorContaining(propertyInfo, "is not a valid office");
			propertySetter(validOfficeCode);
			AssertNoMessageErrors(propertyInfo);
			propertySetter("");
			AssertHasMessageErrors(propertyInfo);
		}

		public void TestHeaderUnloadingRemarkConformsListNotifications()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.UnloadingRemark.G9_Conform = "X";
			AssertHasMessageErrorContaining(header.UnloadingRemark.G9_ConformInfo, "The code you have selected is not in the list.");
			header.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			AssertNoMessageErrorContaining(header.UnloadingRemark.G9_ConformInfo, "The code you have selected is not in the list.");
		}

		public void TestHeaderUnloadingRemarkStateOfSealsOkListNotifications()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.UnloadingRemark.G9_StateOfSealsOk = "X";
			AssertHasMessageErrorContaining(header.UnloadingRemark.G9_StateOfSealsOkInfo, "The code you have selected is not in the list.");
			header.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			AssertNoMessageErrorContaining(header.UnloadingRemark.G9_StateOfSealsOkInfo, "The code you have selected is not in the list.");
		}

		public void TestHeaderUnloadingRemarkUnloadingCompletionListNotifications()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.UnloadingRemark.G9_UnloadingCompletion = "X";
			AssertHasMessageErrorContaining(header.UnloadingRemark.G9_UnloadingCompletionInfo, "The code you have selected is not in the list.");
			header.UnloadingRemark.G9_UnloadingCompletion = YesNoList.Codes.Yes;
			AssertNoMessageErrorContaining(header.UnloadingRemark.G9_UnloadingCompletionInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckArrivalMrnFromUser()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_GB = branch.PK;
			nctsHeader.ArrivalMrnFromUser = "12345";
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			testNctsHeader.BH_GB = branch.PK;
			testNctsHeader.ArrivalMrnFromUser = "12345";
			testNctsHeader.BH_JobReference = "Test123";
			Factory.Save();

			var message = $"Another Entry already contains the same MRN number (Entry: 'Test123', Company: '{testNctsHeader.Company.GC_Name}', Branch: '{testNctsHeader.Branch.GB_BranchName}').";

			CombineAssertions(() =>
			{
				nctsHeader.Validation.ValidateArrivalMrnFromUser();
				AssertNoWarning("The other Entry with the same MRN being Departure, no MRN duplication check.", nctsHeader.ArrivalMrnFromUserInfo, message);

				testNctsHeader.Delete();
				testNctsHeader = Factory.New<NctsHeader>();
				testNctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				testNctsHeader.BH_GB = branch.PK;
				testNctsHeader.ArrivalMrnFromUser = "12345";
				testNctsHeader.BH_JobReference = "Test123";
				Factory.Save();
				nctsHeader.Validation.ValidateArrivalMrnFromUser();
				AssertHasWarning("MRN duplication check", nctsHeader.ArrivalMrnFromUserInfo, message);

				nctsHeader.ArrivalMrnFromUser = "1234";
				nctsHeader.Validation.ValidateArrivalMrnFromUser();
				AssertNoWarning("MRN duplication check, pass.", nctsHeader.ArrivalMrnFromUserInfo, message);
			});
		}

		public void TestCheckLocalReferenceNumber_EnglishCharactersValidation()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			header.LocalReferenceNumber = "【】";
			AssertHasErrorContaining(header.LocalReferenceNumberInfo, "accepts Western European languages characters");
		}

		public void TestGetLocalReferenceNumber_Ncts4Departure_Length()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.LocalReferenceNumber = "1234";
			AssertHasMessageErrorContaining(header.LocalReferenceNumberInfo, "This field must have a value and the value must be longer than 4 characters.");
			header.LocalReferenceNumber = "12345";
			AssertNoMessageErrorContaining(header.LocalReferenceNumberInfo, "This field must have a value and the value must be longer than 4 characters.");
		}

		public void TestCheckLocalReferenceNumber_Phase5Arrival_ReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			CombineAssertions(() =>
			{
				header.LocalReferenceNumber = ZString.Empty;
				AssertNoNotifications("Empty LRN", header.LocalReferenceNumberInfo);
				header.LocalReferenceNumber = "1234";
				AssertNoNotifications("LRN < 4 characters", header.LocalReferenceNumberInfo);
				header.LocalReferenceNumber = "12345";
				AssertNoNotifications("Valid LRN", header.LocalReferenceNumberInfo);
				header.LocalReferenceNumber = "【】";
				AssertNoNotifications("No western characters", header.LocalReferenceNumberInfo);
			});
		}

		public void TestCheckConsignee_NR0069()
		{
			const string messageError = "[NR0069] Consignee must have EORI number if additional reference code Y023 is used.";
			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				_ = deciderTestContext.ClearCachedValidationDecider(nctsHeader);
				deciderTestContext.EnableRule(c => c.IsRuleNR0069Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
				var consignee = nctsHeader.Consignee;
				var propertyInfo = consignee.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Y023, No Consignee", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Y023, Has Consignee (no eori)", propertyInfo, messageError);

					var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
					additionalReference.CSI_SubType = "REF";
					additionalReference.CSI_Code = "Y023";
					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("Has Y023, No Consignee", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageError("Has Y023, Has Consignee (no eori)", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Y023, Has Consignee (with eori)", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0069Active);
					consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Y023, Has Consignee (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestCheckPrincipal_NR0071()
		{
			const string messageError = "[NR0071] Principal/Holder of the Transit Procedure must have EORI number if additional reference code Y026 is used.";
			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				_ = deciderTestContext.ClearCachedValidationDecider(nctsHeader);
				deciderTestContext.EnableRule(c => c.IsRuleNR0071Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
				var principal = nctsHeader.Principal;
				var propertyInfo = principal.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					principal.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Y026, No Principal", propertyInfo, messageError);

					principal.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Y026, Has Principal (no eori)", propertyInfo, messageError);

					var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
					additionalReference.CSI_SubType = "REF";
					additionalReference.CSI_Code = "Y026";
					principal.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("Has Y026, No Principal", propertyInfo, messageError);

					principal.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageError("Has Y026, Has Principal (no eori)", propertyInfo, messageError);

					principal.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Y026, Has Principal (with eori)", propertyInfo, messageError);

					principal.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0071Active);
					principal.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Y026, Has Principal (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestCheckPrincipal_NR0074()
		{
			const string messageError = "[NR0074] Principal/Holder of the Transit Procedure must have EORI number if Authorization ACR or SSE is used.";
			using (var deciderTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				_ = deciderTestContext.ClearCachedValidationDecider(nctsHeader);
				deciderTestContext.EnableRule(c => c.IsRuleNR0074Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", CountryCodes.Germany);
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", CountryCodes.Italy);
				var principal = nctsHeader.Principal;
				var propertyInfo = principal.OrganisationPKInfo;

				var authorization = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
				CombineAssertions(() =>
				{
					authorization.AGC_Code = "XXX";
					principal.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Authorisation, No Principal", propertyInfo, messageError);

					principal.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Authorisation, Has Principal (no eori)", propertyInfo, messageError);

					authorization.AGC_Code = "ACR";
					principal.Validation.ValidateOrganisationPK();
					AssertHasMessageError("Has Authorisation (ACR), Has Principal (no eori)", propertyInfo, messageError);

					principal.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Authorisation, Has Principal (with eori)", propertyInfo, messageError);

					principal.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0074Active);
					principal.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Authorisation, Has Principal (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestValidateSecurityConsigneeTrader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();

			var c188Error = "Security Consignee Trader must be present at the header or detail level.(C188)";
			void AssertHasC188Error(string because)
			{
				header.SecurityConsignee.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining(because, header.SecurityConsignee.OrganisationPKInfo, c188Error);
			}
			void AssertNoC188Error(string because)
			{
				header.SecurityConsignee.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(because, header.SecurityConsignee.OrganisationPKInfo, c188Error);
			}

			header.BH_FTZMove = false;
			AssertNoC188Error("FTZMode is false, no validation is executed.");

			header.BH_FTZMove = true;
			AssertHasC188Error("FTZMode is true, but there is no security consignee.");

			additionalInfo.CSI_Code = "10600";
			AssertNoC188Error("There is a 10600 special mention, which indicates security consignee is unknown.");

			additionalInfo.CSI_Code = "";
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "ABC", header.SecurityConsignee);
			AssertNoC188Error("Security consignee is set on header level.");

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "DEF", goodsItem.SecurityConsignee);
			AssertHasC188Error("Security consignee is set both on header and goods item level.");

			header.SecurityConsignee.E2_OA_Address = ZGuid.Empty;
			AssertNoC188Error("Security consignee is set on goods item level.");
		}

		public void TestValidateSecurityConsignorTrader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var goodsItem = header.MovementHeader.GoodsItems.AddNew();

			var c188Error = "Security Consignor Trader must be present at the header or detail level.(C187)";
			void AssertHasC187Error(string because)
			{
				header.SecurityConsignor.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining(because, header.SecurityConsignor.OrganisationPKInfo, c188Error);
			}
			void AssertNoC187Error(string because)
			{
				header.SecurityConsignor.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining(because, header.SecurityConsignor.OrganisationPKInfo, c188Error);
			}

			header.BH_FTZMove = false;
			AssertNoC187Error("FTZMode is false, no validation is executed.");

			header.BH_FTZMove = true;
			AssertHasC187Error("FTZMode is true, but there is no security consignor on header or goods item level.");

			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertNoC187Error("When Phase5, no validation is executed.");

			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "ABC", header.SecurityConsignor);
			AssertNoC187Error("Security consignor is set on header level.");

			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "DEF", goodsItem.SecurityConsignor);
			AssertHasC187Error("Security consignor is set both on header and goods item level.");

			header.SecurityConsignor.E2_OA_Address = ZGuid.Empty;
			AssertNoC187Error("Security consignor is set on goods item level.");
		}
	}
}
