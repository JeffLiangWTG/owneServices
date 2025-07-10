using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class AnnexesTabUserControl : ZUserControl
	{
		public AnnexesTabUserControl()
		{
			InitializeComponent();
			SetUpAnnexGridColumns();
		}

		void SetUpAnnexGridColumns()
		{
			AnnexGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZGuidDropEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("7F90C96A-ED0B-406B-B081-09BAE96012AC", "eDoc"),
					ColumnName = CusStorageDocPivot.Schema.CSD_StorageDocReference,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("68DA3918-F435-4B9E-82D1-90BC3A27C911", "Description"),
					ColumnName = CusStorageDocPivot.Schema.CSD_Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZDropEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("26EF02C3-F71C-40B8-94B5-602AB65F5DF7", "Document Extension"),
					ColumnName = CusStorageDocPivot.Schema.DocumentExtension,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("88459C4E-F524-46B0-8480-B997F3AFDE8C", "Document Size"),
					ColumnName = CusStorageDocPivot.Schema.DocumentSize,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("F5E9367D-1EED-4A35-B871-AFD4D3285DB1", "Message Status"),
					ColumnName = CusStorageDocPivot.Schema.MessageStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				}
			});
		}
	}
}
