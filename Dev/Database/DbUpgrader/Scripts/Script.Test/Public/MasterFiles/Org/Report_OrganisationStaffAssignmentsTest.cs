using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org
{
	[TestedType(typeof(Report_OrganisationStaffAssignments))]
	class Report_OrganisationStaffAssignmentsTest : DbCreateScriptTest
	{
		string GetStringWithSameLengthAsColumn(SchemaStringColumn column)
		{
			return new string('A', column.MaxLength);
		}

		public void TestHardCodedLengths()
		{
			Guid companyPK = Guid.NewGuid();
			Guid orgPK = Guid.NewGuid();
			string staffCode = "BIL";
			InsertCompany(companyPK, "AAA", "companyName");
			InsertOrg(orgPK, "orgCode", "orgName");
			InsertOrgCompanyData(orgPK, companyPK, Guid.Empty);
			InsertStaff(staffCode, "bilbo.baggins", "staffName");
			AddAssignedStaff(staffCode, "SAL", orgPK, companyPK);
			AssertNoExceptionThrown("Precondition: hardcoded fields, likely to change length, are OK for now", () => RunReportForTesting());

			UpdateCompany(companyPK, GetStringWithSameLengthAsColumn(GlbCompanySchema.GC_Name));
			AssertNoExceptionThrown("If there's an exception: Company name's max length is larger than the length in the report", () => RunReportForTesting());

			UpdateOrg(orgPK, GetStringWithSameLengthAsColumn(OrgHeaderSchema.OH_Code), GetStringWithSameLengthAsColumn(OrgHeaderSchema.OH_FullName));
			AssertNoExceptionThrown("If there's an exception: Org code/name's max length is larger than the length in the report", () => RunReportForTesting());

			UpdateStaff(staffCode, GetStringWithSameLengthAsColumn(GlbStaffSchema.GS_FullName));
			AssertNoExceptionThrown("If there's an exception: Staff name's max length is larger than the length in the report", () => RunReportForTesting());
		}

		void RunReportForTesting()
		{
			string query = "SELECT * FROM Report_OrganisationStaffAssignments(@BranchPK, @CompanyPK) ORDER BY CompanyName, Initials";

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.ExecuteNonQuery();
			}
		}

		#region Update Data

		void UpdateCompany(Guid pk, string name)
		{
			string query = string.Format("UPDATE {0} SET {1} = @GC_Name, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE {2} = @GC_PK",
				GlbCompanySchema.Constants.TableName,
				GlbCompanySchema.GC_Name.Name,
				GlbCompanySchema.PK.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@GC_Name", name, GlbCompanySchema.GC_Name);
				command.AddParameterBasedOnDbColumn("@GC_PK", pk, GlbCompanySchema.PK);
				command.ExecuteNonQuery();
			}
		}

		void UpdateOrg(Guid pk, string code, string name)
		{
			string query = string.Format("UPDATE {0} SET {1} = @OH_Code, {2} = @OH_FullName WHERE {3} = @OH_PK",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.OH_Code.Name,
				OrgHeaderSchema.OH_FullName.Name,
				OrgHeaderSchema.PK.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", name, OrgHeaderSchema.OH_FullName);
				command.AddParameterBasedOnDbColumn("@OH_PK", pk, OrgHeaderSchema.PK);
				command.ExecuteNonQuery();
			}
		}

		void UpdateStaff(string code, string name)
		{
			string query = string.Format("UPDATE {0} SET {1} = @GS_FullName, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE {2} = @GS_Code",
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.GS_FullName.Name,
				GlbStaffSchema.GS_Code.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@GS_FullName", name, GlbStaffSchema.GS_FullName);
				command.AddParameterBasedOnDbColumn("@GS_Code", code, GlbStaffSchema.GS_Code);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		public void TestAssignedCompanyAndBranchFilters()
		{
			string query = "SELECT * FROM Report_OrganisationStaffAssignments(@BranchPK, @CompanyPK)";
			Guid companyEDI = Guid.NewGuid();
			Guid companyDEM = Guid.NewGuid();
			Guid companyOdd = Guid.NewGuid(); // Odd company that has no local staff assignments

			InsertCompany(companyEDI, "EEECompany", "EDI Company");
			InsertCompany(companyDEM, "DDDCompany", "DEM Company");
			InsertCompany(companyOdd, "ODDCompany", "Odd Company");

			Guid branchED1 = Guid.NewGuid(); // Represents EDI's BNE
			Guid branchED2 = Guid.NewGuid(); // Represents EDI's SYD
			Guid branchDE1 = Guid.NewGuid(); // Represents DEM's DEM

			InsertBranch(branchED1, "ED1", companyEDI);
			InsertBranch(branchED2, "ED2", companyEDI);
			InsertBranch(branchDE1, "DE1", companyDEM);

			Guid orgGANEXP = Guid.NewGuid();
			Guid orgMAOALT = Guid.NewGuid();
			Guid orgBILCLO = Guid.NewGuid();

			InsertOrg(orgGANEXP, "orgGANEXP", "GANEXP Organisation");
			InsertOrgCompanyData(orgGANEXP, companyEDI, branchED2);
			InsertOrgCompanyData(orgGANEXP, companyDEM, Guid.Empty);
			InsertOrgCompanyData(orgGANEXP, companyOdd, Guid.Empty);

			InsertOrg(orgMAOALT, "orgMAOALT", "MAOALT Organisation");
			InsertOrgCompanyData(orgMAOALT, companyEDI, branchED1);
			InsertOrgCompanyData(orgMAOALT, companyDEM, branchDE1);
			InsertOrgCompanyData(orgMAOALT, companyOdd, Guid.Empty);

			InsertOrg(orgBILCLO, "orgBILCLO", "BILCLO Organisation");
			InsertOrgCompanyData(orgBILCLO, companyEDI, branchED1);
			InsertOrgCompanyData(orgBILCLO, companyDEM, branchDE1);
			InsertOrgCompanyData(orgBILCLO, companyOdd, Guid.Empty);

			InsertStaff("BIL", "bilbo.baggins");
			InsertStaff("GAN", "gandalf.grey");
			InsertStaff("DON", "donald.duck");

			AddAssignedStaff("BIL", "SAL", orgGANEXP, Guid.Empty);
			AddAssignedStaff("GAN", "SAL", orgGANEXP, companyEDI);

			AddAssignedStaff("DON", "ACT", orgMAOALT, Guid.Empty);
			AddAssignedStaff("DON", "SAL", orgMAOALT, companyDEM);

			AddAssignedStaff("GAN", "ACT", orgBILCLO, Guid.Empty);
			AddAssignedStaff("BIL", "ACT", orgBILCLO, companyEDI);

			using (DbCommand command = TestConnection.Command(query + " ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return all six results", 6, result.Rows.Count);
					AssertEquals(" - GANEXP Organisation - BIL - BIL", AssignedStaff(0, result));
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(1, result));
					AssertEquals(" - BILCLO Organisation - GAN - None", AssignedStaff(2, result));
					AssertEquals("DEM Company - MAOALT Organisation - DON - DON", AssignedStaff(3, result));
					AssertEquals("EDI Company - BILCLO Organisation - BIL - None", AssignedStaff(4, result));
					AssertEquals("EDI Company - GANEXP Organisation - GAN - BIL", AssignedStaff(5, result));
				});
			}

			// The report will automatically apply the WHERE clause when filters are applied
			using (DbCommand command = TestConnection.Command(query + " WHERE BranchPK = @BranchPK AND CompanyPK = @CompanyPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchED2);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyEDI);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return 1 result - only org GAN is controlled by this branch (ED2)", 1, result.Rows.Count);
					AssertEquals("EDI Company - GANEXP Organisation - GAN - BIL", AssignedStaff(0, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE CompanyPK = @CompanyPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyDEM);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return four results", 4, result.Rows.Count);
					AssertEquals(" - GANEXP Organisation - BIL - BIL", AssignedStaff(0, result));
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(1, result));
					AssertEquals(" - BILCLO Organisation - GAN - None", AssignedStaff(2, result));
					AssertEquals("DEM Company - MAOALT Organisation - DON - DON", AssignedStaff(3, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE CompanyPK = @CompanyPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyEDI);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return 3 results instead of 5. Show local staff if there isn't a global, else fallback to global", 3, result.Rows.Count);
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(0, result));
					AssertEquals("EDI Company - BILCLO Organisation - BIL - None", AssignedStaff(1, result));
					AssertEquals("EDI Company - GANEXP Organisation - GAN - BIL", AssignedStaff(2, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE CompanyPK = @CompanyPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyOdd);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return three results - there are no local staff for the odd company", 3, result.Rows.Count);
					AssertEquals(" - GANEXP Organisation - BIL - BIL", AssignedStaff(0, result));
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(1, result));
					AssertEquals(" - BILCLO Organisation - GAN - None", AssignedStaff(2, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE BranchPK = @BranchPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchED1);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return four results - only org BIL and MAO are controlled by this branch (ED1)", 4, result.Rows.Count);
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(0, result));
					AssertEquals(" - BILCLO Organisation - GAN - None", AssignedStaff(1, result));
					AssertEquals("DEM Company - MAOALT Organisation - DON - DON", AssignedStaff(2, result));
					AssertEquals("EDI Company - BILCLO Organisation - BIL - None", AssignedStaff(3, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE BranchPK = @BranchPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchED2);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return two results - only org GAN is controlled by this branch (ED2)", 2, result.Rows.Count);
					AssertEquals(" - GANEXP Organisation - BIL - BIL", AssignedStaff(0, result));
					AssertEquals("EDI Company - GANEXP Organisation - GAN - BIL", AssignedStaff(1, result));
				});
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE BranchPK = @BranchPK ORDER BY CompanyName, Initials"))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchDE1);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				CombineAssertions(delegate
				{
					AssertEquals("Should return four results - only orgs DON and MAO are controlled by this branch (DE1)", 4, result.Rows.Count);
					AssertEquals(" - MAOALT Organisation - DON - DON", AssignedStaff(0, result));
					AssertEquals(" - BILCLO Organisation - GAN - None", AssignedStaff(1, result));
					AssertEquals("DEM Company - MAOALT Organisation - DON - DON", AssignedStaff(2, result));
					AssertEquals("EDI Company - BILCLO Organisation - BIL - None", AssignedStaff(3, result));
				});
			}
		}

		#region Insert Test Data

		void InsertCompany(Guid pk, string code, string name, string gC_RN_NKCountryCode = "AU", string gC_RX_NKLocalCurrency = "AUD")
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES (",
				GlbCompanySchema.Constants.TableName,
				GlbCompanySchema.PK.Name,
				GlbCompanySchema.GC_Code.Name,
				GlbCompanySchema.GC_Name.Name,
				GlbCompanySchema.GC_RN_NKCountryCode.Name,
				GlbCompanySchema.GC_RX_NKLocalCurrency.Name);
			using (DbCommand command = TestConnection.Command(query + "@GC_PK, @GC_Code, @GC_Name, @GC_RN_NKCountryCode, @GC_RX_NKLocalCurrency)"))
			{
				command.AddParameterBasedOnDbColumn("@GC_PK", pk, GlbCompanySchema.PK);
				command.AddParameterBasedOnDbColumn("@GC_Code", code, GlbCompanySchema.GC_Code);
				command.AddParameterBasedOnDbColumn("@GC_Name", name, GlbCompanySchema.GC_Name);
				command.AddParameter("@GC_RN_NKCountryCode", SqlDbType.VarChar, gC_RN_NKCountryCode);
				command.AddParameter("@GC_RX_NKLocalCurrency", SqlDbType.VarChar, gC_RX_NKLocalCurrency);
				command.ExecuteNonQuery();
			}
		}

		void InsertBranch(Guid pk, string branchCode, Guid companyPk)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES (",
				GlbBranchSchema.Constants.TableName,
				GlbBranchSchema.PK.Name,
				GlbBranchSchema.GB_Code.Name,
				GlbBranchSchema.GB_GC.Name);
			using (DbCommand command = TestConnection.Command(query + "@GB_PK, @GB_Code, @GB_GC)"))
			{
				command.AddParameterBasedOnDbColumn("@GB_PK", pk, GlbBranchSchema.PK);
				command.AddParameterBasedOnDbColumn("@GB_Code", branchCode, GlbBranchSchema.GB_Code);
				command.AddParameterBasedOnDbColumn("@GB_GC", companyPk, GlbBranchSchema.GB_GC);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrg(Guid orgPk, string code, string fullname)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES (",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.PK.Name,
				OrgHeaderSchema.OH_Code.Name,
				OrgHeaderSchema.OH_FullName.Name);
			using (DbCommand command = TestConnection.Command(query + "@OH_PK, @OH_Code, @OH_FullName)"))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", fullname, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgCompanyData(Guid orgPk, Guid companyPk, Guid branchPk)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES (",
				OrgCompanyDataSchema.Constants.TableName,
				OrgCompanyDataSchema.PK.Name,
				OrgCompanyDataSchema.OB_OH.Name,
				OrgCompanyDataSchema.OB_GC.Name,
				OrgCompanyDataSchema.OB_GB_ControllingBranch.Name);
			using (DbCommand command = TestConnection.Command(query + "@OB_PK, @OB_OH, @OB_GC, @OB_GB_ControllingBranch)"))
			{
				command.AddParameterBasedOnDbColumn("@OB_PK", Guid.NewGuid(), OrgCompanyDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@OB_OH", orgPk, OrgCompanyDataSchema.OB_OH);
				command.AddParameterBasedOnDbColumn("@OB_GC", companyPk, OrgCompanyDataSchema.OB_GC);
				command.AddParameterBasedOnDbColumn("@OB_GB_ControllingBranch", branchPk == Guid.Empty ? DBNull.Value : branchPk, OrgCompanyDataSchema.OB_GB_ControllingBranch);
				command.ExecuteNonQuery();
			}
		}

		void InsertStaff(string code, string login, string fullname = "")
		{
			var personPk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')"))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbStaffSchema.PK);
				command.ExecuteNonQuery();
			}

			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}) VALUES (",
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.PK.Name,
				GlbStaffSchema.GS_Code.Name,
				GlbStaffSchema.GS_LoginName.Name,
				GlbStaffSchema.GS_FullName.Name,
				GlbStaffSchema.GS_PER.Name,
				GlbStaffSchema.Constants.GS_SystemCreateTimeUtc,
				GlbStaffSchema.Constants.GS_SystemCreateUser,
				GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc,
				GlbStaffSchema.Constants.GS_SystemLastEditUser);

			using (DbCommand command = TestConnection.Command(query + "newid(), @GS_Code, @GS_LoginName, @GS_FullName, @GS_PER, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@GS_Code", code, GlbStaffSchema.GS_Code);
				command.AddParameterBasedOnDbColumn("@GS_LoginName", login, GlbStaffSchema.GS_LoginName);
				command.AddParameterBasedOnDbColumn("@GS_FullName", string.IsNullOrEmpty(fullname) ? login : fullname, GlbStaffSchema.GS_FullName);
				command.AddParameterBasedOnDbColumn("@GS_PER", personPk, GlbStaffSchema.GS_PER);
				command.ExecuteNonQuery();
			}
		}

		void AddAssignedStaff(string code, string role, Guid orgPk, Guid companyPK)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES (",
				OrgStaffAssignmentsSchema.Constants.TableName,
				OrgStaffAssignmentsSchema.PK.Name,
				OrgStaffAssignmentsSchema.O8_Role.Name,
				OrgStaffAssignmentsSchema.O8_Department.Name,
				OrgStaffAssignmentsSchema.O8_OH.Name,
				OrgStaffAssignmentsSchema.O8_GC.Name,
				OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible.Name);
			using (DbCommand command = TestConnection.Command(query + "newid(), @O8_Role, @O8_Department, @O8_OH, @O8_GC, @O8_GS_NKPersonResponsible)"))
			{
				command.AddParameterBasedOnDbColumn("@O8_Role", role, OrgStaffAssignmentsSchema.O8_Role);
				command.AddParameterBasedOnDbColumn("@O8_Department", "ALL", OrgStaffAssignmentsSchema.O8_Department);
				command.AddParameterBasedOnDbColumn("@O8_OH", orgPk, OrgStaffAssignmentsSchema.O8_OH);
				command.AddParameterBasedOnDbColumn("@O8_GC", companyPK == Guid.Empty ? DBNull.Value : companyPK, OrgStaffAssignmentsSchema.O8_GC);
				command.AddParameterBasedOnDbColumn("@O8_GS_NKPersonResponsible", code, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		string AssignedStaff(int row, DataTable result)
		{
			// Company (blank if global) - organisation - staff
			return string.Concat(result.Rows[row][3].ToString(), " - ", result.Rows[row][8].ToString(), " - ", result.Rows[row][14].ToString(), " - ", result.Rows[row][18].ToString());
		}
	}
}

