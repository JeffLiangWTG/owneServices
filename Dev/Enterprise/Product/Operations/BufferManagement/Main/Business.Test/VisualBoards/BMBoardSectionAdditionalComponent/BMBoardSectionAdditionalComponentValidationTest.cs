using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionAdditionalComponentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestComponent_ShouldCheckRelationshipNotEmpty()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);
			var additionalComponent = BMSTestHelper.CreateAdditionalComponent(section, relationship);

			AssertHasError(additionalComponent.BSA_FC_ComponentInfo, "This relationship is empty and cannot be added as an additional component.");
		}

		public void TestComponent_WithPrimaryBuffer_ShouldCheckRelationshipHasBuffers()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			var bucketAdditionalComponent = AddRelationshipToSection(section, BMSTestHelper.CreateBucket(system));
			var bufferAdditionalComponent = AddRelationshipToSection(section, BMSTestHelper.CreateBuffer(system));

			AssertHasError(bucketAdditionalComponent.BSA_FC_ComponentInfo, "Components in this relationship must be of the same type as the primary board section component.");
			AssertNoErrors(bufferAdditionalComponent.BSA_FC_ComponentInfo);
		}

		public void TestComponent_WithPrimaryBucket_ShouldCheckRelationshipHasBuckets()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			var bucketAdditionalComponent = AddRelationshipToSection(section, BMSTestHelper.CreateBucket(system));
			var bufferAdditionalComponent = AddRelationshipToSection(section, BMSTestHelper.CreateBuffer(system));

			AssertNoErrors(bucketAdditionalComponent.BSA_FC_ComponentInfo);
			AssertHasError(bufferAdditionalComponent.BSA_FC_ComponentInfo, "Components in this relationship must be of the same type as the primary board section component.");
		}

		public void TestComponent_ShouldRequireSameTypeAsParentComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			var additionalComponent = section.SectionConfiguration.AdditionalComponents.AddNew();
			additionalComponent.BSA_FC_Component = bucket2.PK;
			AssertHasError(additionalComponent.BSA_FC_ComponentInfo, "A primary board section component must be selected before specifying an additional component.");

			section.MS_FC_Component = bucket1.PK;
			additionalComponent.Validation.ValidateAll();

			additionalComponent.BSA_FC_Component = bucket1.PK;
			AssertHasError(additionalComponent.BSA_FC_ComponentInfo, "This component has already been chosen as the primary board section component.");

			additionalComponent.BSA_FC_Component = buffer2.PK;
			AssertHasError(additionalComponent.BSA_FC_ComponentInfo, "Additional components must be the same type of component as the primary board section component.");

			section.MS_FC_Component = buffer1.PK;
			additionalComponent.Validation.ValidateAll();
			AssertNoErrors(additionalComponent.BSA_FC_ComponentInfo);
		}

		public void TestComponent_ShouldNotBeDuplicated()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system);
			var bucket3 = BMSTestHelper.CreateBucket(system);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket1.PK;

			var additionalComponent1 = section.SectionConfiguration.AdditionalComponents.AddNew();
			var additionalComponent2 = section.SectionConfiguration.AdditionalComponents.AddNew();

			additionalComponent1.BSA_FC_Component = bucket2.PK;
			additionalComponent2.BSA_FC_Component = bucket2.PK;

			AssertHasError(additionalComponent2.BSA_FC_ComponentInfo, "The Component has been duplicated and must be unique.");

			additionalComponent2.BSA_FC_Component = bucket3.PK;
			AssertNoErrors(additionalComponent2.BSA_FC_ComponentInfo);
		}

		public void TestComponent_AdditionalComponentsNotAllowedOnReleaseSchedulerSections()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system);

			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(buffer1, Factory.NewWithValidTestData<GlbGroup>());
			var additionalComponent = section.SectionConfiguration.AdditionalComponents.AddNew();
			additionalComponent.BSA_FC_Component = buffer2.PK;

			AssertHasError(additionalComponent.BSA_FC_ComponentInfo, "Release Scheduler sections may not have additional components specified.");
		}

		#region Implementation

		BMBoardSectionAdditionalComponent AddRelationshipToSection(BMBoardSection section, BMComponent component)
		{
			var relationship = BMSTestHelper.CreateComponentRelationship(Factory);
			BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, component);

			return BMSTestHelper.CreateAdditionalComponent(section, relationship);
		}

		#endregion
	}
}
