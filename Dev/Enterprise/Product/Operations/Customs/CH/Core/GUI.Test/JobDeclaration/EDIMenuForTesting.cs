using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class EDIMenuForTesting : EDIMenu
{
	public new bool DisplayGenerateEntriesMenuOption => base.DisplayGenerateEntriesMenuOption;

	public new MenuItem sendToCustomsMenuItem => base.sendToCustomsMenuItem;

	internal ZMenuItem SendToCustomsMenuItem => (ZMenuItem)MenuItems.FindByText("Send to Customs");

	internal ZMenuItem SendDocumentsMenuItem => (ZMenuItem)MenuItems.FindByText("Send Accompanying Documents (eBD)");

	internal ZMenuItem EComplaintMenuItem => (ZMenuItem)MenuItems.FindByText("ECom");

	internal ZMenuItem EvvMenuItem => (ZMenuItem)MenuItems.FindByText("Electronic Assessment Decision (eVV)");

	public new BaseMessageSendingObjectParent CreateNewMessageSendingObjectParent(JobDeclaration declaration) => base.CreateNewMessageSendingObjectParent(declaration);
}
