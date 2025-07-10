using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using URCRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public static class CDSFeeCodeFromRateCodeMapper
	{
		public static string GetMapping(CusEntryLine entryLine, string rateCode)
		{
			var isUsingEuTariffForNI = entryLine.IsEuTariffToBeUsedForNorthernIreland;
			if (!isUsingEuTariffForNI)
			{
				return rateCode;
			}
			else
			{
				switch (rateCode)
				{
					case URCRateCodes.CustomsDutyOnIndustrialProducts:
						return GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty;

					case URCRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge:
						return GBCommonConstants.NorthernIrelandDutyCodes.AdditionalDuty;

					case URCRateCodes.DefinitiveAntiDumpingDuty:
						return GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty;

					case URCRateCodes.ProvisionalAntiDumpingDuty:
						return GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalAntiDumpingDuty;

					case URCRateCodes.DefinitiveCountervailingDuty:
						return GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty;

					case URCRateCodes.ProvisionalCountervailingDuty:
						return GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalCountervailingDuty;

					default:
						return rateCode;
				}
			}
		}
	}
}
