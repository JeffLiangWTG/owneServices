using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	[TestedType(typeof(ProfitLossSummaryDetailView))]
	public class ProfitLossSummaryDetailViewTest : NonPersistentBusinessObjectTestCase
	{
		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var row = new DataTable().Rows.Add(System.Array.Empty<object>());
			var profitLoss = new ProfitLossSummaryDetail(Factory, row);
			return new ProfitLossSummaryDetailView(profitLoss);
		}

		public void TestDecimals()
		{
			var profitLossDetailView = (ProfitLossSummaryDetailView)GetNewBusinessObject();
			AssertEquals(profitLossDetailView.ProfitLossSummaryDetail.Decimals, profitLossDetailView.Decimals);
		}
	}
}
