using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.TransitWarehouse
{
	[TestedType(typeof(SetDefaultDGLimitWeightUnitAndVolumeUnit))]
	public class SetDefaultDGLimitWeightUnitAndVolumeUnitTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(WhsUNDGLimitSchema.Constants.TableName, "Constraint_WWD_TotalWeightLimitUQ");
			DBTransformationTestHelper.DropConstraintIfExists(WhsUNDGLimitSchema.Constants.TableName, "Constraint_WWD_TotalVolumeLimitUQ");
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var undgLimit1 = new WhsUNDGLimit(whs, "1") { WWD_TotalVolumeLimit = 0, WWD_TotalVolumeLimitUQ = string.Empty, WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "LB" }.AppendInsertAndReturnObject(sql);
			limitPK1 = undgLimit1.PK;

			var undgLimit2 = new WhsUNDGLimit(whs, "2") { WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "CC", WWD_TotalWeightLimit = 0, WWD_TotalWeightLimitUQ = string.Empty }.AppendInsertAndReturnObject(sql);
			limitPK2 = undgLimit2.PK;

			var undgLimit3 = new WhsUNDGLimit(whs, "3") { WWD_TotalVolumeLimit = 100, WWD_TotalVolumeLimitUQ = "CC", WWD_TotalWeightLimit = 100, WWD_TotalWeightLimitUQ = "LB" }.AppendInsertAndReturnObject(sql);
			limitPK3 = undgLimit3.PK;

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			WhsUNDGLimit.AssertFromDB(TestConnection, limitPK1)
				.ExpectEquals("WWD_TotalWeightLimitUQ", limit => limit.WWD_TotalWeightLimitUQ, "LB")
				.ExpectEquals("WWD_TotalVolumeLimitUQ", limit => limit.WWD_TotalVolumeLimitUQ, "M3")
				.VerifyAll();

			WhsUNDGLimit.AssertFromDB(TestConnection, limitPK2)
				.ExpectEquals("WWD_TotalWeightLimitUQ", limit => limit.WWD_TotalWeightLimitUQ, "KG")
				.ExpectEquals("WWD_TotalVolumeLimitUQ", limit => limit.WWD_TotalVolumeLimitUQ, "CC")
				.VerifyAll();

			WhsUNDGLimit.AssertFromDB(TestConnection, limitPK3)
				.ExpectEquals("WWD_TotalWeightLimitUQ", limit => limit.WWD_TotalWeightLimitUQ, "LB")
				.ExpectEquals("WWD_TotalVolumeLimitUQ", limit => limit.WWD_TotalVolumeLimitUQ, "CC")
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SetDefaultDGLimitWeightUnitAndVolumeUnit();

		Guid limitPK1;
		Guid limitPK2;
		Guid limitPK3;
	}
}
