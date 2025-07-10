using System.Windows.Forms;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceHeaderExportPreviousDocumentsUserControl : PreviousDocumentsUserControl
	{
		public InvoiceHeaderExportPreviousDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.ColumnStyles.Clear();
			PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_Code,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47),
					CharacterCasing = CharacterCasing.Upper
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(219),
					CharacterCasing = CharacterCasing.Upper
				},
			});
		}
	}
}
