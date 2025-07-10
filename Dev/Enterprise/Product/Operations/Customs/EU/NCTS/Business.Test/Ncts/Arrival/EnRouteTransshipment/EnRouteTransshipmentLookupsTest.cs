using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class EnRouteTransshipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEventCountries()
		{
			var eventCountries = lookups.EventCountries;
			eventCountries.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "AD", "GB" }, eventCountries.Select(c => c.ZZD_Code).ToArray());
		}

		public void TestIncidentEndorsementCountries()
		{
			var incidentEndorsementCountries = lookups.IncidentEndorsementCountries;
			AssertContainsExactElementsInAnyOrder("AD, GB", incidentEndorsementCountries.CodesAsString);
		}

		public void TestNewTransportCountries()
		{
			AssertType<RefCountryCollection>(lookups.NewTransportCountries);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "AD", "Andorra", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var transshipment = header.EnRouteTransshipments.AddNew();
			lookups = new EnRouteTransshipmentLookups(transshipment);
		}

		EnRouteTransshipmentLookups lookups;
	}
}
