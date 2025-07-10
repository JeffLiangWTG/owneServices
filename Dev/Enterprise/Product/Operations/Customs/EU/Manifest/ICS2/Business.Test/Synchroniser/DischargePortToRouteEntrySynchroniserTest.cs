using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

[TestedType(typeof(DischargePortToRouteEntrySynchroniser))]
sealed class DischargePortToRouteEntrySynchroniserTest : TestCaseWithFactory
{
	public void TestSynchronise()
	{
		var sourceConsol = Factory.New<ForwardingConsol>();
		sourceConsol.Transports.RemoveAndDeleteAll();
		sourceConsol.JK_RL_NKLoadPort = "AUSYD";
		sourceConsol.JK_RL_NKDischargePort = "IEADA";

		var transport = sourceConsol.Transports[0];

		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.SetParent(sourceConsol);

		var itinerary = manifestHeader.Itinerary.AddNew();

		var synchroniser = new DischargePortToRouteEntrySynchroniser(itinerary, transport);
		synchroniser.SetEnabled(true, false);
		synchroniser.Synchronise();

		CombineAssertions(() =>
		{
			Assert("ReadOnly", itinerary.ReadOnly);
			AssertEquals("Port", "IEADA", itinerary.CY_Code);
		});
	}
}
