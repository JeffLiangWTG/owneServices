using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public partial class CusLineTariffDetailValidation : AutoCHCusLineTariffDetailValidation
{
	public CusLineTariffDetailValidation(CusLineTariffDetail parent) : base(parent)
	{
	}

	new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.InvoiceLine.JobDeclaration));
	PlausiValidation plausiValidation;

	protected override void CheckBZ_Tariff()
	{
		base.CheckBZ_Tariff();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BZ_TariffInfo);

		if (Parent.IsAdditionalTax)
		{
			PlausiValidation.CheckR137(Parent.BZ_TariffInfo, Parent);
			PlausiValidation.CheckR138(Parent.BZ_TariffInfo, Parent);
			PlausiValidation.CheckR330(Parent);
			PlausiValidation.CheckR326R327(Parent.BZ_TariffInfo, Parent);
		}
	}

	protected override void CheckBZ_Qty1()
	{
		base.CheckBZ_Qty1();
		if (Parent.IsAdditionalFee)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BZ_Qty1Info);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BZ_Qty1Info);
		}

		if (Parent.IsAdditionalTax)
		{
			PlausiValidation.CheckR149a(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR146(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR147(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR148(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR262(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR334(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR337(Parent.BZ_Qty1Info, Parent);
			PlausiValidation.CheckR339(Parent.BZ_Qty1Info, Parent);
		}
	}

	protected override void CheckBZ_ManualRate()
	{
		base.CheckBZ_ManualRate();

		if (Parent.IsAdditionalFee)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BZ_ManualRateInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.BZ_ManualRateInfo);
		}

		if (Parent.IsAdditionalTax && Parent.IsAdditionalTaxApplied)
		{
			PlausiValidation.CheckR220a(Parent.BZ_ManualRateInfo, Parent);
		}
	}

	protected override void CheckBZ_AlcoholPercentage()
	{
		base.CheckBZ_AlcoholPercentage();

		if (Parent.IsAdditionalTax)
		{
			PlausiValidation.CheckR149b(Parent.BZ_AlcoholPercentageInfo, Parent);
		}
	}
}
