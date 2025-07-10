using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	[TestedType(typeof(SimilarIncidentsFilter))]
	public class SimilarIncidentsFilterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2019, 6, 6, 9, 47, 8)]
		public void TestPrepareSearchOptions()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = ZGuid.NewZGuid();
			var testProduct = "TPD";
			incident.IM_Product = testProduct;
			var testProductArea = "TPA";
			incident.ProductArea = testProductArea;

			AssertSearchOptions(incident, SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.All, ModuleDateFilter.DateRangeSearchTexts.Today, false,
				IncidentStatus.Closed | IncidentStatus.Open, new ZDateTime(2019, 6, 6, 0, 0, 0), new ZDateTime(2019, 6, 6, 23, 59, 59));

			AssertSearchOptions(incident, SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Closed, ModuleDateFilter.DateRangeSearchTexts.Last7Days, true,
				IncidentStatus.Closed, new ZDateTime(2019, 5, 31, 0, 0, 0), new ZDateTime(2019, 6, 6, 23, 59, 59), incident.IM_OH_Client);

			AssertSearchOptions(incident, SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Closed, ModuleDateFilter.DateRangeSearchTexts.Last7Days, false,
				IncidentStatus.Closed, new ZDateTime(2019, 5, 31, 0, 0, 0), new ZDateTime(2019, 6, 6, 23, 59, 59),null, true,
				testProduct);

			AssertSearchOptions(incident, SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Closed, ModuleDateFilter.DateRangeSearchTexts.Last7Days, false,
				IncidentStatus.Closed, new ZDateTime(2019, 5, 31, 0, 0, 0), new ZDateTime(2019, 6, 6, 23, 59, 59), null, false,
				testProduct, true, testProductArea);

			AssertSearchOptions(incident, SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Open, ModuleDateFilter.Past, false,
				IncidentStatus.Open, expectedTo: new ZDateTime(2019, 6, 6, 9, 47, 8));
		}

		void AssertSearchOptions(SupportIncident parentIncident, string status, string dates, bool sameClient,
			IncidentStatus expectedStatus, ZDateTime? expectedFrom = null, ZDateTime? expectedTo = null, ZGuid? expectedCompanyGuid = null, bool sameProduct = false, ZString? expectedProduct = null, bool sameProductArea = false, ZString? expectedProductArea = null)
		{
			var filter = new SimilarIncidentsFilter(parentIncident);
			filter.IncidentStatusFilter = status;
			filter.SameClient = sameClient;
			filter.SameProduct = sameProduct;
			filter.SameProductArea = sameProductArea;
			filter.DateFilter.PropertySearch = dates;

			var options = filter.PrepareSearchOptions();

			AssertEquals(expectedStatus, options.IncidentStatus);

			if (expectedFrom == null)
			{
				AssertNull(options.FromTime);
			}
			else
			{
				AssertEquals(expectedFrom.Value, options.FromTime.Value);
			}

			if (expectedTo == null)
			{
				AssertNull(options.ToTime);
			}
			else
			{
				Assert((expectedTo.Value.ToDateTime() - options.ToTime.Value).TotalSeconds < 2.0);
			}

			if (expectedCompanyGuid == null)
			{
				AssertNull(options.CompanyGuid);
			}
			else
			{
				AssertEquals(expectedCompanyGuid, options.CompanyGuid);
			}

			if (!expectedProduct.HasValue)
			{
				AssertNull(options.Product);
			}
			else
			{
				AssertEquals(expectedProduct, options.Product);
			}

			if (!expectedProductArea.HasValue)
			{
				AssertNull(options.ProductArea);
			}
			else
			{
				AssertEquals(expectedProduct, options.Product);
				AssertEquals(expectedProductArea, options.ProductArea);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SimilarIncidentsFilter(Factory.New<SupportIncident>());
		}

		public void TestSearchOptionsToString()
		{
			var incident = Factory.New<SupportIncident>();
			var incidentFilter = new SimilarIncidentsFilter(incident);
			var tup = incidentFilter.SearchOptionsToTuple();

			Assert(tup.DateRange == ModuleDateFilter.DateRangeSearchTexts.Last12Mths);
			Assert(tup.AllCustomers);
			Assert(tup.IncidentStatus == "All");
		}

		public void TestChangedSearchOptionsToString()
		{
			var incident = Factory.New<SupportIncident>();
			var incidentFilter = new SimilarIncidentsFilter(incident);
			incidentFilter.DateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.LastWeek;
			incidentFilter.SameClient = true;
			incidentFilter.IncidentStatusFilter = "Open";
			var tup = incidentFilter.SearchOptionsToTuple();

			Assert(tup.DateRange == ModuleDateFilter.DateRangeSearchTexts.LastWeek);
			Assert(!tup.AllCustomers);
			Assert(tup.IncidentStatus == "Open");
		}

		public void TestDateRangeReturnsTheSameStringIrrespectiveOfTheRange()
		{
			var incident = Factory.New<SupportIncident>();
			var incidentFilter = new SimilarIncidentsFilter(incident);
			incidentFilter.DateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			incidentFilter.DateFilter.Property1 = ZDateTime.UtcNow.AddDays(-100);
			incidentFilter.DateFilter.Property2 = ZDateTime.UtcNow.AddDays(-50);
			var tup = incidentFilter.SearchOptionsToTuple();

			Assert(tup.DateRange == ModuleDateFilter.SpecifiedDateRange);
			Assert(tup.AllCustomers);
			Assert(tup.IncidentStatus == "All");
		}
	}
}
