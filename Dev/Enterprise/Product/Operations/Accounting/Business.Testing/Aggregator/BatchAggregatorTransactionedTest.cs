using System;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class BatchAggregatorTransactionedTest : TestCase
	{
		public void TestErrorMustRunInTransaction()
		{
			bool exceptionCaught = false;
			try
			{
				DbCommand takeUpSubledgersCmd = Db.Connection.Command("EXEC TakeUpSubledgers @Company, 'TST'");
				takeUpSubledgersCmd.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				takeUpSubledgersCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				AssertEquals("This must run in a transaction.", ex.Message);
				exceptionCaught = true;
			}
			Assert("There should have been an error", exceptionCaught);
		}
	}
}
