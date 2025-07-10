using Enterprise.Accounting.GUI.XmlExport;

namespace Enterprise.Client.DFD.GUI
{
	public partial class DFDFlatFileXmlExportForm : FlatFileXmlExportForm
	{
		public DFDFlatFileXmlExportForm(XmlExportGUIWrapper wrapper)
			: base(wrapper)
		{
			InitializeComponent();
		}

		protected override void DisableAllUserInterfaceOptions()
		{
			base.DisableAllUserInterfaceOptions();
			ExportBatchNumberCalcEdit.Enabled = false;
			NewExportBatchGroupBox.Enabled = true;
			DatesGroupBox.Enabled = true;
			FromZDateEdit.Enabled = true;
			ToZDateEdit.Enabled = true;
		}

		// internals for testing
		internal ZArchitecture.ZCalcEdit InternalExportBatchNumberCalcEdit {
			get { return ExportBatchNumberCalcEdit; }
		}
		internal ZArchitecture.GUI.ZGroupBox InternalDatesGroupBox {
			get { return DatesGroupBox; }
		}
		internal ZArchitecture.GUI.ZDateEdit InternalFromZDateEdit
		{
			get { return FromZDateEdit; }
		}
		internal ZArchitecture.GUI.ZDateEdit InternalToZDateEdit
		{
			get { return ToZDateEdit; }
		}
		internal ZArchitecture.GUI.ZGroupBox InternalNewExportBatchGroupBox
		{
			get { return NewExportBatchGroupBox; }
		}
	}
}
