using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentLink))]
	public class BMComponentLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region Clone

		public void TestClone()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var component2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = BMSTestHelper.LinkComponents(component1, component2);

			link.FL_Sequence = new ZByte(69);

			var filterData = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });

			var filter = link.FilterRule;
			filter.S9_FilterData = filterData;

			Factory.Save();

			var clone = (BMComponentLink)link.Clone();

			Factory.Save();

			AssertEquals(new ZByte(69), clone.FL_Sequence);
			AssertNotEquals(link.FilterRule, clone.FilterRule);
			AssertEquals(link.FilterRule.S9_FilterData, clone.FilterRule.S9_FilterData);

			var secondClone = (BMComponentLink)link.Clone();

			Factory.Save();

			AssertEquals(string.Empty, secondClone.FilterRule.S9_FilterName);
		}

		#endregion

		#region Delete

		public void TestLoadDelete()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system, "1");
			var component2 = BMSTestHelper.CreateBucket(system, "2");
			var link = Factory.New<BMComponentLink>();
			link.FL_FC_ComponentFrom = component1.PK;
			link.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			AssertNoExceptionThrown(() => newFactory.Load<BMComponentLink>(link.PK).Delete());
		}

		#endregion

		#region Filter

		public void TestFilter_ShouldBeLazilyCreated()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var filter = config.ComponentLink.FilterRule;

			AssertNotNull(filter);

			Factory.Save();

			var loadedLink = Factory.CreateNewFactory().Load<BMComponentLink>(config.ComponentLink.PK);
			var loadedFilter = loadedLink.FilterRule;

			AssertEquals(filter.PK, loadedFilter.PK);
		}

		public void TestFilterName()
		{
			var link = Factory.NewWithValidTestData<BMComponentLink>();
			var filter = link.FilterRule;

			AssertEquals(string.Empty, filter.S9_FilterName);
		}

		#endregion

		#region Properties

		public void TestSetDestinationComponentAsBuffer_ShouldSetIsReleaseGate()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			AssertEquals(false, link.FL_IsReleaseGateRuleApplied);

			link.FL_FC_ComponentTo = buffer.PK;
			AssertEquals(true, link.FL_IsReleaseGateRuleApplied);

			link = BMSTestHelper.LinkComponents(bucket1, buffer);
			AssertEquals(true, link.FL_IsReleaseGateRuleApplied);
		}

		public void TestTransferHint()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var link = Factory.New<BMComponentLink>();

			link.FL_FC_ComponentFrom = bucket1.PK;
			AssertEquals("TransferHint should reflect a component link that doesn't have an endpoint",
				"Please create a Component Link to begin configuring Component Link Filter Rules.",
				link.TransferHint);

			link.FL_FC_ComponentTo = buffer.PK;
			AssertEquals("Workflows in the Component [bucket1] will move to the Component [buffer] when they match the below filters:", link.TransferHint);

			link.FL_FC_ComponentTo = bucket2.PK;
			AssertEquals("Workflows in the Component [bucket1] will move to the Component [bucket2] when they match the below filters:", link.TransferHint);
		}

		public void TestHumanReadableName()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var link = config.ComponentLink;
			link.FL_Name = ZString.Empty;
			AssertEquals("Buffer Management System: WTGDEV - ORG, Component Link: bucket -> buffer, Sequence: 0", link.HumanReadableName);

			link.FL_Name = "Baby Legs";
			AssertEquals("Buffer Management System: WTGDEV - ORG, Component Link: Baby Legs", link.HumanReadableName);
		}

		public void TestDisplayText()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var link = config.ComponentLink;
			link.FL_Name = ZString.Empty;
			AssertEquals("bucket -> buffer, Sequence: 0", link.DisplayText);

			link.FL_Name = "Regular Legs";
			AssertEquals("Regular Legs", link.DisplayText);
		}

		public void TestShouldErrorReport_WhenChangingComponentFrom_OnSavedComponentLink()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var link = config.ComponentLink;
			var newBucket = BMSTestHelper.CreateBucket(config.System, "Another bucket");

			Factory.Save();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			link.FL_FC_ComponentFrom = newBucket.PK;

			AssertEquals("Unexpected change of ComponentFrom on a BMComponentLink", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion
	}
}
