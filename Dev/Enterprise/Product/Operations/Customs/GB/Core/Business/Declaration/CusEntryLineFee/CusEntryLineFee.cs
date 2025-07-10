using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.GB.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => ((JobDeclaration)EntryLine?.Declaration)?.ApplicationExtender?.GetCusEntryLineFeeValidation(this, new Func<Customs.Business.CusEntryLineFeeValidation>(base.GetNewValidation)) ?? base.GetNewValidation();

		public bool IsInNorthernIrelandDuty => CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty
			|| CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.AdditionalDuty
			|| CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty
			|| CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalAntiDumpingDuty
			|| CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty
			|| CF_ChargeType == GBCommonConstants.NorthernIrelandDutyCodes.ProvisionalCountervailingDuty;

		protected override IFeeRounder GetNewChargeAmountRounder() => ((JobDeclaration)EntryLine?.Declaration)?.ApplicationExtender?.ChargeAmountNoRounder ?? base.GetNewChargeAmountRounder();

		protected override ZString GetDefaultMethodOfPaymentValue() => ((JobDeclaration)EntryLine?.Declaration)?.ApplicationExtender?.GetDefaultMethodOfPaymentValue(this) ?? ZString.Empty;

		protected override bool ShouldResetDataOnMergingCore =>
			EntryLine.Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
			&& GBCustomsDataRegistry.Instance.ShouldResetCDSCalculatedEntryLineFeesAtMerger.Value
			&& CF_Source == GBCommonConstants.SourceTypeCodes.CW1
			&& CF_RateOverrideReasonCode.IsEmpty;

		protected override void ResetDataCore()
		{
			CF_MethodOfPayment = ZString.Empty;
		}

		protected override bool IncludeForVatCalculationCore
		{
			get
			{
				var rateCodeViews = CusRefRateCodeView.Loader.LoadByRateCode(Factory, CountryCode, CF_ChargeType);
				return rateCodeViews.Length == 0 || rateCodeViews[0].ZY1_RateType != Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
			}
		}
	}
}
