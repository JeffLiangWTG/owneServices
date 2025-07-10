using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI.Testing
{
	public class UPEAirCargoMasterMenuTest : TestCaseWithFactory
	{
		public void TestBaseClassOverriddenForClient()
		{
			string message = "No constructors should exist on the base class, factory method New() should be called";
			AssertEquals(message, 0, typeof(AirCargoMasterMenu).GetConstructors().Length);
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			using (AirCargoMasterMenu menu = AirCargoMasterMenu.New(uPECusMAWB, new CusMAWBMessageManager(delegate
			{
				return uPECusMAWB;
			})))
			{
				AssertEquals("The UPE client-specific menu should be created", typeof(UPEAirCargoMasterMenu), menu.GetType());
			}
		}

		public void TestShowMatchingStatisticsMenuItem()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.LogMatchEvent(true, "");
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.LogMatchEvent(false, "");
			uPECusHAWB.LogMatchEvent(true, "");
			uPECusHAWB.LogMatchEvent(true, "");
			using (UPEAirCargoMasterMenu menu = (UPEAirCargoMasterMenu)AirCargoMasterMenu.New(uPECusMAWB, new CusMAWBMessageManager(delegate
			{
				return uPECusMAWB;
			})))
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
				menu.MatchingStatisticsMenuItem.PerformClick();
				AssertEquals("Total Housebills = 2\nTotal AutoMatches = 3\nTotal Manual Matches = 1", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
