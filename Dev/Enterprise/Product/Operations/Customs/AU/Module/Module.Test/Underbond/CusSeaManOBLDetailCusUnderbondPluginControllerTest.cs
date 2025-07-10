using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CusSeaManOBLDetailCusUnderbondPluginController))]
	sealed class CusSeaManOBLDetailCusUnderbondPluginControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugin()
		{
			var controller = new CusSeaManOBLDetailCusUnderbondPluginController();
			var tranHead = Factory.New<CusSeaManTranHead>();

			using (var plugin = controller.GetPlugInInternal(tranHead))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		public void TestPlugInCaption()
		{
			var controller = new CusSeaManOBLDetailCusUnderbondPluginController();
			AssertEquals("Customs Underbond Movement", controller.PluginTabPageCaption.Caption);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CusSeaManOBLDetailCusUnderbondPluginController;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "124";
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "TestVesselName";
			vessel.RV_LloydsNumber = "12345";
			tranHead.BT_VesselName = vessel.RV_Code;
			return tranHead;
		}
	}
}
