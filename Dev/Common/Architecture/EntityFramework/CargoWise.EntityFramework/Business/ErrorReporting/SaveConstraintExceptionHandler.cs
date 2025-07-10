using CargoWise.ComponentModel;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Re-runs validation on the entire business entity hierarchy if a constraint failure occurs on save.
	/// Reports back to CargoWise if this causes validation to be added.
	/// </summary>
	public static class SaveConstraintExceptionHandler
	{
		public static bool Handle(ZSaveException ex, ISaveInitiator saveInitiator)
		{
			bool handled = false;

			if (ex.InnerException != null && ex.InnerException.CoreErrorHandler != null &&
				(ex.InnerException.CoreErrorHandler.ExceptionType == DbErrorType.UpdateConflictedWithCheckConstraint ||
				ex.InnerException.CoreErrorHandler.ExceptionType == DbErrorType.InsertConflictedWithCheckConstraint ||
				ex.InnerException.CoreErrorHandler.ExceptionType == DbErrorType.InsertConflictedWithForeignKey ||
				ex.InnerException.CoreErrorHandler.ExceptionType == DbErrorType.UpdateConflictedWithForeignKey))
			{
				handled = HandleSaveConstraintExceptionByFullValidateAndSave(ex.Message, saveInitiator);
			}

			return handled;
		}

		internal static bool HandleSaveConstraintExceptionByFullValidateAndSave(string exceptionMessage, ISaveInitiator saveInitiator)
		{
			if (!saveInitiator.SaveExceptionCaughtAlready && !saveInitiator.BusinessEntityForValidation.IsValidationSuspended)
			{
				saveInitiator.SaveExceptionCaughtAlready = true;
				saveInitiator.BusinessEntityForValidation.MarkAsNeedingValidationIncludingChildren();
				saveInitiator.BusinessEntityForValidation.RunPreSaveValidation();
				if (saveInitiator.BusinessEntityForValidation.HasErrors())
				{
					string allErrors = "";
					BusinessObject businessObjectForValidation = saveInitiator.BusinessEntityForValidation as BusinessObject;
					if (businessObjectForValidation != null)
					{
						allErrors = businessObjectForValidation.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString();
					}
					saveInitiator.ShowErrorsDialog();
					return true;
				}
			}
			return false;
		}
	}
}
