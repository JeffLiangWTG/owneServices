using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_CheckForDuplicateRowAndThenInsertToJobChargeRevRecognition))]
	class TG_CheckForDuplicateRowAndThenInsertToJobChargeRevRecognitionTest : DbCreateScriptTest
	{
		public void TestTrigger()
		{
			testHelper = new TestDbHelper(TestConnection);
			var branchPK = testHelper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = testHelper.InsertDepartment("ZZD");
			var jobPK = testHelper.InsertJob("JOB1", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", Guid.NewGuid(), "WRK", DateTime.Now);

			var today = DateTime.Today.Date;
			var now = DateTime.UtcNow.ToSmallDateTimeFloor();
			var jobChargeRecognition = new JobChargeRevRecognition
			{
				D3_JH = jobPK,
				D3_RecognitionDate = today,
				D3_RecognitionType = "JOP",
				D3_SystemCreateTimeUtc = now,
				D3_SystemLastEditTimeUtc = now.AddDays(1),
				D3_SystemCreateUser = "A",
				D3_SystemLastEditUser = "B"
			}.InsertAndReturnObject(TestConnection);
			JobChargeRevRecognition.AssertFromDB(TestConnection, jobChargeRecognition.PK)
				.ExpectEquals(nameof(JobChargeRevRecognition.D3_SystemCreateTimeUtc), jc => jc.D3_SystemCreateTimeUtc, now)
				.ExpectEquals(nameof(JobChargeRevRecognition.D3_SystemLastEditTimeUtc), jc => jc.D3_SystemLastEditTimeUtc, now.AddDays(1))
				.ExpectEquals(nameof(JobChargeRevRecognition.D3_SystemCreateUser), jc => jc.D3_SystemCreateUser, "A")
				.ExpectEquals(nameof(JobChargeRevRecognition.D3_SystemLastEditUser), jc => jc.D3_SystemLastEditUser, "B")
				.VerifyAll();

			AssertNoExceptionThrown("Trying to Insert a Duplicate row. No Exception should be thrown as the trigger will handle it.",
				() => new JobChargeRevRecognition { D3_JH = jobPK, D3_RecognitionDate = today, D3_RecognitionType = "JOP" }.Insert(TestConnection));
			AssertNoExceptionThrown("Trying to Insert a Duplicate row with same Job Header and same Revenue Recognition Type and same Recognition Date but different Hour. No Exception should be thrown as the trigger will handle it.",
				() => new JobChargeRevRecognition { D3_JH = jobPK, D3_RecognitionDate = today.AddHours(1.0), D3_RecognitionType = "JOP" }.Insert(TestConnection));
			AssertNoExceptionThrown("Trying to Insert a row with same Job Header but different Revenue Recognition Type. No Exception should be thrown.",
				() => new JobChargeRevRecognition { D3_JH = jobPK, D3_RecognitionDate = today.AddDays(1), D3_RecognitionType = "JCL" }.Insert(TestConnection));
			AssertExceptionThrown(string.Format("Cannot insert duplicate key row in object 'dbo.JobChargeRevRecognition' with unique index 'FK_UC__D3_JH_D3_RecognitionType'. The duplicate key value is ({0}, JOP).", jobPK.ToString()), typeof(SqlException),
				() => new JobChargeRevRecognition { D3_JH = jobPK, D3_RecognitionDate = today.AddDays(-1), D3_RecognitionType = "JOP" }.Insert(TestConnection));
		}

		class JobChargeRevRecognition : SQLDataObject<JobChargeRevRecognition>
		{
			public JobChargeRevRecognition()
			{
				D3_SystemCreateTimeUtc = DateTime.UtcNow;
				D3_SystemLastEditTimeUtc = DateTime.UtcNow;
				D3_SystemCreateUser = "~BP";
				D3_SystemLastEditUser = "~BP";
			}

			public string D3_RecognitionType { get; set; }
			public DateTime D3_RecognitionDate { get; set; }
			public Guid D3_JH { get; set; }
			public DateTime? D3_SystemCreateTimeUtc { get; set; }
			public DateTime? D3_SystemLastEditTimeUtc { get; set; }
			public string D3_SystemCreateUser { get; set; }
			public string D3_SystemLastEditUser { get; set; }
		}

		TestDbHelper testHelper;
	}
}
