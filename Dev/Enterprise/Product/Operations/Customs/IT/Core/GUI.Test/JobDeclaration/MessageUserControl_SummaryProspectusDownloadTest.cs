using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_SummaryProspectusDownloadTest : TestCaseWithFactory
{
	public void TestSummaryProspectusDownloadMenuItem()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var message = entryHeader.Messages.AddNew();
		message.EM_MessageType = "SPR";
		message.EM_ReceiveTransmit = "RCV";

		using var form = new ZForm(declaration);
		using var messageUserControl = new MessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();
		var entriesBoundGrid = messageUserControl.EntriesBoundGrid;
		var requestToCustomsMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Request to Customs");
		AssertNotNull("Request to Customs menu item should not be null", requestToCustomsMenuItem);

		var summaryProspectusDownloadMenuItem = requestToCustomsMenuItem.MenuItems.FindByText("Summary Prospectus Download");
		AssertNotNull("Summary Prospectus Download menu item should not be null", summaryProspectusDownloadMenuItem);

		entriesBoundGrid.Select(0);
		requestToCustomsMenuItem.ShowPopupMenu();
		AssertEquals("Visible", expected: true, summaryProspectusDownloadMenuItem.Visible);
		AssertEquals("Enabled", expected: true, summaryProspectusDownloadMenuItem.Enabled);
	}
}
