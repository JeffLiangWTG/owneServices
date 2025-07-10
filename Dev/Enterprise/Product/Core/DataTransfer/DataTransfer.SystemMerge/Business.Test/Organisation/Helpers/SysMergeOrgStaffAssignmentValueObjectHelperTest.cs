using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrgStaffAssignmentValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestImportAndExportValueObjectCollection()
		{
			SysMergeOrgStaffAssignmentValueObjectHelper orgStaffAssignmentHelper = new SysMergeOrgStaffAssignmentValueObjectHelper();
			INotifications notifications = new NotificationBuffer();
			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();
			AddNewStaffAssignmentsToOrg(organisation);
			OrgStaffAssignments[] assigments = Factory.Load<OrgStaffAssignments>(new ZQuery(OrgStaffAssignmentsSchema.O8_OH, organisation.PK));
			AssertEquals("Original Staff Assignments records", 2, assigments.Length);

			#region Export Ok Test

			Xsd.SysMergeOrgStaffAssignmentCollection xsdOrgStaffAssignments = new Xsd.SysMergeOrgStaffAssignmentCollection();
			orgStaffAssignmentHelper.ExportToValueObjectCollection(organisation, xsdOrgStaffAssignments, notifications);
			AssertEquals("Staff Assignments count", 2, xsdOrgStaffAssignments.Count);

			AssertEquals("Staff Assignments 1 - Role", "CAR", xsdOrgStaffAssignments[0].Role);
			AssertEquals("Staff Assignments 1 - Department", "FRT", xsdOrgStaffAssignments[0].Department);
			AssertEquals("Staff Assignments 1 - OH_OrgHeader_PK", organisation.PK.ToString(), xsdOrgStaffAssignments[0].OH_OrgHeader_PK);
			AssertEquals("Staff Assignments 1 - GS_ResponsiblePerson_NK", GlbStaff.CurrentUser.GS_Code, xsdOrgStaffAssignments[0].GS_ResponsiblePerson_NK);
			AssertEquals("Staff Assignments 1 - GC_Company_NK", GlbCompany.CurrentCompany.GC_Code, xsdOrgStaffAssignments[0].GC_Company_NK);

			AssertEquals("Staff Assignments 2 - Role", "ACT", xsdOrgStaffAssignments[1].Role);
			AssertEquals("Staff Assignments 2 - Department", "RAI", xsdOrgStaffAssignments[1].Department);
			AssertEquals("Staff Assignments 2 - OH_OrgHeader_PK", organisation.PK.ToString(), xsdOrgStaffAssignments[1].OH_OrgHeader_PK);
			AssertEquals("Staff Assignments 2 - GS_ResponsiblePerson_NK", GlbStaff.CurrentUser.GS_Code, xsdOrgStaffAssignments[1].GS_ResponsiblePerson_NK);
			AssertEquals("Staff Assignments 2 - GC_Company_NK", GlbCompany.CurrentCompany.GC_Code, xsdOrgStaffAssignments[1].GC_Company_NK);

			#endregion

			#region Import Ok Test

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeaderForDataTransfer newOrganisation = newFactory.New<OrgHeaderForDataTransfer>();
			assigments = newFactory.Load<OrgStaffAssignments>(new ZQuery(OrgStaffAssignmentsSchema.O8_OH, newOrganisation.PK));
			AssertEquals("New Staff Assignments records", 0, assigments.Length);

			IValueObjectImportContext testContext = new ValueObjectImportContext(newOrganisation.Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notifications);
			orgStaffAssignmentHelper.ImportFromValueObjectCollection(xsdOrgStaffAssignments, newOrganisation, testContext);
			assigments = newFactory.Load<OrgStaffAssignments>(new ZQuery(OrgStaffAssignmentsSchema.O8_OH, newOrganisation.PK));
			AssertEquals("Staff Assignments after import records", 2, assigments.Length);

			AssertEquals("Staff Assignments 1 - Role", "CAR", assigments[0].O8_Role);
			AssertEquals("Staff Assignments 1 - Department", "FRT", assigments[0].O8_Department);
			AssertEquals("Staff Assignments 1 - OH_OrgHeader_PK", assigments[0].O8_OH, newOrganisation.PK);
			AssertEquals("Staff Assignments 1 - GS_ResponsiblePerson_NK", xsdOrgStaffAssignments[0].GS_ResponsiblePerson_NK, assigments[0].PersonResponsible.GS_Code);
			AssertEquals("Staff Assignments 1 - GC_Company_NK", xsdOrgStaffAssignments[0].GC_Company_NK, newOrganisation.Factory.Load<GlbCompany>(assigments[0].O8_GC).GC_Code);

			AssertEquals("Staff Assignments 2 - Role", "ACT", assigments[1].O8_Role);
			AssertEquals("Staff Assignments 2 - Department", "RAI", assigments[1].O8_Department);
			AssertEquals("Staff Assignments 2 - OH_OrgHeader_PK", assigments[1].O8_OH, newOrganisation.PK);
			AssertEquals("Staff Assignments 2 - GS_ResponsiblePerson_NK", xsdOrgStaffAssignments[1].GS_ResponsiblePerson_NK, assigments[1].PersonResponsible.GS_Code);
			AssertEquals("Staff Assignments 2 - GC_Company_NK", xsdOrgStaffAssignments[1].GC_Company_NK, newOrganisation.Factory.Load<GlbCompany>(assigments[1].O8_GC).GC_Code);

			#endregion
		}

		public void TestExportValueObjectCollection_WithBadData()
		{
			SysMergeOrgStaffAssignmentValueObjectHelper orgStaffAssignmentHelper = new SysMergeOrgStaffAssignmentValueObjectHelper();
			INotifications notifications = new NotificationBuffer();

			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();
			AddNewStaffAssignmentsToOrg_1ValidAnd1Invalid(organisation);
			OrgStaffAssignments[] assigments = Factory.Load<OrgStaffAssignments>(new ZQuery(OrgStaffAssignmentsSchema.O8_OH, organisation.PK));
			AssertEquals("Original Staff Assignments records", 2, assigments.Length);

			Xsd.SysMergeOrgStaffAssignmentCollection newXsdOrgStaffAssignments = new Xsd.SysMergeOrgStaffAssignmentCollection();
			orgStaffAssignmentHelper.ExportToValueObjectCollection(organisation, newXsdOrgStaffAssignments, notifications);
			AssertEquals("Only 1 Staff Assignment should be exported", 1, newXsdOrgStaffAssignments.Count);
			AssertEquals("Staff Assignment Role", "VAL", newXsdOrgStaffAssignments[0].Role);
		}

		public void TestImportValueObjectCollection_WithBadData()
		{
			SysMergeOrgStaffAssignmentValueObjectHelper orgStaffAssignmentHelper = new SysMergeOrgStaffAssignmentValueObjectHelper();
			INotifications notifications = new NotificationBuffer();

			OrgHeaderForDataTransfer organisation = Factory.New<OrgHeaderForDataTransfer>();
			AddNewStaffAssignmentsToOrg(organisation);
			Xsd.SysMergeOrgStaffAssignmentCollection xsdOrgStaffAssignments = new Xsd.SysMergeOrgStaffAssignmentCollection();
			orgStaffAssignmentHelper.ExportToValueObjectCollection(organisation, xsdOrgStaffAssignments, notifications);
			AssertEquals("Staff Assignments count", 2, xsdOrgStaffAssignments.Count);

			// Change ResponsiblePerson to a non-existent staff code
			xsdOrgStaffAssignments[0].GS_ResponsiblePerson_NK = "#~Non-Existent-Staff~#";

			try
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				organisation = newFactory.New<OrgHeaderForDataTransfer>();
				IValueObjectImportContext testContext = new ValueObjectImportContext(organisation.Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), notifications);
				orgStaffAssignmentHelper.ImportFromValueObjectCollection(xsdOrgStaffAssignments, organisation, testContext);
				Assert("An exception should be thrown", false);
			}
			catch (Exception ex)
			{
				Assert("Exception does not match expected value. Actual value was:\r\n" + ex.Message, ex.Message.StartsWith("Could not find Staff code = [#~Non-Existent-Staff~#]"));
			}
		}

		void AddNewStaffAssignmentsToOrg(OrgHeaderForDataTransfer organisation)
		{
			OrgStaffAssignments staffAssignment1 = organisation.Factory.New<OrgStaffAssignments>();
			staffAssignment1.O8_Role = "CAR";
			staffAssignment1.O8_Department = "FRT";
			staffAssignment1.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			staffAssignment1.O8_OH = organisation.PK;
			staffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;

			OrgStaffAssignments staffAssignment2 = organisation.Factory.New<OrgStaffAssignments>();
			staffAssignment2.O8_Role = "ACT";
			staffAssignment2.O8_Department = "RAI";
			staffAssignment2.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			staffAssignment2.O8_OH = organisation.PK;
			staffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
		}

		void AddNewStaffAssignmentsToOrg_1ValidAnd1Invalid(OrgHeaderForDataTransfer organisation)
		{
			// Invalid: empty O8_GS_ResponsiblePerson 
			OrgStaffAssignments staffAssignment1 = organisation.Factory.New<OrgStaffAssignments>();
			staffAssignment1.O8_Role = "INV";
			staffAssignment1.O8_Department = "FRT";
			staffAssignment1.O8_OH = organisation.PK;
			staffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;

			// Valid
			OrgStaffAssignments staffAssignment2 = organisation.Factory.New<OrgStaffAssignments>();
			staffAssignment2.O8_Role = "VAL";
			staffAssignment2.O8_Department = "RAI";
			staffAssignment2.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			staffAssignment2.O8_OH = organisation.PK;
		}
	}
}
