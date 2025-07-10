using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleFountainFilter))]
	public sealed class ModuleFountainFilterTest : ModuleNumberFilterTest
	{
		#region TestExpandValue

		public void TestExpandValue()
		{
			var filter = new ModuleFountainFilter("blah", DummyBizoSchema.Z0_Description, "Z");

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1234";
			AssertEquals("Z00001234", filter.Property);

			filter.Property = "";
			AssertEquals("", filter.Property);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "4321";
			AssertEquals("4321", filter.Property);

			filter.SqlComparisonOperator = SQLComparisonOperator.EndsWith;
			filter.Property = "666";
			AssertEquals("666", filter.Property);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "1235";
			AssertEquals("1235", filter.Property);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertEquals("Z00001235", filter.Property);

			filter = new ModuleFountainFilter("blah", DummyBizoSchema.Z0_Description, "ZX");
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1";
			AssertEquals("ZX00000001", filter.Property);

			filter = new ModuleFountainFilter("blah", DummyBizoSchema.Z0_Description, "");
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1";
			AssertEquals("00000001", filter.Property);

			filter = new ModuleFountainFilter("blah", DummyBizoSchema.Z0_Description, "ZX", 7);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1";
			AssertEquals("ZX0000001", filter.Property);

			filter = new ModuleFountainFilter("blah", DummyBizoSchema.Z0_Description, "", 7);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "1";
			AssertEquals("0000001", filter.Property);
		}

		#endregion

		#region Implementation

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ModuleFountainFilter("moo", DummyBizoSchema.Z0_Description, "M");
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		#endregion
	}
}
