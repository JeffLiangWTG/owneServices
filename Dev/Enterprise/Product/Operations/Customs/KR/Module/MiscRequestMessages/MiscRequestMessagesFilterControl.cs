using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.Module
{
	public partial class MiscRequestMessagesFilterControl : ZFilterStripControl
	{
		public MiscRequestMessagesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			ChangeColumnsInGrid();
			AddNewColumnsInGrid();
			ReOrderColumns();
		}
		void AddNewColumnsInGrid()
		{
			Grid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusMiscRequestHeader.ApplicationStartPeriod),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
					CaptionResourceString = Enterprise.Customs.KR.Module.Res.GetData("60305941-70D4-4DCB-83E8-635B1FBE34EC", "Application Start Period")
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusMiscRequestHeader.ReviewDate5SG),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
					CaptionResourceString = Enterprise.Customs.KR.Module.Res.GetData("EA49F2C9-FB15-48C2-8D89-60294E387D2D", "Review Date (5SG)")
				},
			});
		}
		void ChangeColumnsInGrid()
		{
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.StatusName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.MessageTypeName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_JobNumber).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.FormattedApplicationNumber)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_RequestDate).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.FormattedCustomsOffice)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.CustomsOfficeName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.FormattedRequestDetails)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(540);

			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_GB).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.BrokerName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_GS_NKBroker).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.DeclarantCompanyName)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			Grid.GetColumnStyle(nameof(CusMiscRequestHeader.LinesCount)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_MessageType).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			Grid.GetColumnStyle(CusMiscRequestHeader.Schema.CMR_Status).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
		}

		void ReOrderColumns()
		{
			Grid.ReOrderColumnsAndChangeVisibility(listColumns);
		}
		readonly string[] listColumns =
		{
			nameof(CusMiscRequestHeader.StatusName),
			nameof(CusMiscRequestHeader.MessageTypeName),
			CusMiscRequestHeader.Schema.CMR_JobNumber,
			nameof(CusMiscRequestHeader.FormattedApplicationNumber),
			CusMiscRequestHeader.Schema.CMR_RequestDate,
			nameof(CusMiscRequestHeader.FormattedCustomsOffice),
			nameof(CusMiscRequestHeader.CustomsOfficeName),
			nameof(CusMiscRequestHeader.FormattedRequestDetails),
			nameof(CusMiscRequestHeader.ApplicationStartPeriod),
			nameof(CusMiscRequestHeader.ReviewDate5SG),
		};
	}
}
