using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(AddConstraintToRefDocSourceCode))]
	public class AddConstraintToRefDocSourceCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new AddConstraintToRefDocSourceCode();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(RefDocSourceSchema.Constants.TableName, "Constraint_RDS_Code_NoCheck");
			DBTransformationTestHelper.DropConstraintIfExists(RefDocSourceSchema.Constants.TableName, "Constraint_RDS_Desc_NoCheck");

			var helper = new TransformationTestDataCreator();
			helper.CreateRefDocSource("AAA", "description");
			helper.CreateRefDocSource("", "");
		}

		protected override void AssertPreConditions()
		{
			var sql = "SELECT COUNT(1) FROM RefDocSource;";
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(sql));
		}

		protected override void AssertTransformationResults()
		{
			var sql = "SELECT COUNT(1) FROM RefDocSource;";
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(sql));

			sql = "SELECT COUNT(1) FROM RefDocSource WHERE RDS_Code = '';";
			AssertEquals(0, (int)Db.Connection.ExecuteScalar(sql));
		}
	}
}
