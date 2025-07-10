using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(ArrivalCusTransportMeansLookups))]
sealed class ArrivalCusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportStateList_OriginalStatusNotNEW()
	{
		arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
		Factory.Save();

		var list = lookups.TransportStateList;
		CombineAssertions(() =>
		{
			AssertEquals("Values", "DEC, DIF, MIS", list.CodesAsString);
			AssertSame("Cached", list, lookups.TransportStateList);
		});
	}

	public void TestTransportStateList_OriginalStatusNEW()
	{
		var list = lookups.TransportStateList;
		CombineAssertions(() =>
		{
			AssertEquals("Values", "DEC, DIF, MIS, NEW", list.CodesAsString);
			AssertSame("Cached", list, lookups.TransportStateList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalCusTransportMeans = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();

		lookups = arrivalCusTransportMeans.Lookups;
	}
	ArrivalCusTransportMeans arrivalCusTransportMeans;
	ArrivalCusTransportMeansLookups lookups;
}
