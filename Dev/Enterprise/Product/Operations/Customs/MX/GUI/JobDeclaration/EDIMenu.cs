using System;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		internal ZMenuItem SendToCustomsMenuItem;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			SendToCustomsMenuItem = new ZMenuItem(Res.GetString("43012B42-378F-41CA-874D-86049AC783AD", "Send to Customs"), SendMessageMenuItem_Click);
			MenuItems.Add(SendToCustomsMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SendToCustomsMenuItem.Visible = !(Declaration?.IsInterface ?? false);
		}

		protected void SendMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
			{
				var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(Declaration);
				if (messageSendingObjectParent != null)
				{
					using (var form = new JobDeclarationMessageSendingForm(messageSendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
						{
						}
					}
				}
			}
		}

		bool DeclarationHasEntry(JobDeclaration declaration)
		{
			if (declaration == null || declaration.ActiveEntryHeaders.Count == 0)
			{
				Globals.Message.Show(Res.GetString("8057478D-E9BF-425C-AA20-49480AEA644D", "Declaration {0} has no entry.", declaration.JE_DeclarationReference));
				return false;
			}
			return true;
		}
	}
}
