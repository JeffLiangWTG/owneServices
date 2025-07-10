using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class UCC5ImportPreviousDocumentsUserControl : PreviousDocumentsUserControl
	{
		public UCC5ImportPreviousDocumentsUserControl()
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
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = PreviousDocument.Schema.CSI_LineNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66),
					CaptionResourceString = Res.GetData("20D19A6E-06DF-4997-B937-51CF295B45C3", "Line No."),
					MaxLengthOverride = 5,
				}
			});
		}
	}
}
