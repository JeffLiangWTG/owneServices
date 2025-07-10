using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SavedScheduleSettingTest : NUnit.Framework.TestCase
	{
		public void TestCanSaveAndDelete()
		{
			AssertEquals("CanSaveAndDelete", false, Setting.CanSaveAndDelete);
		}

		public void TestLoad()
		{
			ColumnHeading[] headings =
			{
				new ColumnHeading("1"),
				new ColumnHeading("2")
			};

			var columnSettings = new ReportColumnSettings();
			columnSettings.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headings), "Sheet1"));

			Setting.AddHeadings(columnSettings);
			Setting.Load();
			AssertEquals("HeadingManager.Headings.Count", 2, Setting.HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			AssertEquals("HeadingManager.Headings[0]", headings[0], Setting.HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0]);
			AssertEquals("HeadingManager.Headings[0]", headings[1], Setting.HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1]);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveXml()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateRangeFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				Setting.Load();
				Setting.SaveXml(report);
				AssertNotNull(Setting.GetXml());
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFiltersSerialiseAndDeserialise()
		{
			var factory = new BusinessObjectFactory();
			var excelTemplate = new ExcelTemplateForUnitTesting("FilterSortGroupby.xls", TestFilesSubFolder.ReportTestFiles);
			var menuItem = factory.LoadTop1<StmMenuItem>(new ZQuery());
			using (var report = new Report(new DocumentPack(menuItem), excelTemplate))
			{
				report.PrepareForRender();
				Setting.Load();
				Setting.SaveXml(report);
				var xml = Setting.GetXml();

				var text = new TextField(factory) { DisplayName = "Text Stuff" };
				var number = new NumberField(factory) { DisplayName = "Age" };
				var multipleChoice = new MultipleChoice(factory) { DisplayName = "Description Display" };
				var filters = new CollectionOfIFilter();
				filters.AddRange(new IFilter[] { text, number, multipleChoice });

				var newSetting = new SavedScheduleConfigurationManagerForTest(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
				newSetting.DeserialiseForFilters(xml, filters);

				AssertEquals("Hello", text.Value);
				AssertEquals(42m, number.Value);
				AssertEquals("HDR", multipleChoice.Value);
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveCore()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateRangeFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				Setting.Load();
				Setting.Save(report.ColumnHeadingManager.LinkedFilterFields, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.OrientationManager.Value, report.Parent.Language);
				AssertNotNull(Setting.GetXml());
			}
		}

		public void TestToString()
		{
			AssertEquals("ToString()", "Last Saved Setting", Setting.ToString());
		}

		SavedScheduleConfigurationManagerForTest setting;

		SavedScheduleConfigurationManagerForTest Setting
		{
			get
			{
				if (setting == null)
				{
					setting = new SavedScheduleConfigurationManagerForTest(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
				}
				return setting;
			}
		}

		sealed class SavedScheduleConfigurationManagerForTest : SavedScheduleConfigurationManager
		{
			public SavedScheduleConfigurationManagerForTest(ColumnConfigurationsManager headingManager)
				: base(headingManager)
			{
			}

			internal new string GetXml() => base.GetXml();

			internal void DeserialiseForFilters(string xml, CollectionOfIFilter filters) => Deserialise(xml, filters, null, null, null, null);
		}
	}
}
