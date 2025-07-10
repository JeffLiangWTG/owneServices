using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public class MessageSendingFormValidation : IMessageSendingFormValidation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
	bool IMessageSendingFormValidation.CheckSubscriberForMessageSending(ZString customsMessageSendingMode, ZString subscriber)
	{
		if (customsMessageSendingMode == CustomsMessageSendingModeList.Codes.AutomaticProcedure && subscriber.IsEmpty)
		{
			Globals.Message.ShowError(ValidationCaptions.MessageSending.SubscriberNotFoundDialogErrorMessage, ValidationCaptions.MessageSending.SendMessageErrorDialogCaption);
			return false;
		}

		return true;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
	bool IMessageSendingFormValidation.CheckNodePresentInCompanyAllowedList(ZString node)
	{
		if (CustomsCredentialHelper.NodePresentInCompanyAllowedListWithMAUCertificate(node))
		{
			return true;
		}

		Globals.Message.ShowError(ValidationCaptions.Shared.SelectedNodeHasNoMAUCertificate, ValidationCaptions.MessageSending.SendMessageErrorDialogCaption);
		return false;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
	bool IMessageSendingFormValidation.CheckCurrentUserHasFiscalCode()
	{
		if (CustomsCredentialHelper.CurrentUserHasFiscalCode())
		{
			return true;
		}

		Globals.Message.ShowError(ValidationCaptions.MessageSending.SubscriberHasNoFiscalCode, ValidationCaptions.MessageSending.SendMessageErrorDialogCaption);
		return false;
	}
}
