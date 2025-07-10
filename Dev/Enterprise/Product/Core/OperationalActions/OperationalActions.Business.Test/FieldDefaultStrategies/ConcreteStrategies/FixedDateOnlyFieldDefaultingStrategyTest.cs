using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedDateOnlyFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			var supporter = new OperationalActionDateFieldSupporter("fieldName", false);
			var strategy = new FixedDateOnlyFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Date, strategy.DetailFieldType);
			AssertEquals(10, strategy.DetailMaxLength);
			AssertEquals(new ZDate(1981, 02, 05), strategy.GetDefaultValue("05-FEB-81"));
			AssertEquals(new ZDate(2008, 09, 07), strategy.GetDefaultValue("07-SEP-08"));
			var boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		public void TestInvalidDefaultValue()
		{
			var supporter = new OperationalActionDateFieldSupporter("fieldName", false);
			var strategy = new FixedDateOnlyFieldDefaultingStrategy(supporter);
			AssertNoExceptionThrown(() => strategy.GetDefaultValue("Not a date"));
		}
	}
}
