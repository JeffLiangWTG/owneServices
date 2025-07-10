using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagDefinition))]
	sealed class TagDefinitionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSystemTagsReadOnly()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "RUM", "Lets get ready, to Rumble!");
			definition.TGD_IsSystem = true;

			AssertEquals(true, definition.TGD_IsSystemInfo.ReadOnly);
			AssertEquals(true, definition.TGD_IsExclusiveInfo.ReadOnly);
			AssertEquals(true, definition.TGD_CodeInfo.ReadOnly);
			AssertEquals(true, definition.TGD_DescriptionInfo.ReadOnly);
			AssertEquals(true, definition.TGD_ScopeInfo.ReadOnly);
			AssertEquals(true, definition.TGD_UsageScopeInfo.ReadOnly);

			AssertEquals(false, definition.TGD_VisualizationDataInfo.ReadOnly);

			definition.TGD_IsSystem = false;

			AssertEquals(true, definition.TGD_IsSystemInfo.ReadOnly);

			AssertEquals(false, definition.TGD_IsExclusiveInfo.ReadOnly);
			AssertEquals(false, definition.TGD_CodeInfo.ReadOnly);
			AssertEquals(false, definition.TGD_DescriptionInfo.ReadOnly);
			AssertEquals(false, definition.TGD_ScopeInfo.ReadOnly);
			AssertEquals(false, definition.TGD_UsageScopeInfo.ReadOnly);
			AssertEquals(false, definition.TGD_VisualizationDataInfo.ReadOnly);
		}

		public void TestApplicableTo()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "LEE", scope: TagScopeList.Codes.All);
			AssertEquals(true, definition.ApplicableToTasks);
			AssertEquals(true, definition.ApplicableToWorkflows);

			definition.TGD_Scope = TagScopeList.Codes.Task;
			AssertEquals(true, definition.ApplicableToTasks);
			AssertEquals(false, definition.ApplicableToWorkflows);

			definition.TGD_Scope = TagScopeList.Codes.Workflow;
			AssertEquals(false, definition.ApplicableToTasks);
			AssertEquals(true, definition.ApplicableToWorkflows);
		}

		public void TestTagDelete()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "DAN";
			definition.TGD_Description = "Yolo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TEL", "The best magnitude");

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var link = (TagLink)workflow.AddTag(magnitude).Link;

			Factory.Save();
			definition.Delete();
			Assert(link.IsDeleted);
			Assert(magnitude.IsDeleted);
			Assert(definition.IsDeleted);
		}

		public void TestHasTagRules()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "LEE");
			var mag = BMSTestHelper.CreateTagMagnitude(definition, "ROW");

			AssertEquals(false, definition.HasTagRules);

			var rule = BMSTestHelper.CreateTagRule(mag, "The Boopoo Rule", "ADD");

			Factory.Save();

			AssertEquals(true, definition.HasTagRules);
		}

		public void TestTagDefinitionHumanReadableName()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "RUM", "Lets get ready, to Rumble!");
			AssertEquals("Human readable name should be 'Tag Group - RUM'", tagDefinition.HumanReadableName, "Tag Group - RUM");

			var emptyTagDefinition = BMSTestHelper.CreateTagDefinition(Factory, string.Empty, string.Empty);
			AssertEquals("Human readable name should be 'Tag Group - '", emptyTagDefinition.HumanReadableName, "Tag Group - ");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Logs

		public void TestNoStmALogs()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF", "Definition");

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, definition.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			definition.TGD_Description = "New description";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			definition.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<TagDefinition>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<TagDefinition>();
		}

		#endregion
	}
}
