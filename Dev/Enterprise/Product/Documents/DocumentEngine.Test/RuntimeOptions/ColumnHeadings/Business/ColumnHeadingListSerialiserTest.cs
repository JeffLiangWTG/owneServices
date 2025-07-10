using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnHeadingListSerialiserTest : TestCaseWithFactory
	{
		public void TestGetEmailOfLastEditingUser()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "a@b.com";
			staff1.GS_LoginName = "ab";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "c@d.com";
			staff2.GS_LoginName = "cd";

			ReportCommand reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_MenuPath = "Testing/";
			reportCommand.SU_MenuName = "Test Stuff";
			reportCommand.SU_FilterList = "";
			reportCommand.IsTopLevel = true; //so it creates logs on saving

			//ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("FilterSortGroupby.xls", TestFilesSubFolder.ReportTestFiles);
			StmTemplate template = Factory.New<StmTemplate>();
			template.SO_Name = "Filter Sort Group by";
			//template.SO_Template = excelTemplate.GetAsByteArray();

			StmMenuTemplatePivot pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (DocumentPack pack = new DocumentPack(reportCommand))
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				Report report = (Report)pack[0];
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();
				{
					var context = new TemporaryUserContext();
					context.StaffLoginName = staff1.GS_LoginName;
					context.BranchPK = Environment.Env.Instance.CurrentBranch.PK;
					context.DepartmentPK = Environment.Env.Instance.CurrentDepartment.PK;
					using (context.Set())
					{
						scheduleTask.S5_ScheduleDescription = "Staff A";
						Factory.Save();
					}

					AssertEquals("a@b.com", ColumnHeadingListSerialiser.GetEmailOfLastEditingUser(report));
				}

				{
					var context = new TemporaryUserContext();
					context.StaffLoginName = staff2.GS_LoginName;
					context.BranchPK = Environment.Env.Instance.CurrentBranch.PK;
					context.DepartmentPK = Environment.Env.Instance.CurrentDepartment.PK;
					using (context.Set())
					{
						scheduleTask.S5_ScheduleDescription = "Staff B";
						Factory.Save();
					}

					AssertEquals("c@d.com", ColumnHeadingListSerialiser.GetEmailOfLastEditingUser(report));
				}
			}
		}

		public void TestMergeDeserialisedAndTemplateHeadingsWhereDeserialisedSettingsReferencesAWorkSheetThatNoLongerExists()
		{
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_BusinessContext = "Kelvin's Context";
			stmMenuItem.SU_MenuName = "BestGoalieEver";
			Factory.Save();

			DefaultTemplateConfigurationManager templateSetting = new DefaultTemplateConfigurationManager(new ColumnConfigurationsManager(stmMenuItem.PK, false));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("Column1", "Description1", "Heading1", 1, 1, 100, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("Column2", "Description2", "Heading2", 2, 2, 100, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("Column3", "Description3", "Heading3", 3, 3, 100, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("Column4", "Description4", "Heading4", 4, 4, 100, true));
			templateSetting.SetTitle("Sheet1", "Title1");

			ReportColumnSettings deserialisedSettings = new ReportColumnSettings();
			Worksheet deserialisedWorkSheet1 = new Worksheet("Sheet1", "DeserialisedTitle1");
			deserialisedWorkSheet1.ColumnHeadings.Add(new ColumnHeading("Column1", "DeserialisedDescription1", "DeserialisedHeading1", 10, 10, 100, false));
			deserialisedWorkSheet1.ColumnHeadings.Add(new ColumnHeading("Column2", "DeserialisedDescription2", "DeserialisedHeading2", 20, 20, 200, false));
			deserialisedWorkSheet1.ColumnHeadings.Add(new ColumnHeading("Column3", "DeserialisedDescription3", "DeserialisedHeading3", 30, 30, 300, false));
			deserialisedWorkSheet1.ColumnHeadings.Add(new ColumnHeading("Column4", "DeserialisedDescription4", "DeserialisedHeading4", 40, 40, 400, false));
			deserialisedSettings.Worksheets.Add(deserialisedWorkSheet1);

			ReportColumnSettings mergedHeadings = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, null);
			AssertEquals("mergedHeadings.Length", 1, mergedHeadings.Worksheets.Count);
			AssertEquals("mergedHeadings.Length", 4, mergedHeadings.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[0], "Column1", "DeserialisedDescription1", "DeserialisedHeading1", 1, 10, 100, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[1], "Column2", "DeserialisedDescription2", "DeserialisedHeading2", 2, 20, 200, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[2], "Column3", "DeserialisedDescription3", "DeserialisedHeading3", 3, 30, 300, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[3], "Column4", "DeserialisedDescription4", "DeserialisedHeading4", 4, 40, 400, false);
			AssertEquals("The report title after deserialising should be - DeserialisedTitle1", "DeserialisedTitle1", mergedHeadings.Worksheets["Sheet1"].Title);

			Worksheet deserialisedWorkSheet2 = new Worksheet("Sheet2", "DeserialisedTitle2");
			deserialisedWorkSheet2.ColumnHeadings.Add(new ColumnHeading("Column1", "DeserialisedDescription1", "DeserialisedHeading1", 10, 10, 100, false));
			deserialisedWorkSheet2.ColumnHeadings.Add(new ColumnHeading("Column2", "DeserialisedDescription2", "DeserialisedHeading2", 20, 20, 200, false));
			deserialisedWorkSheet2.ColumnHeadings.Add(new ColumnHeading("Column3", "DeserialisedDescription3", "DeserialisedHeading3", 30, 30, 300, false));
			deserialisedWorkSheet2.ColumnHeadings.Add(new ColumnHeading("Column4", "DeserialisedDescription4", "DeserialisedHeading4", 40, 40, 400, false));
			deserialisedSettings.Worksheets.Add(deserialisedWorkSheet2);

			ReportColumnSettings mergedHeadingsWithDeserialisedSettingsThatHasWorksheetThatNoLongerExists = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, null);
			AssertEquals("mergedHeadings.Length", 1, mergedHeadings.Worksheets.Count);
			AssertEquals("mergedHeadings.Length", 4, mergedHeadings.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[0], "Column1", "DeserialisedDescription1", "DeserialisedHeading1", 1, 10, 100, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[1], "Column2", "DeserialisedDescription2", "DeserialisedHeading2", 2, 20, 200, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[2], "Column3", "DeserialisedDescription3", "DeserialisedHeading3", 3, 30, 300, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[3], "Column4", "DeserialisedDescription4", "DeserialisedHeading4", 4, 40, 400, false);
			AssertEquals("The report title after deserialising should be - DeserialisedTitle1", "DeserialisedTitle1", mergedHeadings.Worksheets["Sheet1"].Title);
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text"
				, @"The Configuration you have selected has Column Configurations in it for a Worksheet [Sheet2] that no longer exists.

This can happen when the design of the report has been changed since the time that the Configuration you are trying to use was last saved.

These settings have been ignored, please check your Configuration settings then Save them to remove the redundant settings from your Configuration.

Report details:
- Menu Name: BestGoalieEver
- Business Context: Kelvin's Context
- Worksheet Name: Sheet2"
				, UnitTestUserNotification.Instance.LastMessage.Text);

			var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
			reportCommand.SU_BusinessContext = "Test";
			reportCommand.SU_MenuPath = "Testing/";
			reportCommand.SU_MenuName = "Test Stuff";
			reportCommand.SU_FilterList = "";
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Filter Sort Group by";
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Factory.Save();
			using (var pack = new DocumentPack(reportCommand))
			{
				var report = (Report)pack[0];
				mergedHeadingsWithDeserialisedSettingsThatHasWorksheetThatNoLongerExists = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, report);
				AssertEquals("The warning should be sent for the report."
					, @"The Configuration you have selected has Column Configurations in it for a Worksheet [Sheet2] that no longer exists.

This can happen when the design of the report has been changed since the time that the Configuration you are trying to use was last saved.

These settings have been ignored, please check your Configuration settings then Save them to remove the redundant settings from your Configuration.

Report details:
- Menu Name: BestGoalieEver
- Business Context: Kelvin's Context
- Worksheet Name: Sheet2"
					, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				report.IsAnalyzed = true;
				mergedHeadingsWithDeserialisedSettingsThatHasWorksheetThatNoLongerExists = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, report);
				AssertNull("The warning should not be resent for the same report after being analyzed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestMergDeserialiseWhenNoHeadingsPresent()
		{
			ReportColumnSettings deserialisedSettings = new ReportColumnSettings();
			deserialisedSettings.Worksheets.AddNew("Test", "Sheet title deserialised");

			DefaultTemplateConfigurationManager templateSetting = new DefaultTemplateConfigurationManager(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("0", "0d", "1", 2, 3, 4, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("5", "5d", "6", 7, 8, 9, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("10", "10d", "11", 12, 13, 14, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("15", "15d", "16", 17, 18, 19, true));
			templateSetting.SetTitle("Sheet1", "Sheet title default");

			ReportColumnSettings mergedHeadings = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, null);
		}

		public void TestMergeDeserialisedAndTemplateHeadings()
		{
			ColumnHeading[] deserialisedHeadings =
			{
				new ColumnHeading("0", "0d", "", 0, 0, 0, true),
				new ColumnHeading("5", "5d", "", -1, -1, 0, false),
				new ColumnHeading("10", "10d", "11", 20, 30, 14, false),
				new ColumnHeading("15", "15d", "0", 0, 0, 0, true)
			};

			ReportColumnSettings deserialisedSettings = new ReportColumnSettings();
			deserialisedSettings.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(deserialisedHeadings), "Sheet1", "Title1"));

			DefaultTemplateConfigurationManager templateSetting = new DefaultTemplateConfigurationManager(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("0", "0d", "1", 2, 3, 4, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("5", "5d", "6", 7, 8, 9, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("10", "10d", "11", 12, 13, 14, true));
			templateSetting.AddHeading("Sheet1", new ColumnHeading("15", "15d", "16", 17, 18, 19, true));
			templateSetting.SetTitle("Sheet1", "Title2");

			ReportColumnSettings mergedHeadings = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedSettings, templateSetting, null);
			AssertEquals("mergedHeadings.Length", 1, mergedHeadings.Worksheets.Count);
			AssertEquals("mergedHeadings.Length", 4, mergedHeadings.Worksheets["Sheet1"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[0], "0", "0d", "1", 2, 3, 4, true);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[1], "5", "5d", "6", 7, 8, 9, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[2], "10", "10d", "11", 12, 30, 14, false);
			ColumnHeadingTest.AssertPropertyValues(mergedHeadings.Worksheets["Sheet1"].ColumnHeadings[3], "15", "15d", "16", 17, 18, 19, true);
			AssertEquals("The report title after deserialising should be - Title1", "Title1", mergedHeadings.Worksheets["Sheet1"].Title);
		}

		public void TestSerialiseAndDeserialise()
		{
			ColumnHeading[] headingsSheet1 =
			{
				new ColumnHeading("0", "0d", "1", 2, 3, 4, true),
				new ColumnHeading("5", "5d", "6", 7, 8, 9, false),
				new ColumnHeading("10", "10d", "11", 12, 13, 14, false),
				new ColumnHeading("15", "15d", "16", 17, 18, 19, true)
			};

			ColumnHeading[] headingsSheet2 =
			{
				new ColumnHeading("0", "0d", "1", 2, 3, 4, true),
				new ColumnHeading("5", "5d", "6", 7, 8, 9, true),
				new ColumnHeading("10", "10d", "11", 12, 13, 14, true),
				new ColumnHeading("15", "15d", "16", 17, 18, 19, true)
			};

			ColumnHeading[] headingsSheet3 =
			{
				new ColumnHeading("0", "0d", "1", 2, 3, 4, false),
				new ColumnHeading("5", "5d", "6", 7, 8, 9, true),
				new ColumnHeading("10", "10d", "11", 12, 13, 14, true),
				new ColumnHeading("15", "15d", "16", 17, 18, 19, true)
			};

			ReportColumnSettings reportColumnSettings = new ReportColumnSettings();
			reportColumnSettings.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headingsSheet1), "Sheet1"));
			reportColumnSettings.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headingsSheet2), "Sheet2"));
			reportColumnSettings.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(headingsSheet3), "Sheet3", "Title3"));

			string xml = ColumnHeadingListSerialiser.Serialise(reportColumnSettings);

			ColumnConfigurationsManager manager = new ColumnConfigurationsManager(new ZGuid(), false);
			ColumnSettingXMLVersionUpgraderCurrentVersionParameters parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(manager);

			ReportColumnSettings deserialisedHeadings = ColumnHeadingListSerialiser.Deserialise(xml, parameters);
			AssertEquals("there should be only two sheets", 2, deserialisedHeadings.Worksheets.Count);
			AssertEquals("one should be called sheet1", true, deserialisedHeadings.Worksheets.Contains("Sheet1"));
			AssertEquals("sheet2 should not be there", false, deserialisedHeadings.Worksheets.Contains("Sheet2"));
			AssertEquals("one should be called sheet3", true, deserialisedHeadings.Worksheets.Contains("Sheet3"));
			AssertEquals("Sheet1 should have 2 columns", 2, deserialisedHeadings.Worksheets["Sheet1"].ColumnHeadings.Count);
			AssertEquals("Sheet3 should have 1 coumns", 1, deserialisedHeadings.Worksheets["Sheet3"].ColumnHeadings.Count);
			ColumnHeadingTest.AssertPropertyValues(deserialisedHeadings.Worksheets["Sheet1"].ColumnHeadings[0], "5", "5d", "6", 7, 8, 9, false);
			ColumnHeadingTest.AssertPropertyValues(deserialisedHeadings.Worksheets["Sheet1"].ColumnHeadings[1], "10", "10d", "11", 12, 13, 14, false);
			ColumnHeadingTest.AssertPropertyValues(deserialisedHeadings.Worksheets["Sheet3"].ColumnHeadings[0], "0", "0d", "1", 2, 3, 4, false);
			AssertEquals("Sheet1 should have no title", string.Empty, deserialisedHeadings.Worksheets["Sheet1"].Title);
			AssertEquals("Sheet3 should have the title Title3", "Title3", deserialisedHeadings.Worksheets["Sheet3"].Title);
		}
	}
}
