using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Module.Testing
{
	[TestedType(typeof(DENctsMovementController))]
	sealed class DENctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestPlugin()
		{
			var consol = Factory.New<ForwardingConsol>();
			var getPlugInMethod = Controller.GetType().GetMethod("GetPlugIn", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var plugIn = (ZPlugIn)getPlugInMethod.Invoke(Controller, new object[] { consol }))
			{
				AssertType<GUI.NctsPlugin>(plugIn);
			}
		}

		public void TestForm_NC5_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var getPlugInMethod = Controller.GetType().GetMethod("GetForm", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var form = (IDisposable)getPlugInMethod.Invoke(Controller, new object[] { nctsHeader }))
			{
				AssertType<Phase5DepartureMovementForm>(form);
			}
		}

		public void TestForm_NC5_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var getPlugInMethod = Controller.GetType().GetMethod("GetForm", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var form = (IDisposable)getPlugInMethod.Invoke(Controller, new object[] { nctsHeader }))
			{
				AssertType<Phase5ArrivalMovementForm>(form);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();
			return nctsHeader;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
