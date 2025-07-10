using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportsExitControl()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(filterBizObj.SupportsExitControl);
		}

		public void TestJobDeclarationFilterLookups()
		{
			AssertType<JobDeclarationFilterLookups>(filterBusinessObject.Lookups);
		}

		public void TestPresentationEndDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.ZG_PresentationEndDate = new ZDateTime(2020, 01, 15, 10, 30, 00);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.ZG_PresentationEndDate = new ZDateTime(2020, 02, 01, 11, 20, 00);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.ZG_PresentationEndDate = new ZDateTime(2020, 02, 25, 02, 30, 00);
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.ZG_PresentationEndDate = ZDate.Empty;
			Factory.Save();

			var dateFilter = (ModuleDateFilter)filterBusinessObject[JobDeclarationFilterBusinessObject.FilterConstants.PresentationEndDate];
			dateFilter.IsActive = true;

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Presentation End Date", dateFilter.Description);
				AssertEquals("Category", FilterCategories.Dates, dateFilter.Category);

				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				dateFilter.Property1 = new ZDate(2020, 02, 01);
				AssertMatch("After 01/02/2020", false, true, true, false);

				dateFilter.Property1 = ZDateTime.Empty;
				dateFilter.Property2 = new ZDate(2020, 02, 28);
				AssertMatch("Before 28/02/2020", true, true, true, false);

				dateFilter.Property1 = new ZDate(2020, 02, 01);
				AssertMatch("Between 01/02/2020 - 28/02/2020", false, true, true, false);

				dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
				AssertMatch("Has Date", true, true, true, false);

				dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
				AssertMatch("No Date", false, false, false, true);
			});

			void AssertMatch(ZString message, bool match1, bool match2, bool match3, bool match4)
			{
				var filter = filterBusinessObject.Filter;
				AssertEquals(message + "->declaration1", match1, declaration1.MatchesFilter(filter));
				AssertEquals(message + "->declaration2", match2, declaration2.MatchesFilter(filter));
				AssertEquals(message + "->declaration3", match3, declaration3.MatchesFilter(filter));
				AssertEquals(message + "->declaration4", match4, declaration4.MatchesFilter(filter));
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBusinessObject;

		protected override void SetUp()
		{
			base.SetUp();
			filterBusinessObject = new JobDeclarationFilterBusinessObject();
		}
		JobDeclarationFilterBusinessObject filterBusinessObject;
	}
}
