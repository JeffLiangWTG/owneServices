using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_IvistoRequestTest : TestCaseWithFactory
{
	public void TestContextMenuIsAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123434");
		_ = Factory.NewCusEntryNumber(entryHeader, "CLR", "XYZ123", ZDateTime.Now);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			using var form = new ZForm(declaration);
			using var messageUserControl = new MessageUserControl();
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesBoundGrid = messageUserControl.EntriesBoundGrid;
			var requestToCustomsMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Request to Customs");
			var ivistoRequestMenuItem = requestToCustomsMenuItem.MenuItems.FindByText("IVISTO Request");

			entriesBoundGrid.Select(0);
			requestToCustomsMenuItem.ShowPopupMenu();
			AssertEquals("Visible", true, ivistoRequestMenuItem.Visible);
			AssertEquals("Enabled", true, ivistoRequestMenuItem.Enabled);
		}
	}
}
