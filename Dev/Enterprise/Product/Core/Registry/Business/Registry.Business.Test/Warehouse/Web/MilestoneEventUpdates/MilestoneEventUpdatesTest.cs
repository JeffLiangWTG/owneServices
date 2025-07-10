using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	abstract class MilestoneEventUpdatesTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Test Cases

		public void TestConstructors()
		{
			var testObject = GetNewObjectTemplate();
			AssertNotNull(testObject);
			AssertEquals(ZString.Empty, testObject.EventType);

			testObject = GetNewObjectTemplate("ENT");
			AssertNotNull(testObject);
			AssertEquals("ENT", testObject.EventType);
		}

		public void TestEventType()
		{
			AssertNotNull(TestObjectTemplate);
			TestObjectTemplate.EventType = "TST";

			AssertEquals("TST", TestObjectTemplate.EventType);
		}

		public void TestEventTypeInfo()
		{
			AssertNotNull(TestObjectTemplate);
			AssertEquals("EventType", TestObjectTemplate.EventTypeInfo.Name);
			AssertEquals(3, TestObjectTemplate.EventTypeInfo.MaxLength);
		}

		public void TestEventTypeList()
		{
			AssertNotNull(TestObjectTemplate);
			AssertContainsExactElementsInAnyOrder(GetExpectedEventTypeList(), TestObjectTemplate.EventTypeList);
		}

		public void TestWorkflowType()
		{
			AssertNotNull(TestObjectTemplate);
			AssertEquals(GetExpectedWorkflowType(), TestObjectTemplate.WorkflowType);
		}

		public void TestEventTypeUnknownCodeValidation()
		{
			AssertNotNull(TestObjectTemplate);
			Assert(!TestObjectTemplate.EventTypeInfo.HasWarnings());

			Assert(!TestObjectTemplate.EventTypeList.ContainsCode("TST"));
			TestObjectTemplate.EventType = "TST";
			Assert("Contains warning for unknown event code", TestObjectTemplate.EventTypeInfo.HasWarning("Unknown event code"));

			TestObjectTemplate.EventType = TestObjectTemplate.EventTypeList[0].Code;
			Assert(!TestObjectTemplate.EventTypeInfo.HasWarnings());
		}

		public void TestEventTypeEmptyValidation()
		{
			AssertNotNull(TestObjectTemplate);

			TestObjectTemplate.EventType = TestObjectTemplate.EventTypeList[0].Code;
			Assert(!TestObjectTemplate.EventTypeInfo.HasErrors());

			TestObjectTemplate.EventType = ZString.Empty;
			Assert(TestObjectTemplate.EventTypeInfo.HasErrors());
		}

		public void TestEventTypeDuplicateValidation()
		{
			AssertNotNull(TestObjectTemplateCollection);
			AssertEquals(1, TestObjectTemplateCollection.Count);
			AssertEquals(TestObjectTemplate, TestObjectTemplateCollection[0]);

			TestObjectTemplate.EventType = TestObjectTemplate.EventTypeList[0].Code;
			Assert(!TestObjectTemplate.EventTypeInfo.HasErrors());

			var newTestObjectTemplate = GetNewObjectTemplate();
			newTestObjectTemplate.EventType = TestObjectTemplate.EventType;
			Assert(!TestObjectTemplate.EventTypeInfo.HasErrors());
			Assert(!newTestObjectTemplate.EventTypeInfo.HasErrors());

			TestObjectTemplateCollection.Add(newTestObjectTemplate);
			newTestObjectTemplate.RunPreSaveValidation();
			Assert(newTestObjectTemplate.EventTypeInfo.HasError("Event code must be unique"));

			newTestObjectTemplate.EventType = TestObjectTemplate.EventTypeList[1].Code;
			AssertNotEquals(TestObjectTemplate.EventType, newTestObjectTemplate.EventType);
			Assert(!newTestObjectTemplate.EventTypeInfo.HasErrors());

			newTestObjectTemplate.EventType = TestObjectTemplate.EventType;
			Assert(newTestObjectTemplate.EventTypeInfo.HasError("Event code must be unique"));
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return TestObjectTemplate;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return TestObjectTemplate;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TestObjectTemplate;
		}

		protected virtual CodeDescriptionPairList GetExpectedEventTypeList()
		{
			return EventTypeListProvider.CreateMilestoneEventTypeList(Constants.Workflow.MilestoneType, string.Empty, false, Factory);
		}

		protected override void SetUp()
		{
			testObjectTemplateCollection = GetNewObjectTemplateCollection();
			testObjectTemplate = GetNewObjectTemplate();
			if (testObjectTemplate.EventType.IsEmpty)
			{
				testObjectTemplate.EventType = "ADD";
			}
			TestObjectTemplateCollection.Add(TestObjectTemplate);
			base.SetUp();
		}

		protected abstract MilestoneEventUpdates GetNewObjectTemplate();
		protected abstract MilestoneEventUpdates GetNewObjectTemplate(ZString eventType);
		protected abstract MilestoneEventUpdatesCollection GetNewObjectTemplateCollection();
		protected abstract ZString GetExpectedWorkflowType();

		protected MilestoneEventUpdatesCollection TestObjectTemplateCollection
		{
			get { return testObjectTemplateCollection; }
		}

		protected MilestoneEventUpdates TestObjectTemplate
		{
			get { return testObjectTemplate; }
		}

		MilestoneEventUpdates testObjectTemplate;
		MilestoneEventUpdatesCollection testObjectTemplateCollection;

		#endregion
	}
}
