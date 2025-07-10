using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterCollectionBuilderTest : FilterBuilderTestWithTempFile
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnConfigurationFieldIsAddedWhenThereIsAtLeastOneFilter()
		{
			Prepare("TextFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(ColumnConfigurationField), fb.IFilterCollection[0].GetType());
			AssertEquals("Filter type", typeof(TextField), fb.IFilterCollection[1].GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithAllFiltersOutsideOfPrimaryFilters()
		{
			Prepare("TextNothingInPrimaryFilters.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 4, fb.IFilterCollection.Count);
			AssertEquals("Tabs count", 3, fb.IFilterCollection.FilterGroups.Count);
			AssertEquals("First tab is Primary Fields (with empty key)", "", fb.IFilterCollection.FilterGroups[0].Code);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoFilterSheet()
		{
			Prepare("NoFilterSheet.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Should have an empty filter collection if there is no filter sheet", 0, fb.IFilterCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestNoEndMarker()
		{
			Prepare("EmptyFilterSheet.xls");
			fb.Build();
			ExpectError("no #end marker");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoFilters()
		{
			Prepare("NoFilters.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals(0, fb.IFilterCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadOnlyIfRelation()
		{
			Prepare("MultipleFiltersWithReadOnlyIf.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 3, fb.IFilterCollection.Count);

			var filter = (TextField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Filter 1", filter.DisplayName);

			var filter2 = (TextField)fb.IFilterCollection[2];
			AssertEquals("Display name", "Filter 2", filter2.DisplayName);
			AssertEquals("ReadOnlyIf Relation is set up", true, filter2.HasReadOnlyIfFilter);
			AssertEquals("ReadOnlyIf Relation is set up", true, filter2.ReadOnly);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDependentFilterRelation()
		{
			Prepare("MultipleFiltersWithDependentFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 6, fb.IFilterCollection.Count);

			var filter1 = (CodeListMultipleChoice)fb.IFilterCollection[1];
			AssertEquals("DependentFilter Relation is set up", true, filter1.HasDependentFilter);
			AssertEquals("Display name", "Filter 1", filter1.DisplayName);

			var filter2 = (LookupField)fb.IFilterCollection[2];
			AssertEquals("Display name", "Filter 2", filter2.DisplayName);
			AssertEquals("ReadOnlyIf Relation is set up", true, filter2.HasReadOnlyIfFilter);
			AssertEquals("ReadOnlyIf Relation is set up", true, filter2.ReadOnly);
			AssertNotNull("Collection provider should not be null", filter2.CollectionProvider);
			AssertNull(filter2.CollectionProvider.CollectionForFindbox);
			AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned, filter2.CollectionProvider.ModuleID);

			filter1.ZValue = "ORG";
			AssertEquals("ReadOnlyIf Relation is reset", false, filter2.ReadOnly);
			AssertNotNull("Collection provider should not be null", filter2.CollectionProvider);
			AssertNotNull(filter2.CollectionProvider.CollectionForFindbox);
			AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation, filter2.CollectionProvider.ModuleID);

			var filter3 = (LookupField)fb.IFilterCollection[3];
			AssertEquals("Display name", "Filter 3", filter3.DisplayName);
			AssertEquals("No ReadOnlyIf Relation", false, filter3.HasReadOnlyIfFilter);
			AssertEquals("ReadOnlyIf Relation is set up for no dependent filter", true, filter3.ReadOnly);

			var filter4 = (CodeListMultipleChoice)fb.IFilterCollection["Filter 4"];
			Assert(filter4.HasDependentFilter);

			var filter5 = (CodeListMultipleChoice)fb.IFilterCollection["Filter 5"];
			Assert(filter5.HasReadOnlyIfFilter);
			AssertNotNull(filter5.DependenceListProvider);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTextFilter()
		{
			Prepare("TextFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(TextField), fb.IFilterCollection[1].GetType());
			var tf = (TextField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some description", tf.DisplayName);
			AssertEquals("Field name", "XX_Field", tf.FieldName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTextFilterWithDefault()
		{
			Prepare("TextFilterWithDefault.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(TextField), fb.IFilterCollection[1].GetType());
			var tf = (TextField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some description", tf.DisplayName);
			AssertEquals("Field name", "XX_Field", tf.FieldName);
			AssertEquals("Field name", "Hello", tf.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountingPeriodFilter()
		{
			Prepare("AccountingPeriodFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(AccountingPeriodField), fb.IFilterCollection[1].GetType());
			var apf = (AccountingPeriodField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some period", apf.DisplayName);
			AssertEquals("Field name", "XX_Period", apf.FieldName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDateFilter()
		{
			Prepare("DateFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(DateField), fb.IFilterCollection[1].GetType());
			var df = (DateField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some date", df.DisplayName);
			AssertEquals("Field name", "XX_date", df.FieldName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDateRangeFilter()
		{
			Prepare("DateRangeFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(DateRangeField), fb.IFilterCollection[1].GetType());
			var drf = (DateRangeField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some date", drf.DisplayName);
			AssertEquals("Field name", "XX_date", drf.FieldName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleSelectionLookupFilter()
		{
			Prepare("MultipleSelectionLookup.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 3, fb.IFilterCollection.Count);
			var filter = fb.IFilterCollection[1];
			AssertType("Filter type", typeof(MultipleSelectionLookup), filter);
			var lookup = (MultipleSelectionLookup)filter;
			AssertEquals("Display name", "Orgs", lookup.DisplayName);
			AssertEquals("Field name", "OH_PK", lookup.FieldName);
			AssertEquals("Field name", false, lookup.IsFilterValueExcluded);
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.OrgHeaderCollection), lookup.BindToList.GetType());

			filter = fb.IFilterCollection[2];
			AssertType("Filter type", typeof(MultipleSelectionLookup), filter);
			lookup = (MultipleSelectionLookup)filter;
			AssertEquals("Display name", "Excluded Orgs", lookup.DisplayName);
			AssertEquals("Field name", "OH_PK", lookup.FieldName);
			AssertEquals("Field name", true, lookup.IsFilterValueExcluded);
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.OrgHeaderCollection), lookup.BindToList.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookupFilter()
		{
			Prepare("LookupFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 3, fb.IFilterCollection.Count);
			AssertEquals("Filter 1 type", typeof(LookupField), fb.IFilterCollection[1].GetType());
			var lf = (LookupField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some lookup", lf.DisplayName);
			AssertEquals("Field name", "XX_YY", lf.FieldName);
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.CreditorCollection), lf.BindToList.GetType());

			AssertEquals("Filter 2 type", typeof(LookupField), fb.IFilterCollection[2].GetType());
			lf = (LookupField)fb.IFilterCollection[2];
			AssertEquals("Display name", "Other lookup", lf.DisplayName);
			AssertEquals("Field name", "XX_RL", lf.FieldName);
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.RefUNLOCOCollection), lf.BindToList.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookupFilterWithDefault()
		{
			Prepare("LookupFilterWithDefault.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 3, fb.IFilterCollection.Count);
			AssertEquals("Filter 1 type", typeof(LookupField), fb.IFilterCollection[1].GetType());
			var lf = (LookupField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some lookup", lf.DisplayName);
			AssertEquals("Field name", "XX_YY", lf.FieldName);
			AssertEquals("Default", "e8bd88d2-a5c1-43fe-a788-a53adbb86403", lf.Value.ToString());
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.CreditorCollection), lf.BindToList.GetType());

			AssertEquals("Filter 2 type", typeof(LookupField), fb.IFilterCollection[2].GetType());
			lf = (LookupField)fb.IFilterCollection[2];
			AssertEquals("Display name", "Other lookup", lf.DisplayName);
			AssertEquals("Field name", "XX_RL", lf.FieldName);
			AssertEquals("Lookup type", typeof(Enterprise.MasterFiles.Business.RefUNLOCOCollection), lf.BindToList.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLookupFilterInvalidFindboxType()
		{
			Prepare("LookupFilterInvalid.xls");
			fb.Build();
			ExpectError("unknown lookup type \"Rocky the Flying Squirrel\"");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionGroupFilter()
		{
			Prepare("OptionGroupFilter.xls");
			fb.Build();
			ExpectNoErrors();

			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(OptionGroup), fb.IFilterCollection[1].GetType());
			var og = (OptionGroup)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some code", og.DisplayName);
			AssertEquals("Field name", "XX_code", og.FieldName);

			AssertEquals("Option count", 4, og.DescriptionCodePairList.Count);

			AssertEquals("Option 1 display name", "One", og.DescriptionCodePairList[0].Description);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionGroupFilterNoOptions()
		{
			Prepare("OptionGroupFilterNoOptions.xls");
			fb.Build();
			ExpectError("at least one option is required");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTextRangeFilter()
		{
			Prepare("TextRangeFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(TextRangeField), fb.IFilterCollection[1].GetType());
			var df = (TextRangeField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some text range", df.DisplayName);
			AssertEquals("Field name", "XX_whatever", df.FieldName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRegistrationCodeFilter()
		{
			Prepare("RegistrationCodeFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(RegistrationCodeField), fb.IFilterCollection[1].GetType());
			var registrationCodeField = (RegistrationCodeField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Some registration code", registrationCodeField.DisplayName);
			AssertEquals("Field name", "RegistrationCode", registrationCodeField.FieldName);
			AssertEquals("Field name code country", "ACodeCountry", registrationCodeField.FieldNameCodeCountry);
			AssertEquals("Field name custom type", "ACustomType", registrationCodeField.FieldNameCustomType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSalesTradeLaneChecklistFilter()
		{
			Prepare("SalesTradeLaneChecklistFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(SalesTradeLaneChecklistField), fb.IFilterCollection[1].GetType());
			var field = (SalesTradeLaneChecklistField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Trade Lane", field.DisplayName);
			AssertEquals("Product Field name", "XX_Product", field.FieldName);
			AssertEquals("Mode name", "XX_Mode", field.ModeField);
			AssertEquals("Type name", "XX_Type", field.TypeField);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPermitTypeChecklistFilter()
		{
			Prepare("PermitTypeChecklistFilter.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Filter count", 2, fb.IFilterCollection.Count);
			AssertEquals("Filter type", typeof(PermitTypeChecklistField), fb.IFilterCollection[1].GetType());
			var field = (PermitTypeChecklistField)fb.IFilterCollection[1];
			AssertEquals("Display name", "Permit Type/Sub Type", field.DisplayName);
			AssertEquals("Type name", "XX_Type", field.FieldName);
			AssertEquals("SubType name", "XX_SubType", field.SubTypeField);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmptyCollectionReturnedWhenAnyErrorIsPresent()
		{
			Prepare("TwoOutOfThreeAintBad.xls");
			fb.Build();
			ExpectError("not allowed in this type of filter");
			AssertEquals("Should not get ANY filters when there are ANY errors", 0, fb.IFilterCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParameters()
		{
			Prepare("Parameters.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("Param count", 3, fb.IFilterCollection.Count);
			((TextField)fb.IFilterCollection[1]).Value = "xxx";
			AssertEquals("Where clause of param", "", fb.IFilterCollection[0].WhereClause());
			AssertEquals("SQL param of param", "xxx", fb.IFilterCollection[1].SqlParameters()[0].Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildFilterGroups()
		{
			Prepare("MultipleFiltersWithGroups.xls");
			fb.Build();
			ExpectNoErrors();
			AssertEquals("There should be three groups", 3, fb.IFilterCollection.FilterGroups.Count);
			AssertEquals("Filter groups should contain a '' group", true, fb.IFilterCollection.FilterGroups.ContainsCode(""));
			AssertNullOrEmpty("Filter groups should contain 'Primary Filters' Description", fb.IFilterCollection.FilterGroups[""].Description);
			AssertEquals("Filter groups should contain a 'One' group", true, fb.IFilterCollection.FilterGroups.ContainsCode("One"));
			AssertEquals("Filter groups should contain 'Filter Group One' Description", true, fb.IFilterCollection.FilterGroups["One"].Description == "Filter Group One");
			AssertEquals("Filter groups should contain a 'Two' group", true, fb.IFilterCollection.FilterGroups.ContainsCode("Two"));
			AssertEquals("Filter groups should contain 'Filter Group Two' Description", true, fb.IFilterCollection.FilterGroups["Two"].Description == "Filter Group Two");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilterGroupCollectionDoesntReturnWhenTheresAnError()
		{
			Prepare("MultipleFiltersWithGroupsAndErrors.xls");
			fb.Build();
			ExpectError("not allowed in this type of filter");
			AssertEquals("There should be no filter groups", 0, fb.IFilterCollection.FilterGroups.Count);
		}

		protected override void TearDown()
		{
			base.TearDown();
			rpt?.Dispose();
		}

		FilterCollectionBuilder fb;
		ReportAnalyser ra;
		ExcelTemplateForUnitTesting excelTemplate;
		Report rpt;
		DocumentPack pack;

		void Prepare(string filename)
		{
			pack = new DocumentPack();
			excelTemplate = new ExcelTemplateForUnitTesting(filename, TestFilesSubFolder.ReportTestFiles);
			rpt = new Report(pack, excelTemplate);
			ra = new ReportAnalyser(rpt);
			fb = new FilterCollectionBuilder(ra.DataSourceParameters, ra.ValidatorPack, new StringTreeBuilder(rpt.FilterSheet).GetTree(), DummyEvaluator);
		}

		void ExpectNoErrors()
		{
			var errorList = "";
			foreach (ReportProcessingError error in fb.Errors)
			{
				errorList += error + System.Environment.NewLine;
			}

			Assert("There should not be any errors, but these errors were present:\n" + errorList, !fb.HasErrors);
		}

		void ExpectError(string errorMessage)
		{
			foreach (ReportProcessingError error in fb.Errors)
			{
				if (Regex.IsMatch(error.ToString(), errorMessage, RegexOptions.IgnoreCase))
				{
					AssertionCount++;
					return;
				}
			}

			Fail("Expected error message <" + errorMessage + "> was not found. Error count: " + fb.Errors.Count);
		}
	}
}
