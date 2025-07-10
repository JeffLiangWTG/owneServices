using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsControlBag : ControlBag
	{
		Phase5GoodsItemDifferencesDetailsControlBag()
		{
			SequenceNumberTextBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsUserControl.SequenceNumberTextBox));
			ItemNumberTextBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsUserControl.ItemNumberTextBox));
		}

		public static Phase5GoodsItemDifferencesDetailsControlBag Instance => instance ?? (instance = new Phase5GoodsItemDifferencesDetailsControlBag());

		[ThreadStatic]
		static Phase5GoodsItemDifferencesDetailsControlBag instance;

		public ControlReference SequenceNumberTextBox { get; }

		public ControlReference ItemNumberTextBox { get; }

		protected override Control CreateTemplate() => new Phase5GoodsItemDifferencesDetailsUserControl();
	}
}
