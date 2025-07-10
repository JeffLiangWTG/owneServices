using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	abstract class ColumnConfigurationManagerAbstractTest<T> : TestCaseWithFactory where T : ColumnConfigurationManager
	{
		public void TestCanSaveAndDelete()
		{
			AssertEquals("CanSaveAndDelete", true, Setting.CanSaveAndDelete);
		}

		public void TestDeleteByName()
		{
			AssertEquals("HeadingManager.IsEmpty", true, HeadingManager.IsEmpty);
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("Column 1") }), "Sheet1"));
			AssertEquals("HeadingManager.IsEmpty", false, HeadingManager.IsEmpty);

			Setting.Save();
			Setting.Delete();

			ColumnConfigurationsManager newHeadingManager = new ColumnConfigurationsManager(HeadingManager.ReportID, HeadingManager.Scheduled);
			AssertEquals("newHeadingManager.IsEmpty", true, newHeadingManager.IsEmpty);
			GetNewSetting(newHeadingManager).Load();
			AssertEquals("newHeadingManager.IsEmpty", true, newHeadingManager.IsEmpty);
		}

		public abstract void TestNotEquals();

		public void TestEquals()
		{
			T a = GetNewSetting(HeadingManager);
			T b = GetNewSetting(HeadingManager);
			AssertEquals("A is same as b", a, b);
		}

		public void TestDeserialise()
		{
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1", "1", "1", 1, 1, 1, false));
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("2", "2", "2", 2, 2, 2, false));

			string oldXml =
				@"<?xml version=""1.0""?>
					<ArrayOfColumnHeading>
						<ColumnHeading>
							<DisplayLabel>1</DisplayLabel>
							<OriginalColumnNumber>1</OriginalColumnNumber>
							<Hidden>False</Hidden>
							<CurrentPosition>1</CurrentPosition>
						</ColumnHeading>
						<ColumnHeading>
							<DisplayLabel>2</DisplayLabel>
							<OriginalColumnNumber>2</OriginalColumnNumber>
							<Hidden>True</Hidden>
							<CurrentPosition>2</CurrentPosition>
						</ColumnHeading>
					</ArrayOfColumnHeading>";

			Setting.Deserialise(oldXml);
			AssertEquals("HeadingManager.Headings.Worksheets[Sheet1].ColumnHeadings.Count", 2, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "1", "1", "1", 1, 1, 1, false);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1], "2", "2", "2", 2, 2, 2, true);

			string newXml =
				@"<?xml version=""1.0""?>
					<ArrayOfColumnHeading>
						<ColumnHeading>
							<DisplayLabel>1</DisplayLabel>
							<OriginalColumnNumber>1</OriginalColumnNumber>
							<Hidden>False</Hidden>
							<CurrentPosition>1</CurrentPosition>
							<HeadingText>3</HeadingText>
							<Width>4</Width>
						</ColumnHeading>
						<ColumnHeading>
							<DisplayLabel>2</DisplayLabel>
							<OriginalColumnNumber>2</OriginalColumnNumber>
							<Hidden>True</Hidden>
							<CurrentPosition>2</CurrentPosition>
							<HeadingText>5</HeadingText>
							<Width>6</Width>
						</ColumnHeading>
					</ArrayOfColumnHeading>";

			Setting.Deserialise(newXml);
			AssertEquals("HeadingManager.Headings.Worksheets.Count", 1, HeadingManager.CurrentConfiguration.Worksheets.Count);
			AssertEquals("HeadingManager.Headings.Worksheets[\"Sheet1\"].ColumnHeadings.Count", 2, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "1", "1", "3", 1, 1, 4, false);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1], "2", "2", "2", 2, 2, 2, true);
		}

		public void TestRefreshColumnSettingsIfLanguageChangedBack()
		{
			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("Test1", "Test1", "Test1", 1, 1, 1, false));

			string xml =
			@"<?xml version=""1.0"" encoding=""utf-16""?>
