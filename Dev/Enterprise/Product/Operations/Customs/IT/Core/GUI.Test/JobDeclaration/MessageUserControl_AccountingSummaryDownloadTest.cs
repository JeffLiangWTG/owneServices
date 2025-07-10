using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_AccountingSummaryDownloadTest : TestCaseWithFactory
{
	public void TestAccountingSummaryDownloadMenuItem()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using var form = new ZForm(declaration);
		using var control = new MessageUserControl();
		form.Controls.Add(control);
		form.Show();

		Func<MenuItem> getMenuItem = () =>
		{
			var grid = control.EntriesBoundGrid;
			var ctxMenu = grid.ContextMenu;
			grid.Select(0);
			ctxMenu.DoPopup();

			var requestToCustoms = ctxMenu.MenuItems.FindByText("Request to Customs");
			AssertNotNull("Request to Customs menu item should not be null", requestToCustoms);
			requestToCustoms.ShowPopupMenu();

			var menuItem = requestToCustoms.MenuItems.FindByText("Accounting Summary Download");
			AssertNotNull("Accounting Summary Download menu item should not be null", menuItem);

			return menuItem;
		};

		CombineAssertions("Accounting Summary Download menu item for export declaration", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var menuItem = getMenuItem();

			Assert("Accounting Summary Download menu item should not be visible", !menuItem.Visible);
			Assert("Accounting Summary Download menu item should not be enabled", !menuItem.Enabled);
		});

		CombineAssertions("Accounting Summary Download menu item for import declaration", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var menuItem = getMenuItem();

			Assert("Accounting Summary Download menu item should be visible", menuItem.Visible);
			Assert("Accounting Summary Download menu item should not be enabled", !menuItem.Enabled);
		});

		CombineAssertions("Accounting Summary Download menu item for import declaration with processed account summary request", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = "PRR";
			message.EM_ReceiveTransmit = "RCV";

			var menuItem = getMenuItem();

			Assert("Accounting Summary Download menu item should be visible", menuItem.Visible);
			Assert("Accounting Summary Download menu item should be enabled", menuItem.Enabled);
		});
	}
}
