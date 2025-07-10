using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ExportInvoiceLineDetailsControlBag : ControlBag
	{
		public static ExportInvoiceLineDetailsControlBag Instance => exportInvoiceLineDetailsControlBag.Value;

		ExportInvoiceLineDetailsControlBag()
		{
			IsMainPackCheckBox = RegisterControl(nameof(ExportInvoiceLineDetailsUserControl.IsMainPackCheckBox));
		}

		public ControlReference IsMainPackCheckBox { get; }

		protected override Control CreateTemplate() => new ExportInvoiceLineDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ExportInvoiceLineDetailsControlBag> exportInvoiceLineDetailsControlBag = new Lazy<ExportInvoiceLineDetailsControlBag>(() => new ExportInvoiceLineDetailsControlBag());
	}
}
