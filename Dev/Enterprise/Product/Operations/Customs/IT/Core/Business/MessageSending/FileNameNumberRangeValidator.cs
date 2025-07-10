using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public class FileNameNumberRangeValidator
{
	public FileNameNumberRangeValidator(BusinessObjectFactory factory, ZString customsProfile)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.customsProfile = Argument.NotNull(customsProfile, nameof(customsProfile));
	}

	readonly BusinessObjectFactory factory;
	readonly ZString customsProfile;

	public bool CheckFilenameNumberRangeValidity(ZString customsMessageSendingMode)
	{
		var isOkToSend = true;
		if (customsMessageSendingMode == CustomsMessageSendingModeList.Codes.AutomaticProcedure || customsMessageSendingMode == CustomsMessageSendingModeList.Codes.ManualProcedure)
		{
			var account = CustomsCredentialHelper.GetAccountFromInternalCode(customsProfile);

			isOkToSend = CheckFileNameRangeValidityForAccount(account);
		}

		return isOkToSend;
	}

	#region Implementation

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
	bool CheckFileNameRangeValidityForAccount(Registry.Account account)
	{
		if (account == null)
		{
			Globals.Message.ShowError(ValidationCaptions.MessageSending.AccountNotFoundDialogErrorMessage, ValidationCaptions.MessageSending.SendMessageErrorDialogCaption);
			return false;
		}

		var filenameGenerator = new DailySequenceNumberGenerator(account, factory);

		if (!filenameGenerator.CanGenerate())
		{
			Globals.Message.ShowError(ValidationCaptions.MessageSending.GetFilenameRangeRunOutForTodayDialogErrorMessage(account), ValidationCaptions.MessageSending.SendMessageErrorDialogCaption);
			return false;
		}

		return true;
	}

	#endregion
}
