using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	class Report_StaffByGroupMembershipIntegrationTest : TransactionedTestCase
	{
		public void TestReport_StaffByGroupMembership()
		{
			var result = new DataTable();
			PrepareTestData();

			//Note: As of WI00194878, we are changing the company/branch filters to be a lot less strict.
			//Since this isn't security logic that will actually grant/deny things, just quality of life filters on a report,
			//we don't need to be exact, we just need to get common and expected cases right.

			//We don't test for implicit right anymore, unlikely to come up and difficult to code.
			var dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, string.Empty, "A1");
			/*result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 has implicit security right", 1, result.Rows.Count);*/

			var groupSecurityRightPK = CreateSecurityRight(saleGroupPK, true, company1PK, Guid.Empty, false);
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("sale group has no right to login to CM1", 0, result.Rows.Count);

			UpdateSecurityRight(groupSecurityRightPK, true);
			var staffCompany1SecurityRightPK = CreateSecurityRight(staffPK, false, company1PK, Guid.Empty, false);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, string.Empty, "A1");
			result.Clear();
			//We don't test for right on group being shadowed by right on staff anymore, unlikely to come up and difficult to code.
			/*result.Load(dbCommand.ExecuteReader());
			AssertEquals("now sale group has right to login to CM1, however staff A1 has no right to login to CM1", 0, result.Rows.Count);*/

			UpdateSecurityRight(staffCompany1SecurityRightPK, true);
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 has right to login to CM1", 1, result.Rows.Count);

			UpdateSecurityRight(staffCompany1SecurityRightPK, false);
			CreateSecurityRight(staffPK, false, Guid.Empty, branch1PK, false);
			CreateSecurityRight(staffPK, false, Guid.Empty, branch2PK, true);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "BR1,BR2", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can not login CM1 nor BR1", 0, result.Rows.Count);
			UpdateSecurityRight(staffCompany1SecurityRightPK, true);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can't log into CM1", 1, result.Rows.Count);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(Guid.Empty, "BR1,BR2", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can login to BR2", 1, result.Rows.Count);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(Guid.Empty, "BR2", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can login to BR2", 1, result.Rows.Count);
			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "BR1,BR2", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can login to CM1 and BR2", 1, result.Rows.Count);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(Guid.Empty, "BR1", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can't log in to BR1", 0, result.Rows.Count);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, string.Empty, "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can log into CM1", 1, result.Rows.Count);

			UpdateSecurityRight(staffCompany1SecurityRightPK, false);
			CreateSecurityRight(staffPK, false, Guid.Empty, Guid.Empty, true);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(Guid.Empty, "BR1", "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can't log into BR1 still", 0, result.Rows.Count);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, string.Empty, "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can't log into CM1", 0, result.Rows.Count);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company2PK, string.Empty, "A1");
			result.Clear();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff A1 can log into CM2", 1, result.Rows.Count);
		}

		void PrepareTestData()
		{
			staffPK = CreateStaff("A1", "A1", true);
			var staffInActivePK = CreateStaff("A2", "A2", false);

			saleGroupPK = CreateGroup("SAL");

			CreateGroupLink(saleGroupPK, staffPK);
			CreateGroupLink(saleGroupPK, staffInActivePK);

			company1PK = CreateCompany("CM1");
			company2PK = CreateCompany("CM2");

			branch1PK = CreateBranch("BR1", company1PK);
			branch2PK = CreateBranch("BR2", company2PK);
		}
		public void TestSimultaneousCompanyBranchFilterIsExclusive()
		{
			var result = new DataTable();
			PrepareTestData();

			var branchRight = CreateSecurityRight(staffPK, false, company1PK, Guid.Empty, false);
			var companyRight = CreateSecurityRight(staffPK, false, Guid.Empty, branch2PK, true);

			var dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "BR2", "A1");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("A1 has rights to only branch", 0, result.Rows.Count);

			UpdateSecurityRight(branchRight, true);
			UpdateSecurityRight(companyRight, false);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "BR2", "A1");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("A1 has rights to only company", 0, result.Rows.Count);

			UpdateSecurityRight(companyRight, true);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "BR2", "A1");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("A1 has rights to both", 1, result.Rows.Count);
		}

		public void Test_CS00625094()
		{
			var result = new DataTable();
			PrepareTestData2();

			var dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "HKG", "VI");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff VI can't log in to this branch", 0, result.Rows.Count);

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "DUI", "VI");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("Can log in to this branch", 9, result.Rows.Count);
			result.Clear();

			dbCommand = FilteredStaffByGroupMembershipReportCommand(company2PK, "FRA", "VI");
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("Can log in to this branch", 9, result.Rows.Count);
		}

		void PrepareTestData2()
		{
			staffPK = CreateStaff("DG0311", "VI", true);
			CreateGroupLink(Enterprise.Core.Constants.Groups.AllPK, staffPK);
			var lG_YDGDUI = CreateGroup("LG_YDGDUI"); CreateGroupLink(lG_YDGDUI, staffPK);
			var lG_YDGFRA = CreateGroup("LG_YDGFRA"); CreateGroupLink(lG_YDGFRA, staffPK);
			var nO_FEE = CreateGroup("NO_FEE"); CreateGroupLink(nO_FEE, staffPK);
			var pRT = CreateGroup("PRT"); CreateGroupLink(pRT, staffPK);
			var rL_FWD_REPORT = CreateGroup("RL_FWD_REPORT"); CreateGroupLink(rL_FWD_REPORT, staffPK);
			var rL_FWD_VIEW = CreateGroup("RL_FWD_VIEW"); CreateGroupLink(rL_FWD_VIEW, staffPK);
			var rL_SALES = CreateGroup("RL_SALES"); CreateGroupLink(rL_SALES, staffPK);
			var yDG = CreateGroup("YDG"); CreateGroupLink(yDG, staffPK);

			company1PK = CreateCompany("YHK");
			company2PK = CreateCompany("C2P");

			var hKG = CreateBranch("HKG", company1PK);
			//not sure what belongs to what company, I'll have one of each
			var dUI = CreateBranch("DUI", company1PK);
			var fRA = CreateBranch("FRA", company2PK);

			var fIS = CreateDepartment("FIS");
			var wFS = CreateDepartment("WFS");
			var fID = CreateDepartment("FID");
			var fEA = CreateDepartment("FEA");
			var fES = CreateDepartment("FES");
			var fIA = CreateDepartment("FIA");
			var fSD = CreateDepartment("FSD");

			CreateSecurityRight(Enterprise.Core.Constants.Groups.AllPK, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(lG_YDGDUI, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(nO_FEE, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(pRT, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(rL_FWD_REPORT, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(rL_FWD_VIEW, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(rL_SALES, true, Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateSecurityRight(yDG, true, Guid.Empty, Guid.Empty, Guid.Empty, false);

			CreateSecurityRight(lG_YDGDUI, true, Guid.Empty, dUI, fIS, true);
			CreateSecurityRight(lG_YDGDUI, true, Guid.Empty, dUI, wFS, true);
			CreateSecurityRight(lG_YDGDUI, true, Guid.Empty, dUI, fID, true);
			CreateSecurityRight(lG_YDGDUI, true, Guid.Empty, dUI, fSD, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fIS, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fEA, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fIA, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, wFS, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fES, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fID, true);
			CreateSecurityRight(lG_YDGFRA, true, Guid.Empty, fRA, fSD, true);
		}

		public void TestNewStaffShouldHaveLoginPermission()
		{
			Guid allStaffGroupPK = new Guid("94755E71-A87A-4034-8DFA-785773A49607");//constant for system defined group PK

			//If the all staff group doesn't explicitly denies login premission, the new created staff should have perimssion to login
			staffPK = CreateStaff("DG0311", "VI", true);
			company1PK = CreateCompany("YHK");
			CreateGroupLink(allStaffGroupPK, staffPK);

			var dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "", "VI");
			var result = new DataTable();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff VI can log in to this company", 1, result.Rows.Count);
		}

		public void TestStaffShouldNotHaveLoginPermissionWhenExplicitlyDenied()
		{
			Guid allStaffGroupPK = new Guid("94755E71-A87A-4034-8DFA-785773A49607");//constant for system defined group PK

			staffPK = CreateStaff("DG0311", "VI", true);
			company1PK = CreateCompany("YHK");
			CreateGroupLink(allStaffGroupPK, staffPK);

			CreateSecurityRight(staffPK, false, Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateSecurityRight(staffPK, false, company1PK, Guid.Empty, Guid.Empty, false);

			var dbCommand = FilteredStaffByGroupMembershipReportCommand(company1PK, "", "VI");
			var result = new DataTable();
			result.Load(dbCommand.ExecuteReader());
			AssertEquals("staff VI can not log in to this company", 0, result.Rows.Count);
		}

		DbCommand FilteredStaffByGroupMembershipReportCommand(Guid companyPK, string branches, string staff)
		{
			var command = TestConnection.Command(@"
				SELECT * FROM Report_StaffByGroupMembership(
					@Company,
					@Branch)
				WHERE GS_Code = @code");
			command.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
			command.AddParameter("@Branch", SqlDbType.VarChar, branches);
			command.AddParameter("@code", SqlDbType.VarChar, staff);
			return command;
		}

		Guid CreateStaff(string loginName, string code, bool isActive)
		{
			var personPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"insert into dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values (@PER_PK, 'name', GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbStaffSchema.PK);
				command.ExecuteNonQuery();
			}

			var staffPK = Guid.NewGuid();
			const string createStaffSql = @"INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_IsActive, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
			VALUES (@pk, @loginName, @code, @isActive, @personPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(createStaffSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, staffPK);
				command.AddParameter("@loginName", SqlDbType.VarChar, GlbStaffSchema.GS_LoginName.MaxLength, loginName);
				command.AddParameter("@code", SqlDbType.VarChar, GlbStaffSchema.GS_Code.MaxLength, code);
				command.AddParameter("@isActive", SqlDbType.Bit, isActive);
				command.AddParameter("@personPk", SqlDbType.UniqueIdentifier, personPk);
				command.ExecuteNonQuery();
			}

			return staffPK;
		}

		Guid CreateGroup(string code)
		{
			var groupPK = Guid.NewGuid();
			const string createGroupSql = @"INSERT INTO dbo.GlbGroup (GG_PK, GG_Code, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) 
			VALUES (@pk, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(createGroupSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, groupPK);
				command.AddParameter("@code", SqlDbType.VarChar, GlbGroupSchema.GG_Code.MaxLength, code);
				command.ExecuteNonQuery();
			}

			return groupPK;
		}

		void CreateGroupLink(Guid groupPK, Guid staffPK)
		{
			const string createGroupSql = @"INSERT INTO dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) 
			VALUES (NEWID(), @groupPK, @staffPK)";

			using (var command = TestConnection.Command(createGroupSql))
			{
				command.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
				command.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateCompany(string code, string countryCode = "AU", string currencyCode = "AUD")
		{
			var companyPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@companyPK, @code, 'AU company', @countryCode, @currencyCode)"))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@code", SqlDbType.VarChar, GlbCompanySchema.GC_Code.MaxLength, code);
				command.AddParameter("@countryCode", SqlDbType.VarChar, GlbCompanySchema.GC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@currencyCode", SqlDbType.VarChar, GlbCompanySchema.GC_RX_NKLocalCurrency.MaxLength, currencyCode);
				command.ExecuteNonQuery();
			}

			return companyPK;
		}

		Guid CreateDepartment(string code)
		{
			object result = TestConnection.ExecuteScalar(@"select top 1 GE_PK from dbo.GlbDepartment where GE_Code = '" + code + "'");
			if (result is Guid && (Guid)result != Guid.Empty) { return (Guid)result; }

			var departmentPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@departmentPK, @code)"))
			{
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@code", SqlDbType.VarChar, GlbDepartmentSchema.GE_Code.MaxLength, code);
				command.ExecuteNonQuery();
			}

			return departmentPK;
		}

		Guid CreateBranch(string code, Guid companyPK)
		{
			var branchPK = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES (@branchPK, @code, @companyPK)"))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@code", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, code);
				command.ExecuteNonQuery();
			}

			return branchPK;
		}

		Guid CreateSecurityRight(Guid ownerPK, bool isGroup, Guid companyPK, Guid branchPK, bool isAllowed)
		{
			return CreateSecurityRight(ownerPK, isGroup, companyPK, branchPK, Guid.Empty, isAllowed);
		}

		Guid CreateSecurityRight(Guid ownerPK, bool isGroup, Guid companyPK, Guid branchPK, Guid departmentPK, bool isAllowed)
		{
			var securityPK = Guid.NewGuid();
			var ownerColumnName = isGroup ? "GU_GG" : "GU_GS";

			var createSecurityRightSql = $@"INSERT INTO dbo.GlbSecurity (GU_PK, {ownerColumnName}, GU_GC, GU_GB, GU_GE, GU_SecurityRight, GU_SecurityItemIsAllowed) 
			VALUES(@pk, @ownerPK, @companyPK, @branchPK, @departmentPK, 'Login', @isAllowed)";

			using (var command = TestConnection.Command(createSecurityRightSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, securityPK);
				command.AddParameter("@ownerPK", SqlDbType.UniqueIdentifier, ownerPK);

				if (companyPK == Guid.Empty)
				{
					command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				}

				if (branchPK == Guid.Empty)
				{
					command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				}

				if (departmentPK == Guid.Empty)
				{
					command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				}

				command.AddParameter("@isAllowed", SqlDbType.Bit, isAllowed);
				command.ExecuteNonQuery();
			}

			return securityPK;
		}

		void UpdateSecurityRight(Guid securityPK, bool isAllowed)
		{
			using (var command = TestConnection.Command(@"UPDATE dbo.GlbSecurity SET GU_SecurityItemIsAllowed = @isAllowed, GU_SystemLastEditUser = 'E', GU_SystemLastEditTimeUtc = GetDate() WHERE GU_PK = @pk"))
			{
				command.AddParameterBasedOnDbColumn("@pk", securityPK, GlbSecuritySchema.PK);
				command.AddParameterBasedOnDbColumn("@isAllowed", isAllowed, GlbSecuritySchema.GU_SecurityItemIsAllowed);
				command.ExecuteNonQuery();
			}
		}

		Guid staffPK;
		Guid saleGroupPK;

		Guid company1PK;
		Guid company2PK;
		Guid branch1PK;
		Guid branch2PK;
	}
}

