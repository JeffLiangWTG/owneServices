using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		InvoiceLineDetailsControlBag()
		{
			PreviousEntryNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.PreviousEntryNumberTextBox));
			PreviousEntryLineNumberCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PreviousEntryLineNumberCalcEdit));
		}

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		public ControlReference PreviousEntryNumberTextBox { get; }

		public ControlReference PreviousEntryLineNumberCalcEdit { get; }
	}
}
