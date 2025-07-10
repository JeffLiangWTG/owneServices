using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class AtLeastOneFilterNotEmptyValidatorTest : TestCase
	{
		public void TestAtLeastOneWithOneNonEmptyAndOneEmpty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
		}

		public void TestAtLeastOneWithMoreThanOneNonEmpty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter2 = new DummyFilterField(false, "NonEmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter2);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter2));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
		}

		public void TestAtLeastOneWithAllEmpty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("At least one of the 'EmptyFilter' and 'EmptyFilter2' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWith3Empty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			DummyFilterField emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			testFilterCollectionValidator.Filters.Add(emptyFilter3);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter3));
			AssertEquals("At least one of the 'EmptyFilter', 'EmptyFilter2' and 'EmptyFilter3' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWithOneEmpty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("'EmptyFilter' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWithOneNonEmpty()
		{
			AtLeastOneFilterNotEmptyValidator testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidator();
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
		}
	}
}
