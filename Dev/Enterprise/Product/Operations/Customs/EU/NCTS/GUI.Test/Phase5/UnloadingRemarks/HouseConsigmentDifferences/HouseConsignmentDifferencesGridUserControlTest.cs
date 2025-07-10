using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentDifferencesGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(INctsBillCollection<NctsBill>), userControl.BindingSource.DataSourceType);
		}

		public void TestGrid()
		{
			var control = userControl.Grid;
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", control);
				AssertEquals("BindingMember", ".", control.GetBindingMember());
				AssertEquals("Dock style", DockStyle.Fill, control.Dock);
			});
		}

		public void TestColumnStyles()
		{
			var grid = userControl.Grid;
			CombineAssertions(() =>
			{
				AssertEquals("MovementDetail+B9_SeqNo", 80, ((ZTextBoxColumnStyleInfo)grid.GetColumnStyle("MovementDetail+B9_SeqNo")).Width);
				AssertEquals("B0_SecurityIndicatorFromExport", 80, ((ZCheckBoxColumnStyleInfo)grid.GetColumnStyle("B0_SecurityIndicatorFromExport")).Width);
				AssertEquals("B0_ReferenceID", 120, ((ZTextBoxColumnStyleInfo)grid.GetColumnStyle("B0_ReferenceID")).Width);
				AssertEquals("MovementDetail+B9_UnloadedState", 100, ((ZDropEditColumnStyleInfo)grid.GetColumnStyle("MovementDetail+B9_UnloadedState")).Width);
				AssertEquals("B0_Weight", 80, ((ZTextBoxColumnStyleInfo)grid.GetColumnStyle("B0_Weight")).Width);
				AssertEquals("B0_WeightUQ", 80, ((ZDropEditColumnStyleInfo)grid.GetColumnStyle("B0_WeightUQ")).Width);
				AssertEquals("MovementDetail+ConsignorName", 80, ((ZTextBoxColumnStyleInfo)grid.GetColumnStyle("MovementDetail+ConsignorName")).Width);
				AssertEquals("MovementDetail+ConsigneeName", 80, ((ZTextBoxColumnStyleInfo)grid.GetColumnStyle("MovementDetail+ConsigneeName")).Width);
				var b0_GrossWeightUnloadedColumnStyle = ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle("B0_GrossWeightUnloaded"));
				AssertEquals("B0_GrossWeightUnloaded Width", 80, b0_GrossWeightUnloadedColumnStyle.Width);
				AssertEquals("B0_GrossWeightUnloaded Caption", "Gross Weight Unloaded", b0_GrossWeightUnloadedColumnStyle.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentDifferencesGridUserControl();
		}
		HouseConsignmentDifferencesGridUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
