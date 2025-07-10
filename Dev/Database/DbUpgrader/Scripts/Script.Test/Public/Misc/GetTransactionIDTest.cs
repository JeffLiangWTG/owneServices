using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetTransactionID))]
	class GetTransactionIDTest : DbCreateScriptTest
	{
		#region TestGetTransactionID

		public void TestGetTransactionID()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				using (var command = connection1.Command("SELECT ID FROM dbo.GetTransactionID()"))
				{
					AssertNotEquals("When not in a Transaction, TransactionID will be different each time it is called.", command.ExecuteScalar(), command.ExecuteScalar());
				}

				connection1.BeginTransaction();
				using (var commandInTransaction1 = connection1.Command("SELECT ID FROM dbo.GetTransactionID()"))
				{
					AssertEquals("When in a Transaction, TransactionID will be the same each time it is called.", commandInTransaction1.ExecuteScalar(), commandInTransaction1.ExecuteScalar());

					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						connection2.BeginTransaction();

						using (var commandInTransaction2 = connection2.Command("SELECT ID FROM dbo.GetTransactionID()"))
						{
							AssertEquals("When in a Transaction, TransactionID will be the same each time it is called.", commandInTransaction2.ExecuteScalar(), commandInTransaction2.ExecuteScalar());
							AssertNotEquals("TransactionID for different transactions should be different.", commandInTransaction2.ExecuteScalar(), commandInTransaction1.ExecuteScalar());
						}
					}
				}
			}
		}
		#endregion
	}
}
