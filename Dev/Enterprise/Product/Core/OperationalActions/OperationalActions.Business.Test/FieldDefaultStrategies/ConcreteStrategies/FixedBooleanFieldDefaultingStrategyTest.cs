using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedBooleanFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			OperationalActionBooleanFieldSupporter supporter = new OperationalActionBooleanFieldSupporter("fieldName", false);
			FixedBooleanFieldDefaultingStrategy strategy = new FixedBooleanFieldDefaultingStrategy(supporter);
			AssertEquals("DetailFieldType", FieldType.TextDropEdit, strategy.DetailFieldType);
			AssertEquals("DetailMaxLength", 3, strategy.DetailMaxLength);
			AssertEquals("set", BooleanChangeType.Codes.Set, strategy.GetDefaultValue(BooleanChangeType.Codes.Set));
			AssertEquals("unset", BooleanChangeType.Codes.UnSet, strategy.GetDefaultValue(BooleanChangeType.Codes.UnSet));
			AssertEquals("empty", "", strategy.GetDefaultValue(""));
		}
	}
}
