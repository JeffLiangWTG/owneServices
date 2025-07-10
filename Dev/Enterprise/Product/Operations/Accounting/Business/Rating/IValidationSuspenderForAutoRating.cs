using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// Implementing this Interface on your Rating Supporter will Suspend Validation while AutoRating is Executing
	/// </summary>
	public interface IValidationSuspenderForAutoRating
	{
		// This is run at the end of Autorating when Validation has been resumed
		void OnValidationResumed();
		// This is used to control whether or not RunPreSaveValidation is run on specific entities before Autorating begins
		bool ShouldRunPreSaveValidationBeforeAutorating(IBusiness hostEntity);
	}
}
