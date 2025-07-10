using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class CusContainerValidation : AutoINCusContainerValidation
{
	public CusContainerValidation(CusContainer parent)
		: base(parent)
	{
	}

	new CusContainer Parent => (CusContainer)base.Parent;

	protected override void CheckCO_SealType()
	{
		base.CheckCO_SealType();
		var parent = Parent;
		if (!parent.SealNumberForBinding.IsEmpty && parent.CO_SealType.IsEmpty)
		{
			MandatoryValidation.AddYouHaveNotEnteredMessage(parent.CO_SealTypeInfo);
		}
	}
}
