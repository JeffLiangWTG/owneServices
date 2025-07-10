using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class InvoiceLineCertificateOfOriginControlBag : ControlBag
	{
		public InvoiceLineCertificateOfOriginControlBag()
		{
			COOIndicatorDropEdit = RegisterControl(nameof(InvoiceLineCertificateOfOriginUserControl.COOIndicatorDropEdit));
			COODeterminationRuleDropEdit = RegisterControl(nameof(InvoiceLineCertificateOfOriginUserControl.COODeterminationRuleDropEdit));
			COOLabelLocationDropEdit = RegisterControl(nameof(InvoiceLineCertificateOfOriginUserControl.COOLabelLocationDropEdit));
			FTATypeDropEdit = RegisterControl(nameof(InvoiceLineCertificateOfOriginUserControl.FTATypeDropEdit));
		}

		public static InvoiceLineCertificateOfOriginControlBag Instance => instance ?? (instance = new InvoiceLineCertificateOfOriginControlBag());

		[ThreadStatic]
		static InvoiceLineCertificateOfOriginControlBag instance;

		public ControlReference COOIndicatorDropEdit { get; }
		public ControlReference COODeterminationRuleDropEdit { get; }
		public ControlReference COOLabelLocationDropEdit { get; }
		public ControlReference FTATypeDropEdit { get; }

		protected override Control CreateTemplate() => new InvoiceLineCertificateOfOriginUserControl();
	}
}
