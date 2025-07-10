using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentResourceLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsCapacityConstrained_ShouldBeValidWhenBufferHasConstraintSubComponent()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = resource.GS_Code;

			resourceLink.FD_IsCapacityConstrained = true;
			AssertHasError(resourceLink.FD_IsCapacityConstrainedInfo, "A resource can only be marked as constrained when there is a Constraint sub-component of the selected Buffer.");

			resourceLink.FD_IsCapacityConstrained = false;
			AssertNoErrors(resourceLink.FD_IsCapacityConstrainedInfo);

			var constraint = buffer.ChildComponents.AddNew();
			constraint.FC_Type = BMComponentTypeList.Codes.Constraint;

			resourceLink.FD_IsCapacityConstrained = true;
			AssertNoErrors(resourceLink.FD_IsCapacityConstrainedInfo);
		}

		public void TestCapacityLimitPercent()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var component = Factory.New<BMSystem>().Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			var link = component.ResourceLinks.AddNew();
			link.FD_GS_NKResource = resource.GS_Code;

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			link.FD_CapacityLimitPercent = 20;
			AssertHasError(link.FD_CapacityLimitPercentInfo, "A Capacity Limit Percent is only valid on a Buffer component.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			link.FD_CapacityLimitPercent = 0;
			AssertNoErrors("Capacity limit percent should not be mandatory - may want to allow no work onto a buffer for a resource", link.FD_CapacityLimitPercentInfo);

			link.FD_CapacityLimitPercent = 101;
			AssertHasError(link.FD_CapacityLimitPercentInfo, "Please enter a 'Capacity Limit Percent' less than or equal to 100.");

			link.FD_CapacityLimitPercent = 100;
			AssertNoErrors(link.FD_CapacityLimitPercentInfo);
		}

		public void TestResource()
		{
			var link = Factory.New<BMSystem>().Components.AddNew().ResourceLinks.AddNew();
			link.Validation.ValidateAll();
			AssertHasError(link.FD_GS_NKResourceInfo, "Please enter a Resource.");
		}

		public void TestDuplicateKeys()
		{
			var component = Factory.New<BMSystem>().Components.AddNew();
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var link1 = component.ResourceLinks.AddNew();
			link1.FD_GS_NKResource = resource.GS_Code;

			var link2 = component.ResourceLinks.AddNew();
			link2.FD_GS_NKResource = resource.GS_Code;

			link2.Validation.ValidateAll();
			AssertHasError(link2.FD_FC_ComponentInfo, "This combination of component and resource has been duplicated.");
			AssertHasError(link2.FD_GS_NKResourceInfo, "This combination of component and resource has been duplicated.");

			link2.FD_GS_NKResource = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			link2.Validation.ValidateAll();

			AssertNoErrors(link2.FD_FC_ComponentInfo);
			AssertNoErrors(link2.FD_GS_NKResourceInfo);
		}
	}
}
