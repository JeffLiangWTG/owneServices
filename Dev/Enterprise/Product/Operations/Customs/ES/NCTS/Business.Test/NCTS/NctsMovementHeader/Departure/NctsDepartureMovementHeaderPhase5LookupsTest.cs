using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	internal class NctsDepartureMovementHeaderPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeOfSecurityList()
		{
			AssertListHasCorrectValuesAndIsCached(x => x.TypeOfSecurityList, "NON, EXI");
		}

		public void TestNctsTransitStatusList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.NctsTransitStatusList;
				AssertType<ESNCTS5DepartureCustomsStatusList>(list);
				AssertSame("Cached", list, lookups.NctsTransitStatusList);
			});
		}

		public void TestNctsMovementHeaderTransactionStatusList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.NctsMovementHeaderTransactionStatusList;
				AssertType<ESNctsMovementHeaderTransactionStatusList>(list);
				AssertSame("Cached", list, lookups.NctsMovementHeaderTransactionStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = new NctsDepartureMovementHeaderPhase5Lookups(nctsHeader.MovementHeader);
		}
		NctsDepartureMovementHeaderPhase5Lookups lookups;

		void AssertListHasCorrectValuesAndIsCached(Func<NctsDepartureMovementHeaderPhase5Lookups, CodeDescriptionPairList> getList, string expectedValues)
		{
			CombineAssertions(() =>
			{
				var list = getList.Invoke(lookups);
				AssertEquals("Values", expectedValues, list.CodesAsString);
				AssertSame("Cached", list, getList.Invoke(lookups));
			});
		}
	}
}
