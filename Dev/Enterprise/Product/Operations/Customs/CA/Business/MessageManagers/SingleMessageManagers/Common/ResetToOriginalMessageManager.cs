//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Environment;
	using Enterprise.Security;

	public abstract class CAResetToOriginalMessageManager : CAMessageManager
	{
		protected CAResetToOriginalMessageManager(ICAEDIFACTMessageAttachee dataWrapper, EDIFACTMessageStatusCalculator statusCalculator, IUserNotification notification)
			: base(dataWrapper, statusCalculator, notification)
		{
		}

		public void ResetToOriginal(IUserNotification sender)
		{
			if (ResetToOriginalSecurityCheckpoint?.IsAllowed ?? true)
			{
				var caption = Res.GetString("1B933F41-335E-4030-BDAE-D5F06822B694", "Warning - Reset to original?");
				var prompt = Res.GetString("27160B3B-47CC-41FA-B5F2-451480CA1051", "If you are sure you want to reset the message to original please type:") + " ";
				var confirmationString = Res.GetString("497D6C60-BDFC-4C75-B276-DC3C869FDFA8", "I UNDERSTAND THE CONSEQUENCE OF USING RESET TO ORIGINAL INCORRECTLY");
				if (sender.ShowConfirmation(ResetToOriginalWarning, caption, prompt, confirmationString))
				{
					ResetToOriginalWithLog(BusinessObject);
				}
			}
			else
			{
				Env.Security.ShowError(ResetToOriginalSecurityCheckpoint);
			}
		}

		protected virtual SecurityCheckpoint ResetToOriginalSecurityCheckpoint
		{
			get
			{
				return Env.Security.CustomsResetToOriginal;
			}
		}

		protected string ResetToOriginalWarning
		{
			get
			{
				return Res.GetString("5644590F-6EDA-4267-B9F4-0D17E369F48B", @"Warning - Resetting a message to original is almost never correct unless you have canceled the message.
Do not reset messages because of a message problem or because you wish to try again.
If you reset to original incorrectly the system will not work correctly - cargo may be delayed, storage incurred or the system may become unusable for this shipment.");
			}
		}
	}
}
