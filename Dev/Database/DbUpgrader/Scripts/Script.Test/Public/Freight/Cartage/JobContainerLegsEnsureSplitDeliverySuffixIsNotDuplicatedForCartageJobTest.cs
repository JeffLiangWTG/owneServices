using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Cartage;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Cartage.Testing
{
	[TestedType(typeof(JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob))]
	class JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTest : DbCreateScriptTest
	{
		const string TriggerErrorMessage = "TriggerLikelyConcurrencyError: Attempted to insert duplicate suffix on cartage job leg(s).";
		public void TestOneDuplicateInputLegRaisesError()
		{
			SetupBaseTestData();

			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob), JobContainerLegsSchema.Constants.TableName, TestConnection))
			{
				var newDuplicateLegPK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "A");

				TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
				AssertExceptionThrown<SqlException>(
						"Stored Proc should throw exception with correct message as input was duplicate leg",
						TriggerErrorMessage,
						() => ExecuteProc(newDuplicateLegPK));
			}
		}

		public void TestMultipleDuplicateInputLegsRaisesError()
		{
			SetupBaseTestData();

			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob), JobContainerLegsSchema.Constants.TableName, TestConnection))
			{
				var newDuplicateLeg1PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "A");
				var newDuplicateLeg2PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "B");

				TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
				AssertExceptionThrown<SqlException>(
						"Stored Proc should throw exception with correct message as duplicate leg found among inputs",
						TriggerErrorMessage,
						() => ExecuteProc(newDuplicateLeg1PK, newDuplicateLeg2PK));
			}
		}

		public void TestDuplicateInputLegsFromOtherMovesInSameCartageJobRaisesError()
		{
			SetupBaseTestData();
			var bookedMove2PK = Guid.Empty;

			bookedMove2PK = TestDataCreator.CreateJobBookedCtgMove(CartageJobPK);
			TestDataCreator.CreateJobContainerLeg(bookedMove2PK, "C");
			TestDataCreator.CreateJobContainerLeg(bookedMove2PK, "D");

			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob), JobContainerLegsSchema.Constants.TableName, TestConnection))
			{
				var newDuplicateLeg1PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "C");
				var newDuplicateLeg2PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "D");

				TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors((Exception ex) => ex.Message == TriggerErrorMessage);
				AssertExceptionThrown<SqlException>(
						"Stored Proc should throw exception with correct message as duplicate leg found among inputs when checking other moves in the same cartage job",
						TriggerErrorMessage,
						() => ExecuteProc(newDuplicateLeg1PK, newDuplicateLeg2PK));
			}
		}

		public void TestNoDuplicateInputLegDoesNotRaiseError()
		{
			SetupBaseTestData();

			using (TestWhsDataSetupHelper.SuspendTrigger(nameof(TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob),
JobContainerLegsSchema.Constants.TableName, TestConnection))
			{
				var newNonDuplicateLeg1PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "C");
				var newNonDuplicateLeg2PK = TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "D");
				AssertNoExceptionThrown(
						"Stored Proc should not throw exception as no duplicate leg found among inputs",
						() => ExecuteProc(newNonDuplicateLeg1PK, newNonDuplicateLeg2PK));
			}
		}

		void ExecuteProc(params Guid[] legPKs)
		{
			using (var command = Db.Connection.Command("JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddTableValuedParameter("@JobContainerLegsPKs", "dbo.TVP_uniqueidentifier", legPKs);

				command.ExecuteNonQuery();
			}
		}

		void SetupBaseTestData()
		{
			var companyPK = TestDataCreator.CreateCompany("CX1", "AU", "AUD");
			BranchPK = TestDataCreator.CreateBranch(companyPK, "BR1", "AUXXX");

			CartageJobPK = TestDataCreator.CreateJobCartage(BranchPK, "T1");
			BookedMove1PK = TestDataCreator.CreateJobBookedCtgMove(CartageJobPK);
			TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "A");
			TestDataCreator.CreateJobContainerLeg(BookedMove1PK, "B");
		}

		Guid BranchPK { get; set; }
		Guid CartageJobPK { get; set; }
		Guid BookedMove1PK { get; set; }
	}
}
