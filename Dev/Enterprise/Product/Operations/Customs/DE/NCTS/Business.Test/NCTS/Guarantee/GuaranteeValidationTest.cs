using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class GuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondType()
		{
			const string message = "Guarantee Types '8', 'B' or 'R' only allow one Guarantee to be entered per declaration.";
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.R;
				AssertNoMessageError("Single Guarantee has type in ('8', 'B', 'R')", guarantee.PW_BondTypeInfo, message);

				var guarantee2 = header.MovementHeader.Guarantees.AddNew();
				guarantee2.PW_BondType = NctsGuaranteeTypeList.Codes._3;
				AssertHasMessageError("Multiple Guarantees have one of type 'R'", guarantee2.PW_BondTypeInfo, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._8;
				AssertHasMessageError("Multiple Guarantees have one of type '8'", guarantee.PW_BondTypeInfo, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.B;
				AssertHasMessageError("Multiple Guarantees have one of type 'B'", guarantee.PW_BondTypeInfo, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._3;
				AssertNoMessageError("Multiple Guarantees don't have one of type in ('8', 'B', 'R')", guarantee.PW_BondTypeInfo, message);
			});
		}

		public void TestCheckPW_BondType_Empty()
		{
			guarantee.PW_BondType = ZString.Empty;
			AssertHasMessageErrorContaining($"When PW_BondType is empty and Departure", guarantee.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var guaranteeArrival = (Guarantee)header.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
			guaranteeArrival.PW_BondType = ZString.Empty;

			AssertNoMessageErrorContaining($"When PW_BondType is empty but Arrival", guaranteeArrival.PW_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPW_BondNumber_Mandatory()
		{
			string message = MandatoryValidation.YouHaveNotEntered;
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._0;
				guarantee.Validation.ValidatePW_BondNumber();
				AssertHasMessageErrorContaining("PW_BondNumberInfo_ReadOnly is false and PW_BondNumber is empty", guarantee.PW_BondNumberInfo, message);

				guarantee.PW_BondNumber = "AA";
				AssertNoMessageErrorContaining("PW_BondNumberInfo_ReadOnly is false and PW_BondNumber isn't empty", guarantee.PW_BondNumberInfo, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.B;
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageErrorContaining("PW_BondNumberInfo_ReadOnly is true", guarantee.PW_BondNumberInfo, message);
			});
		}

		public void TestCheckPW_BondNumber_ListValidation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CreateGuaranteeHeader(EUGuaranteeTypeList.Codes.TRA, "DETRA01", NctsGuaranteeTypeList.Codes._0, Core.Constants.CountryCodes.Germany);
			CreateGuaranteeHeader(EUGuaranteeTypeList.Codes.TRA, "DETRA02", GuaranteeSubTypeList.Codes._0, Core.Constants.CountryCodes.Germany);
			Factory.Save();

			string message = ListValidation.InvalidCodeMessageError.ToString();
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._0;
				guarantee.PW_BondNumber = "DETRA01";
				AssertNoMessageErrorContaining("IsDropdownForReferenceNumberAndCode is true and PW_BondNumber is valid", guarantee.PW_BondNumberInfo, message);

				guarantee.PW_BondNumber = "XX";
				AssertHasMessageErrorContaining("IsDropdownForReferenceNumberAndCode is true and PW_BondNumber is invalid", guarantee.PW_BondNumberInfo, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._4;
				guarantee.PW_BondNumber = "XX";
				AssertNoMessageErrorContaining("IsDropdownForReferenceNumberAndCode is false", guarantee.PW_BondNumberInfo, message);
			});

			CusGuaranteeHeader CreateGuaranteeHeader(ZString type, ZString number, ZString subType, ZString countryCode)
			{
				var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				result.CPH_Type = type;
				result.CPH_Number = number;
				result.CPH_OH_PermitHolder = orgHeader.PK;
				result.CPH_RN_NKCountryCode = countryCode;
				result.CPH_SubType = subType;
				return result;
			}
		}

		public void TestCheckPW_BondNumber_Length()
		{
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._4;
				guarantee.PW_BondNumber = "DE00001";
				AssertHasMessageErrorContaining("Length isn't 24", guarantee.PW_BondNumberInfo, "24 characters should be entered.");

				guarantee.PW_BondNumber = "01234567890123456789ABCD";
				AssertNoMessageErrorContaining("Length is 24", guarantee.PW_BondNumberInfo, "24 characters should be entered.");
			});
		}

		public void TestCheckPW_BondNumber2()
		{
			string message = MandatoryValidation.YouHaveNotEntered;
			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._8;
				guarantee.Validation.ValidatePW_BondNumber2();
				AssertHasMessageErrorContaining("PW_BondNumber2_ReadOnly is false and PW_BondNumber2 is empty", guarantee.PW_BondNumber2Info, message);

				guarantee.PW_BondNumber2 = "AA";
				AssertNoMessageErrorContaining("PW_BondNumber2_ReadOnly is false and PW_BondNumber2 isn't empty", guarantee.PW_BondNumber2Info, message);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes.B;
				guarantee.PW_BondNumber2 = ZString.Empty;
				AssertNoMessageErrorContaining("PW_BondNumber2_ReadOnly is true", guarantee.PW_BondNumber2Info, message);
			});
		}

		public void TestCheckPW_Password_ListValidation()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_Number = "DETRA01";
			guaranteeHeader.CPH_SubType = GuaranteeSubTypeList.Codes._1;

			guarantee.PW_BondType = GuaranteeSubTypeList.Codes._1;
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;

			var guaranteeRule1 = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule1.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule1.CPR_ValueFrom = "123";
			guaranteeRule1.CPR_ValueTo = "abc";

			var guaranteeRule2 = guaranteeHeader.AdditionalAccessCodes.AddNew();
			guaranteeRule2.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule2.CPR_ValueFrom = "456";
			guaranteeRule2.CPR_ValueTo = "def";

			Factory.Save();

			string message = "Guarantee Access Code is not included in master data of selected Guarantee.";
			CombineAssertions(() =>
			{
				guarantee.PW_Password = "777";
				guarantee.Validation.ValidatePW_Password();
				AssertHasWarningContaining("Code is not in the list", guarantee.PW_PasswordInfo, message);

				guarantee.PW_Password = "123";
				guarantee.Validation.ValidatePW_Password();
				AssertNoWarningContaining("Code is in the list", guarantee.PW_PasswordInfo, message);
			});
		}

		public void TestCheckPW_RX_NKCurrency()
		{
			const string message = "[TR0089] The current Exchange Rate is missing in CW1 for Currency of Guarantee: {0}. Please maintain the current Rate in Maintain>Reference Files>Exchange Rates.";

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "SSS";

			CombineAssertions(() =>
			{
				guarantee.PW_RX_NKCurrency = "YSN";

				foreach (var status in new ZString[] { NctsMessageStatusList.Codes.DepartureDeclarationNotSent, NctsMessageStatusList.Codes.Rejected, LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Failed, LogicalStatusList.Codes.Error })
				{
					header.EffectiveMessageStatus = status;
					guarantee.Validation.ValidatePW_RX_NKCurrency();
					AssertHasMessageError($"EffectiveMessageStatus is {header.EffectiveMessageStatus} and Specific ExchangeRate not exist, has message error", guarantee.PW_RX_NKCurrencyInfo, string.Format(message, "YSN"));
				}

				exchangeRate.RE_RX_NKExCurrency = "YSN";
				header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
				guarantee.Validation.ValidatePW_RX_NKCurrency();
				AssertNoMessageError("Specific ExchangeRate exists, no message error", guarantee.PW_RX_NKCurrencyInfo, string.Format(message, "YSN"));

				header.EffectiveMessageStatus = "XXX";
				guarantee.Validation.ValidatePW_RX_NKCurrency();
				AssertNoMessageError($"EffectiveMessageStatus is {header.EffectiveMessageStatus} and Specific ExchangeRate exists, no message error", guarantee.PW_RX_NKCurrencyInfo, string.Format(message, "YSN"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			guarantee = header.MovementHeader.Guarantees.AddNew();
		}
		NctsHeader header;
		Guarantee guarantee;
	}
}
