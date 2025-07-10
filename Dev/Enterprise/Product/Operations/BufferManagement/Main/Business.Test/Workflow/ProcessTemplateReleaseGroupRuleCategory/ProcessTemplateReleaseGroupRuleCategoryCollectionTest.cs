using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGroupRuleCategoryCollection))]
	class ProcessTemplateReleaseGroupRuleCategoryCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateReleaseGroupRuleCategoryCollection>
	{
		protected override ProcessTemplateReleaseGroupRuleCategoryCollection GetCollectionToTest()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			return new ProcessTemplateReleaseGroupRuleCategoryCollection(rule);
		}
	}
}
