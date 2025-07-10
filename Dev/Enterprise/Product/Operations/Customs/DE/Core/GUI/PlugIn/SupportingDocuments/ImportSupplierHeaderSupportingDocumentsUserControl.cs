using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportSupplierHeaderSupportingDocumentsUserControl : SupportingDocumentsUserControl
	{
		public ImportSupplierHeaderSupportingDocumentsUserControl()
			: base()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new ImportSupplierHeaderSupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(reorderedColumnsSequence);
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_DateOfIssue
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;
	}
}
