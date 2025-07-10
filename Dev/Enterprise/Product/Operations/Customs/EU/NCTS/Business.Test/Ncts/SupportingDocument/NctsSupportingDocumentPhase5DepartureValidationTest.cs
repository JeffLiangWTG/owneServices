using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase5DepartureValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ItemNumber()
		{
			var supportingDocuments = CreateLowerLevelSupportingDocuments();
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocumentHeader.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Header, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocuments.supportingDocumentHouse.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.House, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocuments.supportingDocumentItem.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCheckCSI_ItemNumber_NotAnyAttribute()
		{
			var supportingDocuments = CreateLowerLevelSupportingDocuments();
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocumentHeader.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Header, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocuments.supportingDocumentHouse.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.House, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocuments.supportingDocumentItem.CSI_ItemNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCSI_Code()
		{
			var targetInfo = supportingDocumentHeader.CSI_CodeInfo;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC44N, "CusCodeTypeDC44N");
			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, Business.UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
			Factory.Save();

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "BAD", "REF01");
			});
		}

		public void TestCheckCSI_Code_RuleE1301_Header()
		{
			AssertCheckCSICodeRuleE1301(supportingDocumentHeader);
		}

		public void TestCheckCSI_Code_RuleE1301_Bills()
		{
			var bill = nctsHeader.Bills.AddNew();
			var supportingDocumentBill = bill.SupportingDocuments.AddNew();
			AssertCheckCSICodeRuleE1301(supportingDocumentBill);
		}

		public void TestCheckCSI_ReferenceNumberEmptyWithRule()
		{
			var warningMessage = "[G0321] You have not entered a Reference Number - '0' will be used.";
			CombineAssertions(() =>
			{
				using var validationDeciderTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory);
				validationDeciderTestContext.EnableRule(decider => decider.IsRuleG0321Active);
				supportingDocumentHeader.CSI_Code = "aa";
				supportingDocumentHeader.Validation.ValidateCSI_ReferenceNumber();
				AssertHasWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is not empty", supportingDocumentHeader.CSI_ReferenceNumberInfo, warningMessage);

				supportingDocumentHeader.CSI_Code = ZString.Empty;
				supportingDocumentHeader.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarning("The rule is active and CSI_ReferenceNumber is empty and CSI_Code is empty", supportingDocumentHeader.CSI_ReferenceNumberInfo, warningMessage);

				supportingDocumentHeader.CSI_Code = "aa";
				supportingDocumentHeader.CSI_ReferenceNumber = "ss";
				AssertNoWarning("The CSI_ReferenceNumber is not empty", supportingDocumentHeader.CSI_ReferenceNumberInfo, warningMessage);

				validationDeciderTestContext.DisableRule(decider => decider.IsRuleG0321Active);
				supportingDocumentHeader.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive", supportingDocumentHeader.CSI_ReferenceNumberInfo, warningMessage);

				supportingDocumentHeader.CSI_Code = "aa";
				supportingDocumentHeader.CSI_ReferenceNumber = ZString.Empty;
				AssertNoWarning("The rule is inactive, CSI_code has value", supportingDocumentHeader.CSI_ReferenceNumberInfo, warningMessage);
			});
		}

		public void TestCheckCSI_ReferenceNumber2_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLength(supportingDocumentHeader.CSI_ReferenceNumber2Info, 35);
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(supportingDocumentHeader.CSI_ReferenceNumber2Info, 26, NctsConstants.ValidationRuleMessagePrefixes.E1117);
			});
		}

		public void TestValidateCSI_ReferenceNumber_Mandatory()
		{
			using var ruleTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory);
			var supportingDocuments = CreateLowerLevelSupportingDocuments();
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				supportingDocumentHeader.CSI_ReferenceNumberInfo,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				RefCusCodeListLevelTypes.Header,
				UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference,
				() => ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active),
				() => ruleTestContext.DisableRule(decider => decider.IsRuleG0321Active));
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				supportingDocuments.supportingDocumentHouse.CSI_ReferenceNumberInfo,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				RefCusCodeListLevelTypes.House,
				UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference,
				() => ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active),
				() => ruleTestContext.DisableRule(decider => decider.IsRuleG0321Active));
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueYAndRule(
				supportingDocuments.supportingDocumentItem.CSI_ReferenceNumberInfo,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
				RefCusCodeListLevelTypes.Item,
				UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference,
				() => ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active),
				() => ruleTestContext.DisableRule(decider => decider.IsRuleG0321Active));
		}

		public void TestValidateCSI_ReferenceNumber_Mandatory_NotAnyAttribute()
		{
			var supportingDocuments = CreateLowerLevelSupportingDocuments();
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocumentHeader.CSI_ReferenceNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Header, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocuments.supportingDocumentHouse.CSI_ReferenceNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.House, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocuments.supportingDocumentItem.CSI_ReferenceNumberInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.Item, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference);
		}

		public void TestValidateCSI_ReferenceNumberIsValid()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			var nctsCodeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC010;
			helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "NC010");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, nctsCodeType, "IE", "123456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, nctsCodeType, "DE", "123456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (var ruleTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(decider => decider.IsRuleNR0006Active);
				var header = Factory.New<NctsHeader>();
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var supportingDocument_Bill = bill.SupportingDocuments.AddNew();
				var supportingDocument = bill.GoodsItems.AddNew().SupportingDocuments.AddNew();

				string invalidARCError = @"[NR0006] Structure does not correspond to an EMCS Administrative Reference Code (ARC). Please enter the ARC in the following format with only numbers and uppercase letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• sixteen alphanumeric characters for unique identification and
• one number check digit";
				string invalidCountryCodeError = "Please enter a valid country/region code.";
				string invalidCheckDigitError = "ARC does not have a valid check (last) digit. The check digit should be {0}";
				CombineAssertions(() =>
				{
					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					supportingDocument.CSI_ReferenceNumber = "123456789012345678901";
					supportingDocument_Bill.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					supportingDocument_Bill.CSI_ReferenceNumber = "123456789012345678901";
					AssertHasMessageErrorContaining("invalid format", supportingDocument.CSI_ReferenceNumberInfo, invalidARCError);
					AssertHasMessageErrorContaining("invalid format", supportingDocument_Bill.CSI_ReferenceNumberInfo, invalidARCError);

					supportingDocument.CSI_ReferenceNumber = "12SB56789012345678901";
					AssertHasMessageErrorContaining("invalid country", supportingDocument.CSI_ReferenceNumberInfo, invalidCountryCodeError);
					supportingDocument_Bill.CSI_ReferenceNumber = "12SB56789012345678901";
					AssertHasMessageErrorContaining("invalid country", supportingDocument_Bill.CSI_ReferenceNumberInfo, invalidCountryCodeError);

					supportingDocument.CSI_ReferenceNumber = "12IE56789012345678901";
					AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, invalidCountryCodeError);
					supportingDocument_Bill.CSI_ReferenceNumber = "12IE56789012345678901";
					AssertNoMessageErrorContaining(supportingDocument_Bill.CSI_ReferenceNumberInfo, invalidCountryCodeError);

					supportingDocument.CSI_ReferenceNumber = "22DE12340123456789012";
					AssertHasMessageErrorContaining("error check digit", supportingDocument.CSI_ReferenceNumberInfo, string.Format(invalidCheckDigitError, "1"));
					supportingDocument_Bill.CSI_ReferenceNumber = "22DE12340123456789012";
					AssertHasMessageErrorContaining("error check digit", supportingDocument_Bill.CSI_ReferenceNumberInfo, string.Format(invalidCheckDigitError, "1"));

					supportingDocument.CSI_ReferenceNumber = "22DE12340123456789011";
					AssertNoMessageErrors(supportingDocument.CSI_ReferenceNumberInfo);
					supportingDocument_Bill.CSI_ReferenceNumber = "22DE12340123456789011";
					AssertNoMessageErrors(supportingDocument_Bill.CSI_ReferenceNumberInfo);

					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C658;
					supportingDocument.CSI_ReferenceNumber = "NBNBNBNBNBNBNBNB";
					AssertNoMessageErrors(supportingDocument.CSI_ReferenceNumberInfo);
					supportingDocument_Bill.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C658;
					supportingDocument_Bill.CSI_ReferenceNumber = "NBNBNBNBNBNBNBNB";
					AssertNoMessageErrors(supportingDocument_Bill.CSI_ReferenceNumberInfo);

					ruleTestContext.DisableRule(decider => decider.IsRuleNR0006Active);
					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					supportingDocument.CSI_ReferenceNumber = "123456789012345678901";
					AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, invalidARCError);

					supportingDocument_Bill.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					supportingDocument_Bill.CSI_ReferenceNumber = "123456789012345678901";
					AssertNoMessageErrorContaining(supportingDocument_Bill.CSI_ReferenceNumberInfo, invalidARCError);
				});
			}
		}

		public void TestCheckCSI_ReferenceNumber_RP30()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var movementHeaderSupportingDocument = header.MovementHeader.SupportingDocuments.AddNew();

			var bill = header.Bills.AddNew();
			var billSupportingDocument =  bill.SupportingDocuments.AddNew();
			var goodsSupportingDocument = bill.GoodsItems.AddNew().SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertReferenceNumberIsMandatory(movementHeaderSupportingDocument, "Movement Header");
				AssertReferenceNumberIsMandatory(billSupportingDocument, "House Consignment");
				AssertReferenceNumberIsMandatory(goodsSupportingDocument, "Goods Item");
			});

			void AssertReferenceNumberIsMandatory(NctsSupportingDocument supportingDocument, string level)
			{
				const string errorMessage = "[RP30] You have not entered a Certificate/Reference number.";
				using var validationDeciderTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory);
				validationDeciderTestContext.EnableRule(decider => decider.IsRuleRP30Active);

				supportingDocument.CSI_Code = "5678";
				supportingDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageError($"{level}: reference number is empty but document type <> 7P17.", supportingDocument.CSI_ReferenceNumberInfo, errorMessage);

				supportingDocument.CSI_Code = NctsTypeOfSupportingDocument.Codes._7P17;
				supportingDocument.CSI_ReferenceNumber = "12345";
				AssertNoMessageError($"{level}: document type is 7P17 but reference number is not empty.", supportingDocument.CSI_ReferenceNumberInfo, errorMessage);

				supportingDocument.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageError($"{level}: document type is 7P17 and reference number is empty.", supportingDocument.CSI_ReferenceNumberInfo, errorMessage);

				validationDeciderTestContext.DisableRule(decider => decider.IsRuleRP30Active);
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError($"{level}: the rule is inactive.", supportingDocument.CSI_ReferenceNumberInfo, errorMessage);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			supportingDocumentHeader = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		}

		(NctsSupportingDocument supportingDocumentHouse, NctsSupportingDocument supportingDocumentItem) CreateLowerLevelSupportingDocuments()
		{
			var bill = nctsHeader.Bills.AddNew();
			var supportingDocumentHouse = bill.SupportingDocuments.AddNew();
			var supportingDocumentItem = bill.GoodsItems.AddNew().SupportingDocuments.AddNew();
			return (supportingDocumentHouse, supportingDocumentItem);
		}

		void AssertCheckCSICodeRuleE1301(NctsSupportingDocument supportingDocument)
		{
			const string error = "[E1301] In transition period, which is now, Supporting Documents must be empty";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			using var ruleTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory);
			using (TemporarilySetTransitionPeriod(true))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleE1301Active);
				CombineAssertions("When TP: ON, NCTS5, RuleE1301: Active", () =>
				{
					supportingDocument.CSI_Code = ZString.Empty;
					AssertNoMessageError("CSI_Code: Empty", supportingDocument.CSI_CodeInfo, error);

					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					AssertHasMessageError("CSI_Code: C651", supportingDocument.CSI_CodeInfo, error);
				});

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				CombineAssertions("When TP: ON, NCTS4, RuleE1301: Active", () =>
				{
					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					AssertNoMessageError("CSI_Code: C651", supportingDocument.CSI_CodeInfo, error);
				});

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				ruleTestContext.DisableRule(rule => rule.IsRuleE1301Active);
				CombineAssertions("When TP: ON, NCTS5, RuleE1301: Inactive", () =>
				{
					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					AssertNoMessageError("CSI_Code: C651", supportingDocument.CSI_CodeInfo, error);
				});
			}

			using (TemporarilySetTransitionPeriod(false))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleE1301Active);
				CombineAssertions("When TP: OFF, NCTS5, RuleE1301: Active", () =>
				{
					supportingDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.C651;
					AssertNoMessageError("CSI_Code: C651", supportingDocument.CSI_CodeInfo, error);
				});
			}
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

		NctsHeader nctsHeader;
		NctsSupportingDocument supportingDocumentHeader;
	}
}
