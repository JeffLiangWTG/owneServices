using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusContainerValidation : EU.Business.Declaration.CusContainerValidation
{
	public CusContainerValidation(CusContainer parent)
		: base(parent)
	{
	}

	public new CusContainer Parent
	{
		get { return (CusContainer)base.Parent; }
	}

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
		if (DoSealPartyCheck && !Parent.CO_Seal.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SealPartyForBindingInfo);
		}

		if (!Parent.JobContainer.JC_SealPartyInfo.HasErrors())
		{
			ListValidation.ErrorIfInvalidCode(Parent.SealPartyForBindingInfo, Parent.Lookups.SealParty_List);
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
		if (DoSealPartyCheck && !Parent.CO_SecondSeal.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AdditionalSealPartyForBindingInfo);
		}

		if (!Parent.JobContainer.JC_AdditionalSealPartyInfo.HasErrors())
		{
			ListValidation.ErrorIfInvalidCode(Parent.AdditionalSealPartyForBindingInfo, Parent.Lookups.SealParty_List);
		}

		Parent.JobContainer.Validation.ValidateJC_AdditionalSealParty();
		Parent.AdditionalSealPartyForBindingInfo.AddAllNotificationsFrom(Parent.JobContainer.JC_AdditionalSealPartyInfo);
	}

	bool DoSealPartyCheck
	{
		get
		{
			var result = false;
			var declaration = Parent.Declaration;
			var entryInstructions = Parent.Declaration.CustomsEntryInstructions;
			ZString[] eiStyles = { Constants.EntryInstructionStyle._A, Constants.EntryInstructionStyle._C, Constants.EntryInstructionStyle._D, Constants.EntryInstructionStyle._H, Constants.EntryInstructionStyle._I, Constants.EntryInstructionStyle._J };

			if (declaration.IsImport || declaration.IsExport)
			{
				if (entryInstructions.Cast<CusEntryInstruction>().Any(ei => eiStyles.Contains(ei.CEI_Style)))
				{
					result = true;
				}
			}

			return result;
		}
	}
}
