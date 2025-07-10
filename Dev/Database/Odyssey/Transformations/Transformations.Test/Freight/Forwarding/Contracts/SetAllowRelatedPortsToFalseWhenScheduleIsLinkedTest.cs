using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Contracts;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding.Contracts
{
	[TestedType(typeof(SetAllowRelatedPortsToFalseWhenScheduleIsLinked))]
	class SetAllowRelatedPortsToFalseWhenScheduleIsLinkedTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new SetAllowRelatedPortsToFalseWhenScheduleIsLinked();

		protected override void PrepareTestData()
		{
			DataCreator = new TransformationTestDataCreator();
			DBTransformationTestHelper.DropConstraintIfExists(RatingContractAllocationLineSchema.Constants.TableName, "RatingContractAllocationLine_RCA_JX_SailingSchedule_FK2_JobSailing_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(RatingContractAllocationLineSchema.Constants.TableName, "Constraint_RCA_AllowRelatedPorts__RCA_JX_SailingSchedule");

			var serviceProviderPK = DataCreator.CreateOrg("ORG");
			var ratingContractPK = DataCreator.CreateRatingContract("Contract1", serviceProviderPK, "PRO", "SEA");

			foreach (var testCase in testCases)
			{
				InsertRatingContractAllocationLineTestCase(testCase, ratingContractPK);
			}
		}

		protected override void AssertTransformationResults()
		{
			foreach (var testCaseToCheck in testCases)
			{
				var selectTransformationResultCommand = "SELECT RCA_AllowRelatedPorts FROM dbo.RatingContractAllocationLine WHERE RCA_PK = @RCA_PK";

				using (var command = Db.Connection.Command(selectTransformationResultCommand))
				{
					command.AddParameter("@RCA_PK", System.Data.SqlDbType.UniqueIdentifier, testCaseToCheck.PK);
					var result = command.ExecuteScalar();
					AssertEquals(testCaseToCheck.ExpectedResult, result);
				}
			}
		}

		void InsertRatingContractAllocationLineTestCase(RatingContractAllocationLine testCaseToInsert, Guid ratingContractPK)
		{
			var insertRatingContractAllocationLineTestCaseSql = @"INSERT INTO dbo.RatingContractAllocationLine (RCA_PK, RCA_AllocationLineID, RCA_RCT_RatingContract, RCA_AllocatedQuantity, RCA_AllocatedUQ, RCA_JX_SailingSchedule, RCA_AllowRelatedPorts, RCA_SystemCreateTimeUtc, RCA_SystemCreateUser, RCA_SystemLastEditTimeUtc, RCA_SystemLastEditUser)
				VALUES (@RCA_PK, @RCA_AllocationLineID, @RCA_RCT_RatingContract, 1, 'TU', @RCA_JX_SailingSchedule, @RCA_AllowRelatedPorts, GetUtcDate(), 'AET', GetUtcDate(), 'AET')";

			using (var command = Db.Connection.Command(insertRatingContractAllocationLineTestCaseSql))
			{
				command.AddParameter("@RCA_PK", System.Data.SqlDbType.UniqueIdentifier, testCaseToInsert.PK);
				command.AddParameter("@RCA_AllocationLineID", System.Data.SqlDbType.VarChar, testCaseToInsert.AllocationLine);
				command.AddParameter("@RCA_RCT_RatingContract", System.Data.SqlDbType.UniqueIdentifier, ratingContractPK);
				command.AddParameter("@RCA_JX_SailingSchedule", System.Data.SqlDbType.UniqueIdentifier, testCaseToInsert.LinkedSchedule != null ? testCaseToInsert.LinkedSchedule : DBNull.Value);
				command.AddParameter("@RCA_AllowRelatedPorts", System.Data.SqlDbType.Bit, testCaseToInsert.AllowRelatedPorts);
				command.ExecuteNonQuery();
			}
		}

		readonly List<RatingContractAllocationLine> testCases = new List<RatingContractAllocationLine>
		{
			new RatingContractAllocationLine(true, Guid.NewGuid(), "00000001", false),
			new RatingContractAllocationLine(false, Guid.NewGuid(), "00000002", false),
			new RatingContractAllocationLine(true, null, "00000003", true),
			new RatingContractAllocationLine(false, null, "00000004", false),
		};

		TransformationTestDataCreator DataCreator;

		class RatingContractAllocationLine
		{
			internal RatingContractAllocationLine(bool allowRelatedPorts, Guid? linkedSchedule, string allocationLineID, bool expectedResult)
			{
				PK = Guid.NewGuid();
				AllowRelatedPorts = allowRelatedPorts;
				AllocationLine = allocationLineID;
				LinkedSchedule = linkedSchedule;
				ExpectedResult = expectedResult;
			}

			public Guid PK { get; }
			public bool AllowRelatedPorts { get; }
			public string AllocationLine { get; }
			public Guid? LinkedSchedule { get; }
			public bool ExpectedResult { get; }
		}
	}
}
