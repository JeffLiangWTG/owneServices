using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Contains a set of Company, Branch and Department PKs as well as the RegistryStorageFlag of the current level.
	/// </summary>
	public class FallbackLevel
	{
		readonly Guid fCompanyPK;
		public readonly Guid BranchPK;
		public readonly Guid DepartmentPK;

		public FallbackLevel(ICompany company, IBranch branch, IDepartment department)
		{
			if (company != null)
			{
				fCompanyPK = company.PK;
			}
			if (branch != null)
			{
				BranchPK = branch.PK;
			}
			if (department != null)
			{
				DepartmentPK = department.PK;
			}
		}

		public FallbackLevel(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			fCompanyPK = companyPK;
			BranchPK = branchPK;
			DepartmentPK = departmentPK;
		}

		public Guid CompanyPK(bool returnEmptyIfBranchPKIsPresent)
		{
			Guid result = fCompanyPK;
			if (returnEmptyIfBranchPKIsPresent &
				BranchPK != Guid.Empty)
			{
				result = Guid.Empty;
			}
			return result;
		}

		public RegistryStorageFlags Level
		{
			get
			{
				RegistryStorageFlags level = new RegistryStorageFlags();

				if (fCompanyPK == Guid.Empty && BranchPK == Guid.Empty)
				{
					level = (DepartmentPK == Guid.Empty) ? RegistryStorageFlags.System : RegistryStorageFlags.SystemDepartment;
				}
				else if (BranchPK == Guid.Empty)
				{
					level = (DepartmentPK == Guid.Empty) ? RegistryStorageFlags.Company : RegistryStorageFlags.CompanyDepartment;
				}
				else
				{
					level = (DepartmentPK == Guid.Empty) ? RegistryStorageFlags.Branch : RegistryStorageFlags.BranchDepartment;
				}

				return level;
			}
		}
	}
}
