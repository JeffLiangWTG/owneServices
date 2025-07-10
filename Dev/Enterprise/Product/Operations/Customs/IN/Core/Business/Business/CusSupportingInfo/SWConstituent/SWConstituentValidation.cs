using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class SWConstituentValidation : AutoINCusSupportingInfoValidation
{
	public SWConstituentValidation(SWConstituent parent) : base(parent)
	{
	}

	public new SWConstituent Parent => (SWConstituent)base.Parent;
	const decimal MaxPercentage = 100.000m;

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		var parent = Parent;
		var info = parent.CSI_QuantityInfo;

		if (parent.CSI_Quantity > MaxPercentage)
		{
			info.AddMessageError(Res.GetString("93EFD73A-48AC-4E5D-B1ED-0A6B984AF020", "Constituent Percentage should not greater than {0}%%.", MaxPercentage));
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsZero(info);
		}
	}

	protected override void CheckCSI_Quantity2()
	{
		base.CheckCSI_Quantity2();
		var parent = Parent;
		var info = parent.CSI_Quantity2Info;

		if (parent.CSI_Quantity2 > MaxPercentage)
		{
			info.AddMessageError(Res.GetString("F10A026C-0597-4DC2-8CFF-C02FD5E5A016", "Constituent Yield Percentage should not greater than {0}%%.", MaxPercentage));
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsZero(info);
		}
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_StatusInfo);
	}
}

