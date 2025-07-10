using CargoWise.Data;

namespace NUnit.Framework
{
	sealed class CreatesCommandBeforeTransactionAndRunsCommandAfterTransactionTest : TransactionedTestCase
	{
		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}
		//			readonly DbConnection testConnection = Db.Connection;

		[ExpectNoExceptions]
		public void TestRunsSuccessfully()
		{
			using (DbCommand command = TestConnection.Command("--"))
			{
				TestConnection.BeginTransaction();
				// If we were to execute the command here, the test should throw an exception...
				TestConnection.RollbackTransaction();

				// ..but if we execute it here, there shouldn't be a problem
				command.ExecuteNonQuery();
			}

			TestConnection.BeginTransaction();
			try
			{
				TestConnection.ExecuteNonQuery("--");
			}
			finally
			{
				TestConnection.RollbackTransaction();
			}
		}

		[ExpectNoExceptions]
		public void TestRunsSuccessfullyInWeb()
		{
			try
			{
				TestRunsSuccessfully();
			}
			finally
			{
				Db.Instance.IsWebTestOverride = null;
			}
		}
	}
}
