using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportSupplierHeaderPreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public ExportSupplierHeaderPreviousDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.ColumnStyles.Clear();
			PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
			{
				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_Code,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
					CharacterCasing = CharacterCasing.Normal
				}
			});
		}
	}
}
