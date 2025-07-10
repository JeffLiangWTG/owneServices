namespace Enterprise.Customs.CH.Business;

public class CusContainerValidation : Customs.Business.CusContainerValidation
{
	public CusContainerValidation(CusContainer parent)
		: base(parent)
	{
	}

	new CusContainer Parent => (CusContainer)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New(Parent.Declaration);
	PlausiValidation plausiValidation;

	protected override void CheckCO_Seal()
	{
		base.CheckCO_Seal();
		PlausiValidation.CheckNP70021(Parent.CO_SealInfo, Parent);
		PlausiValidation.CheckNP70043(Parent.CO_SealInfo, Parent);
	}

	protected override void CheckCO_SecondSeal()
	{
		base.CheckCO_SecondSeal();
		PlausiValidation.CheckNP70043(Parent.CO_SecondSealInfo, Parent);
	}

	protected override void CheckContainerNoHasPackages()
	{
		if (!Parent.Declaration.IsExportDeclarationActivation)
		{
			base.CheckContainerNoHasPackages();
		}
	}
}
