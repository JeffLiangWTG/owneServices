using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FixedGeographyFieldDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestDetail()
		{
			OperationalActionGeographyFieldSupporter supporter = new OperationalActionGeographyFieldSupporter("fieldName", false);
			FixedGeographyFieldDefaultingStrategy strategy = new FixedGeographyFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Geography, strategy.DetailFieldType);
			AssertEquals(127, strategy.DetailMaxLength);
			AssertEquals(new ZGeography("POINT (-121 48)"), strategy.GetDefaultValue("POINT (-121 48)"));
			AssertEquals(new ZGeography("POINT (-122.6 49.9)"), strategy.GetDefaultValue("POINT (-122.6 49.9)"));
			IList boundCollection = strategy.GetBoundCollection(Factory);
			AssertType(typeof(ReadOnlyCodeDescriptionPairList), boundCollection);
			AssertMultilineASCIIEquals("", "", ((ReadOnlyCodeDescriptionPairList)boundCollection).ElementsAsString);
		}

		public void TestInvalidDefaultValue()
		{
			OperationalActionGeographyFieldSupporter supporter = new OperationalActionGeographyFieldSupporter("fieldName", false);
			FixedGeographyFieldDefaultingStrategy strategy = new FixedGeographyFieldDefaultingStrategy(supporter);
			AssertEquals(FieldType.Geography, strategy.DetailFieldType);
			AssertEquals(127, strategy.DetailMaxLength);
			AssertNoExceptionThrown(() => strategy.GetDefaultValue("Not a geography"));
		}
	}
}
