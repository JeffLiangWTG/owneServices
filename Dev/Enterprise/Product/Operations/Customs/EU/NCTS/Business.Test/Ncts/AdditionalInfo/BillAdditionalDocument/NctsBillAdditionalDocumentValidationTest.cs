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
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumberRuleG0321()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			var warningMessage = "[G0321] You have not entered a Reference Number - '0' will be used.";

			CombineAssertions(() =>
			{
				using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(Factory))
				{
					ruleTestContext.DisableRule(r => r.IsRuleG0321Active);
					additionalDocument.CSI_Code = "aA";
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					additionalDocument.CSI_ReferenceNumber = ZString.Empty;
					AssertNoWarning("The rule is inactive, CSI_Code is empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

					ruleTestContext.EnableRule(r => r.IsRuleG0321Active);
					additionalDocument.Validation.ValidateCSI_ReferenceNumber();
					AssertHasWarning("The rule is active, CSI_Code is empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocument.Validation.ValidateCSI_ReferenceNumber();
					AssertNoWarning("The rule is active, CSI_Code is empty, subType is INF and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);

					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					additionalDocument.CSI_Code = string.Empty;
					additionalDocument.Validation.ValidateCSI_ReferenceNumber();
					AssertNoWarning("The rule is active, CSI_Code is not empty, subType is TRA and CSI_ReferenceNumber is empty", additionalDocument.CSI_ReferenceNumberInfo, warningMessage);
				}
			});
		}

		public void TestCSI_ReferenceNumber()
		{
			using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				additionalDocument.CSI_ReferenceNumberInfo,
				RefCusCodeListTypes.Codes.Code_AI44N,
				RefCusCodeListLevelTypes.House,
				RefCusCodeListAttributeTypes.Reference,
				() => ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleG0321Active)),
				() => ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleG0321Active)));
		}

		public void TestCSI_ReferenceNumber_NotAnyAttribute()
		{
			AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(additionalDocument.CSI_ReferenceNumberInfo, RefCusCodeListAttributeTypes.Reference);
		}

		public void TestCSI_ReferenceNumberRuleTR0062()
		{
			using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(Factory).ClearCachedValidationDecider(additionalDocument))
			{
				ruleTestContext.EnableRule(x => x.IsRuleTR0062Active);
				NctsAdditionalInfoValidationTestHelper.AssertWhenRuleActive(additionalDocument.CSI_ReferenceNumberInfo);

				ruleTestContext.DisableRule(x => x.IsRuleTR0062Active);
				NctsAdditionalInfoValidationTestHelper.AssertWhenRuleNotActive(additionalDocument.CSI_ReferenceNumberInfo);
			}
		}

		public void TestCSI_Description()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(additionalDocument.CSI_DescriptionInfo, RefCusCodeListAttributeTypes.Complement);
		}

		public void TestCSI_Description_NotAnyAttribute()
		{
			AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(additionalDocument.CSI_DescriptionInfo, RefCusCodeListAttributeTypes.Complement);
		}

		public void TestCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);
			Factory.Save();

			additionalDocument.CSI_Status = "NEW";
			AssertHasMessageErrorContaining(additionalDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered); //ValidationTestHelper conflicts with setting the CSI_Status to empty when CSI_Code is set to empty, so the ValidationTestHelper function will always fail here

			additionalDocument.CSI_Status = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(additionalDocument.CSI_CodeInfo);

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalDocument.CSI_CodeInfo, "BAD", "REF01", "The code you have selected is not in the list.");
		}

		public void TestCheckCSI_Code_WithMovementHeaderDestinationCountry_R3062_WhenActive()
		{
			const string messageError = "[R3062] For a House Consignment, Additional Information of Type=30600 is not valid when the Country of Destination (declared at Consignment level or Goods Item level) is from CL009 (Country Codes Common Transit) list.";

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			newFactory.Save();

			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			movementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsHeaderArrival = newFactory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeaderArrival = nctsHeaderArrival.ArrivalMovementHeader;
			var billArrival = nctsHeaderArrival.Bills.AddNew();
			var additionalDocumentArrival = billArrival.AdditionalDocuments.AddNew();
			movementHeaderArrival.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var validation = new NctsBillAdditionalDocumentValidationForTest(additionalDocument);
			validation.IsCodeEnabledExposed = true;

			CombineAssertions(() =>
			{
				using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(newFactory))
				{
					ruleTestContext.EnableRule(r => r.IsRuleR3062Active);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Latvia;
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocument.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
					AssertNoMessageError($"Destination Country is {movementHeader.BM_RL_NKDestinationPort}, not in CL009 List", additionalDocument.CSI_CodeInfo, messageError);
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					validation.ValidateCSI_Code();
					AssertNoMessageError("Not exist subType of additironal Document is INF", additionalDocument.CSI_CodeInfo, messageError);
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					validation.ValidateCSI_Code();
					AssertHasMessageError($"Destination Country is {movementHeader.BM_RL_NKDestinationPort}, in CL009 List, Code is 30600 and SubType is INF,", additionalDocument.CSI_CodeInfo, messageError);
					validation.IsCodeEnabledExposed = false;
					validation.ValidateCSI_Code();
					AssertNoMessageError("No Message error because the property IsCodeEnabled is false", additionalDocument.CSI_CodeInfo, messageError);
					validation.IsCodeEnabledExposed = true;
					additionalDocument.CSI_Code = "12345";
					AssertNoMessageError("Not exist code of additironal Document is 30600", additionalDocument.CSI_CodeInfo, messageError);

					movementHeaderArrival.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					additionalDocumentArrival.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocumentArrival.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
					AssertNoMessageError("No Message error because it is in arrival now", additionalDocumentArrival.CSI_CodeInfo, messageError);
				}
			});
		}

		public void TestCheckCSI_Code_WithGoodsItemDestinationCountry_R3062_WhenActive()
		{
			const string messageError = "[R3062] For a House Consignment, Additional Information of Type=30600 is not valid when the Country of Destination (declared at Consignment level or Goods Item level) is from CL009 (Country Codes Common Transit) list.";

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			newFactory.Save();

			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = additionalDocument.Parent.GoodsItems.AddNew();

			var nctsHeaderArrival = newFactory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			var billArrival = nctsHeaderArrival.Bills.AddNew();
			var additionalDocumentArrival = billArrival.AdditionalDocuments.AddNew();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItemArrival = additionalDocumentArrival.Parent.ArrivalGoodsItems.AddNew();

			var validation = new NctsBillAdditionalDocumentValidationForTest(additionalDocument);
			validation.IsCodeEnabledExposed = true;

			CombineAssertions(() =>
			{
				using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(newFactory))
				{
					ruleTestContext.EnableRule(r => r.IsRuleR3062Active);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Latvia;
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocument.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
					AssertNoMessageError($"Destination Country is {movementHeader.BM_RL_NKDestinationPort}, not in CL009 List.", additionalDocument.CSI_CodeInfo, messageError);
					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					validation.ValidateCSI_Code();
					AssertNoMessageError($"Destination Country is {movementHeader.BM_RL_NKDestinationPort}, in CL009 List, SubType is not INF.", additionalDocument.CSI_CodeInfo, messageError);
					additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					validation.ValidateCSI_Code();
					AssertHasMessageError($"Destination Country is {movementHeader.BM_RL_NKDestinationPort}, in CL009 List, Code is 30600 and SubType is INF.", additionalDocument.CSI_CodeInfo, messageError);
					validation.IsCodeEnabledExposed = false;
					validation.ValidateCSI_Code();
					AssertNoMessageError("No Message error because the property IsCodeEnabled is false", additionalDocument.CSI_CodeInfo, messageError);
					validation.IsCodeEnabledExposed = true;
					additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
					AssertNoMessageError("Not exist code of additironal Document is 30600", additionalDocument.CSI_CodeInfo, messageError);

					goodsItemArrival.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
					additionalDocumentArrival.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
					additionalDocumentArrival.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
					AssertNoMessageError("No Message error because it is in arrival now", additionalDocumentArrival.CSI_CodeInfo, messageError);
				}
			});
		}

		public void TestCheckCSI_Code_WithMovementHeaderDestinationCountry_R3062_WhenInactive()
		{
			const string messageError = "[R3062] For a House Consignment, Additional Information of Type=30600 is not valid when the Country of Destination (declared at Consignment level or Goods Item level) is from CL009 (Country Codes Common Transit) list.";

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			newFactory.Save();

			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(newFactory))
			{
				ruleTestContext.DisableRule(r => r.IsRuleR3062Active);
				movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
				AssertCollectionNotContains($"Expected notifications would not contain {messageError}", additionalDocument.CSI_CodeInfo.Notifications.Select(e => e.Message), x => x.Contains(messageError));
			}
		}

		public void TestCheckCSI_Code_WithGoodsItemDestinationCountry_R3062_WhenInactive()
		{
			const string messageError = "[R3062] For a House Consignment, Additional Information of Type=30600 is not valid when the Country of Destination (declared at Consignment level or Goods Item level) is from CL009 (Country Codes Common Transit) list.";

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			newFactory.Save();

			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = bill.GoodsItems.AddNew();

			using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(newFactory))
			{
				ruleTestContext.DisableRule(r => r.IsRuleR3062Active);
				goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument.CSI_Code = NctsConstants.AdditionalInfoCodes.InEXSWhereNegotiableBillsOfLadingAndUnknownConsigneeParticulars;
				AssertCollectionNotContains($"Expected notifications would not contain {messageError}", additionalDocument.CSI_CodeInfo.Notifications.Select(e => e.Message), x => x.Contains(messageError));
			}
		}

		public void TestCSI_SubType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalDocument.CSI_SubTypeInfo, "XX", AdditionalInfoSubTypeList.Codes.AdditionalInformation);
		}

		public void TestCSI_SubType_TR0031()
		{
			AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.AdditionalInformation, "TR0031", x => x.IsRuleTR0031Active);
		}

		public void TestCSI_SubType_TR0032()
		{
			AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.AdditionalReference, "TR0032", x => x.IsRuleTR0032Active);
		}

		public void TestCSI_SubType_TR0033()
		{
			AssertMaxCountForSubType(AdditionalInfoSubTypeList.Codes.TransportDocument, "TR0033", x => x.IsRuleTR0033Active);
		}

		public void TestCheckCSI_SubType_MessageErrorIfNotEntered()
		{
			const string expectedMessageError = "You have not entered a Kind of Document";
			var subTypeInfo = additionalDocument.CSI_SubTypeInfo;

			CombineAssertions("When NCTS5", () =>
			{
				movementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertNoMessageError("When Kind of Document is not empty", subTypeInfo, expectedMessageError);

				additionalDocument.CSI_SubType = ZString.Empty;
				AssertHasMessageError("When Kind of Document is empty", subTypeInfo, expectedMessageError);
			});
			CombineAssertions("When NCTS4", () =>
			{
				movementHeader.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				additionalDocument.CSI_SubType = ZString.Empty;
				AssertNoMessageError("When Kind of Document is empty", subTypeInfo, expectedMessageError);
			});
		}

		public void TestCheckCSI_SubType_RuleE1301()
		{
			const string expectedError = "[E1301] In transition period, which is now, Additional Information and Additional Reference must be empty";
			var info = additionalDocument.CSI_SubTypeInfo;

			using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(Factory).ClearCachedValidationDecider(additionalDocument))
			{
				using (TemporarilySetTransitionPeriod(true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					CombineAssertions("When In transition period, NCTS5, RuleE1301 Active", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleE1301Active);
						additionalDocument.CSI_SubType = ZString.Empty;
						AssertNoMessageError("'Doc.Kind' is empty", info, expectedError);
						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
						AssertNoMessageError("'Doc.Kind' is not [REF,INF]", info, expectedError);

						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						AssertHasMessageError("'Doc.Kind' is REF", info, expectedError);
						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertHasMessageError("'Doc.Kind' is INF", info, expectedError);
					});

					CombineAssertions("When In transition period, NCTS5, RuleE1301 Disable", () =>
					{
						ruleTestContext.DisableRule(x => x.IsRuleE1301Active);

						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						AssertNoMessageError("'Doc.Kind' is REF", info, expectedError);
						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("'Doc.Kind' is INF", info, expectedError);
					});

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					CombineAssertions("When In transition period, NCTS4, RuleE1301 Active", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleE1301Active);

						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						AssertNoMessageError("'Doc.Kind' is REF", info, expectedError);
						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("'Doc.Kind' is INF", info, expectedError);
					});
				}

				using (TemporarilySetTransitionPeriod(false))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					CombineAssertions("When not In transition period, NCTS5, RuleE1301 Active", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleE1301Active);

						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						AssertNoMessageError("'Doc.Kind' is REF", info, expectedError);
						additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						AssertNoMessageError("''Doc.Kind' is INF", info, expectedError);
					});
				}
			}
		}

		void AssertMaxCountForSubType(string subType, string ruleName, Expression<Func<INctsBillAdditionalDocumentPhase5ValidationDecider, bool>> rule)
		{
			using (var ruleTestContext = new NctsBillAdditionalValidationDeciderTestContext<INctsBillAdditionalDocumentPhase5ValidationDecider>(Factory))
			{
				bill.AdditionalDocuments.RemoveAndDeleteAll();
				Enumerable.Range(0, 98)
					.ToList()
					.ForEach(i => bill.AdditionalDocuments.AddNew().CSI_SubType = subType);

				CombineAssertions(() =>
				{
					ruleTestContext.EnableRule(rule);
					var additionalDoc99th = bill.AdditionalDocuments.AddNew();
					additionalDoc99th.CSI_SubType = subType;
					AssertNoMessageErrorContaining($"Additional Document of sub type {subType} #99", additionalDoc99th.CSI_SubTypeInfo, ruleName);

					var additionalDoc100th = bill.AdditionalDocuments.AddNew();
					additionalDoc100th.CSI_SubType = subType;
					AssertHasMessageErrorContaining($"Additional Document of sub type {subType} #100", additionalDoc100th.CSI_SubTypeInfo, ruleName);

					ruleTestContext.DisableRule(rule);
					additionalDoc100th.Validation.ValidateCSI_SubType();
					AssertNoMessageErrorContaining($"Rule [{ruleName}] is disabled", additionalDoc100th.CSI_SubTypeInfo, ruleName);
				});
			}
		}

		public void TestCheckCSI_Code_MessageErrorIfInvalidCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(additionalDocument.CSI_SubTypeInfo, "X", AdditionalInfoSubTypeList.Codes.TransportDocument);
		}

		public void TestCheckCSI_Status_Phase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			var billAdditionalDocument = bill.AdditionalDocuments.AddNew();
			billAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			ValidationTestHelper.AssertErrorIfInvalidCode(billAdditionalDocument.CSI_StatusInfo, "INV", NctsBillAdditionalDocumentStatusList.Codes.DEC);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = "INF";
		}

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(propertyInfo, RefCusCodeListTypes.Codes.Code_AI44N, RefCusCodeListLevelTypes.House, attributeName);
		}

		void AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(ZPropertyInfo propertyInfo, string attributeName)
		{
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(propertyInfo, RefCusCodeListTypes.Codes.Code_AI44N, RefCusCodeListLevelTypes.House, attributeName);
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

		NctsDepartureMovementHeader movementHeader;
		NctsBill bill;
		NctsBillAdditionalDocument additionalDocument;
		NctsHeader nctsHeader;

		class NctsBillAdditionalDocumentValidationForTest : NctsBillAdditionalDocumentValidation
		{
			public NctsBillAdditionalDocumentValidationForTest(NctsBillAdditionalDocument parent) : base(parent)
			{
			}

			public bool IsCodeEnabledExposed { get; set; }

			protected override bool IsCodeEnabled => IsCodeEnabledExposed;
		}
	}
}
