using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(IvistoRequestGridContextMenuComponent))]
sealed class IvistoRequestGridContextMenuComponentTest : GridContextMenuItemComponentAbstractTest<CusEntryHeader>
{
	public void TestConstructor_ParentMenuItem()
	{
		using var entriesGrid = new EntriesGridForTest(declaration);
		AssertExceptionThrown<ArgumentNullException>("parentMenuItem parameter is required", () => new IvistoRequestGridContextMenuComponent(entriesGrid, null));
	}

	public void TestMenuItemVisibility()
	{
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new IvistoRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			AssertNotNull("MenuItem", menuItem);

			declaration.JE_MessageType = "IMP";
			parentMenuItem.ShowPopupMenu();
			AssertEquals("When declaration is not export, Visible", false, menuItem.Visible);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = "EXP";
				parentMenuItem.ShowPopupMenu();
				AssertEquals("When declaration is export non UCC6, Visible", false, menuItem.Visible);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = "EXP";
				parentMenuItem.ShowPopupMenu();
				AssertEquals("When declaration is export and UCC6, Visible", true, menuItem.Visible);
			}
		}
	}

	public void TestMenuItemEnabled()
	{
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new IvistoRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			AssertNotNull("MenuItem", menuItem);

			parentMenuItem.ShowPopupMenu();
			AssertEquals("When entry does not have MRN nor ReleaseCode, Enabled", false, menuItem.Enabled);

			entryHeader.MovementReferenceNumberSetter("REF1234433");
			parentMenuItem.ShowPopupMenu();
			AssertEquals("When entry has MRN but not ReleaseCode, Enabled", false, menuItem.Enabled);

			Factory.NewCusEntryNumber(entryHeader, "CLR", "XYZ123", ZDateTime.Now);
			parentMenuItem.ShowPopupMenu();
			AssertEquals("When entry has both MRN and ReleaseCode, Enabled", true, menuItem.Enabled);
		}
	}

	public void TestCreateIvistoRequestMessage()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader.MovementReferenceNumberSetter("MRN12345");
		Factory.NewCusEntryNumber(entryHeader, "CLR", "XYZ123", ZDateTime.Now);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var entriesGrid = new EntriesGridForTest(declaration))
		{
			var parentMenuItem = entriesGrid.ContextMenu.MenuItems.Add("Request to Customs");
			new IvistoRequestGridContextMenuComponentForTest(entriesGrid, parentMenuItem: parentMenuItem).Initialize();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var menuItem = parentMenuItem.MenuItems.FindByText(MenuItemText);
			menuItem.PerformClick();
			AssertEquals("IVISTO Request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = entryHeader.Messages.GetLastMessageByType("IVI");
			AssertNotNull("Ivisto Message", ivistoMessage);
		}
	}

	protected override string MenuItemText => "IVISTO Request";

	protected override GridContextMenuItemComponent<CusEntryHeader> GetNewContextMenuItemComponent(ZGrid grid)
		=> new IvistoRequestGridContextMenuComponent(grid, new MenuItem("Request to Customs"));

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}

sealed class IvistoRequestGridContextMenuComponentForTest : IvistoRequestGridContextMenuComponent
{
	public IvistoRequestGridContextMenuComponentForTest(ZGrid grid, MenuItem parentMenuItem = null) : base(grid, parentMenuItem)
	{
	}

	protected override IvistoRequestMessageFactory GetNewIvistoRequestMessageFactory()
	{
		return new IvistoRequestMessageFactoryForTest();
	}
}
