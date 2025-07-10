using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(RuleRunnerTagLink))]
	class RuleRunnerTagLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUsageScope()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "DAN";
			definition.TGD_Description = "Yolo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TEL", "The best magnitude");

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var link = Factory.New<RuleRunnerTagLink>();
			link.TGL_TGM_Magnitude = magnitude.PK;
			link.TGL_ParentId = workflow.PK;
			link.TGL_ParentTableCode = workflow.TablePrefix;

			AssertEquals(true, link.InOperationalScope);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			AssertEquals(true, link.InOperationalScope);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.User;
			AssertEquals(false, link.InOperationalScope);

			ErrorReporter.Clear();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectForTest();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBusinessObjectForTest();
		}

		RuleRunnerTagLink GetBusinessObjectForTest()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			var tagLink = Factory.New<RuleRunnerTagLink>();
			tagLink.TGL_TGM_Magnitude = magnitude.PK;
			tagLink.TGL_ParentId = task.PK;
			tagLink.TGL_ParentTableCode = task.TablePrefix;

			return tagLink;
		}

		#endregion
	}
}
