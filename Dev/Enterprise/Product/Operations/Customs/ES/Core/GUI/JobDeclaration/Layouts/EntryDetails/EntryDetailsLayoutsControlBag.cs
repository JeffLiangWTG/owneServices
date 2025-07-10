using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class EntryDetailsLayoutsControlBag : ControlBag
	{
		EntryDetailsLayoutsControlBag()
		{
			InvoiceAmountCalcDropEdit = RegisterControl(nameof(EntryDetailsUserControl.InvoiceAmountCalcDropEdit));
			CircuitTextBox = RegisterControl(nameof(EntryDetailsUserControl.CircuitTextBox));
			CircuitCanTextBox = RegisterControl(nameof(EntryDetailsUserControl.CircuitCanTextBox));
			CSVClearanceTextBox = RegisterControl(nameof(EntryDetailsUserControl.CSVClearanceTextBox));
			VATDeferredCalcEdit = RegisterControl(nameof(EntryDetailsUserControl.VATDeferredCalcEdit));
		}

		public static EntryDetailsLayoutsControlBag Instance => instance ?? (instance = new EntryDetailsLayoutsControlBag());

		[ThreadStatic]
		static EntryDetailsLayoutsControlBag instance;

		protected override Control CreateTemplate() => new EntryDetailsUserControl();

		public ControlReference InvoiceAmountCalcDropEdit { get; }
		public ControlReference CircuitTextBox { get; }
		public ControlReference CircuitCanTextBox { get; }
		public ControlReference CSVClearanceTextBox { get; }
		public ControlReference VATDeferredCalcEdit { get; }
	}
}
