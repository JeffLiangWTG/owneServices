using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class FallbackLevelTest : TestCase
	{
		public void TestGetPKs()
		{
			Guid companyPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();
			FallbackLevel fallback = new FallbackLevel(companyPK, branchPK, departmentPK);

			AssertEquals("CompanyPK", companyPK, fallback.CompanyPK(false));
			AssertEquals("CompanyPK", Guid.Empty, fallback.CompanyPK(true));
			AssertEquals("BranchPK", branchPK, fallback.BranchPK);
			AssertEquals("DepartmentPK", departmentPK, fallback.DepartmentPK);
		}

		public void TestGetLevel()
		{
			Guid companyPK = Guid.Empty;
			Guid branchPK = Guid.Empty;
			Guid departmentPK = Guid.Empty;
			FallbackLevel fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in System level", RegistryStorageFlags.System, fallback.Level);

			companyPK = Guid.NewGuid();
			fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in Company level", RegistryStorageFlags.Company, fallback.Level);

			branchPK = Guid.NewGuid();
			fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in Branch level", RegistryStorageFlags.Branch, fallback.Level);

			companyPK = Guid.Empty;
			branchPK = Guid.Empty;
			departmentPK = Guid.NewGuid();
			fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in SystemDepartment level", RegistryStorageFlags.SystemDepartment, fallback.Level);

			companyPK = Guid.NewGuid();
			fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in CompanyDepartment level", RegistryStorageFlags.CompanyDepartment, fallback.Level);

			branchPK = Guid.NewGuid();
			fallback = new FallbackLevel(companyPK, branchPK, departmentPK);
			AssertEquals("Should be in BranchDepartment level", RegistryStorageFlags.BranchDepartment, fallback.Level);
		}
	}
}
