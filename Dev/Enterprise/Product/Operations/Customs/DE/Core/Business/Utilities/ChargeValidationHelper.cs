using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using ChargeExchangeRateTypeList = Enterprise.Customs.Common.ChargeExchangeRateTypeList;

namespace Enterprise.Customs.DE.Business
{
	public static class ChargeValidationHelper
	{
		internal static void CheckJ7_RX_NKCurrency_010_014_HaveSame(CommonNonApportionedCharge charge)
		{
			var declaration = charge.Parent?.JobDeclaration;
			if (declaration != null && declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Import && declaration.JE_TransportMode == Core.Constants.TransportModes.Air)
			{
				switch (charge.J7_ChargeType)
				{
					case ImportChargeCodeList.Codes._010:
						CheckOtherCurrencyIsEqual(ImportChargeCodeList.Codes._014);
						break;
					case ImportChargeCodeList.Codes._014:
						CheckOtherCurrencyIsEqual(ImportChargeCodeList.Codes._010);
						break;
				}
				void CheckOtherCurrencyIsEqual(string otherChargeType)
				{
					var chargeHolder = (IChargeHolder)charge.Parent;
					var charges = chargeHolder.Charges.Cast<CommonNonApportionedCharge>();
					var otherCurrency = charges.Where(x => x.J7_ChargeType == otherChargeType).Select(x => x.J7_RX_NKCurrency).FirstOrDefault();
					if (!otherCurrency.IsEmpty && otherCurrency != charge.J7_RX_NKCurrency)
					{
						charge.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString("7FA4C8F8-7611-4D32-9370-544043643992", "Currency of Charge Codes '010' and '014' must be equal."));
					}
				}
			}
		}

		internal static void CheckIsJ7_ExchangeRateUserEnterable_010_014<T>(T charge) where T : CommonNonApportionedCharge
		{
			CheckJ7_ExchangeRateType_010_014_HaveSame(charge, x => x.IsJ7_ExchangeRateUserEnterableInfo, () => Res.GetString("0D89E56F-721B-4B0A-8A3A-27FE07C9ADD1", "Fixed Rate Flag of Charge Codes '010' and '014' must be equal."));
		}

		internal static void CheckIsJ7_ExchangeRateIATA_010_014<T>(T charge, Func<T, ZPropertyInfo> getTargetPropertInfo) where T : CommonNonApportionedCharge
		{
			CheckJ7_ExchangeRateType_010_014_HaveSame(charge, getTargetPropertInfo, () => Res.GetString("12C82699-6392-4E91-A3B6-CCA2EE4ABD14", "IATA Flag of Charge Codes '010' and '014' must be equal."));
		}

		static void CheckJ7_ExchangeRateType_010_014_HaveSame<T>(T charge, Func<T, ZPropertyInfo> getTargetPropertInfo, Func<string> getMessage) where T : CommonNonApportionedCharge
		{
			switch (charge.J7_ChargeType)
			{
				case ImportChargeCodeList.Codes._010:
					CheckOtherExchangeRateTypeIsSame(ImportChargeCodeList.Codes._014);
					break;
				case ImportChargeCodeList.Codes._014:
					CheckOtherExchangeRateTypeIsSame(ImportChargeCodeList.Codes._010);
					break;
			}
			void CheckOtherExchangeRateTypeIsSame(string otherChargeType)
			{
				var declaration = charge.Parent?.JobDeclaration;
				if (declaration != null && declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Import && declaration.JE_TransportMode == Core.Constants.TransportModes.Air)
				{
					var chargeHolder = (IChargeHolder)charge.Parent;
					var charges = chargeHolder.Charges.Cast<CommonNonApportionedCharge>();
					var otherCharge = charges.FirstOrDefault(x => x.J7_ChargeType == otherChargeType) as T;
					if (otherCharge != null)
					{
						var thisTargetPropertyInfo = getTargetPropertInfo(charge);
						var otherTargetPropertyInfo = getTargetPropertInfo(otherCharge);
						if ((ZBool)thisTargetPropertyInfo.Value != (ZBool)otherTargetPropertyInfo.Value)
						{
							thisTargetPropertyInfo.AddMessageError(getMessage());
						}
					}
				}
			}
		}

		internal static void CheckJ7_ExchangeRateDate_010_014_HaveSame(CommonNonApportionedCharge charge)
		{
			if (charge.J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate)
			{
				switch (charge.J7_ChargeType)
				{
					case ImportChargeCodeList.Codes._010:
						CheckOtherExchangeRateDateIsSame(ImportChargeCodeList.Codes._014);
						break;
					case ImportChargeCodeList.Codes._014:
						CheckOtherExchangeRateDateIsSame(ImportChargeCodeList.Codes._010);
						break;
				}
			}

			void CheckOtherExchangeRateDateIsSame(string otherChargeType)
			{
				var declaration = charge.Parent?.JobDeclaration;
				if (declaration != null && declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Import && declaration.JE_TransportMode == Core.Constants.TransportModes.Air)
				{
					var chargeHolder = (IChargeHolder)charge.Parent;
					var charges = chargeHolder.Charges.Cast<CommonNonApportionedCharge>();
					var otherDate = charges.Where(x => x.J7_ChargeType == otherChargeType).Select(x => x.J7_ExchangeRateDate).FirstOrDefault();
					if (!otherDate.IsEmpty && charge.J7_ExchangeRateDate != otherDate)
					{
						charge.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("B27C12B9-1E58-483D-A3E7-3F8C8CF5C689", "Exchange Rate Date of Charge Codes '010' and '014' must be equal."));
					}
				}
			}
		}

		internal static void CheckJ7_ChargeType_010_014_EachOtherRequired(CommonNonApportionedCharge charge)
		{
			var propertyInfo = charge.J7_ChargeTypeInfo;
			switch (charge.J7_ChargeType)
			{
				case ImportChargeCodeList.Codes._010:
					Check010And014RequireEachOtherIfTransportAIR(ImportChargeCodeList.Codes._014);
					break;
				case ImportChargeCodeList.Codes._014:
					Check010And014RequireEachOtherIfTransportAIR(ImportChargeCodeList.Codes._010);
					break;
			}
			void Check010And014RequireEachOtherIfTransportAIR(string requiredChargeType)
			{
				var declaration = charge.Parent?.JobDeclaration;
				if (declaration != null && declaration.JE_MessageType == SharedJobMessageTypeList.Codes.Import && declaration.JE_TransportMode == Core.Constants.TransportModes.Air && !declaration.JE_IATALoadPort.IsEmpty)
				{
					var chargeHolder = (IChargeHolder)charge.Parent;
					var charges = chargeHolder.Charges.Cast<CommonNonApportionedCharge>();
					if (!charges.Any(x => x.J7_ChargeType == requiredChargeType))
					{
						charge.RemoveRowMessageError(GetMessage(charge.J7_ChargeType));
						charge.AddRowMessageError(GetMessage(requiredChargeType));
					}
				}
				string GetMessage(ZString chargeType) => Res.GetString("B5E51B11-A015-4569-B50B-2D4391F7CFEF", "Air Freight Costs incomplete! You have not entered a Charge Code '{0}' - Or use 'Calculate Freight' Button on Invoice Header Level.", requiredChargeType);
			}
		}
	}
}
