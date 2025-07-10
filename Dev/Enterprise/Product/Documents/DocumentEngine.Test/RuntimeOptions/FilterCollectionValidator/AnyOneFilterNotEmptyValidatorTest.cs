using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class AnyOneFilterNotEmptyValidatorTest : TestCaseWithFactory
	{
		public void TestAnyOneWithOneNonEmptyAndOneEmpty()
		{
			var testFilterCollectionValidator = new AnyOneFilterNotEmptyValidator();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
		}

		public void TestAnyOneWithMoreThanOneNonEmpty()
		{
			var testFilterCollectionValidator = new AnyOneFilterNotEmptyValidator();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			var nonEmptyFilter2 = new DummyFilterField(false, "NonEmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter2);
			AssertEquals(false, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(false, testFilterCollectionValidator.IsValid(nonEmptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("Only one of the 'EmptyFilter', 'NonEmptyFilter' and 'NonEmptyFilter2' could have data.", testFilterCollectionValidator.GetErrorMessage(nonEmptyFilter));
		}

		public void TestAnyOneWith3Empty()
		{
			var testFilterCollectionValidator = new AnyOneFilterNotEmptyValidator();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			var emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			testFilterCollectionValidator.Filters.Add(emptyFilter3);
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter2));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter3));
		}

		public void TestAnyOneWithOneEmpty()
		{
			var testFilterCollectionValidator = new AnyOneFilterNotEmptyValidator();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
		}

		public void TestAnyOneWithOneNonEmpty()
		{
			var testFilterCollectionValidator = new AnyOneFilterNotEmptyValidator();
			var nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
		}
	}
}
