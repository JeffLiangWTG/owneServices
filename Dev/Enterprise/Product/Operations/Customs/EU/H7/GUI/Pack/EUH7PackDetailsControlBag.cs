using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7PackDetailsControlBag : ControlBag
	{
		public static EUH7PackDetailsControlBag Instance => packDetailsControlBag.Value;

		EUH7PackDetailsControlBag()
		{
			GoodsDescriptionTextBox = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.GoodsDescriptionTextBox));
			PackUQDropEdit = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.PackUQDropEdit));
			PackQtyCalcEdit = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.PackQtyCalcEdit));
			MarksAndNumbersTextBox = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.MarksAndNumbersTextBox));
			WeightCalcDropEdit = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.WeightCalcDropEdit));
			VolumeCalcDropEdit = RegisterControl(nameof(EUH7PackDetailsFieldsUserControl.VolumeCalcDropEdit));
		}

		public ControlReference GoodsDescriptionTextBox { get; }

		public ControlReference PackUQDropEdit { get; }

		public ControlReference PackQtyCalcEdit { get; }

		public ControlReference MarksAndNumbersTextBox { get; }

		public ControlReference WeightCalcDropEdit { get; }

		public ControlReference VolumeCalcDropEdit { get; }

		protected override Control CreateTemplate() => new EUH7PackDetailsFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUH7PackDetailsControlBag> packDetailsControlBag = new(() => new EUH7PackDetailsControlBag());
	}
}
