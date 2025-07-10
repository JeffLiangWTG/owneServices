using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class ITCustomsNumberViewStmNumsWrapperValidation : CustomsNumberViewStmNumsWrapperValidation
{
	public ITCustomsNumberViewStmNumsWrapperValidation(ITCustomsNumberViewStmNumsWrapper parent)
		: base(parent)
	{ }

	protected override void ValidateAllCore()
	{
		ValidateYearOfApplicability();
		ValidateAppliesTo();
	}

	public void ValidateYearOfApplicability()
	{
		ValidateCalculatedProperty(Parent.YearOfApplicabilityInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckYearOfApplicability()
	{
		if (!Parent.StmNums.IsInDatabase && Parent.YearOfApplicability < ZDate.Today.Year)
		{
			Parent.YearOfApplicabilityInfo.AddError(ValidationCaptions.ITCustomsNumberViewStmNumsWrapper.YearOfApplicabilityShouldNotBeEarlierThanCurrentYear);
		}
	}

	public void ValidateAppliesTo()
	{
		ValidateCalculatedProperty(Parent.AppliesToInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckAppliesTo()
	{
		MandatoryValidation.CheckEntered(Parent.AppliesToInfo);
		ListValidation.ErrorIfInvalidCode(Parent.AppliesToInfo);
	}

	protected new ITCustomsNumberViewStmNumsWrapper Parent => (ITCustomsNumberViewStmNumsWrapper)base.Parent;
}
