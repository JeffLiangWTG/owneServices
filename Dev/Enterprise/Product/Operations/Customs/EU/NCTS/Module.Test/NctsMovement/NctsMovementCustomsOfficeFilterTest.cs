using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	class NctsMovementCustomsOfficeFilterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when factory parameter is null", () => new NctsMovementCustomsOfficeFilter(null));
		}

		public void TestAddFilters()
		{
			var nctsMovementCustomsOfficeFilter = new NctsMovementCustomsOfficeFilter(Factory);

			AssertExceptionThrown<ArgumentNullException>("Exception expected when filters parameter is null", () => nctsMovementCustomsOfficeFilter.AddFilters(null));

			var filters = new ModuleFilterCollection();
			nctsMovementCustomsOfficeFilter.AddFilters(filters);

			AssertEquals("Number of filters added", 4, filters.Count(x => x.Category != FilterCategories.Other));

			CombineAssertions(() =>
			{
				AssertNotNull("Customs Office of Departure", filters[NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDeparture]);
				AssertNotNull("Customs Office of Destination", filters[NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestination]);
				AssertNotNull("Customs Office of Transit", filters[NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfTransit]);
				AssertNotNull("Customs Office of Destination For Arrival", filters[NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestinationForArrival]);
			});
		}

		public void TestCustomsOfficeOfDepartureFilter()
		{
			AssertCustomsOffice(officeCode: "DEP", officeData: "IT13000", filterCode: NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfDestinationFilter()
		{
			AssertCustomsOffice(officeCode: "DES", officeData: "IT14000", filterCode: NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestination);
		}

		public void TestCustomsOfficeOfTransit()
		{
			AssertCustomsOffice(officeCode: "TRA", officeData: "IT15000", filterCode: NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfTransit);
		}

		public void TestCustomsOfficeOfDestinationForArrivalFilter()
		{
			AssertCustomsOffice(officeCode: "DSA", officeData: "IT16000", filterCode: NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestinationForArrival);
		}

		void AssertCustomsOffice(string officeCode, string officeData, string filterCode)
		{
			var departure1 = Factory.New<NctsHeader>();
			departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure1.SetMovementType(NctsMovementType.Codes.Departure);

			var customsOffice = departure1.CustomsOffices.AddNew();
			customsOffice.CY_Code = officeCode;
			customsOffice.CY_Data = officeData;

			var departure2 = Factory.New<NctsHeader>();
			departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure2.SetMovementType(NctsMovementType.Codes.Departure);

			var departure3 = Factory.New<NctsHeader>();
			departure3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure3.SetMovementType(NctsMovementType.Codes.Departure);

			var customsOffice2 = departure3.MovementHeader.CustomsOffices.AddNew();
			customsOffice2.CY_Code = officeCode;
			customsOffice2.CY_Data = officeData;

			var departure4 = Factory.New<NctsHeader>();
			departure4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure4.SetMovementType(NctsMovementType.Codes.Departure);

			Factory.Save();

			var filterStripBO = new NctsMovementFilterStripBusinessObject();
			var nkFilter = (ModuleNkFilter)filterStripBO[filterCode];
			AssertNotNull(nkFilter);
			nkFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", nkFilter.MultilingualDescription);

			nkFilter.Property = officeData;
			AssertEquals("Phase4 departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("Phase4 departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			AssertEquals("Phase5 departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));
			AssertEquals("Phase5 departure4 does not match filter", false, departure4.MatchesFilter(filterStripBO.Filter));
		}
	}
}
