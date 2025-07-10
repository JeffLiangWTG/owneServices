using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class CusTempStorageRegLineItemDetailsControlBag : ControlBag
	{
		public static CusTempStorageRegLineItemDetailsControlBag Instance => cusTempStorageRegLineItemDetailsControlBag.Value;

		public CusTempStorageRegLineItemDetailsControlBag()
		{
			GoodsItemNumberCalcEdit = RegisterControl(nameof(CusTempStorageRegLineItemDetailsUserControl.GoodsItemNumberCalcEdit));
			TariffTextBox = RegisterControl(nameof(CusTempStorageRegLineItemDetailsUserControl.TariffTextBox));
			CusC4NumberTextBox = RegisterControl(nameof(CusTempStorageRegLineItemDetailsUserControl.CusC4NumberTextBox));
			GoodsDescriptionTextBox = RegisterControl(nameof(CusTempStorageRegLineItemDetailsUserControl.GoodsDescriptionTextBox));
			GrossWeightCalcDropEdit = RegisterControl(nameof(CusTempStorageRegLineItemDetailsUserControl.GrossWeightCalcDropEdit));
		}

		public ControlReference GoodsItemNumberCalcEdit { get; }

		public ControlReference TariffTextBox { get; }

		public ControlReference CusC4NumberTextBox { get; }

		public ControlReference GoodsDescriptionTextBox { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		protected override Control CreateTemplate() => new CusTempStorageRegLineItemDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CusTempStorageRegLineItemDetailsControlBag> cusTempStorageRegLineItemDetailsControlBag = new (() => new CusTempStorageRegLineItemDetailsControlBag());
	}
}
