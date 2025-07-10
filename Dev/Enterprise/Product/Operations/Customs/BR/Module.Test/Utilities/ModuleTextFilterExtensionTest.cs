using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Module.Testing
{
	public class ModuleTextFilterExtensionTest : TestCaseWithFactory
	{
		public void TestCustomizedStatusFilters()
		{
			var filter = new ModuleTextFilter("Cargo Status", BRCusEntryHeaderSchema.CH_CargoStatus);
			filter.BRCustomizedStatusFilters();
			AssertNull("StartsWith should be null", filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertNull("NotStartsWith should be null", filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			AssertNull("Contains should be null", filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.Contains));
			AssertNull("NotContain should be null", filter.ComparisonOperator_List.GetDescriptionFromCode(ModuleTextFilter.ComparisonConstants.NotContain));
		}
	}
}
