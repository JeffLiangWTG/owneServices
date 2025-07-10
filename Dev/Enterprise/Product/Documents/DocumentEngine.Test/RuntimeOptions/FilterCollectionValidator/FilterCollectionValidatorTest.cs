using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterCollectionValidatorTest : TestCase
	{
		public void TestFileNamesJoined1Name()
		{
			var testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidatorForTesting();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			AssertEquals("'EmptyFilter'", testFilterCollectionValidator.FieldNamesJoined);
		}

		public void TestFileNamesJoined2Names()
		{
			var testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidatorForTesting();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			AssertEquals("'EmptyFilter' and 'EmptyFilter2'", testFilterCollectionValidator.FieldNamesJoined);
		}

		public void TestFileNamesJoined3Names()
		{
			var testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidatorForTesting();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			var emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			testFilterCollectionValidator.Filters.Add(emptyFilter3);
			AssertEquals("'EmptyFilter', 'EmptyFilter2' and 'EmptyFilter3'", testFilterCollectionValidator.FieldNamesJoined);
		}

		public void TestFileNamesJoined4Names()
		{
			var testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidatorForTesting();
			var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			var emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			var emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
			var emptyFilter4 = new DummyFilterField(true, "EmptyFilter4", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			testFilterCollectionValidator.Filters.Add(emptyFilter3);
			testFilterCollectionValidator.Filters.Add(emptyFilter4);
			AssertEquals("'EmptyFilter', 'EmptyFilter2', 'EmptyFilter3' and 'EmptyFilter4'", testFilterCollectionValidator.FieldNamesJoined);
		}

		public void TestFileNamesJoinedIsTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("dcc0de8e-3d2e-4cf5-8097-3ee99fe60c32", new ResourceStringData("dcc0de8e-3d2e-4cf5-8097-3ee99fe60c32", "和"));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var testFilterCollectionValidator = new AtLeastOneFilterNotEmptyValidatorForTesting();
					var emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
					var emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
					var emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
					var emptyFilter4 = new DummyFilterField(true, "EmptyFilter4", new BusinessObjectFactory());
					testFilterCollectionValidator.Filters.Add(emptyFilter);
					testFilterCollectionValidator.Filters.Add(emptyFilter2);
					testFilterCollectionValidator.Filters.Add(emptyFilter3);
					testFilterCollectionValidator.Filters.Add(emptyFilter4);
					AssertEquals("'EmptyFilter', 'EmptyFilter2', 'EmptyFilter3' 和 'EmptyFilter4'", testFilterCollectionValidator.FieldNamesJoined);
				}
			}
		}

		public class AtLeastOneFilterNotEmptyValidatorForTesting : AtLeastOneFilterNotEmptyValidator
		{
			protected internal new string FieldNamesJoined => base.FieldNamesJoined;
		}
	}
}
