using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GuaranteeCalculationLiabilityAmountUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CalculateLiabilityBizObj), control.BindingSource.DataSourceType);
		}

		public void TestLiabilityPercentageIntEdit()
		{
			var liabilityPercentageIntEdit = control.LiabilityPercentageIntEdit;
			CombineAssertions(() =>
			{
				AssertType<ZIntEdit>("Type", liabilityPercentageIntEdit);
				AssertEquals("BindTo", nameof(CalculateLiabilityBizObj.LiabilityPercentage), liabilityPercentageIntEdit.BindTo);
			});
		}

		[RequiresSTA]
		public void TestLiabilityAmountCalcDropEdit()
		{
			var liabilityAmountCalcDropEdit = control.LiabilityAmountCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", liabilityAmountCalcDropEdit);
				AssertEquals("BindToAmount", nameof(CalculateLiabilityBizObj.LiabilityAmount), liabilityAmountCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(CalculateLiabilityBizObj.Currency), liabilityAmountCalcDropEdit.BindToUnit);
			});
		}

		public void TestTotalValueCalcDropEdit()
		{
			var totalValueCalcDropEdit = control.TotalValueCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", totalValueCalcDropEdit);
				AssertEquals("BindToAmount", nameof(CalculateLiabilityBizObj.TotalValue), totalValueCalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", nameof(CalculateLiabilityBizObj.Currency), totalValueCalcDropEdit.BindToUnit);
			});
		}

		[RequiresSTA]
		public void TestTotalValueCalculationMethodControl() => AssertNotNull(control.LiabilityAmountTotalValueCalculationMethodUserControl);

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5GuaranteeCalculationLiabilityAmountUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		Phase5GuaranteeCalculationLiabilityAmountUserControl control;
	}
}
