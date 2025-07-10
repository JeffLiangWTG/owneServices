using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageForm))]
	class SumAFormTest : ZFormBasherTest
	{
		public void TestFormForREXAndSumA()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();

			System.Action<string, string, string, bool, bool> assertFormDesigner = (appCode, caption, mainTabText, decVisible, rexDecVisible) =>
			{
				header.SJH_AppCode = appCode;

				using (var frm = new TemporaryStorageForm(header))
				{
					AssertContains($"Should be {caption} when the AppCode is {appCode}", caption, frm.FormCaption);
					var mainTabPage = (ZTabPage)frm.Controls.Find("MainTabPage", true)[0];
					AssertNotNull(mainTabPage);
					AssertContains($"Should be {mainTabText} when the AppCode is {mainTabText}", mainTabText, mainTabPage.Text);

					var mainTabControl = (TabControl)frm.Controls["MainPanel"].Controls["MainTabControl"];
					var decTabPage = (ZTabPage)mainTabControl.TabPages["SumADeclarationTabPage"];
					var ammendmentsTabPage = (ZTabPage)mainTabControl.TabPages["AmendmentsTabPage"];
					var rexdisMessagesTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(x => x.Name == "REXDISMessagesTabPage");
					if (decVisible)
					{
						AssertNotNull(decTabPage);
						AssertNotNull(ammendmentsTabPage);
					}
					else
					{
						AssertNull(decTabPage);
						AssertNull(ammendmentsTabPage);
					}

					var rexDecTabPage = (ZTabPage)mainTabControl.TabPages["RexDeclarationTabPage"];
					if (rexDecVisible)
					{
						AssertNotNull(rexDecTabPage);
					}
					else
					{
						AssertNull(rexDecTabPage);
					}
					if (appCode == "SUM")
					{
						AssertNull(rexdisMessagesTabPage);
					}
					else if (appCode == "REX")
					{
						AssertNotNull(rexdisMessagesTabPage);
						AssertEquals(rexdisMessagesTabPage.TabVisible, true);
						var workflowTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(x => x.Name == "WorkflowTabPage");
						AssertNotNull(workflowTabPage);
						var notesTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(x => x.Name == "NotesTabPage");
						AssertNotNull(notesTabPage);
						var logsTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(x => x.Name == "LogsTabPage");
						AssertNotNull(logsTabPage);
						var tabPages = new ZTabPage[] { mainTabPage, rexDecTabPage, rexdisMessagesTabPage, workflowTabPage, notesTabPage, logsTabPage };
						for (int i = 0; i < tabPages.Length; i++)
						{
							AssertEquals($"The tab page name '{mainTabControl.TabPages[i].Name}' must be sorted in order {i} on Re-Export Declaration.", true, mainTabControl.TabPages[i] == tabPages[i]);
							AssertEquals($"The tab page name '{mainTabControl.TabPages[i].Name}' should be visibility on Re-Export Declaration Form.", true, tabPages[i].TabVisible);
						}

						var messageUserControl = rexdisMessagesTabPage.FindSingleOrDefault<REXDISMessagesUserControl>(x => x.Name == "REXDISMessagesUserControl");
						AssertEquals("REXDISCusTempStorageDec.Messages", messageUserControl.GetBindingMember());
					}
				}
			};

			assertFormDesigner(TemporaryStorageApplicationCodeList.Codes.SumA, "SumA Declaration", "SumA", true, false);
			assertFormDesigner(TemporaryStorageApplicationCodeList.Codes.REX, "Re-Export Declaration", "Re-Export", false, true);
		}

		public void TestSumADeclarationTabPageTabPageSelectedWithNoToCreate()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sumAForm.Show();
				var mainTabControl = (TabControl)sumAForm.Controls["MainPanel"].Controls["MainTabControl"];
				mainTabControl.SelectTab("SumADeclarationTabPage");
				AssertEquals("Are you sure you want to create a SumA Declaration now?", UnitTestUserNotification.Instance.LastMessage.Text);
				var declarationsTabControl = (TabControl)mainTabControl.SelectedTab.Controls["SumADeclarationTabControl"];
				var sumADecTabPage = declarationsTabControl.SelectedTab;
				var coveringlabel = sumADecTabPage.Controls["CUSPRLTabPageUserControl"].Controls["CoveringLabel"];
				AssertEquals("You have chosen not to create a SumA Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a SumA Declaration.", coveringlabel.Text);
				Assert(coveringlabel.Visible);
			}
		}

		public void TestSumADeclarationTabPageTabPageWithYesToCreate()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sumAForm.Show();
				var mainTabControl = (TabControl)sumAForm.Controls["MainPanel"].Controls["MainTabControl"];
				mainTabControl.SelectTab("SumADeclarationTabPage");
				var declarationsTabControl = (TabControl)mainTabControl.SelectedTab.Controls["SumADeclarationTabControl"];
				var sumADecTabPage = declarationsTabControl.SelectedTab;
				var cusprlControl = sumADecTabPage.Controls["CUSPRLTabPageUserControl"];
				var coveringlabel = cusprlControl.Controls["CoveringLabel"];
				Assert(!coveringlabel.Visible);
				Assert(cusprlControl.Visible);
			}
		}

		public void TestDeclarationTabPageSelected()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				sumAForm.Show();
				var mainTabControl = (TabControl)sumAForm.Controls["MainPanel"].Controls["MainTabControl"];
				mainTabControl.SelectTab("SumADeclarationTabPage");
				var declarationsTabControl = (TabControl)mainTabControl.SelectedTab.Controls["SumADeclarationTabControl"];
				declarationsTabControl.SelectTab("DeclarationMessagesTabPage");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				declarationsTabControl.SelectTab("DeclarationTabPage");
				AssertEquals("Are you sure you want to create a SumA Declaration now?", UnitTestUserNotification.Instance.LastMessage.Text);
				var sumADecTabPage = declarationsTabControl.SelectedTab;
				var coveringlabel = sumADecTabPage.Controls["CUSPRLTabPageUserControl"].Controls["CoveringLabel"];
				AssertEquals("You have chosen not to create a SumA Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a SumA Declaration.", coveringlabel.Text);
				Assert(coveringlabel.Visible);
			}
		}

		public void TestConstructMenu()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				var menu = sumAForm.Menu.MenuItems.FindByText("Messages");
				AssertNotNull(menu);
				AssertNotNull(menu.MenuItems.FindByText("SumA Declaration"));
				AssertSame(header, ((SumAMessagingMenu)menu).Header);
			}

			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.REX;
			using (var rexportForm = new TemporaryStorageForm(header))
			{
				var menu = rexportForm.Menu.MenuItems.FindByText("Messages");
				AssertNotNull(menu);
				AssertNotNull(menu.MenuItems.FindByText("Send Re-Export"));
				AssertSame(header, ((ReExportMessagingMenu)menu).Header);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			Factory.Save();
			return new TemporaryStorageForm(header)
			{
				ControllerID = ControllerIDs.Customs.TemporaryStorage
			};
		}
	}
}
