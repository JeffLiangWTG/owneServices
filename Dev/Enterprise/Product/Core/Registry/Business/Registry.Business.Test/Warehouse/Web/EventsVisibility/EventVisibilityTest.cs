using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibility))]
	class EventVisibilityTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Test Cases

		public void TestConstructors()
		{
			var testObject = new EventVisibility();
			AssertNotNull(testObject);
			AssertEquals(ZString.Empty, testObject.EventCode);

			testObject = new EventVisibility("ENT");
			AssertNotNull(testObject);
			AssertEquals("ENT", testObject.EventCode);
		}

		public void TestEventCode()
		{
			AssertNotNull(TestObjectTemplate);
			TestObjectTemplate.EventCode = "TST";

			AssertEquals("TST", TestObjectTemplate.EventCode);
		}

		public void TestEventCodeInfo()
		{
			AssertNotNull(TestObjectTemplate);
			AssertEquals("EventCode", TestObjectTemplate.EventCodeInfo.Name);
			AssertEquals(3, TestObjectTemplate.EventCodeInfo.MaxLength);
		}

		public void TestEventDescription()
		{
			AssertNotNull(TestObjectTemplate);
			TestObjectTemplate.EventCode = "TST";

			AssertEquals("", TestObjectTemplate.EventDescription);

			foreach (CodeDescriptionPair eventCode in testObjectTemplate.EventCodeList)
			{
				testObjectTemplate.EventCode = eventCode.Code;
				AssertEquals(eventCode.Description, testObjectTemplate.EventDescription);
			}
		}

		public void TestEventDescriptionInfo()
		{
			AssertNotNull(TestObjectTemplate);
			AssertEquals("EventDescription", TestObjectTemplate.EventDescriptionInfo.Name);
		}

		public void TestEventCodeList()
		{
			AssertNotNull(TestObjectTemplate);
			AssertContainsExactElementsInAnyOrder(GetExpectedEventCodeList(), TestObjectTemplate.EventCodeList);
		}

		public void TestEventTypeUnknownCodeValidation()
		{
			AssertNotNull(TestObjectTemplate);
			Assert(!TestObjectTemplate.EventCodeInfo.HasWarnings());

			Assert(!TestObjectTemplate.EventCodeList.ContainsCode("TST"));
			TestObjectTemplate.EventCode = "TST";
			Assert("Contains warning for unknown event code", TestObjectTemplate.EventCodeInfo.HasWarning("Unknown event code"));

			TestObjectTemplate.EventCode = TestObjectTemplate.EventCodeList[0].Code;
			Assert(!TestObjectTemplate.EventCodeInfo.HasWarnings());
		}

		public void TestEventTypeEmptyValidation()
		{
			AssertNotNull(TestObjectTemplate);

			TestObjectTemplate.EventCode = TestObjectTemplate.EventCodeList[0].Code;
			Assert(!TestObjectTemplate.EventCodeInfo.HasErrors());

			TestObjectTemplate.EventCode = ZString.Empty;
			Assert(TestObjectTemplate.EventCodeInfo.HasErrors());
		}

		public void TestEventTypeDuplicateValidation()
		{
			AssertNotNull(TestObjectTemplateCollection);
			AssertEquals(1, TestObjectTemplateCollection.Count);
			AssertEquals(TestObjectTemplate, TestObjectTemplateCollection[0]);

			TestObjectTemplate.EventCode = TestObjectTemplate.EventCodeList[0].Code;
			Assert(!TestObjectTemplate.EventCodeInfo.HasErrors());

			var newTestObjectTemplate = new EventVisibility();
			newTestObjectTemplate.EventCode = TestObjectTemplate.EventCode;
			Assert(!TestObjectTemplate.EventCodeInfo.HasErrors());
			Assert(!newTestObjectTemplate.EventCodeInfo.HasErrors());

			TestObjectTemplateCollection.Add(newTestObjectTemplate);
			Assert(newTestObjectTemplate.EventCodeInfo.HasError("Event code must be unique"));

			newTestObjectTemplate.EventCode = TestObjectTemplate.EventCodeList[1].Code;
			AssertNotEquals(TestObjectTemplate.EventCode, newTestObjectTemplate.EventCode);
			Assert(!newTestObjectTemplate.EventCodeInfo.HasErrors());

			newTestObjectTemplate.EventCode = TestObjectTemplate.EventCode;
			Assert(newTestObjectTemplate.EventCodeInfo.HasError("Event code must be unique"));
		}

		public void TestEventCodeListPerformance()
		{
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				RegistryFactory.RenewFactory();
				var initialLoadCount = GetTotalDatabaseLoadCount();
				var eventVisibility = new EventVisibility();
				_ = eventVisibility.EventCodeList;

				var newEventVisibility = new EventVisibility();
				_ = newEventVisibility.EventCodeList;

				var finalLoadCount = GetTotalDatabaseLoadCount();

				AssertEquals(1, finalLoadCount - initialLoadCount);

				int GetTotalDatabaseLoadCount()
				{
					var factories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.ToArray();
					factories.Append(RegistryFactory.Instance);

					return factories.Aggregate(0, (count, f) => count + f.DatabaseLoadCount);
				}
			}
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

		protected virtual CodeDescriptionPairList GetExpectedEventCodeList()
		{
			return EventTypeListProvider.CreateMilestoneEventTypeList(Constants.Workflow.MilestoneType, string.Empty, false, Factory);
		}

		protected override void SetUp()
		{
			testObjectTemplateCollection = new EventVisibilityCollection();
			testObjectTemplate = new EventVisibility();
			if (testObjectTemplate.EventCode.IsEmpty)
			{
				testObjectTemplate.EventCode = "ADD";
			}
			TestObjectTemplateCollection.Add(TestObjectTemplate);
			base.SetUp();
		}

		protected EventVisibilityCollection TestObjectTemplateCollection
		{
			get { return testObjectTemplateCollection; }
		}

		protected EventVisibility TestObjectTemplate
		{
			get { return testObjectTemplate; }
		}

		EventVisibility testObjectTemplate;
		EventVisibilityCollection testObjectTemplateCollection;

		#endregion
	}
}
