using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class EDIMenuTest : TestCaseWithFactory
	{
		public void TestSetupTopLevelMenu()
		{
			AssertNotNull(ediMenu.MenuItems.FindByText("Send Message"));
		}

		public void TestGenerateEntriesMenuItem()
		{
			AssertEquals(true, ediMenu.GenerateEntriesMenuItem.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ediMenu = new EDIMenu();
		}
		EDIMenu ediMenu;
	}
}
