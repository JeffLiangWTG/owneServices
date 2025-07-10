using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.GUI.Testing
{
	public class TNTAirCargoShipmentMenuTest : TestCaseWithFactory
	{
		public void TestBaseClassOverriddenForClient()
		{
			AssertEquals("No constructors should exist on the base class, factory method New() should be called", 0, typeof(AirCargoShipmentMenu).GetConstructors().Length);
			AssertEquals("No constructors should exist on the class, factory method New() should be called", 0, typeof(TNTAirCargoShipmentMenu).GetConstructors().Length);
			using (AirCargoShipmentMenu menu = AirCargoShipmentMenu.New(MessageManager))
			{
				AssertEquals("The TNT client-specific menu should be created", typeof(TNTAirCargoShipmentMenu), menu.GetType());
			}
		}

		public void TestCreateFormalDecMenuItem_ShowsErrorIfAirCargoUnsaved()
		{
			CusHAWB.CS_GoodsDescription = "Air cargo has been changed by user";
			AssertClick("You must save the form before you can create a Formal Declaration.");
		}

		public void TestCreateFormalDecMenuItem_ShowsErrorIfAirCargoAlreadyHasDeclaration()
		{
			Customs.Business.BaseJobDeclaration declaration = Factory.New<Customs.Business.BaseJobDeclaration>();
			CusHAWB.CS_GoodsDescription = "Air cargo has declaration";
			CusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			Factory.Save();
			AssertClick(string.Format("Formal Declaration ({0}) is already exist for this {1}.", declaration[JobDeclarationSchema.JE_DeclarationReference.Name].ToString(), CusHAWB.UnderbondHumanReadableName));
		}

		public void TestCreateDeclarationFromAirCargo()
		{
			CusHAWB.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Factory.Save();
			int declarationCount = Factory.GetDatabaseCount(typeof(Customs.Business.BaseJobDeclaration));
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Menu, new object[] { EventArgs.Empty });
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu.MenuItems, TNTAirCargoShipmentMenu.CreateFormalMenuHeading);
			AssertNotNull("CreateFormalDecMenuItem should not be null", createFormalDecMenuItem);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			createFormalDecMenuItem.PerformClick();
			AssertEquals("1 new Declaration should have been created", declarationCount + 1, Factory.GetDatabaseCount(typeof(Customs.Business.BaseJobDeclaration)));
			AssertNotNull("CusHAWB should have a Declaration linked to it", CusHAWB.Declaration);
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Message:" + System.Environment.NewLine + lastMessage, string.Format("Formal Declaration ({0}) was successfully created.", CusHAWB.Declaration[JobDeclarationSchema.JE_DeclarationReference.Name].ToString()), lastMessage);
		}

		#region Implementation
		void AssertClick(ZString expectedMessage)
		{
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Menu, new object[] { EventArgs.Empty });
			MenuItem createFormalDecMenuItem = FindMenuItem(Menu.MenuItems, TNTAirCargoShipmentMenu.CreateFormalMenuHeading);
			AssertNotNull("CreateFormalDecMenuItem should not be null", createFormalDecMenuItem);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			createFormalDecMenuItem.PerformClick();
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Message:" + System.Environment.NewLine + lastMessage, expectedMessage, lastMessage);
		}

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
			CusMAWB = Factory.New<CusMAWB>();
			CusHAWB = CusMAWB.ChildBills.AddNew();
			MessageManager = new CusHAWBMessageManager(delegate
			{
				return CusHAWB;
			});
			Menu = TNTAirCargoShipmentMenu.New(MessageManager);
		}

		protected override void TearDown()
		{
			Menu.Dispose();
			base.TearDown();
		}

		CusMAWB CusMAWB;
		CusHAWB CusHAWB;
		CusHAWBMessageManager MessageManager;
		AirCargoShipmentMenu Menu;
		#endregion
	}
}
