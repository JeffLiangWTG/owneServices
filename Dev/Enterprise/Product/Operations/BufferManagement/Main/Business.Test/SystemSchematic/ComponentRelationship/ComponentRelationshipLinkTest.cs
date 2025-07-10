using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ComponentRelationshipLink))]
	public class ComponentRelationshipLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var link = Factory.New<ComponentRelationshipLink>();

			Assert(!link.FL_IsReleaseGateRuleApplied);
			Assert(!link.FL_TransferRulesEnabled);
		}

		public void TestSetFL_FC_ComponentFrom_ReportsErrorWhenNotRelationship()
		{
			var link = Factory.New<ComponentRelationshipLink>();
			var system = Factory.New<BMSystem>();
			var componentTypeArray = new BMComponentTypeList().ToArray();

			foreach (var pair in componentTypeArray)
			{
				var componentFrom = BMSTestHelper.CreateComponent(Factory, pair.Code, system: system);
				link.FL_FC_ComponentFrom = componentFrom.PK;

				if (pair.Code != BMComponentTypeList.Codes.ComponentRelationship)
				{
					AssertEquals(1, ErrorReporter.TotalErrorCount);
					AssertEquals("Attempted to set ComponentFrom to a type other than Component Relationship. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestSetIsReleaseGateRuleApplied_ReportsErrorWhenTrue()
		{
			var link = Factory.New<ComponentRelationshipLink>();

			link.FL_IsReleaseGateRuleApplied = true;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to enable release gate rules on a component relationship link. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSetTransferRulesEnabled_ReportsErrorWhenTrue()
		{
			var link = Factory.New<ComponentRelationshipLink>();

			link.FL_TransferRulesEnabled = true;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to enable transfer rules on a component relationship link. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSetComponentTo_DoesNotSetReleaseGateEnabled()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentTo: buffer);

			Assert("Release gate rule should be disabled regardless of the destination component", !link.FL_IsReleaseGateRuleApplied);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return BMSTestHelper.CreateComponentRelationshipLink(Factory, Factory.NewWithValidTestData<ComponentRelationship>(), Factory.NewWithValidTestData<BMComponent>());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return BMSTestHelper.CreateComponentRelationshipLink(Factory, Factory.NewWithValidTestData<ComponentRelationship>(), Factory.NewWithValidTestData<BMComponent>());
		}

		#endregion
	}
}
