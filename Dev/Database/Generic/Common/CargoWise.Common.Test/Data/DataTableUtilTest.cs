using System.Data;
using NUnit.Framework;

namespace CargoWise.Common
{
	class DataTableUtilTest : TestCase
	{
		const string BodgeyString = "splaty'~\0x0\0x1\t\r\n[]()#/=><+-*%&|^\"1   !@#$%^&*()_)+-=[]\\;',./{}|:\\<>?xxx";
		public void TestGetRowValueAsTokenisedAdoFilterStringValue_ForLikeStartsWith()
		{
			DataTable table = new DataTable();
			table.Columns.Add("string", typeof(string));
			DataRow row = table.Rows.Add(new object[] { BodgeyString });
			DataRow[] rows;
			string expr;
			expr = "string like " + DataTableUtil.GetRowValueAsTokenisedAdoFilterStringValue_ForLikeStartsWith(BodgeyString.Substring(0, BodgeyString.Length - 1));
			rows = table.Select(expr);
			AssertEquals("Expect match", 1, rows.Length);
			expr = "string like " + DataTableUtil.GetRowValueAsTokenisedAdoFilterStringValue_ForLikeStartsWith(BodgeyString + "p");
			rows = table.Select(expr);
			AssertEquals("Expect no match", 0, rows.Length);
		}
	}
}