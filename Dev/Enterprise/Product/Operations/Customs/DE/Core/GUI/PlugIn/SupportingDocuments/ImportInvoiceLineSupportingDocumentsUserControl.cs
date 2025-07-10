using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportInvoiceLineSupportingDocumentsUserControl : SupportingDocumentsUserControl
	{
		public ImportInvoiceLineSupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new ImportInvoiceLineSupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(reorderedColumnsSequence);
			ModifyColumns();
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		void ModifyColumns()
		{
			var csi_unitOfQuantityColumn = SupportingDocumentsGrid.GetColumnStyle(AdditionalInfo.Schema.CSI_UnitOfQuantity);
			csi_unitOfQuantityColumn.CharacterCasing = CharacterCasing.Upper;

			var unitOfQuantityColumn = (ZCalcEditColumnStyleInfo)SupportingDocumentsGrid.GetColumnStyle(AdditionalInfo.Schema.CSI_Quantity);
			unitOfQuantityColumn.Decimals = 3;
		}

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_Status,
			SupportingDocument.Schema.CSI_Quantity,
			SupportingDocument.Schema.CSI_UnitOfQuantity
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;
	}
}
