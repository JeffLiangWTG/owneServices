using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl
	{
		public AdditionalInfosUserControl()
		{
			InitializeComponent();

			AdditionalInfosGroupBox.CaptionResourceString = CaptionProvider.AdditionalInfoTabPageCaption(false);
			AdditionalInfosGroupBox.Text = AdditionalInfosGroupBox.CaptionResourceString.Caption;

			ChangeColumnsToGrid();
		}

		void ChangeColumnsToGrid()
		{
			using (AdditionalInfosGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AdditionalInfosGrid.RemoveFromAvailableColumns(AdditionalInfo.Schema.CSI_RN_NKCountryCode);

				AdditionalInfosGrid.RemoveFromAvailableColumns(AdditionalInfo.Schema.CSI_NctsExportFromEC);

				AdditionalInfosGrid.RemoveFromAvailableColumns(CusSupportingInfoSchema.Constants.CSI_Status);

				var info = new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = CusSupportingInfoSchema.Constants.CSI_DateOfIssue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				};

				AdditionalInfosGrid.ColumnStyles.Add(info);

				AdditionalInfosGrid.ReOrderColumns(ReorderedColumnsSequence);
			}
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						CusSupportingInfoSchema.Constants.CSI_Code,
						CusSupportingInfoSchema.Constants.CSI_Description,
						CusSupportingInfoSchema.Constants.CSI_DateOfIssue
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;
	}
}
