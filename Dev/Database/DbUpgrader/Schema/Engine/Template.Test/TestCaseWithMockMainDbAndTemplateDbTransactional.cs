using System;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public abstract class TestCaseWithMockMainDbAndTemplateDbTransactional : TestCaseWithMockMainDbAndTemplateDb
	{
		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.BeginTransaction();
		}

		protected override void TearDown()
		{
			try
			{
				if (!TestConnection.IsInTransaction)
				{
					throw new Exception("Transaction unexpectedly rolledback during the test.");
				}

				TestConnection.RollbackTransaction();
			}
			finally
			{
				base.TearDown();
			}
		}
	}
}
