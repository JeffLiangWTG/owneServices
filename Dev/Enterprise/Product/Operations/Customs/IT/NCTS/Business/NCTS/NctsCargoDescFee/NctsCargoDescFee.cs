using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsCargoDescFee : EU.NCTS.Business.NctsCargoDescFee, Integration.Customs.IT.INctsCargoDescFee, IFee
{
	public NctsCargoDescFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusInBondFee.Schema
	{
		public const int BaseValueDecimalPlaces = 2;
		public const int RateDecimalPlaces = 6;
		public const int ChargeAmountDecimalPlaces = 2;
	}

	protected override CusInBondFeeValidation GetNewValidation()
	{
		return CargoDesc?.IsPhase5 ?? false
			? base.GetNewValidation()
			: new NctsCargoDescFeePhase4Validation(this);
	}

	public INctsCargoDescFeeLookups ITLookups => (INctsCargoDescFeeLookups)Lookups;

	protected override CusInBondFeeLookups GetNewLookups()
	{
		return CargoDesc?.IsPhase5 ?? false
			? new NctsCargoDescFeePhase5Lookups(this)
			: new NctsCargoDescFeePhase4Lookups(this);
	}

	#region BFE_ChargeType

	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_ChargeType", Caption = "Type")]
	[ReadOnlyMember(nameof(IsRateOverrideReasonCodeEmpty))]
	public override ZString BFE_ChargeType { get => base.BFE_ChargeType; set => base.BFE_ChargeType = value; }

	#endregion

	#region BFE_RateOverrideReasonCode

	[List(nameof(ITLookups) + "." + nameof(INctsCargoDescFeeLookups.RateOverrideReasonList))]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_RateOverrideReasonCode", Caption = "Action")]
	public override ZString BFE_RateOverrideReasonCode { get => base.BFE_RateOverrideReasonCode; set => base.BFE_RateOverrideReasonCode = value; }

	#endregion

	#region BFE_BaseValue

	[DecimalPlaces(Schema.BaseValueDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_BaseValue", Caption = "Base Amount")]
	[ReadOnlyMember(nameof(IsRateOverrideReasonCodeEmpty))]
	public override ZDecimal BFE_BaseValue
	{
		get => base.BFE_BaseValue;
		set
		{
			var oldValue = BFE_BaseValue;
			base.BFE_BaseValue = value;
			if (!IsCopying && oldValue != BFE_BaseValue)
			{
				CalculateChargeAmountIfNeeded();
			}
		}
	}

	#endregion

	#region BFE_MethodOfCalculation

	[List(nameof(ITLookups) + "." + nameof(INctsCargoDescFeeLookups.MethodOfCalculationList))]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_MethodOfCalculation", Caption = "Method of Calculation")]
	[ReadOnlyMember(nameof(IsRateOverrideReasonCodeEmpty))]
	public override ZString BFE_MethodOfCalculation
	{
		get => base.BFE_MethodOfCalculation;
		set
		{
			var oldValue = BFE_MethodOfCalculation;
			base.BFE_MethodOfCalculation = value;
			if (!IsCopying && oldValue != BFE_MethodOfCalculation)
			{
				CalculateChargeAmountIfNeeded();
			}
		}
	}

	#endregion

	#region BFE_Rate

	[DecimalPlaces(Schema.RateDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_Rate", Caption = "Tax Rate")]
	[ReadOnlyMember(nameof(IsRateOverrideReasonCodeEmpty))]
	public override ZDecimal BFE_Rate
	{
		get => base.BFE_Rate;
		set
		{
			var oldValue = BFE_Rate;
			base.BFE_Rate = value;
			if (!IsCopying && oldValue != BFE_Rate)
			{
				CalculateChargeAmountIfNeeded();
			}
		}
	}

	#endregion

	#region BFE_ChargeAmount

	[DecimalPlaces(Schema.ChargeAmountDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_ChargeAmount", Caption = "Total Amount")]
	[ReadOnlyMember(nameof(IsRateOverrideReasonCodeEmpty))]
	public override ZDecimal BFE_ChargeAmount
	{
		get => base.BFE_ChargeAmount;
		set
		{
			var oldValue = BFE_ChargeAmount;
			base.BFE_ChargeAmount = value;
			if (!IsCopying && oldValue != BFE_ChargeAmount)
			{
				BFE_ChargeAmount = FeeHelper.RoundChargeAmountIfNeeded(value);
			}
		}
	}

	#endregion

	#region BFE_MethodOfPayment

	[List(nameof(ITLookups) + "." + nameof(INctsCargoDescFeeLookups.MethodOfPaymentList))]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee|BFE_MethodOfPayment", Caption = "Method of Payment")]
	public override ZString BFE_MethodOfPayment { get => base.BFE_MethodOfPayment; set => base.BFE_MethodOfPayment = value; }

	#endregion

	#region BFE_BY

	[RelatedBusinessObject(nameof(CargoDesc))]
	public override ZGuid BFE_BY { get => base.BFE_BY; set => base.BFE_BY = value; }

	public NctsDepartureCargoDesc CargoDesc => Factory.Load<NctsDepartureCargoDesc>(BFE_BY);

	#endregion

	#region IFee Members

	ZString IFee.ChargeType => BFE_ChargeType;

	ZDecimal IFee.BaseValue => BFE_BaseValue;

	ZDecimal IFee.Rate => BFE_Rate;

	ZString IFee.MethodOfCalculation => BFE_MethodOfCalculation;

	ZDecimal IFee.Amount => BFE_ChargeAmount;

	ZString IFee.MethodOfPayment => BFE_MethodOfPayment;

	#endregion

	#region Implementation

	void CalculateChargeAmountIfNeeded()
	{
		var shouldCalculateChargeAmount = !BFE_RateOverrideReasonCode.IsEmpty
			&& ITLookups.RateOverrideReasonList.ContainsCode(BFE_RateOverrideReasonCode)
			&& BFE_MethodOfCalculation == NctsCargoDescFeeMethodOfCalculationList.Codes.Multiplicative
			&& BFE_ChargeAmount.IsEmpty;

		if (shouldCalculateChargeAmount)
		{
			BFE_ChargeAmount = BFE_BaseValue * BFE_Rate;
		}
	}

	ZBool IsRateOverrideReasonCodeEmpty => BFE_RateOverrideReasonCode.IsEmpty;

	#endregion
}
