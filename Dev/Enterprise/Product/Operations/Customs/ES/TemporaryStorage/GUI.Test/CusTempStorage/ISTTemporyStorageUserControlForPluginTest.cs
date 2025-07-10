using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class ISTTemporyStorageUserControlForPluginTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNoCurrentDataItem()
		{
			using (var frm = new ZForm())
			{
				var userControl = GetISTTemporyStorageUserControlForPlugin();
				frm.Controls.Add(userControl);
				frm.Show();
				var coveringLabel = userControl.FindSingle<ZLabel>(c => c.Name == "CoveringLabel");
				Assert(!coveringLabel.Visible);
				SelectDeclarationTabPage(frm);
				Assert(coveringLabel.Visible);
				AssertEquals(string.Format("You have chosen not to create a Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Declaration."), coveringLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestDecPanelVisible()
		{
			using (var frm = new ZForm())
			{
				var userControl = GetISTTemporyStorageUserControlForPlugin();
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

		[RequiresSTA]
		public void TestTemporaryStorageControlType()
		{
			using (var userControl = GetISTTemporyStorageUserControlForPlugin())
			{
				AssertType<TemporaryStorageHeaderUserControl>(userControl.FindSingle<ZUserControl>(c => c.Name == "TemporaryStorageHeaderUserControl"));
			}
		}
		[RequiresSTA]
		public void TestMessagesUserControlType()
		{
			using (var userControl = GetISTTemporyStorageUserControlForPlugin())
			{
				AssertType<MessagesUserControl>(userControl.FindSingle<ZUserControl>(c => c.Name == "MessagesUserControl"));
			}
		}

		[RequiresSTA]
		public void TestTemporyStorageDecUserControlForPluginType()
		{
			using (var userControl = GetISTTemporyStorageUserControlForPlugin())
			{
				AssertType(typeof(ISTCusTempStorageDecUserControl), userControl.FindSingle<ZUserControl>(c => c.Name == "CusDecTabPageUserControl"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
			header = CusTempStorageJobHeader.New(Factory);
			header.SJH_OH_Customer = customerOrg.PK;
		}

		CusTempStorageJobHeader header;

		void SelectDeclarationTabPage(ZForm frm)
		{
			var tabControl2 = frm.FindSingle<ZTabControl>(c => c.Name == "MainTabControl");
			var tabPage2 = frm.FindSingle<ZTabPage>(c => c.Name == "EntrySummaryDeclarationTabPage");
			tabControl2.SelectedTab = tabPage2;
			var tabControl = frm.FindSingle<ZTabControl>(c => c.Name == "EntrySummaryDeclarationTabControl");
			var tabPage = frm.FindSingle<ZTabPage>(c => c.Name == "DeclarationTabPage");
			tabControl.SelectedTab = tabPage;
		}

		ISTTemporyStorageUserControlForPlugin GetISTTemporyStorageUserControlForPlugin() => new ISTTemporyStorageUserControlForPlugin();
	}
}
