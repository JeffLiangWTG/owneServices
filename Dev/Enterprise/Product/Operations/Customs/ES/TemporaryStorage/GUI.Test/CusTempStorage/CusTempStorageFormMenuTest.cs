using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class CusTempStorageFormMenuTest : TestCaseWithFactory
	{
		public void TestCreateMenuItemsBasedOnTempStorageApplicationCode()
		{
			AssertMenuItems(new string[] { "Receive into Transit Shed", "Transfer to Onward Transit Shed", "Report De-consolidation", "Send DDT" });
		}

		void AssertMenuItems(string[] expectedMenus)
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);

			using (var menu = new CusTempStorageFormMenu())
			{
				menu.Header = jobHeader;

				CombineAssertions(() =>
				{
					Assert("expectedMenus must contain at least one element", expectedMenus.Any());
					AssertEquals("MenuItems.Count", expectedMenus.Length, menu.MenuItems.Count);

					foreach (var menuCaption in expectedMenus)
					{
						AssertNotNull($"'{menuCaption}' menu item", menu.MenuItems.FindByText(menuCaption));
					}
				});
			}
		}

		public void TestReceiveIntoTransitShedMenu()
		{
			RunMenuItemTest(CreateTestJobHeader(), "Receive into Transit Shed", () =>
			{
				AssertEquals("CW1 ES doesn't yet support building messages in Temporary Storage", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestTransferToOnwardTransitShedMenu()
		{
			RunMenuItemTest(CreateTestJobHeader(), "Transfer to Onward Transit Shed", () =>
			{
				AssertEquals("CW1 ES doesn't yet support building messages in Temporary Storage", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestReportDeconsolidationMenu()
		{
			RunMenuItemTest(CreateTestJobHeader(), "Report De-consolidation", () =>
			{
				AssertEquals("CW1 ES doesn't yet support building messages in Temporary Storage", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSendDdtMenu()
		{
			RunMenuItemTest(CreateTestJobHeader(), "Send DDT", () =>
			{
				AssertEquals("CW1 ES doesn't yet support building messages in Temporary Storage", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		void RunMenuItemTest(CusTempStorageJobHeader header, string caption, Action assertionActions)
		{
			using (var frm = new CusTempStorageForm(header))
			{
				var topLevelMenu = frm.Menu.MenuItems.FindByText("Messages");
				AssertNotNull("'Messages' menu not found", topLevelMenu);
				var actionMenu = topLevelMenu.MenuItems.FindByText(caption);
				AssertNotNull($"Action menu '{caption}' not found", actionMenu);

				UnitTestUserNotification.Instance.ClearMessages();
				actionMenu.PerformClick();

				assertionActions.Invoke();
			}
		}

		CusTempStorageJobHeader CreateTestJobHeader()
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var line = jobHeader.CusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_OwnerReferenceType = "AWB";

			return jobHeader;
		}
	}
}
