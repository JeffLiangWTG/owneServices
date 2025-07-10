using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.Customs.AU.Sailing.GUI.Testing
{
	sealed class ArrivalPluginToSailingTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestPluginAlwaysAllowed()
		{
			AssertEquals("CheckPoint", Env.Licence.AlwaysAllow, ((IPlugInInternals)plugin).LicenceCheckPoint);
		}

		public void TestName()
		{
			AssertEquals("Name", "Arrival Reporting", plugin.Name);
		}

		public void TestManager()
		{
			AssertEquals("Manager.GetType", typeof(JobVoyageMessageManager), plugin.Manager.GetType());
		}

		public void TestTopLevelMenu()
		{
			AssertEquals("TopLevelMenu", typeof(ArrivalPluginMenu), plugin.TopLevelMenu.GetType());
		}

		public void TestNotEnabledForSea()
		{
			Host.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			using (var plugin = new ArrivalPluginToSailing(Host))
			{
				AssertEquals("Enabled", false, plugin.Enabled);
			}
		}

		public void TestDeleteRecord()
		{
			Host.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			using (var plugin = new ArrivalPluginToSailing(Host))
			{
				if (plugin.CanDelete)
				{
					plugin.Delete();
				}
			}

			Assert(Host.Voyage.IsDeleted);
		}

		public void TestCannotDeleteRecordWithMessages()
		{
			Host.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			var message = Factory.New<CMRAIRCRMessage>();
			Host.Voyage.Messages.Add(message);
			using (var plugin = new ArrivalPluginToSailing(Host))
			{
				if (plugin.CanDelete)
				{
					plugin.Delete();
				}

				AssertEquals("Cannot delete this record as it has messages associated with it and it needs to be retained for audit trail purposes.", plugin.CannotDeleteMessage);
			}

			Assert(!Host.Voyage.IsDeleted);
		}

		public void TestNotEnabledForLegacy()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Host.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			using (var plugin = new ArrivalPluginToSailing(Host))
			{
				AssertEquals("Enabled", false, plugin.Enabled);
			}
		}

		public void TestEnabledForAirCMR()
		{
			Host.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			using (var plugin = new ArrivalPluginToSailing(Host))
			{
				AssertEquals("Enabled", true, plugin.Enabled);
			}
		}

		public void TestHostAndBusinessEntityDifferent()
		{
			AssertEquals("host", typeof(JobVoyage), ((IPlugInInternals)plugin).HostBusinessEntity.GetType());
			AssertEquals("business", typeof(CustomsJobVoyageWrapper), plugin.BusinessEntity.GetType());
		}

		protected override ZPlugIn GetPlugInToTest() => new ArrivalPluginToSailing(new CustomsJobVoyageWrapper(Factory.New<JobVoyage>()));

		ArrivalPluginToSailing plugin;
		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			plugin = new ArrivalPluginToSailing(Host);
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugin.Dispose();
		}

		CustomsJobVoyageWrapper host;
		CustomsJobVoyageWrapper Host
		{
			get
			{
				if (host == null)
				{
					host = new CustomsJobVoyageWrapper(Factory.New<JobVoyage>());
				}

				return host;
			}
		}
	}
}
