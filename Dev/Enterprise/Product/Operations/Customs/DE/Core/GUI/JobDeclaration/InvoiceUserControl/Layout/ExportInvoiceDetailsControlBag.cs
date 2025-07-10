using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class ExportInvoiceDetailsControlBag : ControlBag
	{
		ExportInvoiceDetailsControlBag()
		{
			FreeOfChargeCheckBox = RegisterControl(nameof(ExportInvoiceDetailsUserControl.FreeOfChargeCheckBox));
		}

		public static ExportInvoiceDetailsControlBag Instance => instance ?? (instance = new ExportInvoiceDetailsControlBag());

		[ThreadStatic]
		static ExportInvoiceDetailsControlBag instance;

		public ControlReference FreeOfChargeCheckBox { get; }

		protected override Control CreateTemplate() => new ExportInvoiceDetailsUserControl();
	}
}
