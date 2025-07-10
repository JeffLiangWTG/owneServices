using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CustomsValidation : ValidationProvider
	{
		public CustomsValidation(ZPropertyInfo infoForError)
		{
			this.infoForError = infoForError;
		}

		public void ErrorOnKeyData(IConsignmentKeyChangeInhibitor businessObject)
		{
			ErrorOnKeyDataCore(businessObject.ShouldStopKeyFieldsChange);
		}

		public void ErrorOnKeyDataWithNoChildren(ZString messageStatus)
		{
			var shouldStopKeyFieldsChange = !CMRStatusHelper.IsAcceptableStatusesForKeyValueChange(infoForError.BizObj.Factory, messageStatus);
			ErrorOnKeyDataCore(shouldStopKeyFieldsChange);
		}

		void ErrorOnKeyDataCore(bool shouldStopKeyFieldsChange)
		{
			if (infoForError.HasChanges && shouldStopKeyFieldsChange)
			{
				infoForError.AddError(ErrorString.Replace("@", infoForError.OriginalValue.ToString()));
			}
		}

		readonly ZPropertyInfo infoForError;
		public const string ErrorString = "This is a key value for messaging. Please withdraw the message before changing this value or revert to the previous value (@)";
	}
}
