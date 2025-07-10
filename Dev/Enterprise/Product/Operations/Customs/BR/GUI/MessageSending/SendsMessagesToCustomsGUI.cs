using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class SendsMessagesToCustomsGUI : Customs.GUI.SendsMessagesToCustomsGUI
	{
		protected override ZForm GetBackDoorForSavingForm(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions)
		{
			switch (savingOptions)
			{
				case DeclarationDeferredAmendmentSavingOptions options:
					return new BackdoorForSavingOnAmendmentForm(options);
				case CatalogDeferredAmendmentSavingOptions option:
					return new CatalogBackdoorForSavingOnAmendmentForm(option);
				default:
					return base.GetBackDoorForSavingForm(detectionResult, savingOptions);
			}
		}

		protected override ContinueWithSave DoActionsBeforeSendingRequiredMessages(IMessageManager manager, RequiredMessagesInformation detectionResult)
		{
			if (detectionResult.AmendmentWithdrawalReason.IsCancelled)
			{
				return ContinueWithSave.No;
			}

			foreach (var sendingObject in detectionResult.BizObjsToSendMessageForForAmendmentOrWithdrawal)
			{
				if (sendingObject is JobDeclarationMessageSendingObject declarationMessageSendingObject)
				{
					declarationMessageSendingObject.VOCReason = detectionResult.AmendmentWithdrawalReason.ReasonText.Left(declarationMessageSendingObject.VOCReasonInfo.MaxLength);
				}
			}

			return base.DoActionsBeforeSendingRequiredMessages(manager, detectionResult);
		}

		public override bool ShouldSendCustomsMessages(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions, IMessageManager messageManager)
		{
			if (savingOptions is NoGuiDeferredAmendmentSavingOptions)
			{
				if (savingOptions.IsCancelled)
				{
					UserNotification.ShowError(GetCannotSaveMessage(detectionResult));
					return false;
				}
				return savingOptions.ShouldSendMessages;
			}
			else
			{
				return base.ShouldSendCustomsMessages(detectionResult, savingOptions, messageManager);
			}
		}

		ZString GetCannotSaveMessage(RequiredMessagesInformation detectionResult)
		{
			var stringBuilder = new ZStringBuilder(Res.GetString("3AFD2652-70F5-4E39-8BA1-FAC15196C4FC", "The following Entry Header(s) already contains an Import License number and cannot be edited."));

			foreach (ImportLicenseMessageSendingObject sendingObject in detectionResult.BizObjsToSendMessageForForAmendmentOrWithdrawal)
			{
				stringBuilder.Append($" • {sendingObject.Header.CH_BGMReference}");
			}
			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}
	}
}
