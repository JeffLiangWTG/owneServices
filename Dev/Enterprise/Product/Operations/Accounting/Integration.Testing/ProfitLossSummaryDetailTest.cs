using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	[TestedType(typeof(ProfitLossSummaryDetail))]
	public class ProfitLossSummaryDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DataRow row = new DataTable().Rows.Add(System.Array.Empty<object>());
			return new ProfitLossSummaryDetail(Factory, row);
		}

		public void TestDecimals()
		{
			ProfitLossSummaryDetail profitLossSummaryDetail = (ProfitLossSummaryDetail)GetNewBusinessObject();
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, profitLossSummaryDetail.Decimals);
		}
	}
}
