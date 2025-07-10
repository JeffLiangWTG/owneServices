using System;
using Enterprise.Customs.MY.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MY.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get => (JobDeclaration)base.Declaration;
			set => base.Declaration = value;
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Add(new ZMenuItem("Send Message", new EventHandler(SendMessageMenuItem_Click)));
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected void SendMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				Declaration.MessageManager.DeclareDeclaration(new Customs.GUI.SendsMessagesToCustomsGUI());
			}
		}
	}
}
