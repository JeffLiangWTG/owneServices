using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(DataSource))]
	sealed class DataSourceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSqlDataCalculation()
		{
			var reportBizObj = new ReportBizObj(Factory);
			var dataSource = new DataSource(reportBizObj);
			dataSource.SQL = "SELECT * FROM dbo.[GetDuration] (NULL)";
			AssertEquals("FunctionName", "GetDuration", dataSource.FunctionName);
			var columns = dataSource.GetFunctionColumns().ToArray();
			AssertEquals("columns.Length", 1, columns.Length);
			AssertEquals("ColumnName", "Value", columns[0].ColumnName);

			dataSource.SQL = "SELECT * FROM dbo.OrgHeader WHERE 1<>0";
			AssertEquals("FunctionName", "OrgHeader", dataSource.FunctionName);
			columns = dataSource.GetFunctionColumns().ToArray();
			AssertEquals("columns.Length", true, 1 < columns.Length);
			AssertNotNull(OrgHeaderSchema.Constants.OH_Code, columns.FirstOrDefault(x => x.ColumnName == OrgHeaderSchema.Constants.OH_Code));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataSource(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;
	}
}
