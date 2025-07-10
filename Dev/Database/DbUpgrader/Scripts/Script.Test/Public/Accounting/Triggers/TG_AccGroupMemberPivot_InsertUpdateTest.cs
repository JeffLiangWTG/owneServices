using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccGroupMemberPivot_InsertUpdate))]
	class TG_AccGroupMemberPivot_InsertUpdateTest : DBCreateTriggerScriptTest
	{
		public void TestTriggerForPOSInsert()
		{
			AssertExceptionThrownSafe("Invalid Charge Code PK.", new Action<DbConnection>((connection) => InsertPOSChargeCodeGroupPivot(connection, null, Guid.NewGuid(), null)));
			AssertExceptionThrownSafe("Invalid Charge Code PK.", new Action<DbConnection>((connection) => InsertPOSChargeCodeGroupPivot(connection, null, null, TestDbHelper.OtherCompanyPK)));
		}

		public void TestTriggerForPOSUpdate()
		{
			AssertExceptionThrownSafe("Invalid Charge Code PK.", new Action<DbConnection>((connection) => UpdatePOSChargeCodeGroupPivot(connection, Guid.NewGuid(), null)));
			AssertExceptionThrownSafe("Invalid Charge Code PK.", new Action<DbConnection>((connection) => UpdatePOSChargeCodeGroupPivot(connection, null, TestDbHelper.OtherCompanyPK)));
		}

		void AssertExceptionThrownSafe(string expectedExceptionMessage, Action<DbConnection> codeToRun)
		{
			using (var newConnection = Db.NewExtraConnectionToMainDb())
			{
				newConnection.BeginTransaction();
				AssertExceptionThrown<SqlException>("Should be: " + expectedExceptionMessage, expectedExceptionMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", delegate { codeToRun(newConnection); });
				newConnection.RollbackTransaction();
			}
		}

		Guid InsertPOSChargeCodeGroupPivot(DbConnection connection, Guid? groupPK, Guid? memberPK, Guid? memberCompanyPK)
		{
			Guid pk = Guid.NewGuid();

			var testHelper = new TestDbHelper(connection);
			var actualGroupPK = groupPK ?? testHelper.InsertAccGroup("POS");
			var actualMemberPK = memberPK ?? testHelper.InsertChargeCode(memberCompanyPK, "CC11");

			string sql = @"INSERT INTO dbo.AccGroupMemberPivot (GRP_PK, GRP_GroupType, GRP_GRO_Group, GRP_MemberTableCode, GRP_MemberID, GRP_SystemCreateTimeUtc, GRP_SystemCreateUser, GRP_SystemLastEditTimeUtc, GRP_SystemLastEditUser)
VALUES(@PK, @GroupType, @GroupPK, @MemberPrefix, @MemberPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GroupType", SqlDbType.Char, "POS");
				cmd.AddParameter("@GroupPK", SqlDbType.UniqueIdentifier, actualGroupPK);
				cmd.AddParameter("@MemberPrefix", SqlDbType.VarChar, "AC");
				cmd.AddParameter("@MemberPK", SqlDbType.UniqueIdentifier, actualMemberPK);
				cmd.ExecuteNonQuery();
			}
			return pk;
		}

		void UpdatePOSChargeCodeGroupPivot(DbConnection connection, Guid? memberPK, Guid? memberCompanyPK)
		{
			var pk = Guid.Empty;
			var groupPK = Guid.Empty;
			var newMemberPK = memberPK;
			var testHelper = new TestDbHelper(connection);
			try
			{
				groupPK = testHelper.InsertAccGroup("POS", companyPK: CompanyPK);
				var chargeCodePK = testHelper.InsertChargeCode(CompanyPK, "CCODE");

				pk = InsertPOSChargeCodeGroupPivot(connection, groupPK, chargeCodePK, CompanyPK);
			}
			catch (SqlException ex)
			{
				Fail("Unexpected exception on inserting: " + ex.Message);
			}

			newMemberPK = newMemberPK ?? testHelper.InsertChargeCode(memberCompanyPK, "CC11");

			string sql = @"UPDATE dbo.AccGroupMemberPivot SET GRP_GroupType = @GroupType, GRP_GRO_Group = @GroupPK, GRP_MemberTableCode = @MemberPrefix, GRP_MemberID = @MemberPK, GRP_SystemLastEditTimeUtc = GETUTCDATE(), GRP_SystemLastEditUser = 'TST'
WHERE GRP_PK = @PK";

			using (DbCommand cmd = connection.Command(sql))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@GroupType", SqlDbType.Char, "POS");
				cmd.AddParameter("@GroupPK", SqlDbType.UniqueIdentifier, groupPK);
				cmd.AddParameter("@MemberPrefix", SqlDbType.VarChar, "AC");
				cmd.AddParameter("@MemberPK", SqlDbType.UniqueIdentifier, newMemberPK);
				cmd.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = TestDbHelper.DefaultCompanyPK;
		}

		Guid CompanyPK;
	}
}

