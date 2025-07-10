using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

[TestedType(typeof(OriginToRouteEntrySynchroniser))]
sealed class OriginToRouteEntrySynchroniserTest : TestCaseWithFactory
{
	public void TestSynchronise()
	{
		var helper = new MasterFilesTestHelper(Factory);
		var austria = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Austria);
		helper.CreateUnlocoIfNotExists("ATSLZ", austria);
		var sourceConsol = Factory.New<ForwardingConsol>();
		sourceConsol.Transports.RemoveAndDeleteAll();

		var shipment = sourceConsol.Shipments.AddNew();
		shipment.JS_RL_NKOrigin = "ATSLZ";

		var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		manifestHeader.SetParent(sourceConsol);
		var itinerary = manifestHeader.Itinerary.AddNew();

		var synchroniser = new OriginToRouteEntrySynchroniser(itinerary, shipment);
		synchroniser.SetEnabled(true, false);
		synchroniser.Synchronise();

		CombineAssertions(() =>
		{
			Assert("ReadOnly", itinerary.ReadOnly);
			AssertEquals("Port", "ATSLZ", itinerary.CY_Code);
		});
	}
}
