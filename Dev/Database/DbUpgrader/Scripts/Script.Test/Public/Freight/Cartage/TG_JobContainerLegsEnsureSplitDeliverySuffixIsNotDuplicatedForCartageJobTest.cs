using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Cartage;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Freight.Cartage
{
	[TestedType(typeof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob))]
	class TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTest : DBCreateTriggerScriptTest
	{
	}

	[TestedType(typeof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob))]
	class Trigger_TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTest : TestCase
	{
		const string TriggerName = nameof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob);
		const string CheckStoredProcName = nameof(JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob);
		const string LegCountSql = @"
SELECT COUNT(*)
	FROM dbo.JobContainerLegs
	JOIN dbo.JobBookedCtgMove ON JU_EW = EW_PK
	JOIN dbo.JobCartage ON EW_JJ = JJ_PK
	WHERE JJ_PK = @cartageJobPK";
		const string TriggerErrorMessage = "TriggerLikelyConcurrencyError: Attempted to insert duplicate suffix on cartage job leg(s).";

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTriggerPreventsDuplicateLegCreation()
		{
			var companyPK = Guid.Empty;
			var branchPK = Guid.Empty;
			var cartageJobPK = Guid.Empty;
			var bookedMove1PK = Guid.Empty;

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				companyPK = TestDataCreator.CreateCompany("CX1", "AU", "AUD");
				branchPK = TestDataCreator.CreateBranch(companyPK, "BR1", "AUXXX");

				cartageJobPK = TestDataCreator.CreateJobCartage(branchPK, "T1");
				bookedMove1PK = TestDataCreator.CreateJobBookedCtgMove(cartageJobPK);
				TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "A");

				manager.CommitTransaction();
			}

			var currentLegs = 0;
			using (var command = Db.Connection.Command(LegCountSql))
			{
				command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
				currentLegs = (int)command.ExecuteScalar();
			}
			AssertEquals("Precondition: cartage job should have 1 leg", 1, currentLegs);

			TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
			AssertInnermostException(
				message: "This should fail as we are attempting to insert a leg with a duplicate JU_SplitDeliverySuffix",
				expectedTypeOfException: typeof(SqlException),
				expectedExceptionMessage: TriggerErrorMessage,
				codeToRun: () =>
				{
					using (var manager = Db.Connection.BeginTransactionWithManager())
					{
						TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "A");
						manager.CommitTransaction();
					}
				},
				assertMessageStartsWith: true);

			using (var command = Db.Connection.Command(LegCountSql))
			{
				command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
				currentLegs = (int)command.ExecuteScalar();
			}
			AssertEquals("Cartage job should still have only 1 leg as trigger prevented duplicate", 1, currentLegs);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTriggerPreventsDuplicateLegCreationFromOtherMovesInSameCartageJob()
		{
			var companyPK = Guid.Empty;
			var branchPK = Guid.Empty;
			var cartageJobPK = Guid.Empty;
			var bookedMove1PK = Guid.Empty;
			var bookedMove2PK = Guid.Empty;

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				companyPK = TestDataCreator.CreateCompany("CX1", "AU", "AUD");
				branchPK = TestDataCreator.CreateBranch(companyPK, "BR1", "AUXXX");

				cartageJobPK = TestDataCreator.CreateJobCartage(branchPK, "T1");
				bookedMove1PK = TestDataCreator.CreateJobBookedCtgMove(cartageJobPK);
				TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "A");
				TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "B");

				bookedMove2PK = TestDataCreator.CreateJobBookedCtgMove(cartageJobPK);
				TestDataCreator.CreateJobContainerLeg(bookedMove2PK, "C");
				TestDataCreator.CreateJobContainerLeg(bookedMove2PK, "D");

				manager.CommitTransaction();
			}

			var currentLegs = 0;
			using (var command = Db.Connection.Command(LegCountSql))
			{
				command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
				currentLegs = (int)command.ExecuteScalar();
			}
			AssertEquals("Precondition: cartage job should have 4 leg", 4, currentLegs);

			TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
			AssertInnermostException(
				message: "This should fail as we are attempting to insert legs on bookedMove1 with duplicate JU_SplitDeliverySuffix to the legs on bookedMove2",
				expectedTypeOfException: typeof(SqlException),
				expectedExceptionMessage: TriggerErrorMessage,
				codeToRun: () =>
				{
					using (var manager = Db.Connection.BeginTransactionWithManager())
					{
						TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "C");
						TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "D");
						manager.CommitTransaction();
					}
				},
				assertMessageStartsWith: true);

			using (var command = Db.Connection.Command(LegCountSql))
			{
				command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
				currentLegs = (int)command.ExecuteScalar();
			}
			AssertEquals("Cartage job should still have only 4 legs as trigger prevented creating duplicates", 4, currentLegs);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestTriggerSuspensionWorks()
		{
			var companyPK = Guid.Empty;
			var branchPK = Guid.Empty;
			var cartageJobPK = Guid.Empty;
			var bookedMove1PK = Guid.Empty;

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				companyPK = TestDataCreator.CreateCompany("CX1", "AU", "AUD");
				branchPK = TestDataCreator.CreateBranch(companyPK, "BR1", "AUXXX");

				cartageJobPK = TestDataCreator.CreateJobCartage(branchPK, "T1");
				bookedMove1PK = TestDataCreator.CreateJobBookedCtgMove(cartageJobPK);
				TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "A");

				manager.CommitTransaction();
			}

			var currentLegs = 0;
			using (var command = Db.Connection.Command(LegCountSql))
			{
				command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
				currentLegs = (int)command.ExecuteScalar();
			}
			AssertEquals("Precondition: cartage job should have 1 leg", 1, currentLegs);

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				var duplicateLegPK = Guid.Empty;
				using (TestWhsDataSetupHelper.SuspendTrigger(TriggerName, JobContainerLegsSchema.Constants.TableName, Db.Connection))
				{
					AssertNoExceptionThrown(() =>
					{
						duplicateLegPK = TestDataCreator.CreateJobContainerLeg(bookedMove1PK, "A");
					});

					using (var command = Db.Connection.Command(LegCountSql))
					{
						command.AddParameter("@cartageJobPK", SqlDbType.UniqueIdentifier, cartageJobPK);
						currentLegs = (int)command.ExecuteScalar();
					}
					AssertEquals("Cartage job should have 2 legs as trigger was suspended so did not prevent duplicate yet", 2, currentLegs);

					var execStoredProcSql = FormattableString.Invariant($"EXEC {CheckStoredProcName} @TVP_LegPKsToCheck");
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
					AssertInnermostException(
						"Check stored proc should detect duplicate leg suffix and raise error",
						typeof(SqlException),
						TriggerErrorMessage,
						() =>
						{
							using (var command = Db.Connection.Command(execStoredProcSql))
							{
								command.AddTableValuedParameter("@TVP_LegPKsToCheck", JobContainerLegsSchema.PK, new Guid[] { duplicateLegPK });
								command.ExecuteNonQuery();
							}
						});

					transactionManager.RollbackTransaction();
				}
			}
		}
	}
}
