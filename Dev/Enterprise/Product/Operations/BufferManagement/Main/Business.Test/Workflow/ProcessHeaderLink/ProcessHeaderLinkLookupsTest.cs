using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTemplates()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT");
			var template3 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "GLW");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();

			AssertCollectionNotContains("It's important that the template from which the link originates does not have that template in the collection, otherwise pressing the elipses to open the module find screen will make the whole form readonly",
				template1, link.Lookups.Templates);

			AssertCollectionContains(template2, link.Lookups.Templates);
			AssertCollectionContains(template3, link.Lookups.Templates);
		}

		public void TestDeleteTemplate()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var workflow1 = BMSTestHelper.CreateWorkflow(template);
			var workflow2 = BMSTestHelper.CreateWorkflow(template);

			var link = BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			workflow1.Delete();

			AssertEquals(true, link.IsDeleted);

			AssertNoExceptionThrown(() => link.Lookups.HeaderTos.Any());
			AssertNoExceptionThrown(() => link.Lookups.HeaderFroms.Any());
		}

		public void TestTemplateLinks_HeaderFrom()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "STAHP");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Pls");

			var headerElsewhere = Factory.NewWithValidTestData<ProcessHeader>();
			headerElsewhere.FH_FH_ParentHeader = ZGuid.NewZGuid();

			var link = BMSTestHelper.CreateDependencyLink(template, headerTo: workflow1);

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, templateJobHeader }, link.Lookups.HeaderTos);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, templateJobHeader }, link.Lookups.HeaderFroms);

			AssertEquals(0, link.Lookups.HeaderTos.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Count());

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "srsly pls");
			var linkOnJob = workflow.LinksFromOthersToMe_ForBinding.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { headerElsewhere, jobHeader, workflow }, linkOnJob.Lookups.HeaderTos);
			AssertContainsExactElementsInAnyOrder(new[] { headerElsewhere, jobHeader, workflow }, linkOnJob.Lookups.HeaderFroms);
		}

		public void TestTemplateLinks_HeaderTo()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "STAHP");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Pls");

			var headerElsewhere = Factory.NewWithValidTestData<ProcessHeader>();
			headerElsewhere.FH_FH_ParentHeader = ZGuid.NewZGuid();

			var link = BMSTestHelper.CreateDependencyLink(template, headerFrom: workflow1);

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, templateJobHeader }, link.Lookups.HeaderTos);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, templateJobHeader }, link.Lookups.HeaderFroms);

			AssertEquals(0, link.Lookups.HeaderTos.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Count());

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "srsly pls");
			var linkOnJob = workflow.LinksFromMeToOthers_ForBinding.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { headerElsewhere, jobHeader, workflow }, linkOnJob.Lookups.HeaderTos);
			AssertContainsExactElementsInAnyOrder(new[] { headerElsewhere, jobHeader, workflow }, linkOnJob.Lookups.HeaderFroms);
		}

		public void TestHeadersTo_ShouldHaveModuleFilterDefaultsApplied()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			var link = header.LinksFromMeToOthers_ForBinding.AddNew();

			AssertModuleDefaultsApplied(link.Lookups.HeaderTos);
		}

		public void TestHeadersFrom_ShouldHaveModuleFilterDefaultsApplied()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			var link = header.LinksFromMeToOthers_ForBinding.AddNew();

			AssertModuleDefaultsApplied(link.Lookups.HeaderFroms);
		}

		#region Implementation

		static void AssertModuleDefaultsApplied(ProcessHeaderCollection collection)
		{
			var defaults = collection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().ToArray();
			AssertEquals(3, defaults.Length);

			AssertEquals(ProcessHeader.ModuleFilterConstants.JobCode, defaults[0].FilterName);
			AssertEquals("Property", defaults[0].PropertyName);
			AssertEquals("MAIORGSYD", defaults[0].Value);

			AssertEquals(ProcessHeader.ModuleFilterConstants.JobCode, defaults[1].FilterName);
			AssertEquals("WorkflowTypeCode", defaults[1].PropertyName);
			AssertEquals("ORG", defaults[1].Value);

			AssertEquals(ProcessHeader.ModuleFilterConstants.JobOrWorkflow, defaults[2].FilterName);
			AssertEquals("Property2", defaults[2].PropertyName);
			AssertEquals(ZBool.True, defaults[2].Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		SchematicTestConfig config;

		#endregion
	}
}
