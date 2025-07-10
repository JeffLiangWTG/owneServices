using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class SEDDetailsGroupBoxUserControl : ZUserControl
	{
		public SEDDetailsGroupBoxUserControl()
		{
			InitializeComponent();

			SetLayout();
		}

		void SetLayout()
		{
			CertificateOfOriginDynamicLayoutPanel.UpdateLayout(new CertificateOfOriginDetailsLayout());
			ManufacturerDynamicLayoutPanel.UpdateLayout(new ManufacturerDetailsLayout());
			ImporterDynamicLayoutPanel.UpdateLayout(new ImporterDetailsLayout());
			SupplierDynamicLayoutPanel.UpdateLayout(new SupplierDetailsLayout());
		}
	}
}
