using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class RequiredFilterValidatorTest : TestCase
	{
		public void TestRequired()
		{
			RequiredFilterValidator testFilterCollectionValidator = new RequiredFilterValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("'EmptyFilter' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}
	}
}
