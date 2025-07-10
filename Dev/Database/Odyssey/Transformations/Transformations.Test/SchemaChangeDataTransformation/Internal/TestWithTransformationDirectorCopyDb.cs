using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations
{
	public abstract class TestWithTransformationDirectorCopyDb : TransactionedTestCase
	{
		protected override void MasterSetUp()
		{
			using (AdminConnection connection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(connection, TransformationDirector.DataCopyDb, Db.DatabaseName);
			}
		}

		protected override void FinalTearDown()
		{
			using (AdminConnection connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, TransformationDirector.DataCopyDb, Db.DatabaseName);
			}
		}
	}
}
