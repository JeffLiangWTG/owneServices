using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class ControlCustomisationLinePropertyCacheTest : BMSTestCaseWithFactory
	{
		public void TestLoadField()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "second time == the weird stuff");
			var task1 = CreateTask(workflow, string.Empty, 7, description: "Your mother");
			var task2 = CreateTask(workflow, string.Empty, 7, description: "Dont expect it");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line = controlCustomisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = ProcessTasksSchema.Constants.P9_Description;

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line, new ProcessTask[] { task1, task2 }, Enumerable.Empty<ProcessHeader>());
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent1 = new FactorylessCardContent(workflow, task1, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());
			var taskCardContent2 = new FactorylessCardContent(workflow, task2, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());

			AssertEquals("Your mother", strategy.GetValue(taskCardContent1));
			AssertEquals("Dont expect it", strategy.GetValue(taskCardContent2));
		}

		public void TestFieldsWithNoZPropertyInfo_ShouldGetValue()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "second time == the weird stuff");
			var task = CreateTask(workflow, string.Empty, 10, description: "Your mother");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line = controlCustomisation.CustomisationLines.AddNew();

			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = "RelevantEstimateHoursLabel";

			AssertNull("This property doesn't have a ZPropertyInfo, but binding should still work. It doesn't matter which property we're testing just that it doesn't have a ZPropertyInfo.", task.ZPropertyInfoHash.GetPropertySafe(line.PropertyName));

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line, new[] { task }, Enumerable.Empty<ProcessHeader>());
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent = new FactorylessCardContent(workflow, task, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());
			AssertEquals("0:15", strategy.GetValue(taskCardContent));
		}

		public void TestFieldsWithNoZPropertyInfo_ShouldGetValue_Macro()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "completion statement 1");
			var task = CreateTask(workflow, string.Empty, 10, description: "task 1");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line1 = controlCustomisation.CustomisationLines.AddNew();
			line1.PropertySource = PropertySourceList.Codes.Job;
			line1.PropertyName = "<OH_Language>";

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line1, new ProcessTask[] { task }, new ProcessHeader[] { workflow });
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent = new FactorylessCardContent(workflow, task, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());
			AssertEquals(@"GIVEN OrgHeader.OH_Language=ENG with control-customisation-line source=Job and property=OH_Language
WHEN getting FactorylessCardContent should get 'ENG'",
				Core.SharedConstants.Languages.EnglishAmerican, strategy.GetValue(taskCardContent));
		}

		public void TestFieldsWithNoZPropertyInfo_ShouldGetValue_Macro_DotNotation()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals(job.CompanyData.Company.GC_Code, "EDI");

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "completion statement 1");
			var task = CreateTask(workflow, string.Empty, 10, description: "task 1");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line1 = controlCustomisation.CustomisationLines.AddNew();
			line1.PropertySource = PropertySourceList.Codes.Job;
			line1.PropertyName = "<CompanyData.Company.GC_Code>";

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line1, new ProcessTask[] { task }, new ProcessHeader[] { workflow });
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent = new FactorylessCardContent(workflow, task, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());
			AssertEquals(@"GIVEN OrgHeader.OH_Language=ENG with control-customisation-line source=Job and property=<CompanyData.Company.GC_Code>
WHEN getting FactorylessCardContent should get 'EDI'",
				"EDI", strategy.GetValue(taskCardContent));
		}

		public void TestFieldsWithNoZPropertyInfo_ShouldGetValue_Macro_Collection()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "completion statement 1");
			var task = CreateTask(workflow, string.Empty, 10, description: "task 1");

			var controlCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line1 = controlCustomisation.CustomisationLines.AddNew();
			line1.PropertySource = PropertySourceList.Codes.Job;
			line1.PropertyName = "<Metadata.NoteTypes>";

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;
			var strategy = new ControlCustomisationLinePropertyCache(line1, new ProcessTask[] { task }, new ProcessHeader[] { workflow });
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent = new FactorylessCardContent(workflow, task, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());
			AssertEquals(@"GIVEN control-customisation-line source=Job and property=Metadata.NoteTypes(collection-type)
WHEN getting FactorylessCardContent should return null without crash.",
				null, strategy.GetValue(taskCardContent));
		}

		#region Implementation

		SchematicTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}

		#endregion
	}
}
