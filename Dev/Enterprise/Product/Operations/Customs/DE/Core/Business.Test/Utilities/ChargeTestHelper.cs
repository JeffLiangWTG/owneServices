using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using static CargoWise.EntityFramework.Testing.TestCaseWithFactory;
using static NUnit.Framework.Assertion;
using ChargeExchangeRateTypeList = Enterprise.Customs.Common.ChargeExchangeRateTypeList;
using DETestHelper = Enterprise.Customs.DE.Business.Testing.TestHelper;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ChargeValidationHelperTest
	{
		internal static void TestCheckJ7_RX_NKCurrency(CommonNonApportionedCharge charge1)
		{
			const string message = "Currency of Charge Codes '010' and '014' must be equal.";

			var chargeHolder = (IChargeHolder)charge1.Parent;
			var declaration = charge1.Parent.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._010;
			var charge2 = chargeHolder.Charges.AddNew();
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;

			charge1.J7_RX_NKCurrency = "AUD";
			charge2.J7_RX_NKCurrency = "AUD";
			charge1.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError("010: AUD/AUD", charge1.J7_RX_NKCurrencyInfo, message);
			AssertNoMessageError("014: AUD/AUD", charge2.J7_RX_NKCurrencyInfo, message);

			charge2.J7_RX_NKCurrency = "USD";
			charge1.Validation.ValidateJ7_RX_NKCurrency();
			AssertHasMessageError("010: AUD/USD", charge1.J7_RX_NKCurrencyInfo, message);
			AssertHasMessageError("014: AUD/USD", charge2.J7_RX_NKCurrencyInfo, message);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			charge1.Validation.ValidateJ7_RX_NKCurrency();
			charge2.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError("010: Road", charge1.J7_RX_NKCurrencyInfo, message);
			AssertNoMessageError("014: Road", charge2.J7_RX_NKCurrencyInfo, message);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			charge1.Validation.ValidateJ7_RX_NKCurrency();
			charge2.Validation.ValidateJ7_RX_NKCurrency();
			AssertNoMessageError("010: Export", charge1.J7_RX_NKCurrencyInfo, message);
			AssertNoMessageError("014: Export", charge2.J7_RX_NKCurrencyInfo, message);
		}

		internal static void TestCheckIsJ7_ExchangeRateUserEnterable<T>(T charge1, Func<T, ZPropertyInfo> getIsJ7_ExchangeRateIATAInfo) where T : CommonNonApportionedCharge
		{
			TestCheckExchangeRateType(charge1, charge => charge.IsJ7_ExchangeRateUserEnterableInfo, getIsJ7_ExchangeRateIATAInfo, "Fixed Rate Flag of Charge Codes '010' and '014' must be equal.");
		}

		internal static void TestCheckIsJ7_ExchangeRateIATA<T>(T charge1, Func<T, ZPropertyInfo> getIsJ7_ExchangeRateIATAInfo) where T : CommonNonApportionedCharge
		{
			TestCheckExchangeRateType(charge1, getIsJ7_ExchangeRateIATAInfo, charge => charge.IsJ7_ExchangeRateUserEnterableInfo, "IATA Flag of Charge Codes '010' and '014' must be equal.");
		}

		static void TestCheckExchangeRateType<T>(T charge1, Func<T, ZPropertyInfo> getTargetPropertyInfo, Func<T, ZPropertyInfo> getOtherPropertyInfo, string message)
			where T : CommonNonApportionedCharge
		{
			var chargeHolder = (IChargeHolder)charge1.Parent;
			var declaration = charge1.Parent.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._010;
			var charge2 = chargeHolder.Charges.AddNew() as T;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;

			var thisPropertyInfo1 = getTargetPropertyInfo(charge1);
			var thisPropertyInfo2 = getTargetPropertyInfo(charge2);
			var otherPropertyInfo1 = getOtherPropertyInfo(charge1);

			thisPropertyInfo2.Value = ZBool.False;
			thisPropertyInfo1.Value = ZBool.False;
			charge1.Validation.ValidateAll();
			AssertNoMessageError(GetAssertionMessage(1), thisPropertyInfo1, message);

			thisPropertyInfo2.Value = ZBool.True;
			thisPropertyInfo1.Value = ZBool.False;
			charge1.Validation.ValidateAll();
			AssertHasMessageError(GetAssertionMessage(2), thisPropertyInfo1, message);

			thisPropertyInfo2.Value = ZBool.False;
			thisPropertyInfo1.Value = ZBool.True;
			charge1.Validation.ValidateAll();
			AssertHasMessageError(GetAssertionMessage(3), thisPropertyInfo1, message);

			thisPropertyInfo2.Value = ZBool.True;
			thisPropertyInfo1.Value = ZBool.True;
			charge1.Validation.ValidateAll();
			AssertNoMessageError(GetAssertionMessage(4), thisPropertyInfo1, message);

			thisPropertyInfo2.Value = ZBool.False;
			thisPropertyInfo1.Value = ZBool.True;
			otherPropertyInfo1.Value = ZBool.True;
			charge1.Validation.ValidateAll();
			AssertNoMessageError(GetAssertionMessage(5), thisPropertyInfo1, message);

			string GetAssertionMessage(int n) => $"#{n} This charge: {charge1.J7_ChargeType}/{charge1.J7_ExchangeRateType}; other charge: {charge2.J7_ChargeType}/{charge2.J7_ExchangeRateType}";
		}

		internal static void TestCheckJ7_ExchangeRateDate(CommonNonApportionedCharge charge1)
		{
			const string message = "Exchange Rate Date of Charge Codes '010' and '014' must be equal.";

			var chargeHolder = (IChargeHolder)charge1.Parent;
			var declaration = charge1.Parent.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge1.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			var charge2 = chargeHolder.Charges.AddNew();
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;

			charge1.J7_ExchangeRateDate = new ZDate(2020, 1, 1);
			charge2.J7_ExchangeRateDate = new ZDate(2020, 1, 1);
			charge1.Validation.ValidateJ7_ExchangeRateDate();
			AssertNoMessageError(GetAssertionMessage(charge1, charge2), charge1.J7_ExchangeRateDateInfo, message);
			AssertNoMessageError(GetAssertionMessage(charge2, charge1), charge2.J7_ExchangeRateDateInfo, message);

			charge1.J7_ExchangeRateDate = new ZDate(2020, 1, 1);
			charge2.J7_ExchangeRateDate = new ZDate(2020, 1, 2);
			charge1.Validation.ValidateJ7_ExchangeRateDate();
			AssertHasMessageError(GetAssertionMessage(charge1, charge2), charge1.J7_ExchangeRateDateInfo, message);
			AssertHasMessageError(GetAssertionMessage(charge2, charge1), charge2.J7_ExchangeRateDateInfo, message);

			charge1.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
			charge2.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
			charge1.Validation.ValidateJ7_ExchangeRateDate();
			charge2.Validation.ValidateJ7_ExchangeRateDate();
			AssertNoMessageError(GetAssertionMessage(charge1, charge2), charge1.J7_ExchangeRateDateInfo, message);
			AssertNoMessageError(GetAssertionMessage(charge2, charge1), charge2.J7_ExchangeRateDateInfo, message);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			charge1.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			charge2.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			charge1.Validation.ValidateJ7_ExchangeRateDate();
			charge2.Validation.ValidateJ7_ExchangeRateDate();
			AssertNoMessageError("Road " + GetAssertionMessage(charge1, charge2), charge1.J7_ExchangeRateDateInfo, message);
			AssertNoMessageError("Road " + GetAssertionMessage(charge2, charge1), charge2.J7_ExchangeRateDateInfo, message);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			charge1.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			charge2.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			charge1.Validation.ValidateJ7_ExchangeRateDate();
			charge2.Validation.ValidateJ7_ExchangeRateDate();
			AssertNoMessageError("Export " + GetAssertionMessage(charge1, charge2), charge1.J7_ExchangeRateDateInfo, message);
			AssertNoMessageError("Export " + GetAssertionMessage(charge2, charge1), charge2.J7_ExchangeRateDateInfo, message);

			string GetAssertionMessage(JobComInvCharge chargeA, JobComInvCharge chargeB) => $"This charge: {chargeA.J7_ChargeType}/{chargeA.J7_ExchangeRateType}/{chargeA.J7_ExchangeRateDate}; other charge: {chargeB.J7_ChargeType}/{chargeB.J7_ExchangeRateType}/{chargeB.J7_ExchangeRateDate}";
		}

		internal static void TestValidateForRowNotificationJ_ChargeTypes_010_014(CommonNonApportionedCharge charge1)
		{
			const string message014 = "Air Freight Costs incomplete! You have not entered a Charge Code '014' - Or use 'Calculate Freight' Button on Invoice Header Level.";
			const string message010 = "Air Freight Costs incomplete! You have not entered a Charge Code '010' - Or use 'Calculate Freight' Button on Invoice Header Level.";

			var chargeHolder = (IChargeHolder)charge1.Parent;
			var declaration = charge1.Parent.JobDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_IATALoadPort = "BER";

			var charge2 = chargeHolder.Charges.AddNew();

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._019;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.Validation.ValidateAll();
			AssertHasRowMessageError(GetAssertionMessage(), charge2, message014);
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message010);

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message014);
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message010);

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._019;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.Validation.ValidateAll();
			AssertHasRowMessageError(GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message014);

			charge1.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError(GetAssertionMessage(), charge2, message014);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			charge1.J7_ChargeType = ImportChargeCodeList.Codes._019;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"Not AIR " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"Not AIR " + GetAssertionMessage(), charge2, message014);
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"Not AIR " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"Not AIR " + GetAssertionMessage(), charge2, message014);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_IATALoadPort = ZString.Empty;
			charge1.J7_ChargeType = ImportChargeCodeList.Codes._019;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"No LoadPort " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"No LoadPort " + GetAssertionMessage(), charge2, message014);
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"No LoadPort " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"No LoadPort " + GetAssertionMessage(), charge2, message014);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_IATALoadPort = "BER";
			charge1.J7_ChargeType = ImportChargeCodeList.Codes._019;
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"Export " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"Export " + GetAssertionMessage(), charge2, message014);
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.Validation.ValidateAll();
			AssertNoRowMessageError($"Export " + GetAssertionMessage(), charge2, message010);
			AssertNoRowMessageError($"Export " + GetAssertionMessage(), charge2, message014);

			string GetAssertionMessage() => $"This charge: {charge2.J7_ChargeType}; other charge: {charge1.J7_ChargeType}";
		}

		internal static void TestJ7_ExchangeRate_IATARateUsed(CommonNonApportionedCharge charge, ZPropertyInfo isJ7_ExchangeRateIATAInfo)
		{
			DETestHelper.SetExchangeRates(charge.Factory);

			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			isJ7_ExchangeRateIATAInfo.Value = ZBool.True;
			AssertEquals("IAT", 0.8m, charge.J7_ExchangeRate);
			isJ7_ExchangeRateIATAInfo.Value = ZBool.False;
			AssertEquals("CUS", 0.9m, charge.J7_ExchangeRate);
		}
	}
}
