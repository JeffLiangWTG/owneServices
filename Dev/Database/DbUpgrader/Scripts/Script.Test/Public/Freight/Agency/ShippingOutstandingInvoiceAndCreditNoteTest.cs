using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(ShippingOutstandingInvoiceAndCreditNote))]
	internal sealed class ShippingOutstandingInvoiceAndCreditNoteTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql =
				"select * from ShippingOutstandingInvoiceAndCreditNote(" +
				"null, null, null, null, null, " +
				"null, null, null, null, null, " +
				"null, null, null, null, null, " +
				"null, null, GETDATE())";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}
