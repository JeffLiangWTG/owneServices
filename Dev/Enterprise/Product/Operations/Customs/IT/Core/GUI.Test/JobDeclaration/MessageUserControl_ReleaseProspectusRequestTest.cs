using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_ReleaseProspectusRequestTest : TestCaseWithFactory
{
	public void TestContextMenuIsAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		_ = Factory.NewCusEntryNumber(entryHeader, "CLR", "XYZ123", ZDateTime.Now);

		using var form = new ZForm(declaration);
		using var messageUserControl = new MessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var entriesBoundGrid = messageUserControl.EntriesBoundGrid;
		var requestToCustomsMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Request to Customs");
		var releaseProspectusRequestMenuItem = requestToCustomsMenuItem.MenuItems.FindByText("Release prospectus");

		entriesBoundGrid.Select(0);
		requestToCustomsMenuItem.ShowPopupMenu();
		AssertEquals("Visible", expected: true, releaseProspectusRequestMenuItem.Visible);
		AssertEquals("Enabled", expected: true, releaseProspectusRequestMenuItem.Enabled);
	}
}
