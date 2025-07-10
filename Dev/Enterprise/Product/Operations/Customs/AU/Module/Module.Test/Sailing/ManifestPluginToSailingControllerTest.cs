using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Sailing.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ManifestPluginToSailingController))]
	sealed class ManifestPluginToSailingControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugin()
		{
			var controller = new ManifestPluginToSailingControllerForTest();
			var voyage = Factory.New<JobVoyage>();

			using (var plugin = controller.GetPlugIn(voyage))
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("Plugin is of type ManifestPluginToSailing", typeof(ManifestPluginToSailing), plugin.GetType());
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.ManifestPluginToSailingController;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var voyage = base.GetBusinessObjectWithoutValidationErrors() as JobVoyage;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);
			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations.Add(destination);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel1";
			var line = Factory.New<OrgHeader>();
			line.OH_Code = "RAILLINE";
			line.OH_IsShippingProvider = true;
			line.OH_IsLineHaulProvider = true;
			vessel.RV_OH = line.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";
			return voyage;
		}

		protected override IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest => new ControllerID[]
		{
			ControllerIDs.AgencyAllocation,
			ControllerIDs.AgencyPortMessaging,
			ControllerIDs.AgencyNZPortMessaging,
			ControllerIDs.AgencyDangerousGoodsManifest,
			ControllerIDs.Customs.US.StowPlan,
			ControllerIDs.Customs.AU.ManifestPluginToSailingController,
			ControllerIDs.ETerminalReleaseManifestPortMessaging
		}.Union(base.NonCustomsPlugInsToExcludeFromTest);

		sealed class ManifestPluginToSailingControllerForTest : ManifestPluginToSailingController
		{
			public new ZPlugIn GetPlugIn(IBusiness businessEntity) => base.GetPlugIn(businessEntity);
		}
	}
}
