using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(SetContainerWeightLimitTypeToAdjustedCodes))]
	public class SetContainerWeightLimitTypeToAdjustedCodesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new SetContainerWeightLimitTypeToAdjustedCodes();
		}

		Guid allocationRoute_ABS;
		Guid allocationRoute_AVG;
		Guid allocationRoute_Empty;

		protected override void PrepareTestData()
		{
			var dataCreator = new TransformationTestDataCreator();

			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE RatingContractAllocationLine DROP CONSTRAINT Constraint_RCA_ContainerWeightLimitType");
			TestConnection.ExecuteNonQuery(sql.ToString());

			var serviceProviderPK = dataCreator.CreateOrg("ORG");
			var ratingContractPK = dataCreator.CreateRatingContract("Contract1", serviceProviderPK, "PRO", "SEA");

			allocationRoute_ABS = InsertRatingContractAllocationLine("AAA", 123, "KG", "ABS", ratingContractPK);
			allocationRoute_AVG = InsertRatingContractAllocationLine("BBB", 123, "KG", "AVG", ratingContractPK);
			allocationRoute_Empty = InsertRatingContractAllocationLine("CCC", 0, "", "", ratingContractPK);
		}

		Guid InsertRatingContractAllocationLine(string allocationLineID, decimal containerWeightLimit, string containerWeightLimitUnit, string containerWeightLimitType, Guid contractPK)
		{
			var insertRatingContractAllocationLineTestCaseSql = @"INSERT INTO
	dbo.RatingContractAllocationLine (
		RCA_PK,
		RCA_AllocationLineID,
		RCA_RCT_RatingContract,
		RCA_ContainerWeightLimit,
		RCA_ContainerWeightLimitUQ,
		RCA_ContainerWeightLimitType,
		RCA_AllocatedQuantity,
		RCA_AllocatedUQ,
		RCA_SystemCreateTimeUtc,
		RCA_SystemCreateUser,
		RCA_SystemLastEditTimeUtc,
		RCA_SystemLastEditUser
	)
VALUES
	(
		@RCA_PK,
		@RCA_AllocationLineID,
		@RCA_RCT_RatingContract,
		@RCA_ContainerWeightLimit,
		@RCA_ContainerWeightLimitUQ,
		@RCA_ContainerWeightLimitType,
		'1',
		'TU',
		GetUtcDate(),
		'CJP',
		GetUtcDate(),
		'CJP'
	)";

			var allocationPK = Guid.NewGuid();

			using (var command = Db.Connection.Command(insertRatingContractAllocationLineTestCaseSql))
			{
				command.AddParameter("@RCA_PK", System.Data.SqlDbType.UniqueIdentifier, allocationPK);
				command.AddParameter("@RCA_AllocationLineID", System.Data.SqlDbType.VarChar, allocationLineID);
				command.AddParameter("@RCA_RCT_RatingContract", System.Data.SqlDbType.UniqueIdentifier, contractPK);
				command.AddParameter("@RCA_ContainerWeightLimit", System.Data.SqlDbType.Decimal, containerWeightLimit);
				command.AddParameter("@RCA_ContainerWeightLimitUQ", System.Data.SqlDbType.VarChar, containerWeightLimitUnit);
				command.AddParameter("@RCA_ContainerWeightLimitType", System.Data.SqlDbType.VarChar, containerWeightLimitType);
				command.ExecuteNonQuery();
			}

			return allocationPK;
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("AVT", GetContainerWeightLimitType(allocationRoute_AVG));
			AssertEquals("ABT", GetContainerWeightLimitType(allocationRoute_ABS));
			AssertEquals("", GetContainerWeightLimitType(allocationRoute_Empty));
		}

		string GetContainerWeightLimitType(Guid allocationRoutePK)
		{
			var selectTransformationResultCommand = @"SELECT
	RCA_ContainerWeightLimitType
FROM
	dbo.RatingContractAllocationLine
WHERE
	RCA_PK = @RCA_PK";

			using (var command = Db.Connection.Command(selectTransformationResultCommand))
			{
				command.AddParameter("@RCA_PK", System.Data.SqlDbType.UniqueIdentifier, allocationRoutePK);
				return command.ExecuteScalar() as string;
			}
		}
	}
}
