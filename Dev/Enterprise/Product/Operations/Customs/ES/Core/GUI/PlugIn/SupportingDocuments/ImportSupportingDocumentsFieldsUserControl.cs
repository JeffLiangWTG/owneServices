using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class ImportSupportingDocumentsFieldsUserControl : BaseCustomsEntryUserControl
	{
		public ImportSupportingDocumentsFieldsUserControl()
		{
			InitializeComponent();
			PopulateDynamicLayoutPanel();
		}

		void PopulateDynamicLayoutPanel()
		{
			SupportingDocumentsFieldsDynamicLayoutPanel.UpdateLayout(new ImportSupportingDocumentsFieldsLayout());
		}
	}
}
