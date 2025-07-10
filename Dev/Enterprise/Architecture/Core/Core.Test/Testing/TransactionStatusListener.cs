using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TransactionStatusListener : BaseTestListener
	{
		public static readonly TransactionStatusListener Instance = new TransactionStatusListener();

		public override void EndTest(TestCase test, System.DateTime endTime)
		{
			bool fail = false;
			try
			{
				while (Db.Connection.IsInTransaction || Db.Connection.AppTransactionCount > 0)
				{
					fail = true;
					Db.Connection.RollbackTransaction();
				}
			}
			finally
			{
				if (fail)
				{
					try
					{
						Db.Connection.CloseConnection();
						Db.Connection.EnsureIsOpen();
					}
					finally
					{
						Assertion.Fail("Transaction was left open on Db.Connection");
					}
				}
			}
		}
	}
}
