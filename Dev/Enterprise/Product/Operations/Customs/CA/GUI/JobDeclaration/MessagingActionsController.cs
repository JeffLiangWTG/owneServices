using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class MessagingActionsController : Customs.GUI.SendsMessagesToCustomsGUI, IConsolSendsMessagesToCustoms
	{
		public SingleMessageManager[] WhichMessagesShouldWeRefresh(SingleMessageManager[] allManagers)
		{
			if (allManagers.Length == 0)
			{
				NotifyUserOfAnInvalidOperation(Res.GetString("0DE7B609-B76F-42B9-897B-0B2592E9ED99", "There is nothing available for refreshing"));
				return allManagers;
			}
			else
			{
				MessageChooserNonPersistent chooser = new MessageChooserNonPersistent(allManagers, Res.GetString("B5AA9BA4-5864-4067-9C08-0A25A154C223", "Which messages do you want to refresh?"), Res.GetString("CD6B2A16-6DFA-4E26-983F-B9070094BA14", "Refresh"));
				return GetManagers(chooser);
			}
		}

		#region Implementation
		protected override ZForm GetBackDoorForSavingForm(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions)
		{
			CAMessageSendingActionCollection actions = (CAMessageSendingActionCollection)savingOptions;

			actions.RemoveActionsNotRequiringAmendment(detectionResult);

			MessageSendingActionForm result = new MessageSendingActionForm(actions);

			result.AddColumnsForAmendmentDetection();

			return result;
		}
		#endregion Implementation
	}
}
