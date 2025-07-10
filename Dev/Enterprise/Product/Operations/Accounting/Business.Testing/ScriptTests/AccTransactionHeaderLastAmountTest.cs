using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccTransactionHeaderLastAmountTest : ScriptTest
	{
		public void TestLastInvoiceDateAndAmount()
		{
			var org1 = TestObjectCreator.ABIGAS;
			var companyData = org1.CompanyData;

			var inv = TestObjectCreator.CreateARReceipt(1m, 110m, ZDateTime.Today, ZDateTime.Today.AddDays(1), org1.PK, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			var dt = RunScript(companyData.PK);
			AssertEquals(110m, Decimal.Parse(dt.Rows[0][OrgCollectionCall.Schema.CC_LastReceiptAmount].ToString()));
			var date = inv.AH_InvoiceDate.AddMilliseconds(-inv.AH_InvoiceDate.Millisecond).AddSeconds(-inv.AH_InvoiceDate.Second);
			AssertEquals(date.ToDateTime(), (DateTime)dt.Rows[0][OrgCollectionCall.Schema.CC_LastReceiptDate]);
		}

		DataTable RunScript(ZGuid pk)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"select * from {0} where {1} = '{2}'", OrgCollectionCall.Schema.TableName, OrgCollectionCall.Schema.PK, pk));
		}
	}
}
