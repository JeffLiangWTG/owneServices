using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	class AMSBillStatusFilterValidationTest : TestCaseWithFactory
	{
		public void TestCheckProperty()
		{
			var filterBO = new DummyFilterBusinessObject();
			var createdTimeFilter = new ModuleDateFilter(FilterDescriptions.CreatedTime, DummyBizoSchema.Z0_AnotherDate, false);
			filterBO.ModuleFilters.AddFilter(createdTimeFilter);
			var filter = new AMSBillStatusFilter("TEST Filter", GetZQueryForTest, new List<ZString>(), filterBO);
			var strip = filterBO.FilterStrips.AddNew();
			strip.FilterDescription = createdTimeFilter.Description;
			var messageError = "'AMS Bill Status' filter can be costly to run. To avoid having a query that times out before completion, please add a 'Created Time' filter with a specific range.";
			filter.Property = ZString.Empty;
			filter.Validation.ValidateAll();
			AssertNoError(filter.PropertyInfo, messageError);
			filter.Property = "FIL";
			filter.Validation.ValidateAll();
			AssertHasError(filter.PropertyInfo, messageError);
			createdTimeFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			createdTimeFilter.IsActive = true;
			filter.Validation.ValidateAll();
			AssertNoError(filter.PropertyInfo, messageError);
			createdTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			createdTimeFilter.Property1 = ZDateTime.Empty;
			createdTimeFilter.Property2 = ZDateTime.Empty;
			createdTimeFilter.IsActive = true;
			filter.Validation.ValidateAll();
			AssertHasError(filter.PropertyInfo, messageError);
			createdTimeFilter.Property1 = ZDateTime.BrettsBirthday;
			filter.Validation.ValidateAll();
			AssertNoError(filter.PropertyInfo, messageError);
		}

		ZQuery GetZQueryForTest(SQLComparisonOperator filterOperator, ZString value)
		{
			return new ZQuery();
		}
	}
}
