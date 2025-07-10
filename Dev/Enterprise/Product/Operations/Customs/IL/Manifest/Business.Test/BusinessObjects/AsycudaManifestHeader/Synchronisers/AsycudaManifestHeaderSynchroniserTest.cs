using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Test
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchroniser_ManifestNumber()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.Transports.RemoveAndDeleteAll();
			sourceConsol.Transports.AddNew("ESAAQ", "TRAMA");
			sourceConsol.Transports.AddNew("TRAMA", "ILASH");
			sourceConsol.JK_RL_NKDischargePort = "ILASH";
			var transport = sourceConsol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).LastOrDefault(x => x.JW_RL_NKDiscPort == sourceConsol.JK_RL_NKDischargePort);
			transport.JW_ArrivalPortRouteId = "12345";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.AMA_Nature = "IMP";

			AssertNotEquals("Should not have value as JW_ArrivalPortRouteId.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync the value from JW_ArrivalPortRouteId to AMA_ManifestNumber.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(false, false);
			transport.JW_RL_NKDiscPort = "ILHFA";
			sourceConsol.Transports.AddNew("ILHFA", "ILASH");

			transport = sourceConsol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).LastOrDefault(x => x.JW_RL_NKDiscPort == sourceConsol.JK_RL_NKDischargePort);
			transport.JW_ArrivalPortRouteId = "67890";

			AssertNotEquals("Should not have value as JW_ArrivalPortRouteId.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync the value from JW_ArrivalPortRouteId to AMA_ManifestNumber.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(false, false);
			manifestHeader.Synchroniser.Synchronise();
			manifestHeader.AMA_Nature = "EXP";
			transport = sourceConsol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).FirstOrDefault(x => x.JW_RL_NKLoadPort == sourceConsol.JK_RL_NKLoadPort);
			transport.JW_DeparturePortRouteId = "123";
			transport.JW_ArrivalPortRouteId = "67890";

			AssertEquals("Should sync the value from JW_ArrivalPortRouteId to AMA_ManifestNumber.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);
			AssertNotEquals("Should not have value as JW_DeparturePortRouteId.", transport.JW_DeparturePortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync the value from JW_DeparturePortRouteId to AMA_ManifestNumber.", transport.JW_DeparturePortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(false, false);
			transport.JW_RL_NKDiscPort = "ILHFA";
			sourceConsol.Transports.AddNew("ILHFA", "ILASH");

			transport = sourceConsol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).FirstOrDefault(x => x.JW_RL_NKLoadPort == sourceConsol.JK_RL_NKLoadPort);
			transport.JW_DeparturePortRouteId = "456";
			transport.JW_ArrivalPortRouteId = "67890";

			AssertNotEquals("Should not have value as JW_DeparturePortRouteId.", transport.JW_DeparturePortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Should sync the value from JW_DeparturePortRouteId to AMA_ManifestNumber.", transport.JW_DeparturePortRouteId, manifestHeader.AMA_ManifestNumber);

			manifestHeader.AMA_Nature = "IMP";
			transport = sourceConsol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).LastOrDefault(x => x.JW_RL_NKDiscPort == sourceConsol.JK_RL_NKDischargePort);
			transport.JW_ArrivalPortRouteId = "12345";
			AssertEquals("Should sync the value from JW_ArrivalPortRouteId to AMA_ManifestNumber.", transport.JW_ArrivalPortRouteId, manifestHeader.AMA_ManifestNumber);
		}

		public void TestGetNewConsolContainerCollectionSynchroniser()
		{
			var factory = Factory;
			var consol = factory.New<ForwardingConsol>();
			var header = factory.New<AsycudaManifestHeader>();
			header.SetParent(consol);

			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(header, consol);
			var result = synchroniser.GetNewConsolContainerCollectionSynchroniser_Expose;

			AssertNotNull("Should not be null.", result);
			AssertType<AsycudaConsolContainerCollectionSynchroniser>(result);
		}

		public void TestGetTargetShippingAgentAtDischarge_WhenTransportModeIsSea()
		{
			var factory = Factory;
			var sourceConsol = factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			sourceConsol.JK_RL_NKDischargePort = "ILASH";

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(manifestHeader, sourceConsol);

			var result = synchroniser.GetTargetShippingAgentAtDischarge_Expose();

			AssertNull("[At discharge] On the Israeli consol and manifest, Shipping agent should not sync when transport mode is sea", result);
		}

		public void TestGetTargetShippingAgentAtDischarge_WhenTransportModeIsNotSea()
		{
			var factory = Factory;
			var sourceConsol = factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Road;
			sourceConsol.JK_RL_NKDischargePort = "ILASH";

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(manifestHeader, sourceConsol);

			var result = synchroniser.GetTargetShippingAgentAtDischarge_Expose();

			AssertEquals("[At discharge] On the Israeli consol and manifest, Shipping agent should be the destination's shipping agent info when transport mode is not sea", manifestHeader.AMA_OA_ShippingAgentInfo, result);
		}

		public void TestGetTargetShippingAgentAtLoad_WhenTransportModeIsSea()
		{
			var factory = Factory;
			var sourceConsol = factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			sourceConsol.JK_RL_NKLoadPort = "ILASH";

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(manifestHeader, sourceConsol);

			var result = synchroniser.GetTargetShippingAgentAtLoad_Expose();

			AssertNull("[At load] On the Israeli consol and manifest, Shipping agent should not sync when transport mode is sea", result);
		}

		public void TestGetTargetShippingAgentAtLoad_WhenTransportModeIsNotSea()
		{
			var factory = Factory;
			var sourceConsol = factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Road;
			sourceConsol.JK_RL_NKLoadPort = "ILASH";

			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var synchroniser = new AsycudaManifestHeaderSynchroniserForTesting(manifestHeader, sourceConsol);

			var result = synchroniser.GetTargetShippingAgentAtLoad_Expose();

			AssertEquals("[At load] On the Israeli consol and manifest, Shipping agent should be the destination's shipping agent info when transport mode is not sea at load", manifestHeader.AMA_OA_ShippingAgentInfo, result);
		}
	}

	class AsycudaManifestHeaderSynchroniserForTesting : AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniserForTesting(AsycudaManifestHeader destination, ForwardingConsol sourceConsol) : base(destination, sourceConsol)
		{
		}

		internal BusinessObjectCollectionSynchroniser GetNewConsolContainerCollectionSynchroniser_Expose
			=> this.GetNewConsolContainerCollectionSynchroniser();

		internal ZPropertyInfo GetTargetShippingAgentAtDischarge_Expose()
			=> this.GetTargetShippingAgentAtDischarge();

		internal ZPropertyInfo GetTargetShippingAgentAtLoad_Expose()
			=> this.GetTargetShippingAgentAtLoad();
	}
}
