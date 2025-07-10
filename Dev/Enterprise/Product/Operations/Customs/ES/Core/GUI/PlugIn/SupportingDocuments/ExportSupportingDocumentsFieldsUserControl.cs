using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportSupportingDocumentsFieldsUserControl : BaseCustomsEntryUserControl
	{
		public ExportSupportingDocumentsFieldsUserControl()
		{
			InitializeComponent();
			PopulateDynamicLayoutPanel();
		}

		void PopulateDynamicLayoutPanel()
		{
			SupportingDocumentsFieldsDynamicLayoutPanel.UpdateLayout(new ExportSupportingDocumentsFieldsLayout());
		}
	}
}
