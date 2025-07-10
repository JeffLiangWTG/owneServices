using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber_G0321()
		{
			var additionalDocument = CreateAdditionalInfo(NctsMovementType.Codes.Departure);
			var warningMessage = "[G0321] You have not entered a Reference Number - '0' will be used.";

			CombineAssertions(() =>
			{
				using var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory);
				ruleTestContext.DisableRule(x => x.IsRuleG0321Active);
				additionalDocument.CSI_Code = "aA";
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive, CSI_Code is empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

				ruleTestContext.EnableRule(x => x.IsRuleG0321Active);
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasWarning("The rule is active, CSI_Code is empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarning("The rule is active, CSI_Code is empty, subType is INF and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = string.Empty;
				additionalDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarning("The rule is active, CSI_Code is not empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);
			});
		}

		public void TestCheckCSI_ReferenceNumber_C0015()
		{
			const string errorMessage = "[C0015] You have not entered a Reference Number for the additional excise document.";
			const string codeFromCL234 = "ABC";
			CreateCL234IfNotExistsAndAddCode(codeFromCL234);

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var additionalInfo = header.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();

			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(x => x.IsRuleC0015Active);

					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					additionalInfo.CSI_Code = codeFromCL234;
					additionalInfo.CSI_ReferenceNumber = "123";
					AssertNoMessageErrorContaining("Goods Items additionalInfo with not empty reference number.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalInfo.CSI_ReferenceNumber = ZString.Empty;
					AssertNoMessageErrorContaining("Goods Items additionalInfo with sub-type <> REF.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_Code = "PL";
					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					additionalInfo.Validation.ValidateCSI_ReferenceNumber();
					AssertNoMessageErrorContaining("Goods Items additionalInfo is not from CL234.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_Code = codeFromCL234;
					additionalInfo.Validation.ValidateCSI_ReferenceNumber();
					AssertHasMessageErrorContaining("Goods Items additionalInfo is REF from CL234 with empty reference number.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					var headerAdditionalInfo = header.AdditionalDocuments.AddNew();
					headerAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					headerAdditionalInfo.CSI_Code = codeFromCL234;
					headerAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
					AssertNoMessageErrorContaining("Additional info parent is not a Goods Item.", headerAdditionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					ruleTestContext.DisableRule(x => x.IsRuleC0015Active);
					additionalInfo.Validation.ValidateCSI_ReferenceNumber();
					AssertNoMessageErrorContaining("Rule is inactive.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);
				});
			}
		}

		public void TestCheckCSI_ReferenceNumber_R0023_DepartureMovement()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var additionalInfo = header.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();

			AssertAdditionalInfo(header.AdditionalDocuments, additionalInfo);
		}

		public void TestCheckCSI_ReferenceNumber_R0023_ArrivalMovement()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var additionalInfo = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();

			AssertAdditionalInfo(nctsHeader.ArrivalMovementHeader.AdditionalDocuments, additionalInfo);
		}

		void AssertAdditionalInfo(INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalDocumentsCollection, NctsAdditionalInfo additionalInfo)
		{
			const string errorMessage = "[R0023] Entered Reference Number '0' is not a valid entry.";
			const string codeFromCL234 = "ABC";
			CreateCL234IfNotExistsAndAddCode(codeFromCL234);

			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(x => x.IsRuleR0023Active);

					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					additionalInfo.CSI_Code = codeFromCL234;
					additionalInfo.CSI_ReferenceNumber = "123";
					AssertNoMessageErrorContaining("Goods Items additionalInfo has reference number <> 0.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalInfo.CSI_ReferenceNumber = "0";
					AssertNoMessageErrorContaining("Goods Items additionalInfo with sub-type <> REF.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_Code = "PL";
					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					additionalInfo.Validation.ValidateCSI_ReferenceNumber();
					AssertNoMessageErrorContaining("Goods Items additionalInfo is not from CL234.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					additionalInfo.CSI_Code = codeFromCL234;
					additionalInfo.CSI_ReferenceNumber = "0";
					AssertHasMessageErrorContaining("Goods Items additionalInfo is REF from CL234 with reference number = 0.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					var headerAdditionalInfo = additionalDocumentsCollection.AddNew();
					headerAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					headerAdditionalInfo.CSI_Code = codeFromCL234;
					headerAdditionalInfo.CSI_ReferenceNumber = "0";
					AssertNoMessageErrorContaining("Additional info parent is not a Goods Item.", headerAdditionalInfo.CSI_ReferenceNumberInfo, errorMessage);

					ruleTestContext.DisableRule(x => x.IsRuleR0023Active);
					additionalInfo.Validation.ValidateCSI_ReferenceNumber();
					AssertNoMessageErrorContaining("Rule is inactive.", additionalInfo.CSI_ReferenceNumberInfo, errorMessage);
				});
			}
		}

		public void TestCheckCSI_ReferenceNumberRuleTR0062()
		{
			var additionalDocument = CreateAdditionalInfo(NctsMovementType.Codes.Departure);
			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(x => x.IsRuleTR0062Active);
				NctsAdditionalInfoValidationTestHelper.AssertWhenRuleActive(additionalDocument.CSI_ReferenceNumberInfo);

				ruleTestContext.DisableRule(x => x.IsRuleTR0062Active);
				NctsAdditionalInfoValidationTestHelper.AssertWhenRuleNotActive(additionalDocument.CSI_ReferenceNumberInfo);
			}
		}

		void CreateCL234IfNotExistsAndAddCode(string codeToAdd)
		{
			const string groupingCodeEUN = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			const string codeCL234 = UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL234;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(groupingCodeEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(codeCL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList(groupingCodeEUN, codeCL234, code: codeToAdd, description: "Code Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		public void TestCheckCSI_Code_Phase5_Header_Mandatory()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeaderAdditionalInfo.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_Phase5_Header_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsHeaderAdditionalInfo.CSI_CodeInfo, "~", "REF01");
		}

		public void TestCheckCSI_SubType_Phase5_Mandatory()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(nctsHeaderAdditionalInfo.CSI_SubTypeInfo, "~", AdditionalInfoSubTypeList.Codes.TransportDocument);
		}

		public void TestCSI_Code_Phase5Arrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item);
			Factory.Save();

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			goodsItemAdditionalInfo = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				goodsItemAdditionalInfo.CSI_Status = "NEW";
				AssertHasErrorContaining(goodsItemAdditionalInfo.CSI_CodeInfo, MandatoryValidation.MustBeEntered); //ValidationTestHelper conflicts with setting the CSI_Status to empty when CSI_Code is set to empty, so the ValidationTestHelper function will always fail here

				goodsItemAdditionalInfo.CSI_Status = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItemAdditionalInfo.CSI_CodeInfo);

				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ValidationTestHelper.AssertInvalidCodeMessageError(goodsItemAdditionalInfo.CSI_CodeInfo, "BAD", "REF01");
			});
		}

		public void TestCheckCSI_Status_Phase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var headerAdditionalInfo = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
			goodsItemAdditionalInfo = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(headerAdditionalInfo.CSI_StatusInfo, "INV", NctsBillAdditionalDocumentStatusList.Codes.DEC);
				ValidationTestHelper.AssertErrorIfInvalidCode(goodsItemAdditionalInfo.CSI_StatusInfo, "INV", NctsBillAdditionalDocumentStatusList.Codes.DEC);
			});
		}

		public void TestCheckCSI_SubType_Phase5Departure_WithMovementHeaderDestinationCountry_R3060()
		{
			const string error = "[R3060] Doc. Kind = INF and Doc. Type = 30600 information cannot be declared at Consignment level Additional Document since Consignment level Destination Country is from CL009 (CountryCodesCommonTransit) list.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;

			using (var deciderTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				CombineAssertions("When R3060 is active", () =>
				{
					deciderTestContext.EnableRule(x => x.IsRuleR3060Active);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.Australia, true, error);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Germany;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.Germany, true, error);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.India, false, error);
				});

				CombineAssertions("When R3060 is inactive", () =>
				{
					deciderTestContext.DisableRule(x => x.IsRuleR3060Active);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
					nctsHeaderAdditionalInfo.Validation.ValidateCSI_SubType();
					AssertNoMessageError("Destination Country is AUS, Doc. Kind is not INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);

					nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					AssertNoMessageError("Destination Country is AUS, Doc. Kind is INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
				});
			}
		}

		public void TestCheckCSI_SubType_Phase5Departure_WithGoodsItemDestinationCountry_R3060()
		{
			const string error = "[R3060] Doc. Kind = INF and Doc. Type = 30600 information cannot be declared at Consignment level Additional Document since one of the Goods Items Destination Country is from CL009 (CountryCodesCommonTransit) list.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			using (var deciderTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				CombineAssertions("When R3060 is active", () =>
				{
					deciderTestContext.EnableRule(x => x.IsRuleR3060Active);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.Australia, true, error);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.Germany, true, error);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.India;
					AssertWithEachCountryCodes_R3060_WhenActive(Core.Constants.CountryCodes.India, false, error);
				});

				CombineAssertions("When R3060 is inactive", () =>
				{
					deciderTestContext.DisableRule(x => x.IsRuleR3060Active);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
					nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
					nctsHeaderAdditionalInfo.Validation.ValidateCSI_SubType();
					AssertNoMessageError("Destination Country is AUS, Doc. Kind is not INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);

					nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					AssertNoMessageError("Destination Country is AUS, Doc. Kind is INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
				});
			}
		}

		void AssertWithEachCountryCodes_R3060_WhenActive(ZString code, bool codeIsC0009, string error)
		{
			nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertNoMessageError($"Destination Country is {code}, Doc. Kind is not INF and Doc. Type is not 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);

			nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
			nctsHeaderAdditionalInfo.Validation.ValidateCSI_SubType();
			AssertNoMessageError($"Destination Country is {code}, Doc. Kind is not INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);

			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			if (codeIsC0009)
			{
				AssertHasMessageError($"Destination Country is {code}, Doc. Kind is INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
			}
			else
			{
				AssertNoMessageError($"Destination Country is {code}, Doc. Kind is INF and Doc. Type is 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
			}

			nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
			nctsHeaderAdditionalInfo.Validation.ValidateCSI_SubType();
			AssertNoMessageError($"Destination Country is {code}, Doc. Kind is INF and Doc. Type is not 30600.", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
		}

		public void TestCheckCSI_SubType_RuleE1301()
		{
			const string error = "[E1301] In transition period, which is now, Additional Information, Additional Reference and Transport Document must be empty";

			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					ruleTestContext.EnableRule(x => x.IsRuleE1301Active);
					CombineAssertions("When TP: ON, NCTS5, RuleE1301: Active", () =>
					{
						nctsHeaderAdditionalInfo.CSI_SubType = ZString.Empty;
						AssertNoMessageError("CSI_SubType: Empty", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);

						nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertHasMessageError("CSI_SubType: INF", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
					});

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					CombineAssertions("When TP: ON, NCTS4, RuleE1301: Active", () =>
					{
						nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("CSI_SubType: INF", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
					});

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					ruleTestContext.DisableRule(x => x.IsRuleE1301Active);
					CombineAssertions("When TP: ON, NCTS5, RuleE1301: Inactive", () =>
					{
						nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("CSI_SubType: INF", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
					});
				}

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				ruleTestContext.EnableRule(x => x.IsRuleE1301Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					CombineAssertions("When TP: OFF, NCTS5, RuleE1301: Active", () =>
					{
						nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("CSI_SubType: INF", nctsHeaderAdditionalInfo.CSI_SubTypeInfo, error);
					});
				}
			}
		}

		public void TestValidateAll_R3061_WhenActive_NotInPhase5TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				nctsHeaderAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				CombineAssertions(() =>
				{
					using (var deciderTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
					{
						deciderTestContext.EnableRule(r => r.IsRuleR3061Active);

						AssertNoNotificationWhenSubtypeINFAndCodeNot30600();
						AssertNoNotificationWhenSubtypeNotINFAndCode30600();
						AssertHasNotificationWhenSubtypeINFAndCode30600();
						AssertNoNotificationWhenParentTableCodeNotBY();
					}
				});
			}

			void AssertNoNotificationWhenSubtypeINFAndCodeNot30600()
			{
				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				goodsItemAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;

				((NctsAdditionalInfoPhase5Validation)goodsItemAdditionalInfo.Validation).ValidateAll();
				AssertNoRowMessageError("SubtypeINFAndCodeNot30600", goodsItemAdditionalInfo, R3061Msg);
			}

			void AssertNoNotificationWhenSubtypeNotINFAndCode30600()
			{
				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				goodsItemAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				((NctsAdditionalInfoPhase5Validation)goodsItemAdditionalInfo.Validation).ValidateAll();
				AssertNoRowMessageError("SubtypeNotINFAndCode30600", goodsItemAdditionalInfo, R3061Msg);
			}

			void AssertHasNotificationWhenSubtypeINFAndCode30600()
			{
				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				goodsItemAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				((NctsAdditionalInfoPhase5Validation)goodsItemAdditionalInfo.Validation).ValidateAll();
				AssertHasRowMessageError("SubtypeINFAndCode30600", goodsItemAdditionalInfo, R3061Msg);
			}

			void AssertNoNotificationWhenParentTableCodeNotBY()
			{
				nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				nctsHeaderAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				((NctsAdditionalInfoPhase5Validation)nctsHeaderAdditionalInfo.Validation).ValidateAll();
				AssertNoRowMessageError("ParentTableCodeNotBY", nctsHeaderAdditionalInfo, R3061Msg);
			}
		}

		public void TestValidateAll_R3061_WhenActive_InPhase5TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				goodsItemAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				using (var deciderTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(r => r.IsRuleR3061Active);

					((NctsAdditionalInfoPhase5Validation)goodsItemAdditionalInfo.Validation).ValidateAll();
					AssertNoRowMessageError(goodsItemAdditionalInfo, R3061Msg);
				}
			}
		}

		public void TestValidateAll_R3061_WhenInactive()
		{
			using (var deciderTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(r => r.IsRuleR3061Active);

				goodsItemAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				goodsItemAdditionalInfo.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;

				((NctsAdditionalInfoPhase5Validation)goodsItemAdditionalInfo.Validation).ValidateAll();
				AssertNoRowMessageError(goodsItemAdditionalInfo, R3061Msg);
			}
		}

		public void TestCheckCSI_ReferenceNumber_RuleE1104_1()
		{
			const string expectedMessage = "[E1104-1] During the transition period, which is active now, Reference Number cannot be longer than 35 characters.";
			Expression<Func<INctsAdditionalInfoPhase5ValidationDecider, bool>> rule = x => x.IsRuleE1104_1Active;

			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule);
				CombineAssertions("Transition period OFF and Phase 4 is active", () =>
				{
					nctsHeader.BH_ApplicationCode = "NCT";
					goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 40);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 40, expectedMessage);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 40, expectedMessage);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "INF", referenceNumberLengthToSet: 40, expectedMessage);
				});

				CombineAssertions("Transition period OFF and Phase 5", () =>
				{
					nctsHeader.BH_ApplicationCode = "NC5";
					goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 41);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 41, expectedMessage);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 41, expectedMessage);
					AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "INF", referenceNumberLengthToSet: 41, expectedMessage);
				});

				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					CombineAssertions("Transition period ON and Phase 4 is active", () =>
					{
						nctsHeader.BH_ApplicationCode = "NCT";
						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 40);
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 40, expectedMessage);
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 40, expectedMessage);
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "INF", referenceNumberLengthToSet: 40, expectedMessage);
					});

					CombineAssertions("Transition period ON and Phase 5 and RuleE1104_1 is active", () =>
					{
						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 41);
						nctsHeader.BH_ApplicationCode = "NC5";
						AssertReferenceNumberHasMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 41, expectedMessage);

						goodsItemAdditionalInfo.CSI_SubType = "TRA";
						nctsHeader.BH_ApplicationCode = "NCT";
						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 41);
						nctsHeader.BH_ApplicationCode = "NC5";
						AssertReferenceNumberHasMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 41, expectedMessage);

						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "INF", referenceNumberLengthToSet: 41, expectedMessage);

						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 30);
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 30, expectedMessage);
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 30, expectedMessage);
					});

					CombineAssertions("Transition period ON and Phase 5 and RuleE1104_1 is disable", () =>
					{
						ruleTestContext.DisableRule(rule);
						nctsHeader.BH_ApplicationCode = "NCT";
						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 41);
						nctsHeader.BH_ApplicationCode = "NC5";
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "REF", referenceNumberLengthToSet: 41, expectedMessage);

						goodsItemAdditionalInfo.CSI_SubType = "TRA";
						nctsHeader.BH_ApplicationCode = "NCT";
						goodsItemAdditionalInfo.CSI_ReferenceNumber = new string('R', 41);
						nctsHeader.BH_ApplicationCode = "NC5";
						AssertReferenceNumberNoMessageErrorForSubTypeAndLength(subType: "TRA", referenceNumberLengthToSet: 41, expectedMessage);
					});
				}
			}
		}

		public void TestCSI_SubType_TR0031() => AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.AdditionalInformation, "TR0031", x => x.IsRuleTR0031Active);

		public void TestCSI_SubType_TR0032() => AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference, "TR0032", x => x.IsRuleTR0032Active);

		public void TestCSI_SubType_TR0033() => AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.TransportDocument, "TR0033", x => x.IsRuleTR0033Active);

		void AssertMaxCountForSubType(string subType, string ruleName, Expression<Func<INctsAdditionalInfoPhase5ValidationDecider, bool>> rule)
		{
			AssertMaxCountForSubTypeCore(subType, ruleName, GetAdditionalDocumentsCollectionForParent("NctsHeader"), rule);
			AssertMaxCountForSubTypeCore(subType, ruleName, GetAdditionalDocumentsCollectionForParent("NctsCargoDesc"), rule);
		}

		void AssertMaxCountForSubTypeCore(string subType, string ruleName, INctsAdditionalInfoCollection<NctsAdditionalInfo> additionalInfos, Expression<Func<INctsAdditionalInfoPhase5ValidationDecider, bool>> rule)
		{
			using (var ruleTestContext = new NctsAdditionalInfoValidationDeciderTestContext<INctsAdditionalInfoPhase5ValidationDecider>(Factory))
			{
				additionalInfos.RemoveAndDeleteAll();
				Enumerable.Range(0, 98).ToList().ForEach(i => additionalInfos.AddNew().CSI_SubType = subType);

				CombineAssertions($"Testing rule '{ruleName}' for subType '{subType}'", () =>
				{
					ruleTestContext.EnableRule(rule);
					var additionalDoc99th = additionalInfos.AddNew();
					additionalDoc99th.CSI_SubType = subType;
					AssertNoMessageErrorContaining($"Additional Document of sub type {subType} #99", additionalDoc99th.CSI_SubTypeInfo, ruleName);

					var additionalDoc100th = additionalInfos.AddNew();
					additionalDoc100th.CSI_SubType = subType;
					AssertHasMessageErrorContaining($"Additional Document of sub type {subType} #100", additionalDoc100th.CSI_SubTypeInfo, ruleName);

					ruleTestContext.DisableRule(rule);
					additionalDoc100th.Validation.ValidateCSI_SubType();
					AssertNoMessageErrorContaining($"Rule [{ruleName}] is disabled", additionalDoc100th.CSI_SubTypeInfo, ruleName);
				});
			}
		}

		IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		void AssertReferenceNumberHasMessageErrorForSubTypeAndLength(string subType, int referenceNumberLengthToSet, string expectedMessage)
		{
			goodsItemAdditionalInfo.CSI_SubType = subType;
			goodsItemAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError($"SubType = {subType} and Reference number length {referenceNumberLengthToSet}", goodsItemAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
		}

		void AssertReferenceNumberNoMessageErrorForSubTypeAndLength(string subType, int referenceNumberLengthToSet, string expectedMessage)
		{
			goodsItemAdditionalInfo.CSI_SubType = subType;
			goodsItemAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"SubType = {subType} and Reference number length {referenceNumberLengthToSet}", goodsItemAdditionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItemAdditionalInfo = nctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
			nctsHeaderAdditionalInfo = nctsHeader.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo goodsItemAdditionalInfo;
		NctsAdditionalInfo nctsHeaderAdditionalInfo;

		const string R3061Msg = "[R3061] Additional Information of type 30600 can't be declared for any of the Goods Item.";

		INctsAdditionalInfoCollection<NctsAdditionalInfo> GetAdditionalDocumentsCollectionForParent(string parent)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			switch (parent)
			{
				case "NctsCargoDesc":
					return nctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos;
				case "NctsHeader":
				default:
					return nctsHeader.AdditionalDocuments;
			}
		}

		NctsAdditionalInfo CreateAdditionalInfo(string movementHeaderType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(movementHeaderType);
			return header.AdditionalDocuments.AddNew();
		}
	}
}
