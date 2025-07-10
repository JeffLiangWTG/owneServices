using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterFieldBaseOnlyTest : TestCaseWithFactory
	{
		public void TestSetupReadOnlyIfRelation()
		{
			df = new DummyFilterField(true, "Filter 1", new BusinessObjectFactory());
			AssertEquals("Should not have filter set", false, df.HasReadOnlyIfFilter);
			AssertEquals(false, df.ReadOnly);

			df.ReadOnlyIfFilter = "Filter 2";
			AssertEquals("Should have filter set", true, df.HasReadOnlyIfFilter);

			var filters = new CollectionOfIFilter();
			filters.Add(df);
			AssertExceptionThrown(typeof(DocumentEngineException), "Filter 'Filter 2' could not be found in master-detail relation.", () => df.SetupReadOnlyIfRelation(filters));

			var filter = new DummyFilterField(true, "Filter 2", new BusinessObjectFactory());
			filters.Add(filter);
			AssertEquals("Precondition", false, filter.IsUsedToDetermineReadOnlyOfRelatedFilter);
			AssertNoExceptionThrown(() => df.SetupReadOnlyIfRelation(filters));
			AssertEquals(true, df.ReadOnly);
			AssertEquals(true, filter.IsUsedToDetermineReadOnlyOfRelatedFilter);
		}

		public void TestSetupDependentRelation()
		{
			df = new DummyFilterField(true, "Filter 1", new BusinessObjectFactory());
			AssertEquals("Should not have dependent filter", false, df.HasDependentFilter);

			df.DependentFilter = "Filter 2";
			AssertEquals("Should have filter set", true, df.HasDependentFilter);

			var filters = new CollectionOfIFilter();
			filters.Add(df);
			AssertExceptionThrown(typeof(DocumentEngineException), "Filter 'Filter 2' could not be found in master-detail relation.", () => df.SetupDependentFilterRelation(filters));

			var filter = new DummyFilterField(true, "Filter 2", new BusinessObjectFactory());
			filters.Add(filter);
			AssertNoExceptionThrown(() => df.SetupDependentFilterRelation(filters));
		}

		public void TestDependentFilterReadonlyIfValue()
		{
			var filters = new CollectionOfIFilter();
			var filter = new CodeListMultipleChoice(Factory)
			{
				DisplayName = "Filter 1",
				Value = "123",
				DependentFilter = "Filter 2",
				ReadOnlyIfFilter = "Filter 2",
				DependentFilterReadonlyIfValue = "abc"
			};
			filters.Add(filter);

			var dependentFilter = new DummyFilterField(true, "Filter 2", new BusinessObjectFactory());
			filters.Add(dependentFilter);

			filter.SetupDependentFilterRelation(filters);
			dependentFilter.SetupReadOnlyIfRelation(filters);

			Assert("Filter 'Filter 2' should not be read only", !dependentFilter.ReadOnly);

			filter.ZValue = "abc";
			Assert("Filter 'Filter 2' should be read only", dependentFilter.ReadOnly);

			filter.ZValue = "def";
			Assert("Filter 'Filter 2' should not be read only", !dependentFilter.ReadOnly);
		}

		public void TestSetupDependentRelationThrownNoExceptionWhenValueAsObjectIsNull()
		{
			var filter1 = new DummyFilterField(true, "Filter 1", new BusinessObjectFactory());
			filter1.DependentFilter = "Filter 2";

			var filter2 = new MultipleChoice(new BusinessObjectFactory());
			filter2.DisplayName = "Filter 2";
			filter2.DependentFilter = "Filter 1";

			var filter3 = new MultipleChoice(new BusinessObjectFactory());
			filter3.DisplayName = "Filter 3";
			filter3.DependentFilter = "Filter 2";

			var filters = new CollectionOfIFilter();
			filters.Add(filter1);
			filters.Add(filter2);
			filters.Add(filter3);

			AssertNoExceptionThrown(() => filter2.SetupDependentFilterRelation(filters));
			AssertNoExceptionThrown(() => filter1.SetupDependentFilterRelation(filters));
		}

		public void TestMultipleDependentFilters()
		{
			var filter1 = new DummyFilterField(true, "Filter 1", new BusinessObjectFactory());
			filter1.DependentFilter = "Filter 2, Filter 3";

			var filter2 = new MultipleChoice(new BusinessObjectFactory());
			filter2.DisplayName = "Filter 2";

			var filter3 = new MultipleChoice(new BusinessObjectFactory());
			filter3.DisplayName = "Filter 3";

			var filters = new CollectionOfIFilter();
			filters.Add(filter1);
			filters.Add(filter2);
			filters.Add(filter3);

			AssertNoExceptionThrown(() => filter1.SetupDependentFilterRelation(filters));
			AssertEquals("Filter 1", filter2.ReadOnlyIfFilter);
			AssertEquals("Filter 1", filter3.ReadOnlyIfFilter);
		}

		public void TestFieldIsCompatibleWithOtherField()
		{
			DummyFilterField field1 = new DummyFilterField(true, "Some Dummy", new BusinessObjectFactory());
			field1.FieldName = "australia";
			DummyFilterField field2 = new DummyFilterField(true, "Some Dummy", new BusinessObjectFactory());
			field2.FieldName = "australia";
			AssertEquals(true, field1.IsCompatibleWith(field2));

			field2.DisplayName = "Hello";
			AssertEquals(false, field1.IsCompatibleWith(field2));

			field2.DisplayName = "Some Dummy";
			field2.FieldName = "new zealand";
			AssertEquals(false, field1.IsCompatibleWith(field2));

			MultipleSelectionLookup field3 = new MultipleSelectionLookup(new BusinessObjectFactory());
			field3.DisplayName = "Some Dummy";
			field3.FieldName = "australia";
			AssertEquals(false, field1.IsCompatibleWith(field3));
		}

		public void TestReturnsEmptiesForWhereClause()
		{
			df = new DummyFilterField(true, "", new BusinessObjectFactory());
			df.FieldName = "x";
			AssertEquals("Where clause should be empty when field is empty", "", df.WhereClause());
			AssertEquals("Parameter list should be empty when field is empty", 0, df.SqlParameters().Count);
		}

		public void TestHumanReadableNameIsDisplayName()
		{
			df = new DummyFilterField(true, "DisplayName", new BusinessObjectFactory());
			AssertEquals("Human readable should be displayname", "DisplayName", df.HumanReadableName);
		}

		public void TestReturnsNonEmptiesForWhereClause()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			df.FieldName = "x";
			AssertEquals("Where clause should not be empty when field is non-empty", "non empty", df.WhereClause());
			AssertEquals("Parameter list should not be empty when field is non-empty", 1, df.SqlParameters().Count);
		}

		public void TestReturnsNonEmptiesForParam()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			df.FieldName = "";
			AssertEquals("Where clause should be empty when fieldname is empty", "", df.WhereClause());
			AssertEquals("Parameter list should not be empty for params", 1, df.SqlParameters().Count);
		}

		public void TestReturnsNonEmptiesForDual()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			df.FieldName = "x";
			AssertEquals("Where clause should not be empty when fieldname is empty", "non empty", df.WhereClause());
			AssertEquals("Parameter list should not be empty for params", 1, df.SqlParameters().Count);
		}

		public void TestDefaultExpression()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			AssertEquals("Default expression after construction", "", df.DefaultExpression);

			df.DefaultExpression = "hi";
			AssertEquals("Default expression after construction", "hi", df.DefaultExpression);
		}

		public void TestIsResponsibleForReplacing()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			df.DisplayName = "abcd";

			int responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<abcd>", Passes.FirstPass))
				{
					AssertEquals(null, provider.GetReplacement("<abcd>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <abcd>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForReplacingUserFriendlyValue_Empty()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			df.DisplayName = "abcd";

			int responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<abcd.UserFriendlyValue>", Passes.FirstPass))
				{
					AssertEquals(null, provider.GetReplacement("<abcd.UserFriendlyValue>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <abcd.UserFriendlyValue>!", 1, responsibleCount);
		}

		public void TestIsResponsibleForReplacingUserFriendlyValue_NonEmpty()
		{
			df = new DummyFilterField(true, "", new BusinessObjectFactory());
			df.DisplayName = "abcd";

			int responsibleCount = 0;
			foreach (ValueProvider provider in df.ValueProviders)
			{
				if (provider.IsResponsibleForReplacing("<abcd.UserFriendlyValue>", Passes.FirstPass))
				{
					AssertEquals("All", provider.GetReplacement("<abcd.UserFriendlyValue>", new Report(null, null)));
					responsibleCount++;
				}
			}
			AssertEquals("Should have one responsible for <abcd.UserFriendlyValue>!", 1, responsibleCount);
		}

		public void TestHeightWidthLeftTop()
		{
			df = new DummyFilterField(false, "", new BusinessObjectFactory());
			AssertEquals("Default top to unspecified value", df.Unspecified, df.Top);
			AssertEquals("Default left to unspecified value", df.Unspecified, df.Left);
			AssertEquals("Default width to unspecified value", df.Unspecified, df.Width);
			AssertEquals("Default height to unspecified value", df.Unspecified, df.Height);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisableFixedValueForUDFWithOverridenValue()
		{
			UserControlProviderList list = new UserControlProviderList();
			TextField field = new TextField(Factory);
			field.DisplayName = "JohnSnow";
			field.Value = "123";
			field.DefaultExpression = "<CompanyCode>";
			field.IsOverriddenInDocData = true;
			list.Add(field);

			DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";

			PrepareReportAndAssertCode("UDF with defaults.xlsx", "123", list);

			GlbCompany.CurrentCompany.GC_Code = "EDI";
			PrepareReportAndAssertCode("UDF with DisableFixedValueCache.xls", "EDI", list);

			//Loading UDF value in a report with DisableFixedValueChache should not change the value in Fixed value cache
			PrepareReportAndAssertCode("UDF with defaults.xlsx", "123", list);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisableFixedValueForUDFWithNoOverridenValue()
		{
			UserControlProviderList list = new UserControlProviderList();

			DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";

			GlbCompany.CurrentCompany.GC_Code = "EDI";
			PrepareReportAndAssertCode("UDF with defaults.xlsx", "EDI", list);

			GlbCompany.CurrentCompany.GC_Code = "ABC";
			PrepareReportAndAssertCode("UDF with DisableFixedValueCache.xls", "ABC", list);

			GlbCompany.CurrentCompany.GC_Code = "XYZ";
			PrepareReportAndAssertCode("UDF with defaults.xlsx", "XYZ", list);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisableFixedValueShouldNotClearNextNormalDocument()
		{
			var list = new UserControlProviderList();
			var field = new TextField(Factory)
			{
				DisplayName = "JohnSnow",
				DefaultExpression = "<CompanyCode>"
			};
			list.Add(field);

			GlbCompany.CurrentCompany.GC_Code = "ABC";
			PrepareReportAndAssertCode("UDF with DisableFixedValueCache.xls", "ABC", list);

			PrepareReportAndAssertCode("UDF with defaults.xlsx", "ABC", list);
		}

		public void TestSpecialValueProviderOfIsEmpty()
		{
			df = new DummyFilterField(false, "DisplayName", Factory);
			AssertEquals("Value Should be false", false, GetIsEmptyValueProvider(df).GetReplacement("<DisplayName.IsEmpty>", Report.NewForTesting(null)));

			df = new DummyFilterField(true, "DisplayName", Factory);
			AssertEquals("Value Should be true", true, GetIsEmptyValueProvider(df).GetReplacement("<DisplayName.IsEmpty>", Report.NewForTesting(null)));

			var isEmptyValueProviderDocumenter = df.ValueProviderDocumenters.Find(v => v.Useage == "<DisplayName.IsEmpty>");
			AssertNotNull(isEmptyValueProviderDocumenter);
			AssertEquals("documenter.Explanation", "Returns a boolean set to true if the current filter has no data, false otherwise.", isEmptyValueProviderDocumenter.Explanation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2017, 2, 9)]
		public void TestGetValueShouldReturnCorrectTypeWhenDisabledFixedValue()
		{
			var list = new UserControlProviderList();
			var field = new DateField(Factory)
			{
				DisplayName = "TestDate",
				DefaultExpression = "<Now>"
			};
			list.Add(field);

			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with DisableFixedValueCache.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				var objectValue = report.MacroTranslator.GetValue("<TestDate>", Passes.SecondPass);
				Assert("Translator.GetValue should return ZDateTime", objectValue is ZDateTime);
				AssertEquals(new ZDateTime(2017, 2, 9), objectValue);
			}
		}

		public void TestDefaultIsOverriddenInDocDataFalse()
		{
			df = new DummyFilterField(true, "filter", Factory);
			Assert(!df.IsOverriddenInDocData);
		}

		ValueProvider GetIsEmptyValueProvider(DummyFilterField dummy)
		{
			return dummy.ValueProviders.Find(v => v.IsResponsibleForReplacing("<DisplayName.IsEmpty>", Passes.FirstPass));
		}

		void PrepareReportAndAssertCode(string templatePath, string companyCode, UserControlProviderList list)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templatePath, TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(new DocumentPack(), excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("Should return expected company code", companyCode, report.MacroTranslator.GetValue("<JohnSnow>", Passes.FirstPass));
			}
		}

		DummyFilterField df;
	}
}
