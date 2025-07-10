using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessagesTabUserControl_EADRequestTest : TestCaseWithFactory
{
	public void TestEADRequestMenuItemAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN123434");

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			using var form = new ZForm(declaration);
			using var messageUserControl = new MessageUserControl();
			form.Controls.Add(messageUserControl);
			form.Show();

			var entriesBoundGrid = messageUserControl.EntriesBoundGrid;
			var requestToCustomsMenuItem = entriesBoundGrid.ContextMenu.MenuItems.FindByText("Request to Customs");
			var ucc6EADRequestMenuItem = requestToCustomsMenuItem.MenuItems.FindByText("EAD Request");

			entriesBoundGrid.Select(0);
			requestToCustomsMenuItem.ShowPopupMenu();
			AssertEquals("Visible", true, ucc6EADRequestMenuItem.Visible);
			AssertEquals("Enabled", true, ucc6EADRequestMenuItem.Enabled);
		}
	}
}
