using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.NZP.GUI
{
	public partial class CMSFlatFileXmlExportForm : FlatFileXmlExportForm
	{
		public CMSFlatFileXmlExportForm(CMSExportGUIWrapper wrapper) : base(wrapper)
		{
			InitializeComponent();
			NewExportBatchGroupBox.Enabled = true;
			DatesGroupBox.Enabled = true;
			ToZDateEdit.Enabled = true;
		}

		internal ZGroupBox InternalNewExportBatchGroupBoxTest => NewExportBatchGroupBox;
		internal ZGroupBox InternalDatesGroupBoxTest => DatesGroupBox;
		internal ZDateEdit InternalToZDateEditTest => ToZDateEdit;
	}
}
