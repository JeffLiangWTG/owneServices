using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExportInvoiceLineDetailsGroupBoxUserControl : ZUserControl
	{
		public ExportInvoiceLineDetailsGroupBoxUserControl()
		{
			InitializeComponent();

			CertificateOfOriginDynamicLayoutPanel.UpdateLayout(new InvoiceLineCertificateOfOriginLayout());
			ReExportDynamicLayoutPanel.UpdateLayout(new ReExportLayout());
		}
	}
}
