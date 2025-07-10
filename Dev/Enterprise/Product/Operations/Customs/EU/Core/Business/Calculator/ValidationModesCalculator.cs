using CargoWise.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class ValidationModesCalculator
	{
		protected ValidationModesCalculator(IValidationModesSupporter supporter)
		{
			this.supporter = Argument.NotNull(supporter, nameof(supporter));
		}

		protected readonly IValidationModesSupporter supporter;

		public bool IsThisValidationOn(ValidationModes currentValidationMode, ValidationModes modeToCheckAgainst)
		{
			return modeToCheckAgainst == (currentValidationMode & modeToCheckAgainst);
		}

		public void RecalculateValidationModes()
		{
			supporter.ValidationModes = RecalculateValidationModesCore();
		}

		protected abstract ValidationModes RecalculateValidationModesCore();

		public void UpdateValidationModes(ValidationModes modeToCheckAgainst, bool isSpecificModeEnabled)
		{
			if (isSpecificModeEnabled)
			{
				supporter.ValidationModes |= modeToCheckAgainst;
			}
			else if ((supporter.ValidationModes & modeToCheckAgainst) == modeToCheckAgainst)//cannot use IsThisValidationOn as this is after a value is set
			{
				int result = supporter.ValidationModes - modeToCheckAgainst;
				supporter.ValidationModes = (ValidationModes)result;
			}
		}
	}
}
