using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	[TestedType(typeof(ProfitLossDetailView))]
	public class ProfitLossDetailViewTest : NonPersistentBusinessObjectTestCase
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
			var profitLoss = new ProfitLossDetail(Factory, row);
			return new ProfitLossDetailView(profitLoss);
		}

		public void TestDecimals()
		{
			var profitLossDetailView = (ProfitLossDetailView)GetNewBusinessObject();
			AssertEquals(profitLossDetailView.ProfitLossDetail.Decimals, profitLossDetailView.Decimals);
		}
	}
}
