using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class CalculateFreightBizObj : AutoCalculateFreightBizObj
{
	public CalculateFreightBizObj(IJobComInvChargeCollection<JobComInvCharge> charges)
		: base(new BusinessObjectFactory())
	{
		this.charges = Argument.NotNull(charges, nameof(charges));
	}

	protected readonly IJobComInvChargeCollection<JobComInvCharge> charges;

	protected override ZDecimal GetAmountToCHBorder() => ZArchitecture.Core.Utilities.Round(TotalAmount * PercentageToCHBoarder / 100, 2);

	protected override ZDecimal GetAmountToFinalDestination() => TotalAmount - AmountToCHBorder;

	protected override ZDecimal GetPercentageToFinalDestination() => 100 - PercentageToCHBoarder;

	[ResourceStringData("CHCalculateFreightBizObj|TotalAmount", Caption = "Total Amount")]
	public override ZDecimal TotalAmount
	{
		get => base.TotalAmount;
		set => base.TotalAmount = value;
	}

	[List(nameof(CurrencyList))]
	public override ZString Currency
	{
		get => base.Currency;
		set => base.Currency = value;
	}

	public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);

	[ResourceStringData("CHCalculateFreightBizObj|PercentageToCHBoarder", Caption = "%Freight to CH Border")]
	public override ZDecimal PercentageToCHBoarder
	{
		get => base.PercentageToCHBoarder;
		set => base.PercentageToCHBoarder = value;
	}

	[ResourceStringData("CHCalculateFreightBizObj|PercentageToFinalDestination", Caption = "%Freight to Final Destination")]
	public override ZDecimal PercentageToFinalDestination => base.PercentageToFinalDestination;

	[ResourceStringData("CHCalculateFreightBizObj|AmountToCHBorder", Caption = "Amount to CH Border")]
	public override ZDecimal AmountToCHBorder => base.AmountToCHBorder;

	[ResourceStringData("CHCalculateFreightBizObj|AmountToFinalDestination", Caption = "Amount to Final Destination")]
	public override ZDecimal AmountToFinalDestination => base.AmountToFinalDestination;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		Currency = Core.Constants.CurrencyCodes.Switzerland;
	}

	public bool AddCharges()
	{
		var result = false;
		RunPreSaveValidation();
		if (!HasErrors)
		{
			SetupBorderCharge(charges.AddNew(), AmountToCHBorder, Currency, true, true, true);
			SetupBorderCharge(charges.AddNew(), AmountToFinalDestination, Currency, true, false, true);
			result = true;
		}
		return result;
	}

	public void SetupBorderCharge(JobComInvCharge charge, ZDecimal amount, string currency, bool isDutiable, bool isStatisticalValueApplicable, bool isGSTApplicable)
	{
		charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
		charge.J7_Amount = amount;
		charge.J7_RX_NKCurrency = currency;
		charge.J7_IsDutiable = isDutiable;
		charge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable;
		charge.J7_IsGSTApplicable = isGSTApplicable;
	}
}
