using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.EConversation.GUI
{
	public class EConversationPlugin : ZPlugIn
	{
		readonly ModuleIdentifier parentModule;
		readonly EConversationSecurityCheckpoints securityItems;
		protected EConversationSecurityCheckpoints SecurityItems => securityItems;
		protected ModuleIdentifier ParentModule => parentModule;

		JobConversation Conversation => ((IConversationProvider)HostBusinessEntity).eConversation;

		public EConversationPlugin(ModuleIdentifier parentModule, IBusiness host)
			: base(host)
		{
			securityItems = new EConversationSecurityCheckpoints(parentModule);

			this.parentModule = parentModule;
		}

		string pluginNotDisplayedMessage;
		public override ZString PlugInNotDisplayedMessage
		{
			get { return pluginNotDisplayedMessage; }
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();

			var actionsMenu = (Form as IFileMenuItemsProvider)?.ActionsMenuItem;
			if (actionsMenu != null && securityItems.SendMessages.IsAllowed)
			{
				var menuItem = new ZMenuItem(Res.GetData("50c41259-4240-451e-850a-cd820fd373f3", "Add Internal Comment"), AddInternalLog_Click);
				menuItem.Shortcut = Shortcut.CtrlM;

				actionsMenu.MenuItems.Add(menuItem);
			}
		}

		void AddInternalLog_Click(object sender, EventArgs e)
		{
			var bizo = HostBusinessEntity as BusinessObject;
			if (bizo != null && !bizo.IsInDatabase)
			{
				Globals.Message.ShowWarning(Res.GetString("c6eaff5e-b7f3-44f9-812f-84b93c3b5dd7", "The form must be saved before an eConversation can be started."));
				return;
			}

			if (Conversation == null)
			{
				ErrorReporter.ReportOnce("The conversation provider does not have a conversation object.", new InvalidOperationException("The conversation provider does not have a conversation object."));
				Globals.Message.ShowError(Res.GetString("03ec7266-f764-43f5-b63c-8b0157758116", "The eConversation is unavailable."));
				return;
			}

			var provider = (IConversationProvider)HostBusinessEntity;
			using (var form = new AddInternalLogForm(new SubscriberAutocompleteHelper(Factory, provider)))
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(form, Form);

				if (result == DialogResult.OK)
				{
					provider.eConversation.AddMessageFromCurrentUser(form.MessageText, isInternal: true);

					var triedToAddExternalParticipant = false;
					foreach (var participant in form.ParticipantsReferenced)
					{
						triedToAddExternalParticipant |= !TryToAddNewSubscriber(participant);
					}

					if (triedToAddExternalParticipant)
					{
						Globals.Message.ShowError(Res.GetString("1b5b83d4-bcdf-4669-8fe8-ca215710163a", "Some participants were not added because you do not have permission to add external participants."));
					}
				}
			}
		}

		bool TryToAddNewSubscriber(SubscriberWrapper wrapper)
		{
			var isAllowed = wrapper.Parent.IsInternal || securityItems.ModifyExternalParticipants.IsAllowed;
			if (isAllowed)
			{
				var participant = Conversation.Participants.GetOrAdd(wrapper.Parent);
				if (!string.IsNullOrEmpty(wrapper.Relation) && string.IsNullOrEmpty(participant.JCP_Relation))
				{
					participant.JCP_Relation = wrapper.Relation;
				}

				return true;
			}

			return false;
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			var bizo = HostBusinessEntity as BusinessObject;
			if (bizo != null && !bizo.IsInDatabase)
			{
				pluginNotDisplayedMessage = Res.GetString("444b6d73-bf21-4f60-95a5-6b4baecad6cb", "The form must be saved and this tab reloaded before an eConversation can be started.");
				return false;
			}

			if (!securityItems.View.IsAllowed)
			{
				pluginNotDisplayedMessage = securityItems.View.ErrorMessageForNotAllowed;
				return false;
			}

			return true;
		}

		protected override bool ShowPreSaveDialogsWhenInactive
		{
			get { return true; }
		}

		protected readonly FunctionalitySuspender DefaultPreSaveMessageNotificationSuspender = new FunctionalitySuspender();

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && !DefaultPreSaveMessageNotificationSuspender.IsSuspended)
			{
				var eConvMessageTextBox = ((IConversationView)UserControl).MessageTextBox;
				if (Conversation != null && eConvMessageTextBox.Enabled && !string.IsNullOrEmpty(eConvMessageTextBox.Text) && Conversation.NextMessage != ZBlob.Empty)
				{
					if (Globals.Message.Show(Res.GetString("1A5DB8C4-8C3B-4FA9-A068-06789F743A0B", "You have unsent eConversation messages. Continue with save?"), Res.GetString("62E8707E-911C-4C8D-B0F5-45FBCA820C57", "Unsent message"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
					{
						result = ContinueWithSave.No;
					}
				}
			}
			return result;
		}

		public override string Name => "eConversation";
		protected override ZBool HasUserControl => true;
		protected override LicenceCheckpoint LicenceCheckPoint => null;
		protected override ZBool AllowPlugInDisplayWithNoLicence => true;

		protected override Control GetNewUserControl()
		{
			return new EConversationFullControl(parentModule, securityItems) { ViewMode = ViewMode };
		}

		public EConversationViewMode ViewMode { get; set; } = EConversationViewMode.ShowEverything;
	}
}
