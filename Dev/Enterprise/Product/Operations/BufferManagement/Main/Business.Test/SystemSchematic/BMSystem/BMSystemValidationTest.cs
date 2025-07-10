using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMSystemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNameValidation()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();

			//Name is mandatory
			system.FS_Name = "";
			AssertHasErrors(system.FS_NameInfo);

			system.FS_Name = "Name1";
			AssertNoErrors(system.FS_NameInfo);

			//Name must be unique
			var anotherSystem = Factory.NewWithValidTestData<BMSystem>();
			anotherSystem.FS_Name = "Name1";
			AssertHasErrors(anotherSystem.FS_NameInfo);

			anotherSystem.FS_Name = "AnotherName";
			AssertNoErrors(anotherSystem.FS_NameInfo);
		}

		public void TestDescriptionValidation()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Description = "";
			AssertHasErrors(system.FS_DescriptionInfo);
			system.FS_Description = "Description1";
			AssertNoErrors(system.FS_DescriptionInfo);
		}

		public void TestAllComponentLinksValidation_AtLeastTwoComponents()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "System";
			system.FS_Description = "Description";

			system.Validation.ValidateAll();
			AssertHasRowError(system, "There must be at least 2 active components in the system.");

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			system.Validation.ValidateAll();
			AssertHasRowError(system, "There must be at least 2 active components in the system.");

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;
			component2.FC_IsActive = false;

			system.Validation.ValidateAll();
			AssertHasRowError(system, "There must be at least 2 active components in the system.");

			component2.FC_IsActive = true;

			system.Validation.ValidateAll();
			AssertNoRowError(system, "There must be at least 2 active components in the system.");
		}

		public void TestAllComponentLinksValidation_OnlyOneStartComponent()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "System";
			system.FS_Description = "Description";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component3 = system.Components.AddNew();
			component3.FC_Name = "Component3";
			component3.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom1To2 = component1.FromMeToOthersLinks.AddNew();
			linkFrom1To2.FL_FC_ComponentFrom = component1.PK;
			linkFrom1To2.FL_FC_ComponentTo = component2.PK;

			var linkFrom2To1 = component2.FromMeToOthersLinks.AddNew();
			linkFrom2To1.FL_FC_ComponentFrom = component2.PK;
			linkFrom2To1.FL_FC_ComponentTo = component1.PK;

			var linkFrom2To3 = component2.FromMeToOthersLinks.AddNew();
			linkFrom2To3.FL_FC_ComponentFrom = component2.PK;
			linkFrom2To3.FL_FC_ComponentTo = component3.PK;

			//No 'start' component => error on all components
			system.Validation.ValidateAll();
			AssertHasRowError(component1, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertHasRowError(component2, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertHasRowError(component3, "There must be only one active 'start' component, without other components within the same system pointing to it.");

			var component0 = system.Components.AddNew();
			component0.FC_Name = "Component0";
			component0.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom0To1 = component1.FromMeToOthersLinks.AddNew();
			linkFrom0To1.FL_FC_ComponentFrom = component0.PK;
			linkFrom0To1.FL_FC_ComponentTo = component1.PK;

			//One 'start' component (component0) => all good
			system.Validation.ValidateAll();
			AssertNoRowError(component0, "There must be at least one active 'end' component, without links to other components.");
			AssertNoRowError(component1, "There must be at least one active 'end' component, without links to other components.");
			AssertNoRowError(component2, "There must be at least one active 'end' component, without links to other components.");
			AssertNoRowError(component3, "There must be at least one active 'end' component, without links to other components.");

			var component0Bis = system.Components.AddNew();
			component0Bis.FC_Name = "Component0Bis";
			component0Bis.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom0BisTo1 = component2.FromMeToOthersLinks.AddNew();
			linkFrom0BisTo1.FL_FC_ComponentFrom = component0Bis.PK;
			linkFrom0BisTo1.FL_FC_ComponentTo = component1.PK;

			//Two 'start' components (component0 and component0Bis) => error on 'start' components
			system.Validation.ValidateAll();
			AssertHasRowError(component0, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertHasRowError(component0Bis, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertNoRowError(component1, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertNoRowError(component2, "There must be only one active 'start' component, without other components within the same system pointing to it.");
			AssertNoRowError(component3, "There must be only one active 'start' component, without other components within the same system pointing to it.");
		}

		public void TestComponentLinksValidation_OnlyOneStartComponent_TwoSystems()
		{
			var system1 = Factory.NewWithValidTestData<BMSystem>();
			system1.FS_Name = "System";
			system1.FS_Description = "Description";

			var component11 = BMSTestHelper.CreateBucket(system1, "Component11");
			var component12 = BMSTestHelper.CreateBucket(system1, "Component12");
			var component13 = BMSTestHelper.CreateBucket(system1, "Component13");

			BMSTestHelper.LinkComponents(component11, component12);
			BMSTestHelper.LinkComponents(component12, component13);

			var system2 = Factory.NewWithValidTestData<BMSystem>();
			system2.FS_Name = "Not System";
			system2.FS_Description = "Not Description";

			var component21 = BMSTestHelper.CreateBucket(system2, "Component21");
			var component22 = BMSTestHelper.CreateBucket(system2, "Component22");
			var component23 = BMSTestHelper.CreateBucket(system2, "Component23");

			BMSTestHelper.LinkComponents(component21, component22);
			BMSTestHelper.LinkComponents(component22, component23);

			BMSTestHelper.LinkComponents(component23, component11);

			system1.Validation.ValidateAll();

			AssertNoRowError(component11, "There must be only one active 'start' component, without other components within the same system pointing to it.");
		}

		public void TestLayoutUsageErrorsAreNotExposedOnSystemLevel()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Description = "Validation Test";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkComponents = component1.FromMeToOthersLinks.AddNew();
			linkComponents.FL_FC_ComponentFrom = component1.PK;
			linkComponents.FL_FC_ComponentTo = component2.PK;

			var customisation1 = system.CustomisedControls.AddNew();
			customisation1.FM_Name = "customisation1";
			customisation1.Width = 150;
			customisation1.Height = 100;

			var line1 = customisation1.CustomisationLines.AddNew();
			line1.Top = 20;
			line1.Height = 20;

			var customisation2 = system.CustomisedControls.AddNew();
			customisation2.FM_Name = "customisation2";
			customisation2.Width = 150;
			customisation2.Height = 100;

			var line2 = customisation2.CustomisationLines.AddNew();
			line2.Top = 20;
			line2.Height = 20;
			line2.ControlType = "TXT";

			system.RunPreSaveValidation();
			AssertEquals("No errors from Layout Usage should be shown on system level, errors from Layout Usage are not relevant here.", 0, system.NotificationsIncludingChildren.Count(n => n.Type == CargoWise.ComponentModel.NotificationType.Error));
		}
	}
}
