using System;
using System.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Cartage;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Freight.Cartage.Testing
{
	[TestedType(typeof(DeduplicateCartageLegByJobAndSplitDeliverySuffix))]
	class DeduplicateCartageLegByJobAndSplitDeliverySuffixTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeduplicateCartageLegByJobAndSplitDeliverySuffix();
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var userCode = "~BP";
			var companyPK = Guid.NewGuid();
			var companyCode = "CMP";
			var countryCode = "AU";
			var currencyCode = "AUD";

			var branchPK = Guid.NewGuid();
			var branchCode = "BR1";
			var homePortCode = "AUSYD";

			var cartageJobT1PK = Guid.NewGuid();
			var jobT1Move1PK = Guid.NewGuid();
			var jobT1Move2PK = Guid.NewGuid();
			var jobT1Move3PK = Guid.NewGuid();
			JobT1Move1Leg1PK = Guid.NewGuid();
			JobT1Move1Leg2PK = Guid.NewGuid();
			JobT1Move2Leg1PK = Guid.NewGuid();
			JobT1Move2Leg2PK = Guid.NewGuid();
			JobT1Move3Leg1PK = Guid.NewGuid();
			JobT1Move3Leg2PK = Guid.NewGuid();

			var cartageJobT2PK = Guid.NewGuid();
			var jobT2Move1PK = Guid.NewGuid();
			var jobT2Move2PK = Guid.NewGuid();
			var jobT2Move3PK = Guid.NewGuid();
			JobT2Move1Leg1PK = Guid.NewGuid();
			JobT2Move1Leg2PK = Guid.NewGuid();
			JobT2Move2Leg1PK = Guid.NewGuid();
			JobT2Move2Leg2PK = Guid.NewGuid();
			JobT2Move3Leg1PK = Guid.NewGuid();
			JobT2Move3Leg2PK = Guid.NewGuid();

			var sql = FormattableString.Invariant(@$"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_SystemCreateUser, GC_SystemCreateTimeUtc, GC_SystemLastEditUser, GC_SystemLastEditTimeUtc)
VALUES ('{companyPK}', '{companyCode}', 'AU company', '{countryCode}', '{currencyCode}', '{userCode}', GETUTCDATE(), '{userCode}', GETUTCDATE());

INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC, GB_RN_NKCountryCode, GB_SystemCreateUser, GB_SystemCreateTimeUtc, GB_SystemLastEditUser, GB_SystemLastEditTimeUtc)
VALUES ('{branchPK}', '{branchCode}', '{homePortCode}', '{companyPK}', '{countryCode}', '{userCode}', GETUTCDATE(), '{userCode}', GETUTCDATE());

-- job T1 with 3 booked moves each with 2 legs, none duplicate
INSERT INTO dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser)
VALUES ('{cartageJobT1PK}', '{branchPK}', '{CartageJobT1ConsignmentID}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT1Move1PK}', '{cartageJobT1PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move1Leg1PK}', '{jobT1Move1PK}', 'A', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move1Leg2PK}', '{jobT1Move1PK}', 'B', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT1Move2PK}', '{cartageJobT1PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move2Leg1PK}', '{jobT1Move2PK}', 'C', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move2Leg2PK}', '{jobT1Move2PK}', 'D', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT1Move3PK}', '{cartageJobT1PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move3Leg1PK}', '{jobT1Move3PK}', 'E', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT1Move3Leg2PK}', '{jobT1Move3PK}', 'F', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

-- job T2 with 3 booked moves each with 2 legs, some duplicate
INSERT INTO dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser)
VALUES ('{cartageJobT2PK}', '{branchPK}', '{CartageJobT2ConsignmentID}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT2Move1PK}', '{cartageJobT2PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move1Leg1PK}', '{jobT2Move1PK}', 'A', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move1Leg2PK}', '{jobT2Move1PK}', 'B', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT2Move2PK}', '{cartageJobT2PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move2Leg1PK}', '{jobT2Move2PK}', 'C', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move2Leg2PK}', '{jobT2Move2PK}', 'D', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser)
VALUES ('{jobT2Move3PK}', '{cartageJobT2PK}', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move3Leg1PK}', '{jobT2Move3PK}', 'C', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');

INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
VALUES ('{JobT2Move3Leg2PK}', '{jobT2Move3PK}', 'D', GETUTCDATE(), '{userCode}', GETUTCDATE(), '{userCode}');
");
			TestConnection.ExecuteNonQuery(SuspendJobContainerLegsTriggerIfExistsSql);
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();
			TestConnection.ExecuteNonQuery(ResumeJobContainerLegsTriggerIfExistsAndSuspendedSql);

			CombineAssertions("Checks that transform ran correctly (should do nothing so duplicates should remain - will be sorted now by DeduplicateCartageLegByJobAndSplitDeliverySuffixV2)", () =>
			{
				AssertUnchangedLegSuffix(JobT1Move1Leg1PK, "A", CartageJobT1ConsignmentID);
				AssertUnchangedLegSuffix(JobT1Move1Leg2PK, "B", CartageJobT1ConsignmentID);
				AssertUnchangedLegSuffix(JobT1Move2Leg1PK, "C", CartageJobT1ConsignmentID);
				AssertUnchangedLegSuffix(JobT1Move2Leg2PK, "D", CartageJobT1ConsignmentID);
				AssertUnchangedLegSuffix(JobT1Move3Leg1PK, "E", CartageJobT1ConsignmentID);
				AssertUnchangedLegSuffix(JobT1Move3Leg2PK, "F", CartageJobT1ConsignmentID);

				AssertUnchangedLegSuffix(JobT2Move1Leg1PK, "A", CartageJobT2ConsignmentID);
				AssertUnchangedLegSuffix(JobT2Move1Leg2PK, "B", CartageJobT2ConsignmentID);
				AssertUnchangedLegSuffix(JobT2Move2Leg1PK, "C", CartageJobT2ConsignmentID);
				AssertUnchangedLegSuffix(JobT2Move2Leg2PK, "D", CartageJobT2ConsignmentID);
				AssertUnchangedLegSuffix(JobT2Move3Leg1PK, "C", CartageJobT2ConsignmentID);
				AssertUnchangedLegSuffix(JobT2Move3Leg2PK, "D", CartageJobT2ConsignmentID);
			});
		}

		void AssertUnchangedLegSuffix(Guid legPK, string unchangedLegSuffix, string cartageJobConsignmentID)
		{
			var command = TestConnection.Command(CheckLegSuffixSql);
			command.AddParameter("@legPK", SqlDbType.UniqueIdentifier, legPK);
			var currentLegSuffix = (string)command.ExecuteScalar();

			AssertEquals(FormattableString.Invariant($"Leg on cartage job {cartageJobConsignmentID} should have unchanged JU_SplitDeliverySuffix '{unchangedLegSuffix}'"), unchangedLegSuffix, currentLegSuffix);
		}

		Guid JobT1Move1Leg1PK { get; set; }
		Guid JobT1Move1Leg2PK { get; set; }
		Guid JobT1Move2Leg1PK { get; set; }
		Guid JobT1Move2Leg2PK { get; set; }
		Guid JobT1Move3Leg1PK { get; set; }
		Guid JobT1Move3Leg2PK { get; set; }
		Guid JobT2Move1Leg1PK { get; set; }
		Guid JobT2Move1Leg2PK { get; set; }
		Guid JobT2Move2Leg1PK { get; set; }
		Guid JobT2Move2Leg2PK { get; set; }
		Guid JobT2Move3Leg1PK { get; set; }
		Guid JobT2Move3Leg2PK { get; set; }
		const string CartageJobT1ConsignmentID = "T1";
		const string CartageJobT2ConsignmentID = "T2";
		const string CheckLegSuffixSql = "SELECT JU_SplitDeliverySuffix FROM dbo.JobContainerLegs WHERE JU_PK = @legPK";
		const string SuspendJobContainerLegsTriggerIfExistsSql = @"IF OBJECT_ID('dbo.TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob') IS NOT NULL
BEGIN
	EXEC dbo.SuspendTrigger 'TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob'
END";
		const string ResumeJobContainerLegsTriggerIfExistsAndSuspendedSql = @"IF OBJECT_ID('dbo.TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob') IS NOT NULL
BEGIN
	DECLARE @IsTriggerSuspended bit;
	SELECT @IsTriggerSuspended = Result FROM dbo.IsTriggerSuspended('TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob');
	IF (@IsTriggerSuspended = 1)
	BEGIN
		EXEC dbo.ResumeTrigger 'TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob';
	END
END";
	}
}
