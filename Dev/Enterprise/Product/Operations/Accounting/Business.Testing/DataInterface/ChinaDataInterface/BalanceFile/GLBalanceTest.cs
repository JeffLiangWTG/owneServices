using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[TestedType(typeof(GLBalance))]
	public class GLBalanceTest : BusinessObjectBaseTestCase
	{
		public void TestSetValue()
		{
			DataTable testTable = new DataTable();
			testTable.Columns.Add("GLAccountNumber");
			testTable.Columns.Add("BalanceAmountInternal");
			DataRow row = testTable.Rows.Add(System.Array.Empty<object>());
			GLBalance balanceToTest = new GLBalance(Factory, row);
			balanceToTest.BalanceAmount = new ZDecimal(50);
			AssertEquals(50m, balanceToTest.BalanceAmount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			DataRow row = new DataTable().Rows.Add(System.Array.Empty<object>());
			return new GLBalance(Factory, row);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			DataTable testTable = new DataTable();
			testTable.Columns.Add("BalanceAmountInternal");
			DataRow row = testTable.Rows.Add(System.Array.Empty<object>());
			return new GLBalance(Factory, row);
		}
	}
}
