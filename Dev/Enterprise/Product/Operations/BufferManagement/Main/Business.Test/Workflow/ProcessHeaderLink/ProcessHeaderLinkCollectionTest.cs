using System.ComponentModel;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderLinkCollection))]
	class ProcessHeaderLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessHeaderLinkCollection>
	{
		public void TestTemplateCollectionDoesntContainEverything()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A smattering of fattering");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "The Orange Peel, Spiel");
			workflow1.MakePrerequisiteOf(workflow2);

			var template = Factory.New<ProcessTaskTemplate>();
			AssertEquals(0, template.ProcessHeaderLinks.Count);
		}

		public void TestDeveloperException_AfterDeleteOfParent_DuringBinding()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow1 = BMSTestHelper.CreateWorkflow(template);
			var workflow2 = BMSTestHelper.CreateWorkflow(template);

			BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			Factory.Save();

			AssertNotNull(workflow1.Links_ForBinding);

			workflow1.Delete();
			AssertNoExceptionThrown(() => { var v = ((IBindingList)workflow1.LinksFromMeToOthers_ForBinding).AllowNew; });
			AssertNoExceptionThrown(() => { var v = ((IBindingList)workflow1.LinksFromOthersToMe_ForBinding).AllowNew; });
		}

		public void TestAllowNew_ShouldOnlyAllowNewOnTemplates()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			AssertEquals(false, ((IBindingList)jobHeader.PrerequisiteLinks_ForBinding).AllowNew);
			AssertEquals(false, ((IBindingList)jobHeader.PostrequisiteLinks_ForBinding).AllowNew);

			AssertEquals(false, ((IBindingList)workflow.PrerequisiteLinks_ForBinding).AllowNew);
			AssertEquals(false, ((IBindingList)workflow.PostrequisiteLinks_ForBinding).AllowNew);

			var template = Factory.New<ProcessTaskTemplate>();
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();

			AssertEquals(true, ((IBindingList)templateWorkflow.PrerequisiteLinks_ForBinding).AllowNew);
			AssertEquals(true, ((IBindingList)templateWorkflow.PostrequisiteLinks_ForBinding).AllowNew);
		}

		public void TestLinks_ForTemplate_ShouldNotLoadJobLinks()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			workflow1.GetOrCreateDependencyLink(workflow2);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "templateWorkflow1");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "templateWorkflow2");
			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);

			Factory.Save();

			var loadedTemplateWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(templateWorkflow1.PK);

			AssertEquals(1, loadedTemplateWorkflow1.Links.Count());
			AssertEquals(templateLink.PK, loadedTemplateWorkflow1.Links.First().PK);
		}

		public void TestLinks_ForProcessHeader_ShouldContainLinksToJobHeaders()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);

			workflow1_1.GetOrCreateDependencyLink(jobHeader2);

			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow1_1.PK);
			AssertEquals(1, loadedWorkflow.LinksFromMeToOthers.Count());
			var loadedJobHeader2 = loadedWorkflow.Factory.Load<ProcessHeader>(jobHeader2.PK);
			AssertEquals(loadedJobHeader2, loadedWorkflow.LinksFromMeToOthers.Single().HeaderTo);
		}

		public void TestLinks_ForJobHeader_ShouldContainLinksForJobsAndWorkflows()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy1, Factory);

			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();

			Factory.Save();
			Assert(!dummy1.HasChanges);

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			jobHeader1.GetOrCreateDependencyLink(workflow2_1);

			Assert(dummy1.HasChanges);

			Factory.Save();

			var loadedJobHeader1 = new BusinessObjectFactory().Load<ProcessHeader>(jobHeader1.PK);
			AssertEquals(2, loadedJobHeader1.LinksFromMeToOthers.Count());

			var loadedJobHeader2 = loadedJobHeader1.Factory.Load<ProcessHeader>(jobHeader2.PK);
			Assert(loadedJobHeader1.LinksFromMeToOthers.Any(l => l.HeaderTo == loadedJobHeader2));

			var loadedWorkflow2_1 = loadedJobHeader1.Factory.Load<ProcessHeader>(workflow2_1.PK);
			Assert(loadedJobHeader1.LinksFromMeToOthers.Any(l => l.HeaderTo == loadedWorkflow2_1));
		}

		public void TestCollectionShouldContainLinksFromAllJobHeaderWorkflows()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			var header2 = jobHeader.ProcessHeaders.AddNew();
			var header3 = jobHeader.ProcessHeaders.AddNew();
			var header4 = jobHeader.ProcessHeaders.AddNew();

			header1.Links_ForBinding.AddNew();
			header2.Links_ForBinding.AddNew();
			header3.Links_ForBinding.AddNew();
			header4.Links_ForBinding.AddNew();

			AssertEquals("No, a collection named Links *shouldn't* arbitrarily contain links from other workflows of the same JobHeader.", 1, ProcessHeaderLinkCollection.Create_ForTest(header1).Count);
			AssertEquals(1, ProcessHeaderLinkCollection.Create_ForTest(header2).Count);
			AssertEquals(1, ProcessHeaderLinkCollection.Create_ForTest(header3).Count);
			AssertEquals(1, ProcessHeaderLinkCollection.Create_ForTest(header4).Count);
		}

		public void TestTemplateCollection_ShouldLoadLinksForWorkflowsInThatTemplate()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var header1_1 = (ProcessHeader)template1.ProcessHeaders.AddNew();
			var header1_2 = (ProcessHeader)template1.ProcessHeaders.AddNew();
			var header2_1 = (ProcessHeader)template2.ProcessHeaders.AddNew();
			var header2_2 = (ProcessHeader)template2.ProcessHeaders.AddNew();

			var link1 = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link1.FP_FH_HeaderFrom = header1_1.PK;
			link1.FP_FH_HeaderTo = header1_2.PK;
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2 = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link2.FP_FH_HeaderFrom = header2_1.PK;
			link2.FP_FH_HeaderTo = header2_2.PK;
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTemplate1 = newFactory.Load<ProcessTaskTemplate>(template1.PK);
			var loadedTemplate2 = newFactory.Load<ProcessTaskTemplate>(template2.PK);

			AssertEquals(1, loadedTemplate1.ProcessHeaderLinks.Count);
			AssertEquals(1, loadedTemplate2.ProcessHeaderLinks.Count);

			BMSTestCaseWithFactory.AssertSamePK(header1_1, loadedTemplate1.ProcessHeaderLinks[0].HeaderFrom);
			BMSTestCaseWithFactory.AssertSamePK(header1_2, loadedTemplate1.ProcessHeaderLinks[0].HeaderTo);

			BMSTestCaseWithFactory.AssertSamePK(header2_1, loadedTemplate2.ProcessHeaderLinks[0].HeaderFrom);
			BMSTestCaseWithFactory.AssertSamePK(header2_2, loadedTemplate2.ProcessHeaderLinks[0].HeaderTo);
		}

		public void TestNonDependentCollectionFromSetWhenNewElementCreated()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders[0];
			var links = ProcessHeaderLinkCollection.Create_ForTest(header);
			var link = links.AddNew();
			AssertEquals(header.PK, link.FP_FH_HeaderFrom);
			AssertEquals(ZGuid.Empty, link.FP_FH_HeaderTo);

			var linksWithType = new ProcessHeaderLinkCollection(header, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkTypeList.Codes.ParentChild);
			var linkWithType = linksWithType.AddNew();
			AssertEquals(ProcessHeaderLinkTypeList.Codes.ParentChild, linkWithType.FP_LinkType);
		}

		public void TestCollectionWithLinkType()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			var header2 = jobHeader.ProcessHeaders.AddNew();
			var header3 = jobHeader.ProcessHeaders.AddNew();
			var header4 = jobHeader.ProcessHeaders.AddNew();
			var header5 = jobHeader.ProcessHeaders.AddNew();

			var link1To2 = header1.GetOrCreateDependencyLink(header2);
			var link1To3 = header1.GetOrCreateDependencyLink(header3);
			var link2To4 = header2.GetOrCreateDependencyLink(header4);
			var link3To4 = header3.GetOrCreateDependencyLink(header4);
			var link5ChildOf4 = header5.GetOrCreateLinkToParent(header4);

			var header1Prerequisites = new ProcessHeaderLinkCollection(header1, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkTypeList.Codes.Dependency);
			AssertEquals(0, header1Prerequisites.Count);

			var header1Postrequisites = new ProcessHeaderLinkCollection(header1, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkTypeList.Codes.Dependency);
			AssertEquals(2, header1Postrequisites.Count);

			var header2Postrequisites = new ProcessHeaderLinkCollection(header2, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkTypeList.Codes.Dependency);
			AssertEquals(1, header2Postrequisites.Count);

			var header3Postrequisites = new ProcessHeaderLinkCollection(header3, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkTypeList.Codes.Dependency);
			AssertEquals(1, header3Postrequisites.Count);

			var header4Children = new ProcessHeaderLinkCollection(header4, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkTypeList.Codes.ParentChild);
			AssertEquals(1, header4Children.Count);
		}

		public void TestLoadCollection_ShouldNotUseOrClauseInQueries()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "INQ");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow3");

			BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);
			BMSTestHelper.CreateDependencyLink(template, workflow2, workflow3);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);

			using (Db.Connection.TrackExecutedCommands())
			{
				new ProcessHeaderLinkCollection(loadedTemplate).ToArray();

				var relevantQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderFrom + " in") || x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderTo + " in"));

				CombineAssertions("The following queries use an OR operator for HeaderFrom and HeaderTo queries. These should be executed separately. They may be caused by fetch hints. The OR is very bad for query performance. SAD! You should fix this.", () =>
				{
					foreach (var query in relevantQueries)
					{
						AssertNotContains("or (FP_FH_HeaderTo in", query, ignoreCase: true);
						AssertNotContains("or (FP_FH_HeaderFrom in", query, ignoreCase: true);
						AssertContains("Table valued parameters should be used. SAD!", "SELECT Value FROM @CWO", query, ignoreCase: true);
					}
				});
			}
		}

		protected override ProcessHeaderLinkCollection GetCollectionToTest()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.Count > 0 ? jobHeader.ProcessHeaders[0] : jobHeader.ProcessHeaders.AddNew();
			return new ProcessHeaderLinkCollection(header, ProcessHeaderLinkSchema.FP_FH_HeaderFrom);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
