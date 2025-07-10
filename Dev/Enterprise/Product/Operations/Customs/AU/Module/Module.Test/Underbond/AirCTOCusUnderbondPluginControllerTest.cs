using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOCusUnderbondPluginController))]
	sealed class AirCTOCusUnderbondPluginControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugin()
		{
			AirCTOCusUnderbondPluginController controller = new AirCTOCusUnderbondPluginController();
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();

			using (ZPlugIn plugin = controller.GetPlugInInternal(mAWB))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type CusUnderbondPlugin", typeof(GUI.CMRCusUnderbondPlugin), plugin.GetType());
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var hawb = Factory.NewWithValidTestData<CTOCusHAWB>();
			var mawb = Factory.NewWithValidTestData<CTOCusMAWB>();
			hawb.CS_CM = mawb.PK;
			mawb.CM_FlightNo = "1";
			Factory.Save();
			return hawb;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCTOCusUnderbondPluginController;

		protected override Type GetBusinessObjectType() => typeof(CTOCusMAWB);
	}
}
