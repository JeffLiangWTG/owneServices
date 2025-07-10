using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public static InvoiceLineDetailsControlBag Instance => instance ??= new InvoiceLineDetailsControlBag();

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		InvoiceLineDetailsControlBag()
		{
			PreferenceDocNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.PreferenceDocNumberTextBox));
			InvoiceNumberDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.InvoiceNumberDropEdit));
		}

		public ControlReference InvoiceNumberDropEdit { get; }

		public ControlReference PreferenceDocNumberTextBox { get; }

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();
	}
}
