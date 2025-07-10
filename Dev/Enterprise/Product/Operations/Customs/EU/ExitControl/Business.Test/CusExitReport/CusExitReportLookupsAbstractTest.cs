using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

public abstract class CusExitReportLookupsAbstractTest : BusinessObjectLookupsTestCase
{
	public void TestTransportTypeList()
	{
		CombineAssertions(() =>
		{
			var list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode empty, List", new[] { "10", "11", "21", "30", "40", "41", "80", "81" }, list.GetAllCodes());
			AssertSame("CER_TransportMode empty, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.Sea;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode SEA, List", new[] { "10", "11" }, list.GetAllCodes());
			AssertSame("CER_TransportMode SEA, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.OwnPropulsion;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode OWN, List", new[] { "10", "11", "21", "30", "40", "41", "80", "81" }, list.GetAllCodes());
			AssertSame("CER_TransportMode OWN, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.Rail;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode RAI, List", new[] { "21" }, list.GetAllCodes());
			AssertSame("CER_TransportMode RAI, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.Mail;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode MAI, List", new[] { "10", "11", "21", "30", "40", "41", "80", "81" }, list.GetAllCodes());
			AssertSame("CER_TransportMode MAI, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.Road;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode ROA, List", new[] { "30" }, list.GetAllCodes());
			AssertSame("CER_TransportMode ROA, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode FIX, List", new[] { "10", "11", "21", "30", "40", "41", "80", "81" }, list.GetAllCodes());
			AssertSame("CER_TransportMode FIX, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.Air;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode AIR, List", new[] { "40", "41" }, list.GetAllCodes());
			AssertSame("CER_TransportMode AIR, Cached", list, lookups.TransportTypeList);

			report.CER_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			list = lookups.TransportTypeList;
			AssertContainsExactElementsInExactOrder("CER_TransportMode IWT, List", new[] { "80", "81" }, list.GetAllCodes());
			AssertSame("CER_TransportMode IWT, Cached", list, lookups.TransportTypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		report = Factory.NewWithValidTestData<CusExitReport>();
		lookups = report.Lookups;
	}
	protected CusExitReport report;
	protected CusExitReportLookups lookups;
}
