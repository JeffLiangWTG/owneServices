using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding
{
	[TestedType(typeof(UpdateTableJobMawbColumnJM_GC_Company))]
	public class UpdateTableJobMawbColumnJM_GC_CompanyTest : DataTransformationTestCase
	{
		Guid companyPk1, mawbPk1, mawbPk2, branchPk1;
		string companyCode1, companyCode2, companyCode3, companyCode4, branchCode1, branchCode2, branchCode3, branchCode4;

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();

			companyCode1 = "USC";
			companyCode2 = "AUC";
			companyCode3 = "UGH";
			companyCode4 = "UGF";

			branchCode1 = "USB";
			branchCode2 = "AUB";
			branchCode3 = "UGG";
			branchCode4 = "UGK";

			companyPk1 = testDataCreator.CreateGlbCompany(companyCode1, "US");
			branchPk1 = testDataCreator.CreateGlbBranch(branchCode1, companyPk1);
			mawbPk1 = CreateJobMawb("919", "12345678", branchPk1);
			mawbPk2 = CreateJobMawb("618", "00001111", null);
		}

		protected override void AssertTransformationResults()
		{
			var testDataCreator = new TransformationTestDataCreator();

			AssertCompanyId(mawbPk1, companyPk1);
			AssertCompanyId(mawbPk2, DBNull.Value);

			var companyPk2 = testDataCreator.CreateGlbCompany(companyCode2, "AU");
			var branchPk2 = testDataCreator.CreateGlbBranch(branchCode2, companyPk2);

			var mawbPk3 = CreateJobMawb("618", "11112222", branchPk2);
			AssertCompanyId(mawbPk3, companyPk2);

			UpdateJobMawbBranch(mawbPk3, null);
			AssertCompanyId(mawbPk3, DBNull.Value);

			var companyPk3 = testDataCreator.CreateGlbCompany(companyCode3, "NZ");
			var branchPk3 = testDataCreator.CreateGlbBranch(branchCode3, companyPk3);

			var companyPk4 = testDataCreator.CreateGlbCompany(companyCode4, "NZ");
			var branchPk4 = testDataCreator.CreateGlbBranch(branchCode4, companyPk4);

			var mawbPk4 = CreateJobMawb("618", "11112225", branchPk3);
			AssertCompanyId(mawbPk4, companyPk3);

			UpdateJobMawbBranch(mawbPk4, branchPk4);
			AssertCompanyId(mawbPk4, companyPk4);

			companyCode2 = "EEK";
			companyCode3 = "MKM";
			companyCode4 = "ABC";

			branchCode2 = "HAT";
			branchCode3 = "CAP";
			branchCode4 = "TIP";

			ResetMawbDates();
		}

		void AssertCompanyId(Guid mawbPk, object expectedCompanyId)
		{
			var companyId = Db.Connection.ExecuteScalar($"SELECT JM_GC_Company FROM JobMawb WHERE JM_PK = '{mawbPk}'");
			AssertEquals(expectedCompanyId, companyId);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateTableJobMawbColumnJM_GC_Company();

		const string sqlInsert = @"
IF EXISTS (SELECT 1 
           FROM sys.check_constraints 
           WHERE name = 'Constraint_JM_GB_JM_GC_Company' 
           AND parent_object_id = OBJECT_ID('dbo.JobMawb'))
BEGIN
    ALTER TABLE dbo.JobMawb 
    NOCHECK CONSTRAINT Constraint_JM_GB_JM_GC_Company;
END

INSERT INTO JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_GB, JM_SystemCreateTimeUtc, JM_SystemCreateUser, JM_SystemLastEditTimeUtc, JM_SystemLastEditUser)
VALUES (@pk, @airline3DigitPrefix, @mawb, @branchID, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobMawb(string airline3DigitPrefix, string mawb, Guid? branchID)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sqlInsert))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@airline3DigitPrefix", SqlDbType.VarChar, airline3DigitPrefix);
				command.AddParameter("@mawb", SqlDbType.VarChar, mawb);
				command.AddParameter("@branchID", SqlDbType.UniqueIdentifier, branchID != null ? branchID : DBNull.Value);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string sqlUpdate = @"
UPDATE JobMawb
SET JM_GB = @branchID,
	JM_SystemLastEditUser = '~BP',
	JM_SystemLastEditTimeUtc = GETUTCDATE()
WHERE JM_PK = @pk";

		public void UpdateJobMawbBranch(Guid pk, Guid? branchID)
		{
			using (var command = Db.Connection.Command(sqlUpdate))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branchID", SqlDbType.UniqueIdentifier, branchID != null ? branchID : DBNull.Value);
				command.ExecuteNonQuery();
			}
		}

		const string resetMawbDatesSql = @"
UPDATE JobMawb
SET JM_SystemCreateTimeUtc = dateadd(year, -2, GETUTCDATE()),
JM_SystemLastEditUser = '~BP',
JM_SystemLastEditTimeUtc = GETUTCDATE()";

		public void ResetMawbDates()
		{
			Db.Connection.ExecuteNonQuery(resetMawbDatesSql);
		}
	}
}
