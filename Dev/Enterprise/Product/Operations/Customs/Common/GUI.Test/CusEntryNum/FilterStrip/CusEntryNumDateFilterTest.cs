using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Module.Testing
{
	[TestedType(typeof(CusEntryNumDateFilter))]
	public class CusEntryNumDateFilterTest : ModuleFilterTestCase<CusEntryNumDateFilter>
	{
		#region TestIsExpensiveQuery
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion
		#region TestClearResetsToDefaults
		public void TestClearResetsToDefaults()
		{
			ZDateTime searchValue = new ZDateTime(2000, 1, 1);
			ZString eventValue = "COC";
			FilterDate.Property1 = searchValue;
			FilterDate.EntryType = eventValue;
			AssertEquals("Precondition", searchValue, FilterDate.Property1);
			AssertEquals("Precondition", eventValue, FilterDate.EntryType);
			FilterDate.Clear();
			AssertEquals(ZDateTime.Empty, FilterDate.Property1);
			AssertEquals(ZString.Empty, FilterDate.EntryType);
		}

		#endregion
		#region TestIsEmpty
		public void TestIsEmpty()
		{
			FilterDate.Property1 = ZDateTime.Empty;
			FilterDate.Property2 = ZDateTime.Empty;
			FilterDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			FilterDate.EntryType = "";
			AssertEquals(true, fFilterDate.IsEmpty);
			FilterDate.Property1 = new ZDateTime(2000, 1, 1);
			AssertEquals(false, FilterDate.IsEmpty);
			FilterDate.EntryType = "COC";
			AssertEquals(false, FilterDate.IsEmpty);
			FilterDate.Property1 = ZDateTime.Empty;
			AssertEquals(false, FilterDate.IsEmpty);
			fFilterDate.EntryType = "";
			AssertEquals(true, fFilterDate.IsEmpty);
		}

		#endregion
		#region TestQuery
		public void TestDateAndTypeQuery()
		{
			CusEntryNumber number1 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number2 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number3 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number4 = Factory.NewWithValidTestData<CusEntryNumber>();
			number1.CE_IssueDate = ZDateTime.Empty;
			number2.CE_IssueDate = new ZDateTime(2000, 1, 1);
			number3.CE_IssueDate = new ZDateTime(2000, 1, 1);
			number4.CE_IssueDate = ZDateTime.Empty;
			number1.CE_EntryType = "EV1";
			number2.CE_EntryType = "EV1";
			number3.CE_EntryType = "EV2";
			number4.CE_EntryType = "EV2";
			DummyBusinessObject bizO1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			number1.CE_ParentID = bizO1.PK;
			number1.CE_ParentTable = "CusEntryHeader";
			number2.CE_ParentID = bizO2.PK;
			number2.CE_ParentTable = "CusEntryHeader";
			number3.CE_ParentID = bizO3.PK;
			number3.CE_ParentTable = "CusEntryHeader";
			number4.CE_ParentID = bizO4.PK;
			number4.CE_ParentTable = "CusEntryHeader";
			Factory.Save();
			DummyFilterBizOForCusEntryNumDate fFilterDate = new DummyFilterBizOForCusEntryNumDate();
			((CusEntryNumDateFilter)fFilterDate["Date"]).EntryType = "EV1";
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property1 = ZDateTime.Empty;
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property2 = ZDateTime.Empty;
			((CusEntryNumDateFilter)fFilterDate["Date"]).PropertySearch = ZString.Empty;
			((CusEntryNumDateFilter)fFilterDate["Date"]).IsActive = true;
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(fFilterDate.Filter);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumDateFilter)fFilterDate["Date"]).EntryType = "EV1";
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property1 = new ZDateTime(2000, 1, 1);
			((CusEntryNumDateFilter)fFilterDate["Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((CusEntryNumDateFilter)fFilterDate["Date"]).IsActive = true;
			collection.Load(fFilterDate.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumDateFilter)fFilterDate["Date"]).EntryType = "EV1";
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property1 = new ZDateTime(2000, 1, 1);
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property2 = new ZDateTime(2000, 1, 2);
			((CusEntryNumDateFilter)fFilterDate["Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((CusEntryNumDateFilter)fFilterDate["Date"]).IsActive = true;
			collection.Load(fFilterDate.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumDateFilter)fFilterDate["Date"]).EntryType = "EV1";
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property1 = ZDateTime.Empty;
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property2 = new ZDateTime(2000, 1, 2);
			((CusEntryNumDateFilter)fFilterDate["Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((CusEntryNumDateFilter)fFilterDate["Date"]).IsActive = true;
			collection.Load(fFilterDate.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumDateFilter)fFilterDate["Date"]).EntryType = "EV1";
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property1 = ZDateTime.Empty;
			((CusEntryNumDateFilter)fFilterDate["Date"]).Property2 = new ZDateTime(1999, 1, 1);
			((CusEntryNumDateFilter)fFilterDate["Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((CusEntryNumDateFilter)fFilterDate["Date"]).IsActive = true;
			collection.Load(fFilterDate.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
		}

		public void TestQuery_HasDateHasNoDate()
		{
			var numberHasNoDate = Factory.NewWithValidTestData<CusEntryNumber>();
			numberHasNoDate.CE_EntryType = "EV1";
			numberHasNoDate.CE_IssueDate = ZDateTime.Empty;
			var bizHasNoDate = Factory.NewWithValidTestData<DummyBusinessObject>();
			numberHasNoDate.CE_ParentID = bizHasNoDate.PK;
			numberHasNoDate.CE_ParentTable = "CusEntryHeader";
			var numberHasDate = Factory.NewWithValidTestData<CusEntryNumber>();
			numberHasDate.CE_EntryType = "EV1";
			numberHasDate.CE_IssueDate = new ZDateTime(2000, 1, 1);
			var bizHasDate = Factory.NewWithValidTestData<DummyBusinessObject>();
			numberHasDate.CE_ParentID = bizHasDate.PK;
			numberHasDate.CE_ParentTable = "CusEntryHeader";
			Factory.Save();
			var filterDate = new CusEntryNumDateFilter("Date", typeof(DummyBusinessObject))
			{ EntryType = "EV1", Property1 = ZDateTime.Empty, Property2 = ZDateTime.Empty, PropertySearch = ModuleDateFilter.HasDateEntered, IsActive = true };
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterDate.Query);
			AssertCollectionNotContains(bizHasNoDate, collection);
			AssertCollectionContains(bizHasDate, collection);
			filterDate.EntryType = ZString.Empty;
			filterDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(filterDate.Query);
			AssertCollectionNotContains(bizHasDate, collection);
			AssertCollectionContains(bizHasNoDate, collection);
		}

		#endregion
		#region Implementation
		#region Filter Getters
		static protected CusEntryNumDateFilter FilterDate
		{
			get
			{
				if (fFilterDate == null)
				{
					fFilterDate = new CusEntryNumDateFilter("Date", typeof(DummyBusinessObject));
				}

				return fFilterDate;
			}
		}

		static CusEntryNumDateFilter fFilterDate;
		#endregion
		#region Dummy BizO
		protected class DummyFilterBizOForCusEntryNumDate : DummyFilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				ModuleFilterCollection collection = new ModuleFilterCollection();
				collection.AddCustomFilter(FilterDate);
				return collection;
			}
		}

		#endregion
		protected override CusEntryNumDateFilter GetNewModuleFilter()
		{
			return new CusEntryNumDateFilter("moo", typeof(DummyBusinessObject));
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				return FilterCategories.Dates;
			}
		}
		#endregion
	}
}
