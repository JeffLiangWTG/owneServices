using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ComponentRelationshipLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFL_FC_ComponentTo_TypeIsBufferOrBucket()
		{
			var componentRelationship = Factory.New<ComponentRelationship>();
			var link = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentRelationship);

			foreach (var pair in componentTypeArray)
			{
				link.FL_FC_ComponentTo = BMSTestHelper.CreateComponent(Factory, pair.Code, system: system).PK;

				if (pair.Code == BMComponentTypeList.Codes.Bucket || pair.Code == BMComponentTypeList.Codes.Buffer)
				{
					AssertNoErrors(link.FL_FC_ComponentToInfo);
				}
				else
				{
					AssertHasError(link.FL_FC_ComponentToInfo, "Please select only buckets or buffers. Other component types may not be included in component relationships.");
				}
			}
		}

		public void TestCheckFL_FC_ComponentTo_IsNotSubcomponent()
		{
			var componentRelationship = Factory.New<ComponentRelationship>();
			var parentBuffer = BMSTestHelper.CreateBuffer(system);
			var subcomponent = BMSTestHelper.CreateSubBuffer(parentBuffer);
			var link = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentRelationship, subcomponent);

			AssertHasError(link.FL_FC_ComponentToInfo, "Please do not select subcomponents as they may not be included in component relationships.");
		}

		public void TestCheckFL_FC_ComponentTo_TypeSameAsRelationship()
		{
			var componentRelationship = BMSTestHelper.CreateComponentRelationship(Factory);
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);
			var link1 = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentRelationship, bucket);
			var link2 = BMSTestHelper.CreateComponentRelationshipLink(Factory, componentRelationship, bucket);
			string typeConflictError = "All components in a relationship must have the same type. Please remove unwanted components.";

			AssertNoError("Both components have the same type, and yet...", link1.FL_FC_ComponentToInfo, typeConflictError);
			AssertNoError("Both components have the same type, and yet...", link2.FL_FC_ComponentToInfo, typeConflictError);

			link2.FL_FC_ComponentTo = buffer.PK;

			AssertHasError("There are multiple distinct types, and yet...", link2.FL_FC_ComponentToInfo, typeConflictError);

			link2.FL_FC_ComponentTo = ZGuid.Empty;

			AssertNoError("A null component should not cause this validation error.", link2.FL_FC_ComponentToInfo, typeConflictError);

			link1.FL_FC_ComponentTo = buffer.PK;

			AssertNoError("All non-null components have the same type, and yet...", link2.FL_FC_ComponentToInfo, typeConflictError);
		}

		public void TestCheckFL_IsReleaseGateRuleApplied_IsFalse()
		{
			var link = Factory.New<ComponentRelationshipLink>();

			link.FL_IsReleaseGateRuleApplied = true;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to enable release gate rules on a component relationship link. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertHasError(link.FL_IsReleaseGateRuleAppliedInfo, "Please disable the release gate rule as it does not apply to component relationships.");
		}

		public void TestCheckFL_TransferRulesEnabled_IsFalse()
		{
			var link = Factory.New<ComponentRelationshipLink>();

			link.FL_TransferRulesEnabled = true;

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to enable transfer rules on a component relationship link. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertHasError(link.FL_TransferRulesEnabledInfo, "Please disable transfer rules as they do not apply to component relationships.");
		}

		#region Implementation

		ICodeDescription[] componentTypeArray;
		BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();

			componentTypeArray = new BMComponentTypeList().ToArray();
			system = BMSTestHelper.CreateSystem(Factory);
		}

		#endregion
	}
}
