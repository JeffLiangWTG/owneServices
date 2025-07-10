using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region FilterApplicatorTestCase

	class SearchFilterApplicatorTest : FilterApplicatorTestCase<SearchFilterApplicator>
	{
		[TestDate(2017, 5, 29)]
		public override void TestApply_DbHits()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", config.Buffer);
			var workflow12 = CreateWorkflow(jobHeader1, "workflow12", config.Buffer);
			var workflow21 = CreateWorkflow(jobHeader1, "workflow21", config.Buffer);
			var workflow22 = CreateWorkflow(jobHeader1, "workflow22", config.Buffer);
			var workflow24444 = CreateWorkflow(jobHeader1, "workflow24444", config.Buffer);
			var workflow31 = CreateWorkflow(jobHeader1, "workflow31", config.Buffer);
			var workflow32 = CreateWorkflow(jobHeader1, "workflow32", config.Buffer);
			var workflow34444 = CreateWorkflow(jobHeader1, "workflow34444", config.Buffer);

			CreateTask(workflow1, "", 15);
			CreateTask(workflow12, "", 15);
			CreateTask(workflow21, "", 15);
			CreateTask(workflow22, "", 15);
			CreateTask(workflow24444, "", 15);

			CreateTask(workflow31, "", 15);
			CreateTask(workflow32, "", 15);
			CreateTask(workflow34444, "", 15);

			var cells = new[]
			{
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
			};

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer).Item1;

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader1.PK);
			newFactory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, loadedJobHeader.ProcessHeaders.ToArray());

			newFactory.ResetDatabaseLoadCount();

			var applicator = new SearchFilterApplicator(new SearchFilter("workflow1"));
			foreach (var group in cells.Zip(loadedJobHeader.ProcessHeaders, (c, w) => new { CardContent = new TaskCardContent(w.GetTasksWithoutAccessingWorkflowParent().Single(), viewModel), Cell = c }))
			{
				applicator.IsApplicable(group.CardContent, group.Cell, viewModel);
			}

			AssertDbHits(new Dictionary<string, int> { }, newFactory); // no hits because we've cached everything...
		}

		public override void TestIsApplicable()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_CardNote = "You win this round, Mr Trampoline.";

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("Mr Trampoline ");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_PropertyOnRow()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("FRO");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_PropertyOnJobRow()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("MAIORG");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_SearchableProperty()
		{
			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("Frodo");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_NoMatchingPropertyValue()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_CardNote = "You win this round, Mr Trampoline.";

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("Ms Trampoline");
			AssertEquals(false, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_Wildcard()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_CardNote = "You win this round, Mr Trampoline.";

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("You win this round, * Trampoline");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_EscapedContent()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_CardNote = "You win this round, /Mr/ Trampoline.";

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("You win this round, /Mr/ Trampoline");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_WorkflowProperty()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "You win this round, Mr Trampoline.");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("Mr Trampoline");
			AssertEquals(true, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_WorkflowWithoutActualParentJob()
		{
			var jobHeader = Factory.New<ProcessJobHeader>();
			jobHeader.FH_CompletionStatement = "job";
			jobHeader.FH_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			jobHeader.FH_ParentId = ZGuid.BrettsGuid;

			var workflow = Factory.New<ProcessHeader>();
			workflow.FH_CompletionStatement = "You win this round, Mr Trampoline";
			workflow.FH_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			workflow.FH_FH_ParentHeader = jobHeader.PK;
			workflow.FH_ParentId = ZGuid.BrettsGuid;

			var task = Factory.New<ProcessTask>();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_ParentID = ZGuid.BrettsGuid;
			task.P9_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var filter = new SearchFilter("zzz");
			AssertEquals("No cards should be applicable and no exceptions should be thrown", false, filter.IsApplicable(task, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_CustomisedPropertyVisible()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.CompanyData.Company.GC_Code = "UWU";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "completion statement 1");
			var task = BMSTestHelper.CreateTaskWithCompany(workflow);

			var newFactory = Factory.CreateNewFactory();
			var job2 = newFactory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "Second";
			job2.CompanyData.Company.GC_Code = "REE";
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, newFactory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "completion statement 2");
			var task2 = BMSTestHelper.CreateTaskWithCompany(workflow2);

			var taskSummaryCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line = taskSummaryCard.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<CompanyData.Company.GC_Code>";

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard, "WKI");
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow, workflow2);
			var customDataCache = new CustomisedControlDataCache(taskSummaryCard, viewModel.Cache, workflow.Tasks, jobHeader.ChildHeaders, viewModel.ShowJobCards, viewModel.ShowWorkflowCards);
			var cardContent = new FactorylessCardContent(workflow, task, viewModel, customDataCache, new TagDefinitionCache(Factory), new PopulateTaskCardStrategy());

			var secondCustomDataCache = new CustomisedControlDataCache(taskSummaryCard, viewModel.Cache, workflow2.Tasks, jobHeader2.ChildHeaders, viewModel.ShowJobCards, viewModel.ShowWorkflowCards);
			var secondCardContent = new FactorylessCardContent(workflow2, task2, viewModel, secondCustomDataCache, new TagDefinitionCache(newFactory), new PopulateTaskCardStrategy());

			Factory.Save();
			newFactory.Save();

			var filter = new SearchFilter("UWU");
			AssertEquals(true, filter.IsApplicable(cardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));
			AssertEquals(false, filter.IsApplicable(secondCardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));

			filter = new SearchFilter("REE");
			AssertEquals(true, filter.IsApplicable(secondCardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));
			AssertEquals(false, filter.IsApplicable(cardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));
		}

		public void TestIsApplicable_CustomisedPropertyOnlyHitWhenExpected()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.CompanyData.Company.GC_Code = "HAM";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "completion statement 1");
			var task = BMSTestHelper.CreateTaskWithCompany(workflow);
			task.P9_CardNote = "Steamed";

			var taskSummaryCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var line = taskSummaryCard.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<CompanyData.Company.GC_Code>";

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard, "WKI");
			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var customDataCache = new CustomisedControlDataCache(taskSummaryCard, viewModel.Cache, workflow.Tasks, jobHeader.ChildHeaders, viewModel.ShowJobCards, viewModel.ShowWorkflowCards);
			var cardContent = new FactorylessCardContent(workflow, task, viewModel, customDataCache, new TagDefinitionCache(Factory), new PopulateTaskCardStrategy());

			Factory.Save();
			var filter = new SearchFilter("steam");
			var applicator = new SearchFilterApplicatorForTest(filter);

			AssertEquals(true, applicator.IsApplicable(cardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));
			AssertEquals(0, applicator.MatchOnVisiblePropertiesSearchCount);

			filter = new SearchFilter("HAM");
			applicator = new SearchFilterApplicatorForTest(filter);
			AssertEquals(true, applicator.IsApplicable(cardContent, new CellContent(0, 0, CellContentType.Cards), viewModel));
			AssertEquals(1, applicator.MatchOnVisiblePropertiesSearchCount);
		}

		class SearchFilterApplicatorForTest : SearchFilterApplicator
		{
			public SearchFilterApplicatorForTest(SearchFilter filter)
				: base(filter)
			{
			}

			protected override bool IsMatchOnVisibleProperties(ICardContent cardContent)
			{
				MatchOnVisiblePropertiesSearchCount++;

				return base.IsMatchOnVisibleProperties(cardContent);
			}

			public int MatchOnVisiblePropertiesSearchCount { get; private set; }
		}

		SchematicTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}
	}

	#endregion

	#region BoardFilterTestCase

	[TestedType(typeof(SearchFilterApplicator))]
	class SearchFilterTest : BoardFilterTestCase<SearchFilterApplicator>
	{
		public override void TestFilterName()
		{
			AssertEquals("Search for 'Poodle'", new SearchFilter("Poodle").FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, new SearchFilter("Poodle").AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals(new SearchFilter("blah"), new SearchFilter("boo"));
		}

		protected override SearchFilterApplicator GetFilter()
		{
			return new SearchFilterApplicator(new SearchFilter("Blah"));
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertEquals(string.Empty, searchControl.SearchTerm);

				searchControl.SearchTerm = "Something";
				searchControl.OnSearchPerformed(false);

				AssertEquals("Something", searchControl.SearchTerm);

				form.SlideShowViewModel.FilterManager.Clear();
				AssertEquals(string.Empty, searchControl.SearchTerm);
			}
		}

		public void TestTrim()
		{
			AssertEquals("Did not trim trailing whitespace", "Search for 'Poodle'", new SearchFilter("Poodle ").FilterName);
			AssertEquals("Should not trim whitespace with characters following", "Search for 'Poodle          s'", new SearchFilter("Poodle          s").FilterName);
			AssertEquals("Should not trim begining whitespace", "Search for ' Poodle'", new SearchFilter(" Poodle  ").FilterName);
		}
	}

	#endregion
}
