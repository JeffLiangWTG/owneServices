using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public class TrainingZoneRateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZones()
		{
			TrainingZoneRate rate = new TrainingZoneRate();
			AssertNotNull(rate.Lookups.Zones);

			ZQuery moreFiltering = new ZQuery(RefZoneHeaderSchema.FZ_Code, SQLComparisonOperator.StartsWith, "_T");

			RefZoneHeader zone = rate.Lookups.Zones.Factory.New<RefZoneHeader>();
			zone.FZ_Code = "_T1";
			zone.FZ_ZoneType = "XYZ";
			rate.Lookups.Zones.AdditionalFilter = moreFiltering;
			AssertEquals(0, rate.Lookups.Zones.Count);

			zone.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			rate.Lookups.Zones.AdditionalFilter = moreFiltering;
			AssertEquals(1, rate.Lookups.Zones.Count);
			AssertCollectionContains(zone, rate.Lookups.Zones);
		}

		public void TestCurrencies()
		{
			TrainingZoneRate rate = new TrainingZoneRate();
			AssertNotNull(rate.Lookups.Currencies);
		}
	}
}