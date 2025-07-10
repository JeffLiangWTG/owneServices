using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class RiskUserControlTest : TestCaseWithFactory
	{
		public void TestRiskGrid()
		{
			var grid = control.FindSingle<ZGrid>("RiskGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code", 115, ((ZDropEditColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_Code))).Width);
				AssertEquals("CSI_ReferenceNumber", 130, ((ZMultiControlColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_ReferenceNumber))).Width);
				AssertEquals("CSI_DateOfIssue", 60, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_DateOfIssue))).Width);
				AssertEquals("CSI_Value", 100, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_Value))).Width);
				AssertEquals("CSI_Quantity", 110, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_Quantity))).Width);
				AssertEquals("CSI_Quantity2", 90, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_Quantity2))).Width);
				AssertEquals("CSI_AdditionalDescription", 150, ((ZMultiLineTextBoxColumnInfo)grid.GetColumnStyle(nameof(RiskManagement.CSI_AdditionalDescription))).Width);
			});
		}

		public void TestCustomsValueCalcEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcEdit>("CustomsValueCalcEdit"));
		}

		public void TestCustomsQuantityCalcEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcEdit>("CustomsQuantityCalcEdit"));
		}

		public void TestNetWeightCalcDropEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcDropEdit>("NetWeightCalcDropEdit"));
		}

		public void TestRemainingCustomsValueCalcEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcEdit>("RemainingCustomsValueCalcEdit"));
		}

		public void TestRemainingCustomsQuantityCalcEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcEdit>("RemainingCustomsQuantityCalcEdit"));
		}

		public void TestRemainingNetWeightCalcDropEdit()
		{
			AssertNotNull(control.FindSingleOrDefault<ZCalcDropEdit>("RemainingNetWeightCalcDropEdit"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new RiskUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		RiskUserControl control;
	}
}
