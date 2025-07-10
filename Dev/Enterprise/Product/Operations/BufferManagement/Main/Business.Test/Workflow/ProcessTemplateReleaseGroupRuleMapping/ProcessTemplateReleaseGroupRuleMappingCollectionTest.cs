using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGroupRuleMappingCollection))]
	class ProcessTemplateReleaseGroupRuleMappingCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateReleaseGroupRuleMappingCollection>
	{
		protected override ProcessTemplateReleaseGroupRuleMappingCollection GetCollectionToTest()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			return new ProcessTemplateReleaseGroupRuleMappingCollection(rule);
		}
	}
}
