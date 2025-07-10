using CargoWise.Common;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PreSaveActionsResult
	{
		PreSaveActionsResult()
		{
		}

		public bool CanProceed { get; private set; }
		public string ErrorMessage { get; private set; }
		public bool HasError => !CanProceed;
		public bool HasErrorForGUI => HasError && !LoggingOnly;
		public bool LoggingOnly { get; private set; }
		public string WarningMessage { get; set; }

		public static PreSaveActionsResult Failure(string errorMessage, bool loggingOnly = false)
		{
			Argument.NotNullOrEmpty(errorMessage, nameof(errorMessage));

			return new PreSaveActionsResult
			{
				CanProceed = false,
				ErrorMessage = errorMessage,
				LoggingOnly = loggingOnly
			};
		}

		public static PreSaveActionsResult Success()
		{
			return new PreSaveActionsResult
			{
				CanProceed = true,
				ErrorMessage = string.Empty
			};
		}
	}
}
