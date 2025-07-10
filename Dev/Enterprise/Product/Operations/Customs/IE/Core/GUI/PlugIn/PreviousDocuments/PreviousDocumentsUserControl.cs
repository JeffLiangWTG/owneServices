using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class PreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl
	{
		public PreviousDocumentsUserControl()
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
				new ZDateEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_DateOfIssue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_LineNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66),
					CaptionResourceString = Res.GetData("53e99a50-1c6d-48d7-b3c9-7d995e02b9b2", "Line No.")
				}
			});
		}
	}
}
