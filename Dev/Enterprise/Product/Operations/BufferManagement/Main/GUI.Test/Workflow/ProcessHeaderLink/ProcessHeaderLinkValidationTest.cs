using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ProcessHeaderLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTemplateDependencyLinks_InvolvingTwoTemplateWorkflowsInDifferentTemplates_ShouldNotMakeLinkReadOnly()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template1, "Workflow 2");
			BMSTestHelper.CreateDependencyLink(template1, workflow1, workflow2);

			var workflow3 = BMSTestHelper.CreateWorkflow(template2, "Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(template2, "Workflow 4");
			BMSTestHelper.CreateDependencyLink(template2, workflow3, workflow4);

			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTemplates))
			{
				module.FilterBusinessObject.AddTextFilterStrip("Workflow Type", "DUM");
				module.PerformSearch_ForTest();
				var gridCollection = module.GridCollection;
				AssertEquals(2, gridCollection.Count);

				var toTemplate = gridCollection.FindByPK(template1.PK);
				AssertEquals("We've found that when business objects are loaded into module grids, they are read-only, which can affect our functionality.", true, toTemplate.ReadOnly);

				var newFactory = toTemplate.Factory;
				var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template2.PK);

				var link = (ProcessHeaderLink)loadedTemplate.ProcessHeaderLinks.AddNew();
				link.FP_FH_HeaderFrom = workflow3.PK;
				link.ToWorkflowExternalTemplatePK = toTemplate.PK;
				link.FP_FH_HeaderTo = workflow1.PK;

				link.RunPreSaveValidation();
				AssertEquals("Validating a link in this way should not make it read only. SAD!", false, link.ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
