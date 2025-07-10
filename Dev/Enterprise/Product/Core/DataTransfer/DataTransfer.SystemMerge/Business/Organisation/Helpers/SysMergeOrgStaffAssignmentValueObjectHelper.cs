using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class SysMergeOrgStaffAssignmentValueObjectHelper
	{
		public SysMergeOrgStaffAssignmentValueObjectHelper()
		{
		}

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgStaffAssignmentCollection xsdOrgStaffAssignments, OrgHeaderForDataTransfer orgHeader, IValueObjectImportContext context)
		{
			if (xsdOrgStaffAssignments.IsSpecified)
			{
				foreach (Xsd.SysMergeOrgStaffAssignment xsdOrgStaffAssignment in xsdOrgStaffAssignments)
				{
					ImportFromValueObject(xsdOrgStaffAssignment, orgHeader, context);
				}
			}
		}

		OrgStaffAssignments ImportFromValueObject(Xsd.SysMergeOrgStaffAssignment xsdOrgStaffAssignment, OrgHeaderForDataTransfer orgHeader, IValueObjectImportContext context)
		{
			OrgStaffAssignments orgStaffAssignment = orgHeader.Factory.New<OrgStaffAssignments>();
			orgStaffAssignment.O8_OH = orgHeader.PK;
			orgStaffAssignment.O8_GS_NKPersonResponsible = context.GetStaffByCodeThrowingErrorIfNotFound(orgStaffAssignment.Factory, xsdOrgStaffAssignment.GS_ResponsiblePerson_NK).GS_Code;
			orgStaffAssignment.O8_Role = xsdOrgStaffAssignment.Role;
			orgStaffAssignment.O8_Department = xsdOrgStaffAssignment.Department;

			if (!xsdOrgStaffAssignment.GC_Company_NK.IsEmpty)
			{
				GlbCompany company = orgStaffAssignment.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, xsdOrgStaffAssignment.GC_Company_NK);

				if (company != null)
				{
					orgStaffAssignment.O8_GC = company.PK;
				}
			}

			return orgStaffAssignment;
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgStaffAssignmentCollection xsdOrgStaffAssignments, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgStaffAssignmentsSchema.O8_OH, org.PK);
			OrgStaffAssignments[] orgStaffAssignments = org.Factory.Load<OrgStaffAssignments>(query);

			foreach (OrgStaffAssignments orgStaffAssignment in orgStaffAssignments)
			{
				Xsd.SysMergeOrgStaffAssignment xsdOrgStaffAssignment = GetExportToValueObject(orgStaffAssignment, notifications);

				if (xsdOrgStaffAssignment != null)
				{
					xsdOrgStaffAssignments.Add(xsdOrgStaffAssignment);
				}
			}
		}

		Xsd.SysMergeOrgStaffAssignment GetExportToValueObject(OrgStaffAssignments orgStaffAssignment, INotifications notifications)
		{
			Xsd.SysMergeOrgStaffAssignment xsdOrgStaffAssignment = null;

			if (!orgStaffAssignment.O8_OH.IsEmpty &&
				!orgStaffAssignment.O8_GS_NKPersonResponsible.IsEmpty)
			{
				GlbStaff glbStaff = orgStaffAssignment.PersonResponsible;

				if (glbStaff != null)
				{
					xsdOrgStaffAssignment = new Xsd.SysMergeOrgStaffAssignment();

					if (!orgStaffAssignment.O8_Role.IsEmpty)
					{
						xsdOrgStaffAssignment.Role = orgStaffAssignment.O8_Role;
						xsdOrgStaffAssignment.RoleSpecified = true;
					}

					if (!orgStaffAssignment.O8_Department.IsEmpty)
					{
						xsdOrgStaffAssignment.Department = orgStaffAssignment.O8_Department;
						xsdOrgStaffAssignment.DepartmentSpecified = true;
					}

					xsdOrgStaffAssignment.OH_OrgHeader_PK = orgStaffAssignment.O8_OH.ToString();
					xsdOrgStaffAssignment.OH_OrgHeader_PKSpecified = true;
					if (!orgStaffAssignment.O8_GC.IsEmpty)
					{
						GlbCompany glbCompany = orgStaffAssignment.Factory.Load<GlbCompany>(orgStaffAssignment.O8_GC);
						if (glbCompany != null)
						{
							xsdOrgStaffAssignment.GC_Company_NK = glbCompany.GC_Code;
							xsdOrgStaffAssignment.GC_Company_NKSpecified = true;
						}
					}
					xsdOrgStaffAssignment.GS_ResponsiblePerson_NK = glbStaff.GS_Code;
					xsdOrgStaffAssignment.GS_ResponsiblePerson_NKSpecified = true;
				}
			}
			return xsdOrgStaffAssignment;
		}

		#endregion
	}
}
