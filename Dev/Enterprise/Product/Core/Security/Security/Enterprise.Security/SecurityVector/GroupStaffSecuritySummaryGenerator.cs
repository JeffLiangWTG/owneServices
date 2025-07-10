using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	public class GroupStaffSecuritySummaryGenerator : ISecuritySummaryGenerator<string>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "We will use the whole array")]
		public string GenerateSummary(GlbStaff staff, ZGuid[] departmentsGuids, ZGuid[] branchesGuids,
			string[] departments, string[] branches,
			IEnumerable<IGlbSecurity> allSecurities, SecurityCalculatorForStaff securityCalculator, SecurityCheckpoint checkpoint, bool startFrom, bool useBranchesAndDepartments)
		{
			if (allSecurities == null)
			{
				return DeniedValue;
			}

			if (staff.GS_IsController)
			{
				return "IsController";
			}

			var grantedSource = new List<string>();
			if (useBranchesAndDepartments)
			{
				for (int i = 0; i < departmentsGuids.Length; i++)
				{
					var departmentPK = departmentsGuids[i].IsEmpty ? Guid.Empty : departmentsGuids[i].ToGuid();
					for (int j = 0; j < branchesGuids.Length; j++)
					{
						var branchPK = branchesGuids[j].IsEmpty ? Guid.Empty : branchesGuids[j].ToGuid();
						Guid companyPK;
						if (branchesGuids[j] == ZGuid.Empty)
						{
							companyPK = Guid.Empty;
						}
						else
						{
							var branch = staff.Factory.Load<GlbBranch>(branchesGuids[j]);
							companyPK = branch.GB_GC.ToGuid();
						}
						grantedSource.AddRange(securityCalculator.RightGrantedGroups(checkpoint, staff.PK.ToGuid(), branchPK, departmentPK, companyPK));
					}
				}
				grantedSource = grantedSource.Distinct().OrderBy(s => s).ToList();
			}
			else
			{
				var companyPK = Guid.Empty;
				var branchPK = Guid.Empty;
				var departmentPK = Guid.Empty;
				grantedSource = securityCalculator.RightGrantedGroups(checkpoint, staff.PK.ToGuid(), branchPK, departmentPK, companyPK);
			}

			return string.Join(",", grantedSource);
		}

		string ISecuritySummaryGenerator<string>.GenerateLocalAdministratorSummary(GlbStaff staff, IEnumerable<IGlbSecurity> securities)
		{
			throw new NotImplementedException();
		}

		public string CalculateInitialValue(GlbStaff staff)
		{
			return string.Join(",", staff.ActiveGroups.Cast<GlbGroup>().Select(group => group.GG_Code).OrderBy(code => code));
		}

		public string DeniedValue => string.Empty;

		public string GrantedValue => "ALL";
	}
}
