using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class InvoiceLineOtherDetailsControlBag : ControlBag
	{
		InvoiceLineOtherDetailsControlBag()
		{
			ApprovalNoTextBox = RegisterControl(nameof(InvoiceLineOtherDetailsUserControl.ApprovalNoTextBox));
			SteelExportEffectiveDateUserControl = RegisterControl(nameof(InvoiceLineOtherDetailsUserControl.SteelExportEffectiveDateUserControl));
		}

		public static InvoiceLineOtherDetailsControlBag Instance => instance ?? (instance = new InvoiceLineOtherDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineOtherDetailsControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLineOtherDetailsUserControl();

		public ControlReference ApprovalNoTextBox { get; }
		public ControlReference SteelExportEffectiveDateUserControl { get; }
	}
}
