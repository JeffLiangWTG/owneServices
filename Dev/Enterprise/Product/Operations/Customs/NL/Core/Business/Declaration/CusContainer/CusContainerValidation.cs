using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusContainerValidation : EU.Business.Declaration.CusContainerValidation
{
	public CusContainerValidation(CusContainer parent)
		: base(parent)
	{
	}

	public new CusContainer Parent => (CusContainer)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateSealPartyForBinding();
		ValidateAdditionalSealPartyForBinding();
	}

	public void ValidateSealPartyForBinding()
	{
		ValidateCalculatedProperty(Parent.SealPartyForBindingInfo);
	}

	protected virtual void CheckSealPartyForBinding()
	{
		if (!Parent.CO_Seal.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SealPartyForBindingInfo);
		}

		if (!Parent.JobContainer.JC_SealPartyInfo.HasErrors())
		{
			ListValidation.ErrorIfInvalidCode(Parent.SealPartyForBindingInfo, Parent.Lookups.SealPartyList);
		}

		Parent.JobContainer.Validation.ValidateJC_SealParty();
		Parent.SealPartyForBindingInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_SealPartyInfo);
	}

	public void ValidateAdditionalSealPartyForBinding()
	{
		ValidateCalculatedProperty(Parent.AdditionalSealPartyForBindingInfo);
	}

	protected virtual void CheckAdditionalSealPartyForBinding()
	{
		if (!Parent.CO_SecondSeal.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AdditionalSealPartyForBindingInfo);
		}

		if (!Parent.JobContainer.JC_AdditionalSealPartyInfo.HasErrors())
		{
			ListValidation.ErrorIfInvalidCode(Parent.AdditionalSealPartyForBindingInfo, Parent.Lookups.SealPartyList);
		}

		Parent.JobContainer.Validation.ValidateJC_AdditionalSealParty();
		Parent.AdditionalSealPartyForBindingInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_AdditionalSealPartyInfo);
	}
}
