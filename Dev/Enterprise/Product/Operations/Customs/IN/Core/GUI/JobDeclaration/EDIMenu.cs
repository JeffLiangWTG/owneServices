using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
		set { base.Declaration = value; }
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		var isDeclarationInterface = Declaration?.IsInterface ?? false;
		sendMessageMenuItem.Visible = !isDeclarationInterface;
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();
		sendMessageMenuItem = new ZMenuItem(SendMessageMenuItemText, new EventHandler(SendMessageMenuItem_Click));
		MenuItems.Add(sendMessageMenuItem);
	}

	protected void SendMessageMenuItem_Click(object sender, EventArgs e)
	{
		var declaration = Declaration;
		if (declaration != null && PreSaveDeclaration(declaration) && DeclarationHasEntry(declaration))
		{
			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			SendMessageHelper.GenerateMessage(sendingObjectParent, () => new SendMessageForm(sendingObjectParent));
		}
	}

	protected override bool DisplayGenerateEntriesMenuOption => true;

	bool DeclarationHasEntry(JobDeclaration declaration)
	{
		var result = true;
		if (declaration.ActiveEntryHeaders.Count == 0)
		{
			Globals.Message.Show(Res.GetString("D3B49A0B-21A3-49D5-BF48-8E0BDB76D6B6", "Declaration {0} has no entry.", declaration.JE_DeclarationReference));
			result = false;
		}
		return result;
	}

	string SendMessageMenuItemText => Res.GetString("INJobDeclarationForm|Brokerage|SendElectronically", "ICEGATE - Send Electronically");

	ZMenuItem sendMessageMenuItem;

	protected override string GenerateEntriesMenuOptionText => Res.GetString("INJobDeclarationForm|Brokerage|GenerateEntries", "Generate Entries");
}
