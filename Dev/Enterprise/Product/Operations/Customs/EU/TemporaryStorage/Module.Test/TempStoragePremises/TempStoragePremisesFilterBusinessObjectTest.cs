using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.EU.Module.TemporaryStorage.Testing
{
	[TestedType(typeof(TempStoragePremisesFilterBusinessObject))]
	sealed class TempStoragePremisesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCodeTextFilter()
		{
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_Code = "1";
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_Code = "2";
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_Code = "1";

			var filter = (ModuleTextFilter)filterObject[TempStoragePremisesFilterBusinessObject.Schema.Code];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "1";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("premises1, SRP_Code matches", true, premises1.MatchesFilter(filterQuery));
				AssertEquals("premises2, SRP_Code doesn't match", false, premises2.MatchesFilter(filterQuery));
				AssertEquals("premises3, SRP_Code matches", true, premises3.MatchesFilter(filterQuery));
			});
		}

		public void TestTypeListFilter()
		{
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			var filter = (ModuleTextFilter)filterObject[TempStoragePremisesFilterBusinessObject.Schema.Type];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.StatusAndFlags, filter.Category);
				AssertType<CusTempStorageRegPremisesTypeList>("List type", filter.List);
				AssertEquals("premises1, SRP_Type matches", true, premises1.MatchesFilter(filterQuery));
				AssertEquals("premises2, SRP_Type doesn't match", false, premises2.MatchesFilter(filterQuery));
				AssertEquals("premises3, SRP_Type matches", true, premises3.MatchesFilter(filterQuery));
			});
		}

		public void TestLocationTextFilter()
		{
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_CustomsLocation = "1";
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_CustomsLocation = "2";
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_CustomsLocation = "1";

			var filter = (ModuleTextFilter)filterObject[TempStoragePremisesFilterBusinessObject.Schema.Location];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "1";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.Locations, filter.Category);
				AssertEquals("premises1, SRP_CustomsLocation matches", true, premises1.MatchesFilter(filterQuery));
				AssertEquals("premises2, SRP_CustomsLocation doesn't match", false, premises2.MatchesFilter(filterQuery));
				AssertEquals("premises3, SRP_CustomsLocation matches", true, premises3.MatchesFilter(filterQuery));
			});
		}

		public void TestDescriptionTextFilter()
		{
			var premises1 = Factory.New<CusTempStorageRegPremises>();
			premises1.SRP_Description = "1";
			var premises2 = Factory.New<CusTempStorageRegPremises>();
			premises2.SRP_Description = "2";
			var premises3 = Factory.New<CusTempStorageRegPremises>();
			premises3.SRP_Description = "1";

			var filter = (ModuleTextFilter)filterObject[TempStoragePremisesFilterBusinessObject.Schema.Description];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "1";
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.TextSearch, filter.Category);
				AssertEquals("premises1, SRP_Description matches", true, premises1.MatchesFilter(filterQuery));
				AssertEquals("premises2, SRP_Description doesn't match", false, premises2.MatchesFilter(filterQuery));
				AssertEquals("premises3, SRP_Description matches", true, premises3.MatchesFilter(filterQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TempStoragePremisesFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterObject = (TempStoragePremisesFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
		TempStoragePremisesFilterBusinessObject filterObject;
	}
}
