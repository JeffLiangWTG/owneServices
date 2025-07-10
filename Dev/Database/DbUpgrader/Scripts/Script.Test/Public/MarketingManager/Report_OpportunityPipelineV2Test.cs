using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(Report_OpportunityPipelineV2))]
	class Report_OpportunityPipelineV2Test : DbCreateScriptTest
	{
		public void TestSingleRecordWhenMoreThanOneOrgStaffAssignmentAsACT()
		{
			Guid orgAAA = Guid.NewGuid();
			Guid orgBBB = Guid.NewGuid();
			Guid orgODD = Guid.NewGuid();
			InsertOrg(orgAAA, "AAA");
			InsertOrg(orgBBB, "BBB");
			InsertOrg(orgODD, "ODD");
			Guid act1 = Guid.NewGuid();
			Guid act2 = Guid.NewGuid();
			InsertStaff(act1, "ONE", "one.act");
			InsertStaff(act2, "TWO", "two.act");
			AddAssignedStaff("ONE", "ACT", "SEA", orgAAA);
			AddAssignedStaff("ONE", "ACT", "FRT", orgAAA);
			AddAssignedStaff("ONE", "ACT", "SEA", orgBBB);
			AddAssignedStaff("TWO", "ACT", "FRT", orgBBB);
			InsertOrgOpportunity(orgAAA, "OppId1");
			InsertOrgOpportunity(orgBBB, "OppId2");
			InsertOrgOpportunity(orgODD, "OppId3"); // Odd org that has no staff assignments

			string query = "SELECT * FROM Report_OpportunityPipelineV2(@AccountManagerPK)";

			using (DbCommand command = TestConnection.Command(query + " ORDER BY Client"))
			{
				command.AddParameter("@AccountManagerPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Should return 3 results, not 5", 3, result.Rows.Count);
				AssertEquals("AAA", result.Rows[0][3].ToString());
				AssertEquals("BBB", result.Rows[1][3].ToString());
				AssertEquals("ODD", result.Rows[2][3].ToString());
			}

			// The report will automatically apply the WHERE clause when filters are applied
			using (DbCommand command = TestConnection.Command(query + " WHERE AccountManagerPK = @AccountManagerPK ORDER BY Client"))
			{
				command.AddParameter("@AccountManagerPK", SqlDbType.UniqueIdentifier, act1);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Should return 2 results, not 3", 2, result.Rows.Count);
				AssertEquals("AAA", result.Rows[0][3].ToString());
				AssertEquals("BBB", result.Rows[1][3].ToString());
			}

			using (DbCommand command = TestConnection.Command(query + " WHERE AccountManagerPK = @AccountManagerPK ORDER BY Client"))
			{
				command.AddParameter("@AccountManagerPK", SqlDbType.UniqueIdentifier, act2);
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Should return 1 result", 1, result.Rows.Count);
				AssertEquals("BBB", result.Rows[0][3].ToString());
			}
		}

		void InsertOrg(Guid pk, string code)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.PK.Name,
				OrgHeaderSchema.OH_Code.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OH_PK, @OH_Code)"))
			{
				command.AddParameter("@OH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@OH_Code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}
		}

		void InsertStaff(Guid pk, string code, string login)
		{
			var personPk = Guid.NewGuid();
			var queryPerson = string.Format("insert into dbo.GlbPerson (PER_PK, PER_FullName) values (@PER_PK, 'name')");
			using (DbCommand command = TestConnection.Command(queryPerson))
			{
				command.AddParameterBasedOnDbColumn("@PER_PK", personPk, GlbPersonSchema.PK);
				command.ExecuteNonQuery();
			}

			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES ",
				GlbStaffSchema.Constants.TableName,
				GlbStaffSchema.PK.Name,
				GlbStaffSchema.GS_Code.Name,
				GlbStaffSchema.GS_LoginName.Name,
				GlbStaffSchema.GS_PER.Name,
				GlbStaffSchema.Constants.GS_SystemCreateTimeUtc,
				GlbStaffSchema.Constants.GS_SystemCreateUser,
				GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc,
				GlbStaffSchema.Constants.GS_SystemLastEditUser);

			using (DbCommand command = TestConnection.Command(query + "(@GS_PK, @GS_Code, @GS_LoginName, @GS_PER, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				command.AddParameterBasedOnDbColumn("@GS_PK", pk, GlbStaffSchema.PK);
				command.AddParameterBasedOnDbColumn("@GS_Code", code, GlbStaffSchema.GS_Code);
				command.AddParameterBasedOnDbColumn("@GS_LoginName", login, GlbStaffSchema.GS_LoginName);
				command.AddParameterBasedOnDbColumn("@GS_PER", personPk, GlbStaffSchema.GS_PER);
				command.ExecuteNonQuery();
			}
		}

		void AddAssignedStaff(string code, string role, string department, Guid orgPk)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}) VALUES ",
				OrgStaffAssignmentsSchema.Constants.TableName,
				OrgStaffAssignmentsSchema.PK.Name,
				OrgStaffAssignmentsSchema.O8_Role.Name,
				OrgStaffAssignmentsSchema.O8_Department.Name,
				OrgStaffAssignmentsSchema.O8_OH.Name,
				OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible.Name);
			using (DbCommand command = TestConnection.Command(query + "(newid(), @O8_Role, @O8_Department, @O8_OH, @O8_GS_NKPersonResponsible)"))
			{
				command.AddParameterBasedOnDbColumn("@O8_Role", role, OrgStaffAssignmentsSchema.O8_Role);
				command.AddParameterBasedOnDbColumn("@O8_Department", department, OrgStaffAssignmentsSchema.O8_Department);
				command.AddParameterBasedOnDbColumn("@O8_OH", orgPk, OrgStaffAssignmentsSchema.O8_OH);
				command.AddParameterBasedOnDbColumn("@O8_GS_NKPersonResponsible", code, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgOpportunity(Guid orgPk, string opportunityId)
		{
			string query = @"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES (newid(), @P8_GC, @P8_OH, @P8_OpportunityId, GetUtcDate(), 'E', GetUtcDate(), 'E')";

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPk, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_GC", GlbCompanyPk, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityId", opportunityId, OrgOpportunitySchema.P8_OpportunityID);

				command.ExecuteNonQuery();
			}
		}

		Guid GlbCompanyPk
		{
			get
			{
				if (!glbCompanyPk.HasValue)
				{
					glbCompanyPk = Guid.NewGuid();
					var insertSql = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'DAN', 'AU company', 'AU', 'AUD')";
					using (var command = TestConnection.Command(insertSql))
					{
						command.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, glbCompanyPk);
						command.ExecuteNonQuery();
					}
				}

				return glbCompanyPk.Value;
			}
		}
		Guid? glbCompanyPk;
	}
}

