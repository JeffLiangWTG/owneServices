using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT.GUI.Testing
{
	public class TNTAirCargoMasterMenuTest : TestCaseWithFactory
	{
		public void TestBaseClassOverriddenForClient()
		{
			AssertEquals("No constructors should exist on the base class, factory method New() should be called", 0, typeof(AirCargoMasterMenu).GetConstructors().Length);
			AssertEquals("No constructors should exist on the class, factory method New() should be called", 0, typeof(TNTAirCargoMasterMenu).GetConstructors().Length);
			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(CusMAWB, MessageManager))
			{
				AssertEquals("The TNT client-specific menu should be created", typeof(TNTAirCargoMasterMenu), menu.GetType());
			}

			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(Consol, MessageManager))
			{
				AssertEquals("The TNT client-specific menu should be created", typeof(TNTAirCargoMasterMenu), menu.GetType());
			}
		}

		public void TestAutoCreateDeclarationsMenuItem_ShowsErrorIfMasterbillUnsaved()
		{
			CusMAWB.CM_Folio = "12";
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Menu, new object[] { EventArgs.Empty });
			MenuItem createDeclarationsMenuItem = FindMenuItem(Menu.MenuItems, TNTAirCargoMasterMenu.CreateDeclarationsMenuHeading);
			AssertNotNull("CreateDeclarationsMenuItem should not be null", createDeclarationsMenuItem);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			createDeclarationsMenuItem.PerformClick();
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Message:" + System.Environment.NewLine + lastMessage, "You must save the form before you can create declarations.", lastMessage);
		}

		#region Implementation
		MenuItem FindMenuItem(Menu.MenuItemCollection menuItems, string menuItemText)
		{
			MenuItem result = null;
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == menuItemText)
				{
					result = item;
				}
			}

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			CusMAWB = Factory.New<CusMAWB>();
			CusMAWB.ChildBills.AddNew();
			MessageManager = new CusMAWBMessageManager(delegate
			{
				return CusMAWB;
			});
			Menu = AirCargoMasterMenu.New(CusMAWB, MessageManager);
		}

		protected override void TearDown()
		{
			Menu.Dispose();
			base.TearDown();
		}

		CusMAWB CusMAWB;
		ForwardingConsol Consol;
		CusMAWBMessageManager MessageManager;
		AirCargoMasterMenu Menu;
		#endregion
	}
}
