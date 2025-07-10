using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class DepartureCusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportNationalityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var countries = lookups.TransportNationalityList;
			countries.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestBorderModeOfTransportList()
		{
			AssertListHasCorrectValuesAndIsCached(x => x.BorderModeOfTransportList, "1, 2, 3, 4, 5, 7, 8, 9");
		}

		public void TestTransportAtBorderTypeOfIdList()
		{
			AssertListHasCorrectValuesAndIsCached(x => x.TransportAtBorderTypeOfIdList, "10, 11, 21, 30, 40, 41, 80, 81, 99");
		}

		public void TestOfficeCodeList()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertSame("Same list as for MovementHeader", nctsHeader.MovementHeader.Lookups.OfficeCodeList, lookups.OfficeCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var cusTransportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();

			lookups = cusTransportMeans.Lookups;
		}
		NctsHeader nctsHeader;
		DepartureCusTransportMeansLookups lookups;

		void AssertListHasCorrectValuesAndIsCached(Func<DepartureCusTransportMeansLookups, CodeDescriptionPairList> getList, string expectedValues)
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
