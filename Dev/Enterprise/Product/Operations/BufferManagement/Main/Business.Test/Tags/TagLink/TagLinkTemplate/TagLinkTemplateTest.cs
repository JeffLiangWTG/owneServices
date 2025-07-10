using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagLinkTemplate))]
	public class TagLinkTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNeverReadonly()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "TIN", "Tin man", usageScope: TagUsageScopeList.Codes.User);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "DOR", "Shipping with Dorothy");
			var template = Factory.New<TagLinkTemplate>();

			template.TGL_TGM_Magnitude = magnitude.PK;

			AssertEquals(false, template.ReadOnly);
		}

		public void TestAddTagGivesCorrectTableCode()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Eat junk food for eternal happiness.");

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "TIN", "Tin man", usageScope: TagUsageScopeList.Codes.User);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "DOR", "Shipping with Dorothy");
			var rule = BMSTestHelper.CreateTagRule(magnitude, "PLIZ HALP", TagRuleActionTypeList.Codes.AddTag);
			var template = rule.TagTemplate;

			template.TGL_TGM_Magnitude = magnitude.PK;
			template.AddTag(workflow, rule);

			AssertEquals(workflow.TablePrefix, workflow.TagLinks.First().TGL_ParentTableCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var magnitude = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(factory, "AAA"), "AAA");

			var link = factory.New<TagLinkTemplate>();
			link.TGL_TGM_Magnitude = magnitude.PK;
			link.TGL_ParentId = ZGuid.NewZGuid();
			link.TGL_ParentTableCode = TagRuleSchema.Constants.Prefix;

			return link;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			return (TagLink)task.AddTag(magnitude).Link;
		}

		#endregion
	}
}
