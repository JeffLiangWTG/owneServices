using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EDIOrgStaffAssignmentsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckO8_Product()
		{
			var staffAssignments = Factory.NewWithValidTestData<EDIOrgStaffAssignments>();
			AssertNoWarnings(staffAssignments.O8_ProductInfo);

			staffAssignments.O8_Product = "AAA";
			AssertHasErrors(staffAssignments.O8_ProductInfo);

			staffAssignments.O8_Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(staffAssignments.O8_ProductInfo);
		}

		public void TestUniqueAssignment_ProductSpecific()
		{
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var orgStaffAssignmentsCollection = new EDIOrgStaffAssignmentsCollection(org2);
			orgStaffAssignmentsCollection.CompanySpecific = false;
			var orgStaffAssignments1 = orgStaffAssignmentsCollection.AddNew();
			orgStaffAssignments1.O8_Department = "FIA";
			orgStaffAssignments1.O8_Role = EDIOrgStaffAssignmentsLookups.PrimaryKAM;
			orgStaffAssignments1.O8_GC = ZGuid.Empty;
			orgStaffAssignments1.O8_Product = ZString.Empty;

				var orgStaffAssignments2 = orgStaffAssignmentsCollection.AddNew();
			orgStaffAssignments2.O8_Department = "FIA";
			orgStaffAssignments2.O8_Role = EDIOrgStaffAssignmentsLookups.PrimaryKAM;
			orgStaffAssignments2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignments2.O8_Product = "AAA";

			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignments2.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors("Product and Company can both be null", orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors(orgStaffAssignments2.O8_RoleInfo);

			orgStaffAssignments1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignments2.O8_GC = ZGuid.Empty;
			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignments2.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors("Product is empty but has company", orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors(orgStaffAssignments2.O8_RoleInfo);

			var orgStaffAssignments3 = orgStaffAssignmentsCollection.AddNew();
			orgStaffAssignments3.O8_Department = "FIA";
			orgStaffAssignments3.O8_Role = EDIOrgStaffAssignmentsLookups.PrimaryKAM;
			orgStaffAssignments3.O8_GC = ZGuid.Empty;
			orgStaffAssignments3.O8_Product = ZString.Empty;

			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignments2.Header.StaffAssignments.Load();
			orgStaffAssignments3.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors("Product and Company can both be null", orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors("Product and Company can both have value", orgStaffAssignments2.O8_RoleInfo);
			AssertNoErrors("Product and Company can both have value", orgStaffAssignments3.O8_RoleInfo);

			orgStaffAssignments1.O8_GC = ZGuid.Empty;
			orgStaffAssignments1.O8_Product = "AAA";
			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertHasError("Duplicated product", orgStaffAssignments1.O8_RoleInfo, "This staff assignment already has a Staff member assigned to it.");
			AssertHasError("Duplicated product", orgStaffAssignments2.O8_RoleInfo, "This staff assignment already has a Staff member assigned to it.");

			orgStaffAssignments1.O8_Product = "BBB";
			orgStaffAssignments1.Header.StaffAssignments.Load();
			orgStaffAssignmentsCollection.RunPreSaveValidation();
			AssertNoErrors("Unique product", orgStaffAssignments1.O8_RoleInfo);
			AssertNoErrors("Unique product", orgStaffAssignments2.O8_RoleInfo);
		}
	}
}
