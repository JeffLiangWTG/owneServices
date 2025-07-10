using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	public class CINTemporyStorageUserControlForPluginTest : TestCaseWithFactory
	{
		public void TestNoCurrentDataItem()
		{
			using (var frm = new ZForm())
			{
				var userControl = GetCINTemporyStorageUserControlForPlugin();
				frm.Controls.Add(userControl);
				frm.Show();
				var coveringLabel = userControl.FindSingle<ZLabel>(c => c.Name == "CoveringLabel");
				Assert(!coveringLabel.Visible);
				SelectDeclarationTabPage(frm);
				Assert(coveringLabel.Visible);
				AssertEquals(string.Format("You have chosen not to create a Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Declaration."), coveringLabel.Text);
			}
		}

		public void TestDecPanelVisible()
		{
			using (var frm = new ZForm())
			{
				var userControl = GetCINTemporyStorageUserControlForPlugin();
				frm.Controls.Add(userControl);
				frm.Show();
				userControl.SetDataBinding(header, ZString.Empty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				SelectDeclarationTabPage(frm);
				Assert(!userControl.FindSingle<ZLabel>(c => c.Name == "CoveringLabel").Visible);
				var decPanel = userControl.FindSingle<ZPanel>(c => c.Name == "DeclarationPanel");
				AssertNotNull(decPanel);
				Assert(decPanel.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
			header = CusTempStorageJobHeader.New(Factory, AppCode);
			header.SJH_OH_Customer = customerOrg.PK;
		}

		void SelectDeclarationTabPage(ZForm frm)
		{
			var tabControl2 = frm.FindSingle<ZTabControl>(c => c.Name == "MainTabControl");
			var tabPage2 = frm.FindSingle<ZTabPage>(c => c.Name == "EntrySummaryDeclarationTabPage");
			tabControl2.SelectedTab = tabPage2;
			var tabControl = frm.FindSingle<ZTabControl>(c => c.Name == "EntrySummaryDeclarationTabControl");
			var tabPage = frm.FindSingle<ZTabPage>(c => c.Name == "DeclarationTabPage");
			tabControl.SelectedTab = tabPage;
		}

		protected virtual CINTemporyStorageUserControlForPlugin GetCINTemporyStorageUserControlForPlugin() => new CINTemporyStorageUserControlForPlugin();

		protected virtual ZString AppCode => FRConstants.TemporaryStorage.AppCodeFRC;

		CusTempStorageJobHeader header;
	}
}
