using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Module.Testing
{
	[TestedType(typeof(CusEntryNumTextFilter))]
	public class CusEntryNumTextFilterTest : ModuleFilterTestCase<CusEntryNumTextFilter>
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
			ZString searchValue = "11111";
			ZString eventValue = "COC";
			FilterText.Property = searchValue;
			FilterText.EntryType = eventValue;
			AssertEquals("Precondition", searchValue, FilterText.Property);
			AssertEquals("Precondition", eventValue, FilterText.EntryType);
			FilterText.Clear();
			AssertEquals("", FilterText.Property);
			AssertEquals("", FilterText.EntryType);
		}

		#endregion
		#region TestIsEmpty
		public void TestIsEmpty()
		{
			FilterText.Property = "";
			FilterText.EntryType = "";
			AssertEquals(true, fFilterText.IsEmpty);
			FilterText.Property = "str";
			AssertEquals(false, FilterText.IsEmpty);
			FilterText.EntryType = "COC";
			AssertEquals(false, FilterText.IsEmpty);
			FilterText.Property = "";
			AssertEquals(false, FilterText.IsEmpty);
			fFilterText.EntryType = "";
			AssertEquals(true, fFilterText.IsEmpty);
		}

		#endregion
		#region TestQuery
		public void TestStringAndTypeQuery()
		{
			CusEntryNumber number1 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number2 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number3 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number4 = Factory.NewWithValidTestData<CusEntryNumber>();
			number1.CE_EntryNum = ZString.Empty;
			number2.CE_EntryNum = "111";
			number3.CE_EntryNum = "111";
			number4.CE_EntryNum = ZString.Empty;
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
			DummyFilterBizOForCusEntryNumText fFilterText = new DummyFilterBizOForCusEntryNumText();
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV1";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "";
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(fFilterText.Filter);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV1";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "111";
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV2";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "1";
			((CusEntryNumTextFilter)fFilterText["text"]).ComparisonOperator = Enterprise.ZArchitecture.Business.ModuleTextBaseFilter.ComparisonConstants.Contains;
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
		}

		public void TestStringAndTypeQuery_Country()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			CusEntryNumber number1 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number2 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number3 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number4 = Factory.NewWithValidTestData<CusEntryNumber>();
			number1.CE_EntryNum = "boo";
			number1.CE_EntryType = "EV1";
			number2.CE_EntryNum = "111";
			number2.CE_EntryType = "EV1";
			number2.CE_RN_NKCountryCode = "US";
			number3.CE_EntryNum = "111";
			number3.CE_EntryType = "EV2";
			number4.CE_EntryNum = ZString.Empty;
			number4.CE_EntryType = "EV2";
			number4.CE_RN_NKCountryCode = "NZ";
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
			DummyFilterBizOForCusEntryNumText fFilterText = new DummyFilterBizOForCusEntryNumText();
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV1";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "";
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(fFilterText.Filter);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV1";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "111";
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			((CusEntryNumTextFilter)fFilterText["text"]).EntryType = "EV2";
			((CusEntryNumTextFilter)fFilterText["text"]).Property = "1";
			((CusEntryNumTextFilter)fFilterText["text"]).ComparisonOperator = Enterprise.ZArchitecture.Business.ModuleTextBaseFilter.ComparisonConstants.Contains;
			((CusEntryNumTextFilter)fFilterText["text"]).IsActive = true;
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			collection.Load(fFilterText.Filter);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
		}

		public void TestStringAndTypeQueryWithNoFilter()
		{
			CusEntryNumber number1 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number2 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number3 = Factory.NewWithValidTestData<CusEntryNumber>();
			CusEntryNumber number4 = Factory.NewWithValidTestData<CusEntryNumber>();
			number1.CE_EntryNum = ZString.Empty;
			number2.CE_EntryNum = "111";
			number3.CE_EntryNum = "112";
			number4.CE_EntryNum = ZString.Empty;
			DummyBusinessObject bizO1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject bizO5 = Factory.NewWithValidTestData<DummyBusinessObject>();
			number1.CE_ParentID = bizO1.PK;
			number1.CE_ParentTable = "CusEntryHeader";
			number2.CE_ParentID = bizO2.PK;
			number2.CE_ParentTable = "CusEntryHeader";
			number3.CE_ParentID = bizO3.PK;
			number3.CE_ParentTable = "CusEntryHeader";
			number4.CE_ParentID = bizO4.PK;
			number4.CE_ParentTable = "CusEntryHeader";
			Factory.Save();
			DummyFilterBizOForCusEntryNumText fFilterText = new DummyFilterBizOForCusEntryNumText();
			var filter = (CusEntryNumTextFilter)fFilterText["text"];
			filter.EntryType = "";
			filter.Property = "2";
			filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotContain;
			filter.IsActive = true;
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(fFilterText.Filter);
			AssertEquals("Should have found 4 Records", 4, collection.Count);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionContains(bizO4, collection);
			AssertCollectionContains(bizO5, collection);
			filter.Property = "";
			filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.IsBlank;
			collection.Load(fFilterText.Filter);
			AssertEquals("Should have found 2 Records", 2, collection.Count);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionContains(bizO4, collection);
			AssertCollectionNotContains(bizO5, collection);
			filter.Property = "";
			filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.IsNotBlank;
			collection.Load(fFilterText.Filter);
			AssertEquals("Should have found 3 Records", 3, collection.Count);
			AssertCollectionNotContains(bizO1, collection);
			AssertCollectionContains(bizO2, collection);
			AssertCollectionContains(bizO3, collection);
			AssertCollectionNotContains(bizO4, collection);
			AssertCollectionContains(bizO5, collection);
			filter.Property = "1";
			filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotStartsWith;
			collection.Load(fFilterText.Filter);
			AssertEquals("Should have found 3 Records", 3, collection.Count);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionNotContains(bizO3, collection);
			AssertCollectionContains(bizO4, collection);
			AssertCollectionContains(bizO5, collection);
			filter.Property = "111";
			filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.NotEqual;
			collection.Load(fFilterText.Filter);
			AssertEquals("Should have found 4 Records", 4, collection.Count);
			AssertCollectionContains(bizO1, collection);
			AssertCollectionNotContains(bizO2, collection);
			AssertCollectionContains(bizO3, collection);
			AssertCollectionContains(bizO4, collection);
			AssertCollectionContains(bizO5, collection);
		}

		#endregion
		#region Implementation
		#region Filter Getters
		static protected CusEntryNumTextFilter FilterText
		{
			get
			{
				if (fFilterText == null)
				{
					fFilterText = new CusEntryNumTextFilter("text", typeof(DummyBusinessObject));
				}

				return fFilterText;
			}
		}

		static CusEntryNumTextFilter fFilterText;
		#endregion
		#region Dummy BizO
		protected class DummyFilterBizOForCusEntryNumText : DummyFilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				ModuleFilterCollection collection = new ModuleFilterCollection();
				fFilterText = null;
				collection.AddCustomFilter(FilterText);
				return collection;
			}
		}

		#endregion
		protected override CusEntryNumTextFilter GetNewModuleFilter()
		{
			return new CusEntryNumTextFilter("moo", typeof(DummyBusinessObject));
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				return FilterCategories.TextSearch;
			}
		}
		#endregion
	}
}
