using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnConfigurationManagerBaseOnlyTest : TestCaseWithFactory
	{
		public void TestLoadReportWithXml()
		{
			CodeLookupField codeLookupField = new CodeLookupField(Factory);
			CodeListMultipleChoice codeListMulipleChoiceField = new CodeListMultipleChoice(Factory);
			LookupField lookupField = new LookupField(Factory);
			DateField dateField1 = new DateField(Factory);

			codeLookupField.DisplayName = "1";
			codeListMulipleChoiceField.DisplayName = "2";
			lookupField.DisplayName = "Link";
			dateField1.DisplayName = "4";

			var reportCommand = Factory.New<ReportCommand>();
			ColumnConfigurationManagerForTest testColumnConfigurationManager;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				report.ColumnHeadingManager.AddLinkedField(codeLookupField);
				report.ColumnHeadingManager.AddLinkedField(codeListMulipleChoiceField);
				report.ColumnHeadingManager.AddLinkedField(lookupField);
				report.ColumnHeadingManager.AddLinkedField(dateField1);
				report.GroupByCollection.Add("aaa", "aaa");
				report.SortOrderCollection.Add("bbb", "bbb");

				testColumnConfigurationManager = new ColumnConfigurationManagerForTest(report.ColumnHeadingManager, false);
				testColumnConfigurationManager.Load(report);

				testColumnConfigurationManager.SaveReport(report);
			}

			AssertEquals("Group By Name", "aaa", testColumnConfigurationManager.SelectedGroupByName);
			AssertEquals("Sort Order Name", "bbb", testColumnConfigurationManager.SelectedSortOrderName);
			AssertEquals("Orientation", ReportOrientationTypeList.Codes.Portrait, testColumnConfigurationManager.SelectedOrientation);
			AssertEquals("Language", Core.SharedConstants.Languages.ChineseSimplified, testColumnConfigurationManager.SelectedLanguage);

			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "1", "Code");
			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "2", "Multiple Choice Value");
			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "4", ZDateTime.Today);
		}

		public void TestLoadReportWithNoXml()
		{
			CodeLookupField codeLookupField = new CodeLookupField(Factory);
			CodeListMultipleChoice codeListMulipleChoiceField = new CodeListMultipleChoice(Factory);
			LookupField lookupField = new LookupField(Factory);
			DateField dateField1 = new DateField(Factory);

			codeLookupField.DisplayName = "1";
			codeListMulipleChoiceField.DisplayName = "2";
			lookupField.DisplayName = "Link";
			dateField1.DisplayName = "4";

			var reportCommand = Factory.New<ReportCommand>();
			ColumnConfigurationManagerForTest testColumnConfigurationManager;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				report.ColumnHeadingManager.AddLinkedField(codeLookupField);
				report.ColumnHeadingManager.AddLinkedField(codeListMulipleChoiceField);
				report.ColumnHeadingManager.AddLinkedField(lookupField);
				report.ColumnHeadingManager.AddLinkedField(dateField1);
				report.GroupByCollection.Add("aaa", "aaa");
				report.SortOrderCollection.Add("bbb", "bbb");

				SetupReportDefaultFilters(report);

				testColumnConfigurationManager = new ColumnConfigurationManagerForTest(report.ColumnHeadingManager, true);
				testColumnConfigurationManager.Load(report);

				testColumnConfigurationManager.SaveReport(report);
			}

			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "1", "Code");
			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "2", "Multiple Choice Value");
			AssertFilterFieldEquals(testColumnConfigurationManager.Filters, "4", ZDateTime.Today);
		}

		void AssertFilterFieldEquals(CollectionOfIFilter filters, string displayName, object expectedValue)
		{
			var filterField = filters[displayName];
			AssertNotNull($"Field {displayName} does not exist", filterField);

			AssertEquals($"Field {displayName} Value {filterField.ValueAsObject} not expected", expectedValue, filterField.ValueAsObject);
		}

		void SetupReportDefaultFilters(Report report)
		{
			ColumnConfigurationField columnConfigField = new ColumnConfigurationField(Factory);
			CodeLookupField codeLookupField = new CodeLookupField(Factory);
			CodeListMultipleChoice codeListMulipleChoiceField = new CodeListMultipleChoice(Factory);
			LookupField lookupField = new LookupField(Factory);
			DateField dateField1 = new DateField(Factory);

			codeLookupField.DisplayName = "1";
			codeLookupField.Value = "Code";

			RefUNLOCOCollectionProvider collectionProvider = new RefUNLOCOCollectionProvider(Factory);
			codeLookupField.SetCollectionProvider(collectionProvider);

			codeListMulipleChoiceField.DisplayName = "2";
			codeListMulipleChoiceField.Value = "Multiple Choice Value";

			lookupField.DisplayName = "Link";
			lookupField.Value = Guid.NewGuid();
			lookupField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));

			dateField1.DisplayName = "4";
			dateField1.Value = ZDateTime.Today;

			report.ColumnHeadingManager.DefaultFilters = new CollectionOfIFilter();
			var defaultFilters = report.ColumnHeadingManager.DefaultFilters;
			defaultFilters.Add(codeLookupField);
			defaultFilters.Add(codeListMulipleChoiceField);
			defaultFilters.Add(lookupField);
			defaultFilters.Add(dateField1);
		}

		sealed class ColumnConfigurationManagerForTest : ColumnConfigurationManager
		{
			public ColumnConfigurationManagerForTest(ColumnConfigurationsManager headingManager, bool xmlEmpty) : base(headingManager)
			{
				this.xmlEmpty = xmlEmpty;
			}

			readonly bool xmlEmpty;

			protected override string GetXml()
			{
				if (xmlEmpty)
				{
					return string.Empty;
				}

				var filters = new CollectionOfIFilter();
				var factory = new BusinessObjectFactory();

				var codeLookupField = new CodeLookupField(factory);
				var codeListMulipleChoiceField = new CodeListMultipleChoice(factory);
				var lookupField = new LookupField(factory);
				var dateField1 = new DateField(factory);

				codeLookupField.DisplayName = "1";
				codeLookupField.Value = "Code";

				var collectionProvider = new RefUNLOCOCollectionProvider(factory);
				codeLookupField.SetCollectionProvider(collectionProvider);

				codeListMulipleChoiceField.DisplayName = "2";
				codeListMulipleChoiceField.Value = "Multiple Choice Value";

				lookupField.DisplayName = "Link";
				lookupField.Value = Guid.NewGuid();
				lookupField.SetCollectionProvider(new RefUNLOCOCollectionProvider(factory));

				dateField1.DisplayName = "4";
				dateField1.Value = ZDateTime.Today;

				filters.Add(codeLookupField);
				filters.Add(codeListMulipleChoiceField);
				filters.Add(lookupField);
				filters.Add(dateField1);

				var selectedGroupByName = "aaa";
				var selectedSortOrderName = "bbb";
				var selectedLanguage = Core.SharedConstants.Languages.ChineseSimplified;
				var selectedOrientation = ReportOrientationTypeList.Codes.Portrait;

				return CreateXml(filters, selectedGroupByName, selectedSortOrderName, selectedOrientation, selectedLanguage);
			}

			protected override void DeleteCore()
			{
			}

			protected override void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string orientation, string selectedLanguage)
			{
				Filters = filters;
				SelectedGroupByName = selectedGroupByName;
				SelectedSortOrderName = selectedSortOrderName;
				SelectedLanguage = selectedLanguage;
				SelectedOrientation = orientation;
			}

			public override string ToString()
			{
				return $"Filters count {Filters.Count}, {SelectedGroupByName}, {SelectedSortOrderName}, {SelectedLanguage}, {SelectedOrientation}";
			}

			public override ZString Description => "Description";

			public override ZGuid LinkPK => ZGuid.NewZGuid();

			public override ZString UniqueDescription => string.Empty;

			public override ZString LinkCode => "LinkCode";

			public void SaveReport(Report report)
			{
				Save(report.ColumnHeadingManager.LinkedFilterFields, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.OrientationManager.Value, report.Parent.Language);
			}

			public CollectionOfIFilter Filters;
			public string SelectedGroupByName;
			public string SelectedSortOrderName;
			public string SelectedLanguage;
			public string SelectedOrientation;
		}
	}
}
