using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_Eur1RequestTest : TestCaseWithFactory
{
	public void TestContextMenuIsAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123434");

		using var form = new ZForm(declaration);
		using var messageUserControl = new MessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var entriesBoundGrid = messageUserControl.EntriesBoundGrid;
		var requestToCustomsMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Request to Customs");
		var eur1RequestMenuItem = requestToCustomsMenuItem.MenuItems.FindByText("EUR1 Request");

		entriesBoundGrid.Select(0);
		requestToCustomsMenuItem.ShowPopupMenu();
		AssertEquals("Visible", true, eur1RequestMenuItem.Visible);
		AssertEquals("Enabled", true, eur1RequestMenuItem.Enabled);
	}
}