<ReportColumnSettings xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Worksheets>
    <Worksheet>
      <Name>Sheet1</Name>
      <Title>Sheet1</Title>
      <ColumnHeadings>
        <ColumnHeading>
          <DisplayLabel>Test1</DisplayLabel>
          <HeadingText>测试T</HeadingText>
          <Hidden>false</Hidden>
          <HideIfDescriptionEmpty>false</HideIfDescriptionEmpty>
        </ColumnHeading>
      </ColumnHeadings>
    </Worksheet>
  </Worksheets>
  <Version>0</Version>
  <SelectedLanguage>ZH-CN</SelectedLanguage>
</ReportColumnSettings>";

			Setting.Deserialise(xml);

			AssertEquals(1, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "测试T", 1, 0, 1, false);

			HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].HeadingText = "TestEng";
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "TestEng", 1, 0, 1, false);

			Setting.RefreshColumnSettingsIfLanguageChangedBack(string.Empty, true);
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "测试T", 1, 0, 1, false);

			HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].HeadingText = "TestEng";
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "TestEng", 1, 0, 1, false);

			Setting.RefreshColumnSettingsIfLanguageChangedBack("US-EN");
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "TestEng", 1, 0, 1, false);

			Setting.RefreshColumnSettingsIfLanguageChangedBack("ZH-CN");
			ColumnHeadingTest.AssertPropertyValues(HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "Test1", "Test1", "测试T", 1, 0, 1, false);
		}

		public void TestSaveInScheduledMode()
		{
			AssertEquals("HeadingManager.IsEmpty", true, HeadingManager.IsEmpty);
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));

			HeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1"));

			string originalHeadingText = "Original Heading Text";
			HeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].HeadingText = originalHeadingText;
			AssertEquals("HeadingManager.IsEmpty", false, HeadingManager.IsEmpty);

			CollectionOfIFilter filters = new CollectionOfIFilter();
			TextField testFilter = new TextField(Factory);
			testFilter.DisplayName = "TestFilter";
			string testFilterOriginalValue = "Test Value 1";
			testFilter.Value = testFilterOriginalValue;
			filters.Add(testFilter);
			Setting.Save(filters, "", "", "", "");

			ColumnConfigurationManager newManager = GetNewSetting(HeadingManager);

			CollectionOfIFilter newFilters = new CollectionOfIFilter();
			TextField testFilter2 = new TextField(Factory);
			testFilter2.DisplayName = "TestFilter";
			newFilters.Add(testFilter2);
			newManager.Load(newFilters, null, null, null, null);
			AssertEquals("Newly loaded filter value should be same as value that was saved", testFilterOriginalValue, ((TextField)newFilters[0]).Value);
			AssertEquals("Newly loaded column Heading.Text should be same as saved", originalHeadingText, HeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].HeadingText);

			string newHeadingText = "New Heading Text";
			Setting.Load(filters, null, null, null, null);
			headingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].HeadingText = newHeadingText;
			filters.Scheduled = true;
			var testFilter3 = filters[0] as TextField;
			string testFilterNewValue = "Test Value 3";
			testFilter3.Value = testFilterNewValue;
			Setting.Save(filters, "", "", "", "");

			newFilters.ClearValues();
			newManager.Load(newFilters, null, null, null, null);
			AssertEquals("Newly loaded filter value should be the same the as the new value because Filters in schedule mode will save", testFilterNewValue, ((TextField)newFilters[0]).Value);
			AssertEquals("Newly loaded column Heading.Text should new value becase columns when in schedule mode shoudl save ", newHeadingText, HeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings[0].HeadingText);
		}

		[ExpectNoExceptions]
		public void TestSavingNewSettingInScheduleModeDoesNotTryToDeserialise()
		{
			CollectionOfIFilter filters = new CollectionOfIFilter();
			TextField testFilter = new TextField(Factory);
			testFilter.DisplayName = "TestFilter";
			testFilter.Value = "Test Value 1";
			filters.Add(testFilter);
			filters.Scheduled = true;
			Setting.Save(filters, null, null, null, null);
		}

		public void TestSaveAndLoad()
		{
			AssertEquals("HeadingManager.IsEmpty", true, HeadingManager.IsEmpty);
			HeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(new ColumnHeading[] { new ColumnHeading("1") }), "Sheet1"));
			AssertEquals("HeadingManager.IsEmpty", false, HeadingManager.IsEmpty);
			Setting.Save();

			ColumnConfigurationsManager newHeadingManager = new ColumnConfigurationsManager(HeadingManager.ReportID, HeadingManager.Scheduled);
			newHeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1"));
			AssertEquals("newHeadingManager.IsEmpty", true, newHeadingManager.IsEmpty);
			GetNewSetting(newHeadingManager).Load();
			AssertEquals("newHeadingManager.IsEmpty", false, newHeadingManager.IsEmpty);
			AssertEquals("newHeadingManager.Headings.Worksheets.Count", 1, newHeadingManager.CurrentConfiguration.Worksheets.Count);
			AssertEquals("newHeadingManager.Headings.Worksheets[\"Sheet1\"].ColumnHeadings.Count", 1, newHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
			AssertEquals("newHeadingManager.Headings.Worksheets[\"Sheet1\"].ColumnHeadings[0].DisplayLabel", "1", newHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].DisplayLabel);
		}

		public void TestToString()
		{
			AssertEquals("ToString()", ExpectedToString, Setting.ToString());
		}

		public void TestFilterValuesAndSelectedGroupByAndAlsoSelectedSortOrderSaveAndLoadCorrectly()
		{
			CollectionOfIFilter filters = new CollectionOfIFilter();
			TextField field = new TextField(Factory);
			field.Value = "ASKJHDSAK";
			filters.Add(field);

			GroupByCollection groupbys = new GroupByCollection();
			groupbys.Add("some description", "val1");
			groupbys.Add("other description", "val2");
			groupbys.Add("third description", "val3");

			SortOrderCollection sortorders = new SortOrderCollection();
			sortorders.Add("some description", "val1");
			sortorders.Add("other description", "val2");
			sortorders.Add("third description", "val3");

			var docPack = new DocumentPack();
			docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;

			var orientationManager = new Report.OrientationManagement() { Value = ReportOrientationTypeList.Codes.Portrait };
			Setting.Save(filters, groupbys[1].DisplayName, sortorders[1].DisplayName, orientationManager.Value, docPack.Language);

			CollectionOfIFilter newFilters = new CollectionOfIFilter();
			TextField newField = new TextField(Factory);
			newFilters.Add(newField);

			ColumnConfigurationsManager newHeadingManager = new ColumnConfigurationsManager(HeadingManager.ReportID, HeadingManager.Scheduled);
			var newDocPack = new DocumentPack();

			T newSetting = GetNewSetting(newHeadingManager);
			newSetting.Load(newFilters, groupbys, sortorders, orientationManager, newDocPack);

			AssertEquals(field.Value, ((TextField)newFilters[0]).Value);
			AssertEquals("Selected Language", docPack.Language, newDocPack.Language);
			AssertEquals("Selected Group By", groupbys[1].DisplayName, groupbys.SelectedGroupBy.DisplayName);
			AssertEquals("Selected Sort Order", sortorders[1].DisplayName, sortorders.SelectedOrder.DisplayName);
			AssertEquals("Selected orientation", ReportOrientationTypeList.Codes.Portrait, orientationManager.Value);
		}

		#region Implementation

		protected abstract string ExpectedToString { get; }

		protected T Setting
		{
			get
			{
				if (setting == null)
				{
					setting = GetNewSetting(HeadingManager);
				}
				return setting;
			}
		}

		protected ColumnConfigurationsManager HeadingManager
		{
			get
			{
				if (headingManager == null)
				{
					headingManager = new ColumnConfigurationsManager(new ColumnSettingWithUpgraderTestHelper().GetReportIDThruFullTemplatePivotMenuBuild("NewStyleTemplate.xls"), false);
				}
				return headingManager;
			}
		}

		protected abstract T GetNewSetting(ColumnConfigurationsManager headingManager);

		T setting;
		ColumnConfigurationsManager headingManager;

		#endregion
	}
}
