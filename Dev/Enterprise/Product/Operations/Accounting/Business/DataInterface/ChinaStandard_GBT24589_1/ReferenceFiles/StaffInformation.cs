
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class StaffInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T108";
		public ZString StaffCode { get; set; }
		public ZString StaffName { get; set; }
		public ZString IDType { get; set; }
		public ZString IDNumber { get; set; }
		public ZString Gender { get; set; }
		public ZString BirthDate { get; set; }
		public ZString DepartmentCode { get; set; }
		public ZString EmploymentDate { get; set; }
		public ZString LeaveDate { get; set; }
	}

	public sealed class StaffInformationCollection : NonPersistentBusinessObjectCollection<StaffInformation>	{
		public StaffInformationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StaffInformation();
		}

		void AddDefaultElements()
		{
			DataTable table = RunScript();
			Dictionary<ZGuid, ZInt> pKs = new Dictionary<ZGuid, ZInt>();

			foreach (DataRow staffrow in table.Rows.Cast<DataRow>().Where(staffrow => !pKs.ContainsKey((Guid)staffrow["GS_PK"])))
			{
				pKs.Add((Guid)staffrow["GS_PK"], 1);
				GlbStaff staff = Factory.Load<GlbStaff>((Guid)staffrow["GS_PK"]);
				if (staff.GS_IsSystemAccount)
				{
					continue;
				}

				StaffInformation staffInformation = AddNew();
				staffInformation.StaffCode = staff.GS_Code;
				staffInformation.StaffName = staff.GS_FullName;

				GetIDTypeAndNumber(staffInformation, staff);
				if (staff.GS_Gender == "F")
				{
					staffInformation.Gender = (NoResString)"女";
				}

				if (staff.GS_Gender == "M")
				{
					staffInformation.Gender = (NoResString)"男";
				}

				staffInformation.BirthDate = staff.GS_Birthdate.ToString("yyyyMMdd");
				staffInformation.DepartmentCode = GetDepartmentCode(staff);

				staffInformation.EmploymentDate = staff.GS_EmploymentDate.ToString("yyyyMMdd");
				staffInformation.LeaveDate = staff.GS_DepartureDate.ToString("yyyyMMdd");
			}
		}

		DataTable RunScript()
		{
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(@"SELECT GS_PK, GS_Code
FROM dbo.Glbstaff 
WHERE GS_IsSystemAccount = 0
 AND GS_IsDeveloper = 0
 AND GS_IsController = 1

UNION  

SELECT distinct GS_PK,GS_Code
FROM dbo.GlbSecurity
JOIN dbo.Glbstaff ON GU_GS = GS_PK
LEFT JOIN dbo.GlbBranch ON GU_GB = GB_PK
WHERE GS_IsSystemAccount = 0
  AND GS_IsDeveloper = 0
  AND GU_SecurityRight = 'Login'
  AND GU_SecurityItemIsAllowed = 1
  AND ((GU_GC IS NULL AND GU_GB IS NULL) OR GB_GC = '{0}' OR GU_GC = '{0}')

UNION 

SELECT distinct GS_PK,GS_Code
FROM dbo.GlbSecurity
JOIN dbo.GlbGroupLink ON GU_GG = GK_GG
JOIN dbo.Glbstaff ON GK_GS = GS_PK
LEFT JOIN dbo.GlbBranch ON GU_GB = GB_PK
WHERE GS_IsSystemAccount = 0
  AND GS_IsDeveloper = 0 
  AND GU_SecurityRight = 'Login' 
  AND GU_SecurityItemIsAllowed = 1
  AND (GU_GC ='{0}' OR GB_GC = '{0}')
ORDER BY GS_Code",
													GlbCompany.CurrentCompany.PK
													)
								);
			return table;
		}

		ZString GetDepartmentCode(GlbStaff staff)
		{
			if (staff.GS_GE_HomeDepartment.IsValid && staff.GS_GE_HomeDepartment.IsValid)
			{
				GlbDepartment glbDept = Factory.Load<GlbDepartment>(staff.GS_GE_HomeDepartment);
				return glbDept != null ? glbDept.GE_Code : ZString.Empty;
			}
			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China's Accounting fixed value")]
		void GetIDTypeAndNumber(StaffInformation staffInformation, GlbStaff staff)
		{
			if (!staff.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.NI1).IsEmpty)
			{
				staffInformation.IDType = "身份证";
				staffInformation.IDNumber = staff.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.NI1);
			}
			else
			{
				if (!staff.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.PA1).IsEmpty)
				{
					staffInformation.IDType = "护照";
					staffInformation.IDNumber = staff.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.PA1);
				}
				else
				{
					staffInformation.IDType = ZString.Empty;
					staffInformation.IDNumber = ZString.Empty;
				}
			}
		}
	}
}

