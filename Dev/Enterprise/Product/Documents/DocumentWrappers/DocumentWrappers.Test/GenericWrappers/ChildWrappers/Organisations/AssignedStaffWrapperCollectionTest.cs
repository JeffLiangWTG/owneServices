using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(AssignedStaffWrapperCollection))]
	sealed class AssignedStaffWrapperCollectionTest : GenericWrapperCollectionTest<AssignedStaffWrapperCollection>
	{
		public void TestContentFromDistinctStaff()
		{
			AssignedStaffWrapperCollection collection = AssembleCollectionWithDistinctStaff();

			string[] expectedStaff =
			{
				"SAL-ALL",
				"SAL-AIR",
				"SAL-SEA",
				"SAL-AIR",
				"SAL-SEA",
				"SAL-WAR",

				"CUS-ALL",
				"CUS-AIR",
				"CUS-SEA",
				"CUS-AIR",
				"CUS-SEA",

				"CAR-ALL",
				"CAR-AIR",
				"CAR-SEA",
				"CAR-AIR",
				"CAR-SEA",
			};

			AssertContainsExactElementsInAnyOrder("All Assigned Staff",
				expectedStaff,
				Array.ConvertAll(collection.ToArray<AssignedStaffWrapper>(), (a) => a.Staff.FullName.ToString()));

			AssertEquals("SAL-ALL", collection["SAL"].Staff.FullName);
			AssertEquals("SAL-AIR", collection["SAL-IMP-AIR"].Staff.FullName);
			AssertEquals("SAL-SEA", collection["SAL-IMP-SEA"].Staff.FullName);
			AssertEquals("SAL-AIR", collection["SAL-EXP-AIR"].Staff.FullName);
			AssertEquals("SAL-SEA", collection["SAL-EXP-SEA"].Staff.FullName);
			AssertEquals("SAL-WAR", collection["SAL-WHS"].Staff.FullName);

			AssertEquals("CUS-ALL", collection["CUS"].Staff.FullName);
			AssertEquals("CUS-AIR", collection["CUS-IMP-AIR"].Staff.FullName);
			AssertEquals("CUS-SEA", collection["CUS-IMP-SEA"].Staff.FullName);
			AssertEquals("CUS-AIR", collection["CUS-EXP-AIR"].Staff.FullName);
			AssertEquals("CUS-SEA", collection["CUS-EXP-SEA"].Staff.FullName);

			AssertEquals("CAR-ALL", collection["CAR"].Staff.FullName);
			AssertEquals("CAR-AIR", collection["CAR-IMP-AIR"].Staff.FullName);
			AssertEquals("CAR-SEA", collection["CAR-IMP-SEA"].Staff.FullName);
			AssertEquals("CAR-AIR", collection["CAR-EXP-AIR"].Staff.FullName);
			AssertEquals("CAR-SEA", collection["CAR-EXP-SEA"].Staff.FullName);
		}

		public void TestContentFromOnlyOverallStaff()
		{
			AssignedStaffWrapperCollection collection = AssembleCollectionWithOnlyOverallStaff();

			string[] expectedStaff =
			{
				"SAL-ALL",
				"CUS-ALL",
				"CAR-ALL",
			};

			AssertContainsExactElementsInAnyOrder("Only 'Overall' Assigned Staff",
				expectedStaff,
				Array.ConvertAll(collection.ToArray<AssignedStaffWrapper>(), (a) => a.Staff.FullName.ToString()));

			AssertEquals("SAL-ALL", collection["SAL"].Staff.FullName);
			AssertEquals("SAL-ALL", collection["SAL-IMP-AIR"].Staff.FullName);
			AssertEquals("SAL-ALL", collection["SAL-IMP-SEA"].Staff.FullName);
			AssertEquals("SAL-ALL", collection["SAL-EXP-AIR"].Staff.FullName);
			AssertEquals("SAL-ALL", collection["SAL-EXP-SEA"].Staff.FullName);
			AssertEquals("SAL-ALL", collection["SAL-WHS"].Staff.FullName);

			AssertEquals("CUS-ALL", collection["CUS"].Staff.FullName);
			AssertEquals("CUS-ALL", collection["CUS-IMP-AIR"].Staff.FullName);
			AssertEquals("CUS-ALL", collection["CUS-IMP-SEA"].Staff.FullName);
			AssertEquals("CUS-ALL", collection["CUS-EXP-AIR"].Staff.FullName);
			AssertEquals("CUS-ALL", collection["CUS-EXP-SEA"].Staff.FullName);

			AssertEquals("CAR-ALL", collection["CAR"].Staff.FullName);
			AssertEquals("CAR-ALL", collection["CAR-IMP-AIR"].Staff.FullName);
			AssertEquals("CAR-ALL", collection["CAR-IMP-SEA"].Staff.FullName);
			AssertEquals("CAR-ALL", collection["CAR-EXP-AIR"].Staff.FullName);
			AssertEquals("CAR-ALL", collection["CAR-EXP-SEA"].Staff.FullName);
		}

		public void TestRelationships()
		{
			AssignedStaffWrapperCollection collection = AssembleCollectionWithDistinctStaff();

			AssertEquals(CommonResourceStrings.OverallRepresentative, collection["SAL"].Relationship);
			AssertEquals(CommonResourceStrings.ImportAirRepresentative, collection["SAL-IMP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ImportSeaRepresentative, collection["SAL-IMP-SEA"].Relationship);
			AssertEquals(CommonResourceStrings.ExportAirRepresentative, collection["SAL-EXP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ExportSeaRepresentative, collection["SAL-EXP-SEA"].Relationship);
			AssertEquals(CommonResourceStrings.WarehousingRepresentative, collection["SAL-WHS"].Relationship);

			AssertEquals(CommonResourceStrings.OverallCustomerServices, collection["CUS"].Relationship);
			AssertEquals(CommonResourceStrings.ImportAirCustomerServices, collection["CUS-IMP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ImportSeaCustomerServices, collection["CUS-IMP-SEA"].Relationship);
			AssertEquals(CommonResourceStrings.ExportAirCustomerServices, collection["CUS-EXP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ExportSeaCustomerServices, collection["CUS-EXP-SEA"].Relationship);

			AssertEquals(CommonResourceStrings.OverallLocalTransportCoordinator, collection["CAR"].Relationship);
			AssertEquals(CommonResourceStrings.ImportAirLocalTransportCoordinator, collection["CAR-IMP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ImportSeaLocalTransportCoordinator, collection["CAR-IMP-SEA"].Relationship);
			AssertEquals(CommonResourceStrings.ExportAirLocalTransportCoordinator, collection["CAR-EXP-AIR"].Relationship);
			AssertEquals(CommonResourceStrings.ExportSeaLocalTransportCoordinator, collection["CAR-EXP-SEA"].Relationship);
		}

		public void TestCategories()
		{
			AssignedStaffWrapperCollection collection = AssembleCollectionWithDistinctStaff();

			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL-IMP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL-IMP-SEA"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL-EXP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL-EXP-SEA"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, collection["SAL-WHS"].Category);

			AssertEquals(StaffAssignmentRoles.Codes.CustomerServiceRep, collection["CUS"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CustomerServiceRep, collection["CUS-IMP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CustomerServiceRep, collection["CUS-IMP-SEA"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CustomerServiceRep, collection["CUS-EXP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CustomerServiceRep, collection["CUS-EXP-SEA"].Category);

			AssertEquals(StaffAssignmentRoles.Codes.CartageCoordinator, collection["CAR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CartageCoordinator, collection["CAR-IMP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CartageCoordinator, collection["CAR-IMP-SEA"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CartageCoordinator, collection["CAR-EXP-AIR"].Category);
			AssertEquals(StaffAssignmentRoles.Codes.CartageCoordinator, collection["CAR-EXP-SEA"].Category);
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			return new AssignedStaffWrapper(staff, "TST", "Test", Factory);
		}

		protected override AssignedStaffWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new AssignedStaffWrapperCollection(Factory);
		}

		AssignedStaffWrapperCollection AssembleCollectionWithDistinctStaff()
		{
			string[] category = new string[]
			{
				StaffAssignmentRoles.Codes.SalesRep,
				StaffAssignmentRoles.Codes.CustomerServiceRep,
				StaffAssignmentRoles.Codes.CartageCoordinator,
			};

			string[] departments = new string[]
			{
				OrgStaffAssignmentsLookups.AllServices,
				OrgStaffAssignmentsLookups.AirFreightServices,
				OrgStaffAssignmentsLookups.SeaFreightServices,
				OrgStaffAssignmentsLookups.WarehouseServices,
			};

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i < category.Length; i++)
			{
				for (int j = 0; j < departments.Length; j++)
				{
					GlbStaff newstaff = Factory.New<GlbStaff>();
					newstaff.GS_Code = string.Format("X{0}{1}", i, j);
					newstaff.GS_FullName = category[i] + "-" + departments[j];

					OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
					assignment.O8_GS_NKPersonResponsible = newstaff.GS_Code;
					assignment.O8_Role = category[i];
					assignment.O8_Department = departments[j];
					assignment.O8_GC = GlbCompany.CurrentCompany.PK;
				}
			}

			return new AssignedStaffWrapperCollection(client, Factory);
		}

		AssignedStaffWrapperCollection AssembleCollectionWithOnlyOverallStaff()
		{
			string[] category = new string[]
			{
				StaffAssignmentRoles.Codes.SalesRep,
				StaffAssignmentRoles.Codes.CustomerServiceRep,
				StaffAssignmentRoles.Codes.CartageCoordinator,
			};

			string[] departments = new string[]
			{
				OrgStaffAssignmentsLookups.AllServices,
				OrgStaffAssignmentsLookups.AirFreightServices,
				OrgStaffAssignmentsLookups.SeaFreightServices,
				OrgStaffAssignmentsLookups.WarehouseServices,
			};

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			for (int i = 0; i < category.Length; i++)
			{
				GlbStaff newstaff = Factory.New<GlbStaff>();
				newstaff.GS_Code = string.Format("X{0}", i);
				newstaff.GS_FullName = category[i] + "-ALL";

				for (int j = 0; j < departments.Length; j++)
				{
					OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
					assignment.O8_GS_NKPersonResponsible = newstaff.GS_Code;
					assignment.O8_Role = category[i];
					assignment.O8_Department = departments[j];
					assignment.O8_GC = GlbCompany.CurrentCompany.PK;
				}
			}

			return new AssignedStaffWrapperCollection(client, Factory);
		}

		#endregion
	}
}
