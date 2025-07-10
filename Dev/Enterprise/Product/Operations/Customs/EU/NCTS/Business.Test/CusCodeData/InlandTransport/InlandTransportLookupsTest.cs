using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class InlandTransportLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportCountryList()
		{
			var countryList = lookups.Countries;
			CombineAssertions(() =>
			{
				AssertType<RefCountryCollection>("Type", countryList);
				AssertSame("Cached", countryList, lookups.Countries);
			});
		}

		public void TestTransportNationalityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var countries = lookups.TransportNationalityList;
			countries.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var inlandTransport = nctsHeader.MovementHeader.InlandTransportList.AddNew();
			lookups = inlandTransport.Lookups;
		}
		InlandTransportLookups lookups;
	}
}
