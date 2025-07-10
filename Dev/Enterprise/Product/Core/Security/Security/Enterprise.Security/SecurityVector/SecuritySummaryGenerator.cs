using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	public abstract class SecuritySummaryGenerator<T> : ISecuritySummaryGenerator<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "We will use the whole array")]
		public T GenerateSummary(GlbStaff staff, ZGuid[] departmentsGuids, ZGuid[] branchesGuids,
			string[] departments, string[] branches,
			IEnumerable<IGlbSecurity> allSecurities, SecurityCalculatorForStaff securityCalculator, SecurityCheckpoint checkpoint, bool startFrom, bool useBranchesAndDepartments)
		{
			if (allSecurities == null)
			{
				return DeniedValue;
			}

			if (staff.GS_IsController)
			{
				return GrantedValue;
			}

			bool[,] rights = new bool[departmentsGuids.Length, branchesGuids.Length];
			if (startFrom || useBranchesAndDepartments)
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

						var allowed = securityCalculator.IsRightAllowed(checkpoint, staff.PK.ToGuid(), branchPK, departmentPK, companyPK);
						rights[i, j] = allowed;
					}
				}
			}
			else
			{
				var companyPK = Guid.Empty;
				var branchPK = Guid.Empty;
				var departmentPK = Guid.Empty;
				var allowed = securityCalculator.IsRightAllowed(checkpoint, staff.PK.ToGuid(), branchPK, departmentPK, companyPK);
				bool baseRights = allowed;
				for (int i = 0; i < departmentsGuids.Length; i++)
				{
					for (int j = 0; j < branchesGuids.Length; j++)
					{
						rights[i, j] = baseRights;
					}
				}
			}

			return GenerateSummaryCore(departments, branches, rights);
		}

		public T GenerateLocalAdministratorSummary(GlbStaff staff, IEnumerable<IGlbSecurity> securities)
		{
			if (staff.GS_IsController)
			{
				return GrantedValue;
			}

			return GenerateLocalAdministratorSummaryCore(securities);
		}

		public abstract T DeniedValue { get; }

		public abstract T GrantedValue { get; }

		protected abstract T GenerateSummaryCore(string[] departments, string[] branches, bool[,] rights);
		protected abstract T GenerateLocalAdministratorSummaryCore(IEnumerable<IGlbSecurity> securities);

		public T CalculateInitialValue(GlbStaff staff)
		{
			return GrantedValue;
		}
	}
}
