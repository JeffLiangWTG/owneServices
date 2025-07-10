using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	sealed class DisposableEnvironmentTest : TestCase
	{
		public void TestDisposableEnvironmentForCompany()
		{
			Guid currentCompanyPK = Env.CurrentCompany.PK;

			foreach (var company in DisposableEnvironment.GetActiveCompanies())
			{
				using (DisposableEnvironment.ForCompany(company))
				{
					AssertEquals(company, Env.CurrentCompany.Code);
					AssertEquals(User.ServiceUserName, Env.CurrentUser.LoginName);
				}
			}

			AssertEquals(currentCompanyPK, Env.CurrentCompany.PK);
		}

		[UseSnapshotProtection]
		public void TestCompanyWithNoActiveBranch()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));

			company[GlbCompanySchema.GC_Code] = "FOO";
			branch[GlbBranchSchema.GB_GC] = company.PK;

			factory.Save();
			AssertCollectionContains("FOO", DisposableEnvironment.GetActiveCompanies());

			branch[GlbBranchSchema.GB_IsActive] = false;
			factory.Save();

			AssertCollectionNotContains("FOO", DisposableEnvironment.GetActiveCompanies());
		}

		[UseSnapshotProtection]
		public void TestCompanyIsntDuplicated()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));

			company[GlbCompanySchema.GC_Code] = "FOO";
			for (var i = 0; i < 5; i++)
			{
				var branch = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
				branch[GlbBranchSchema.GB_GC] = company.PK;
			}

			factory.Save();
			AssertEquals(1, DisposableEnvironment.GetActiveCompanies().Count(code => code == "FOO"));
		}

		public void TestDisposableEnvironmentForBranch()
		{
			Guid currentBranchPK = Env.CurrentBranch.PK;

			foreach (var company in new[] { "EDI", "SIN" })
			{
				foreach (var branch in GetActiveBranches(company))
				{
					using (DisposableEnvironment.ForBranch(branch))
					{
						AssertEquals(branch, Env.CurrentBranch.PK);
						AssertEquals(company, Env.CurrentCompany.Code);
						AssertEquals(User.ServiceUserName, Env.CurrentUser.LoginName);
					}
				}
			}

			AssertEquals(currentBranchPK, Env.CurrentBranch.PK);
		}

		Guid[] GetActiveBranches(string companyCode)
		{
			string sqlText = String.Format(
				"SELECT {0} FROM {1} JOIN {2} ON {3} = {4} WHERE {5} = 1 AND {6} = 1 AND {7} = @CompanyCode ORDER BY {0}",
				GlbBranchSchema.Constants.PK,
				GlbCompanySchema.Constants.TableName,
				GlbBranchSchema.Constants.TableName,
				GlbBranchSchema.Constants.GB_GC,
				GlbCompanySchema.Constants.PK,
				GlbCompanySchema.Constants.GC_IsActive,
				GlbBranchSchema.Constants.GB_IsActive,
				GlbCompanySchema.Constants.GC_Code);
			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@CompanyCode", companyCode, GlbCompanySchema.GC_Code);

				var result = new List<Guid>();
				using (IDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0));
					}
				}

				return result.ToArray();
			}
		}

		public void TestGetActiveCompanies()
		{
			string[] activeCompanies = DisposableEnvironment.GetActiveCompanies();
			AssertEquals("2 active companies when no parameter passed in", 2, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies("XX");
			AssertEquals("0 active company when countryCode = 'XX", 0, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies("AU");
			AssertEquals("1 active company only when countryCode = 'AU'", 1, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies("SG");
			AssertEquals("1 active company only when countryCode = 'SG'", 1, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies(new[] { "SG", "" });
			AssertEquals("1 active company only when countryCode in 'SG',''", 1, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies(new[] { "SG", "XX" });
			AssertEquals("1 active company only when countryCode in 'SG','XX'", 1, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies(new[] { "SG", "AU" });
			AssertEquals("2 active companies when countryCode in 'SG','AU", 2, activeCompanies.Length);

			activeCompanies = DisposableEnvironment.GetActiveCompanies(new[] { "" });
			AssertEquals("2 active companies when empty filter", 2, activeCompanies.Length);
		}

		public void TestDisposableEnvironmentWithNoCurrentCompany()
		{
			var companyCode = Env.CurrentCompany.Code;
			var branchPK = Env.CurrentBranch.PK;
			using (Env.SetTemporaryUserContext(null))
			{
				using (DisposableEnvironment.ForBranch(branchPK))
				{
					AssertEquals(companyCode, Env.CurrentCompany.Code);
					AssertEquals(branchPK, Env.CurrentBranch.PK);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDisposableEnvironmentHasActiveDepartment()
		{
			using (DisposableEnvironmentForTest.CleanupDepartment_ForTest())
			{
				string sql = "UPDATE dbo.GlbDepartment SET GE_IsActive = '0', GE_SystemLastEditUser = 'E', GE_SystemLastEditTimeUtc = GetDate() WHERE GE_Code = 'BRN'";
				Db.Connection.ExecuteNonQuery(sql);

				Assert("Precondition: there is at least one active department", (int)Db.Connection.ExecuteScalar("SELECT COUNT(GE_PK) FROM dbo.GlbDepartment WHERE GE_IsActive = '1'") > 0);

				using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
				{
					string sqlText = String.Format("SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_Code = @DepartmentCode", GlbDepartmentSchema.Constants.GE_Code);
					using (DbCommand command = Db.Connection.Command(sqlText))
					{
						command.AddParameterBasedOnDbColumn("@DepartmentCode", Env.CurrentDepartment.Code, GlbDepartmentSchema.GE_Code);
						bool isActive = (bool)command.ExecuteScalar();

						Assert(isActive);
						AssertNotEquals("BRN", Env.CurrentDepartment.Code);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAllBranchesDeletedFromCompanyJustBeforeTheyAreAccessed()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_Code] = "FOO";

			var branch1 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch1[GlbBranchSchema.GB_GC] = company.PK;
			var branch2 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_GC] = company.PK;

			DisposableEnvironment.BranchPKsToDeleteForTest = new[] { branch1.PK.ToGuid(), branch2.PK.ToGuid() };
			DisposableEnvironment.DeleteAllAtOnce = false;

			factory.Save();

			using (DisposableEnvironment.ForCompany("FOO"))
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContains("Company FOO attempted to load a corresponding branch from DB twice but it was missing both times.", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestAllBranchesDeletedFromCompanyImmediately()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_Code] = "FOO";

			var branch1 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch1[GlbBranchSchema.GB_GC] = company.PK;
			var branch2 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_GC] = company.PK;

			DisposableEnvironment.BranchPKsToDeleteForTest = new[] { branch1.PK.ToGuid(), branch2.PK.ToGuid() };
			DisposableEnvironment.DeleteAllAtOnce = true;

			factory.Save();

			using (DisposableEnvironment.ForCompany("FOO"))
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContains("Cannot find an active branch for company with code 'FOO'.", ErrorReporter.LastMessageReported);
				AssertContains("initialCurrentCompanyFromEnvironment IsUberFactory: False", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestTopBranchDeletedFromCompany()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_Code] = "FOO";

			var branch1 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch1[GlbBranchSchema.GB_GC] = company.PK;

			var branch2 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_GC] = company.PK;

			DisposableEnvironment.BranchPKsToDeleteForTest = new[] { branch1.PK.ToGuid() };

			factory.Save();

			using (DisposableEnvironment.ForCompany("FOO"))
			{
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		[UseSnapshotProtection]
		public void TestBranchesInactiveFromCompanyJustBeforeTheyAreAccessed()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_Code] = "FOO";

			var branch1 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch1[GlbBranchSchema.GB_GC] = company.PK;
			branch1[GlbBranchSchema.GB_Code] = "BR1";
			branch1[GlbBranchSchema.GB_IsActive] = false;

			var branch2 = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch2[GlbBranchSchema.GB_GC] = company.PK;
			branch2[GlbBranchSchema.GB_Code] = "BR2";
			branch2[GlbBranchSchema.GB_IsActive] = false;

			factory.Save();

			using (DisposableEnvironment.ForCompany("FOO"))
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);

				var actualMessages = ErrorReporter.LastMessageReported;
				AssertContains("Cannot find an active branch for company with code 'FOO'.", actualMessages);
				AssertContains("initialCurrentCompanyFromEnvironment IsUberFactory: False", ErrorReporter.LastMessageReported);
				AssertContains("companyCode:FOO, branch:", actualMessages);
				AssertContains("BranchInFactory:False,", actualMessages);
				AssertContains("Env CurrentCompany:", actualMessages);
				AssertContains(" - Branch in CurrentCompany", actualMessages);

				AssertContains("The Company FOO exist in GetActiveCompanies(): False", actualMessages);
				AssertContains("InDB Company:FOO, IsActive:True,", actualMessages);
				AssertContains("Code:BR1, IsActive:False", actualMessages);
				AssertContains("Code:BR2, IsActive:False", actualMessages);
			}

			ErrorReporter.Clear();
		}
	}
}
