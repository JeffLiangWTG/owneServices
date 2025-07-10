using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	[SuppressFormsLocalizedTest]
	public class OrganisationPlugInMenu : KMenuItem
	{
		public OrganisationPlugInMenu(OrgHeaderWrapper organisation, Action licenceLogIn)
		{
			this.licenceLogIn = licenceLogIn;
			this.organisation = organisation;
			this.Text = "Customs Messaging";

			InitialiseMenu();
		}

		internal readonly Action licenceLogIn;
		readonly OrgHeaderWrapper organisation;

		internal MenuItem clientRegistrationRequest;

		void InitialiseMenu()
		{
			clientRegistrationRequest = new ZMenuItem("Client Registration Request", new EventHandler(SendClientRegistrationRequest_Click));
			this.MenuItems.Add(clientRegistrationRequest);
		}

#if DEBUG
		internal bool? sendMessageOverrideFortesting;
#endif

		void SendClientRegistrationRequest_Click(object sender, EventArgs e)
		{
			if (organisation.OrgHeader.HasChanges && Globals.Message.Show(SaveOrganisationAdvice, "Save Organisation", MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
			{
				return;
			}

			bool sendMessage = false;
			using (var msgDataForm = new ClientRegistrationRequestForm(organisation))
			{
				ZFormModaliser.ShowDialogWithoutDispose(msgDataForm);
				sendMessage = msgDataForm.IsOKToSendMessage;
			}

#if DEBUG
			if (sendMessageOverrideFortesting.HasValue)
			{
				sendMessage = sendMessageOverrideFortesting.Value;
			}
#endif

			if (sendMessage)
			{
				licenceLogIn();
				new CLREGMessageBuilder(organisation.CLREGInfoProvider, organisation.Factory).PopulateMessagesReturningResult();

				PerformFactorySave();
				Globals.Message.ShowInformation(messageSentNotification, "Message Sent");
			}
		}
		internal const string messageSentNotification = "The Client Registration Request message has been sent. Response will be attached to that organization.";
		internal const string SaveOrganisationAdvice = "The Organisation has not yet been saved. Do you want to save and proceed?";
		internal const string CCIDExists = "Customs Client ID already exists for this organization. You cannot send Client Registration Request message.";

		void PerformFactorySave()
		{
			try
			{
				organisation.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}
	}
}
