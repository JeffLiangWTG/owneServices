using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class NodeProgressiveAnnualNumberValidation
{
	public NodeProgressiveAnnualNumberValidation(ICustomsMessageFountainProvider fountainProvider)
	{
		this.fountainProvider = Argument.NotNull(fountainProvider, nameof(fountainProvider));
	}

	readonly ICustomsMessageFountainProvider fountainProvider;

	public void Validate(ZPropertyInfo targetPropertyInfo)
	{
		Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));

		if (!targetPropertyInfo.Value.IsEmpty)
		{
			var fountainProviderWrapper = fountainProvider.Wrapper;

			if (fountainProviderWrapper == null)
			{
				if (!fountainProvider.HasClonableNumberRanges)
				{
					targetPropertyInfo.AddMessageError(ValidationCaptions.ProgressiveAnnualNumber.CannotRetrievePanNumberRangeForSelectedNode);
				}
			}
			else if (fountainProviderWrapper.StmNums.HasReachedLimit)
			{
				targetPropertyInfo.AddWarning(ValidationCaptions.ProgressiveAnnualNumber.NumberRangeHasReachedTheLimit(fountainProviderWrapper.SN_Type, fountainProvider.DeclarantTaxNumber, fountainProviderWrapper.StmNums.TotalAvailableNumbers));
			}
		}
	}
}
