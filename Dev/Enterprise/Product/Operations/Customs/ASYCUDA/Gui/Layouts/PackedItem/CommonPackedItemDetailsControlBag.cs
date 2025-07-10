using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class CommonPackedItemDetailsControlBag : ControlBag
	{
		public static CommonPackedItemDetailsControlBag Instance => commonPackedItemDetailsControlBag.Value;

		CommonPackedItemDetailsControlBag()
		{
			TariffFindBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.TariffFindBox));
			CustomsQtyCalcDropEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomsQtyCalcDropEdit));
			GoodsOriginCodeFindBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.GoodsOriginCodeFindBox));
			GoodsDescriptionTextBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.GoodsDescriptionTextBox));
			CustomsValueCalcEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomsValueCalcEdit));
			TaxAmountCalcEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.TaxAmountCalcEdit));
			DutyAmountCalcEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.DutyAmountCalcEdit));
			MessageStatusTextBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.MessageStatusTextBox));
			PackStatusTextBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.PackStatusTextBox));
			CustomEntriesSeparatorUserControl = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomEntriesSeparatorUserControl));
			CustomEntriesGrid = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomEntriesGrid));
			BrandTextBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.BrandTextBox));
			ModelTextBox = RegisterControl(nameof(CommonPackedItemDetailsUserControl.ModelTextBox));
			GrossWeightCalcDropEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.NetWeightCalcDropEdit));
			CustomsQty2CalcDropEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomsQty2CalcDropEdit));
			CustomsQty3CalcDropEdit = RegisterControl(nameof(CommonPackedItemDetailsUserControl.CustomsQty3CalcDropEdit));
			GoodsValueLocalCurrencyControl = RegisterControl(nameof(CommonPackedItemDetailsUserControl.GoodsValueLocalCurrencyControl));
		}

		public ControlReference TariffFindBox { get; }
		public ControlReference CustomsQtyCalcDropEdit { get; }
		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference CustomsValueCalcEdit { get; }
		public ControlReference TaxAmountCalcEdit { get; }
		public ControlReference DutyAmountCalcEdit { get; }
		public ControlReference MessageStatusTextBox { get; }
		public ControlReference PackStatusTextBox { get; }
		public ControlReference CustomEntriesSeparatorUserControl { get; }
		public ControlReference CustomEntriesGrid { get; }
		public ControlReference BrandTextBox { get; }
		public ControlReference ModelTextBox { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference CustomsQty2CalcDropEdit { get; }
		public ControlReference CustomsQty3CalcDropEdit { get; }
		public ControlReference GoodsValueLocalCurrencyControl { get; }

		protected override Control CreateTemplate() => new CommonPackedItemDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<CommonPackedItemDetailsControlBag> commonPackedItemDetailsControlBag = new Lazy<CommonPackedItemDetailsControlBag>(() => new CommonPackedItemDetailsControlBag());
	}
}
