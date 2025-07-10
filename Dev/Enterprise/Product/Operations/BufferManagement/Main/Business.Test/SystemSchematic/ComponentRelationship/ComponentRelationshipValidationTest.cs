using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ComponentRelationshipValidationTest : BusinessObjectValidationTestCase
	{
		#region Name

		public void TestName_ComponentView_ShouldNotThrowNullReferenceException()
		{
			var component = Factory.NewWithValidTestData<ComponentRelationship>();

			AssertNoExceptionThrown("GIVEN Component is ComponentView (has no system) WHEN validating Name, it should not throw NullReferenceException", () =>
			{
				component.FC_Name = "Component View";
				AssertNoErrors(component.FC_NameInfo);
			});
		}

		public void TestName_ComponentView_ShouldBeUnique()
		{
			var component1 = Factory.NewWithValidTestData<ComponentRelationship>();
			component1.FC_Name = "Component XXX";

			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;
			component2.FC_Name = "Component";

			component1.Validation.ValidateAll();
			AssertNoErrors("WHEN componentView is unique THEN no error", component1.FC_NameInfo);

			component2.FC_Name = "Component XXX";
			component1.Validation.ValidateAll();
			AssertNoErrors("WHEN non componentView has same name THEN no error", component1.FC_NameInfo);

			component2.FC_Type = BMComponentTypeList.Codes.ComponentRelationship;
			component1.Validation.ValidateAll();
			AssertHasError(
				"WHEN other componentView has same name THEN error",
				component1.FC_NameInfo,
				"The Name has been duplicated and must be unique.");
		}

		#endregion

		public void TestSystem_ShouldNotHaveSystem()
		{
			var componentRelationship = Factory.New<ComponentRelationship>();
			componentRelationship.Validation.ValidateAll();
			AssertNoErrors("WHEN type is ComponentView and system is blank THEN should not error", componentRelationship.FC_FS_SystemInfo);

			componentRelationship.FC_FS_System = ZGuid.NewZGuid();

			AssertEquals("Component Relationship should report an error when system is set with a non-empty ZGuid.", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("Attempted to set a system on a component relationship. This makes no sense. SAD!", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertHasError("WHEN type is ComponentView and system is set THEN should error",
				componentRelationship.FC_FS_SystemInfo,
				"Please do not enter a Buffer Management System.");
		}

		public void TestRelatedComponentLinks_ShouldNotBeEmpty()
		{
			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);

			relationship.Validation.ValidateAll();

			AssertHasRowError(relationship, "Component relationships cannot be empty. Please add a component to the relationship");

			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, bucket);

			relationship.Validation.ValidateAll();

			AssertNoRowErrors(relationship);
		}
	}
}
