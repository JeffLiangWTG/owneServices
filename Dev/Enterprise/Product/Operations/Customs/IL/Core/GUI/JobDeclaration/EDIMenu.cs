using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public EDIMenu() : base()
		{
		}

		public MenuItem SendMessageMenuItem;

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = Declaration;
			SendMessageMenuItem.Visible = GenerateEntriesMenuItem.Visible;
		}

		public static ZString LabelMenuSendMessage => ResString.GetMultilingualString("3F9D8868-2C4E-42EE-BF57-737BC0DB8020", "Send to Customs");

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			SendMessageMenuItem = new ZMenuItem(LabelMenuSendMessage, SendMessageMenuItem_Click);
			MenuItems.Add(SendMessageMenuItem);
		}

		protected void SendMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (IsMessagingSecurityAllowed() && PreSaveDeclaration(Declaration) && CheckDeclarationHasEntries(Declaration))
			{
				SendMessage(Declaration);
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected override string GenerateEntriesMenuOptionText => Res.GetString("D90FFAB9-BA1A-4E42-A902-8657B963FBDF", "Generate Entries (Merge)");

		ZBool IsMessagingSecurityAllowed()
		{
			if (Declaration.IsImport && !Env.Security.ImportMessaging.IsAllowed)
			{
				Env.Security.ImportMessaging.ShowError();
				return false;
			}

			if (Declaration.IsExport && !Env.Security.ExportMessaging.IsAllowed)
			{
				Env.Security.ExportMessaging.ShowError();
				return false;
			}

			return true;
		}

		bool CheckDeclarationHasEntries(JobDeclaration declaration)
		{
			bool result = true;
			if (declaration.CustomsEntryHeaders.Count == 0)
			{
				Globals.Message.Show(Res.GetString("9BBD8162-FD5E-436E-8D13-360812C8A92E", "Declaration {0} has no entry – Please generate entries before attempting to send a message.", declaration.JE_DeclarationReference));
				result = false;
			}
			return result;
		}

		void SendMessage(JobDeclaration declaration)
		{
			bool continueWithSend = true;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);

			using (var form = new MessageSendingForm(decWrapper))
			{
				continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}

			if (continueWithSend)
			{
				var selectedEntryHeader = decWrapper.SendingObjectsCollection.Where(x => x.ShouldSend);
				foreach (var objectToSend in selectedEntryHeader)
				{
					var messageBuilder =
						objectToSend.MessageType == ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest
						? new ILDEC275MessageBuilder(objectToSend.Header)
						: (ILMessageBuilderBase)new ILDEC751MessageBuilder(objectToSend.Header);
					var messageManager = new BasicMessageManager(objectToSend.Header, messageBuilder, objectToSend.MessageType);
					messageManager.SendMessage(new SendsMessagesToCustomsGUI());
				}
			}
		}
	}
}
