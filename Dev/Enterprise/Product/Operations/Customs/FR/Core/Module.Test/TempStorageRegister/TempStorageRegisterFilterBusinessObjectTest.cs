using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorageRegister.Testing
{
	[TestedType(typeof(TempStorageRegisterFilterBusinessObject))]
	public class TempStorageRegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestStatusFilter()
		{
			var header1 = Factory.New<CusTempStorageRegHeader>();
			header1.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			var header2 = Factory.New<CusTempStorageRegHeader>();
			header2.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			var header3 = Factory.New<CusTempStorageRegHeader>();
			header3.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;

			var filter = (ModuleTextFilter)filterObject[TempStorageRegisterFilterBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = TempStorageDeclarationStatusList.Codes.Open;
			var filterQuery = filterObject.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Category", FilterCategories.StatusAndFlags, filter.Category);
				AssertType<TempStorageDeclarationStatusList>("List type", filter.List);
				AssertEquals("Comparison operators", "exact", filter.ComparisonOperator_List.CodesAsString);

				AssertEquals("header1, SRH_Status matches", true, header1.MatchesFilter(filterQuery));
				AssertEquals("header2, SRH_Status doesn't match", false, header2.MatchesFilter(filterQuery));
				AssertEquals("header3, SRH_Status matches", true, header3.MatchesFilter(filterQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TempStorageRegisterFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterObject = (TempStorageRegisterFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
		TempStorageRegisterFilterBusinessObject filterObject;
	}
}
