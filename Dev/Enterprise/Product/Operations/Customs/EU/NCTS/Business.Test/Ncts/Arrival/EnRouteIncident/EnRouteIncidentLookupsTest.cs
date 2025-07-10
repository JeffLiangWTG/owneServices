using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class EnRouteIncidentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncidentCodeList()
		{
			var list = incident.Lookups.IncidentCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "1, 2, 3, 4, 5, 6", list.CodesAsString);
				AssertSame("Cached", list, incident.Lookups.IncidentCodeList);
			});
		}

		public void TestEventCountries()
		{
			var eventCountries = (ZZRefCusCodeListCombinedCollection)incident.Lookups.EventCountries;
			eventCountries.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "AD", "GB" }, eventCountries.Select(c => c.ZZD_Code).ToArray());
		}

		public void TestIncidentEndorsementCountries()
		{
			var incidentEndorsementCountries = (ZZRefCusCodeListCombinedCollection)incident.Lookups.IncidentEndorsementCountries;
			incidentEndorsementCountries.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "AD", "GB" }, incidentEndorsementCountries.Select(c => c.ZZD_Code).ToArray());
		}

		public void TestEventCountriesList()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var eventCountriesList = (CodeDescriptionPairList)incident.Lookups.EventCountries;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("Codes", "AD, GB", eventCountriesList.CodesAsString);
				AssertSame("Cached", eventCountriesList, incident.Lookups.EventCountries);
			});
		}

		public void TestIncidentEndorsementCountriesList()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var incidentEndorsementCountriesList = (CodeDescriptionPairList)incident.Lookups.IncidentEndorsementCountries;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("Codes", "AD, GB", incidentEndorsementCountriesList.CodesAsString);
				AssertSame("Cached", incidentEndorsementCountriesList, incident.Lookups.IncidentEndorsementCountries);
			});
		}

		public void TestTransportAtDepartureTypes()
		{
			var list = incident.Lookups.TransportAtDepartureTypes;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81, 99", list.CodesAsString);
				AssertSame("Cached", list, incident.Lookups.TransportAtDepartureTypes);
			});
		}

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "AD", "Andorra", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
		EnRouteIncident incident;
	}
}
