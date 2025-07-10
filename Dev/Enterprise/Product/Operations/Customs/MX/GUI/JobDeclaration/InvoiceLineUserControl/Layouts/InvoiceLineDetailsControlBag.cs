using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		InvoiceLineDetailsControlBag()
		{
			EntryInstructionGuidDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.EntryInstructionGuidDropEdit));
			ObservationsTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ObservationsTextBox));
			VehicleDetailsUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.VehicleDetailsUserControl));
		}

		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		public ControlReference EntryInstructionGuidDropEdit { get; }
		public ControlReference ObservationsTextBox { get; }
		public ControlReference VehicleDetailsUserControl { get; }
	}
}
