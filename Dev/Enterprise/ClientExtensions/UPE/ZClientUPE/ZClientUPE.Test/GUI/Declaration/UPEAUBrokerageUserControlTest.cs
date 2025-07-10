using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class UPEAUBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestUPSTabPage()
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			using (ZForm form = new ZForm(uPEJobDeclaration))
			using (UPEAUBrokerageUserControl userControl = new UPEAUBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				userControl.JobDeclaration = Factory.New<UPEJobDeclaration>();
				form.Show();
				AssertEquals(true, userControl.MainTabControl.TabPages.Contains(userControl.UPSTabPage));
				userControl.MainTabControl.SelectedTab = userControl.UPSTabPage;
				AssertNotNull("UPSUserControl  has Loaded", userControl.UPSUserControlForTest);
				AssertEquals("UPSUserControl has dock still of fill", DockStyle.Fill, userControl.UPSUserControlForTest.Dock);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
