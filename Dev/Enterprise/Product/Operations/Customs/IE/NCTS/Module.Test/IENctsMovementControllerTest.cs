using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Module.Testing
{
	[TestedType(typeof(IENctsMovementController))]
	sealed class IENctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
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

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			Factory.Save();
			return nctsHeader;
		}
	}
}
