using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportInvoiceLinePreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public ExportInvoiceLinePreviousDocumentsUserControl()
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
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					CharacterCasing = CharacterCasing.Upper
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_ItemNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
				},
				new ZCalcEditColumnStyleInfo()
				{
					ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_Quantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					CharacterCasing = CharacterCasing.Normal,
					CaptionResourceString = Res.GetData("99fa7ddb-e209-4ac1-b95e-34b64277e4e4", "Complement")
				},
			});
		}
	}
}
