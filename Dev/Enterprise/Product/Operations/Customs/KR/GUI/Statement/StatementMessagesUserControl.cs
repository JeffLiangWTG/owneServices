using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.GUI
{
	public partial class StatementMessagesUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public StatementMessagesUserControl()
		{
			InitializeComponent();
			ChangeUsingColumnStyles();
			AddToUseColumnStyles();
			ReOrderColumns();
		}

		void ChangeUsingColumnStyles()
		{
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageType)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageType)).CaptionResourceString = Res.GetData("C09DE684-C7E7-4E6A-A9FA-65A714906D5C", "Message Type");
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_InterchangeStatus)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_User)).CaptionResourceString = Res.GetData("EDFE151D-7C22-4828-88E3-06FA7D4D3683", "Create by");
			if (!DesignModeFinder.IsDesigning)
			{
				MessagesGrid.GetColumnStyle("Interchange+eHubID").IsVisible = false;
			}
		}
		void AddToUseColumnStyles()
		{
			MessagesGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EDIMessage.EM_StatusDescription),
					IsVisible = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EDIMessage.EM_MessageTypeDescription),
					IsVisible = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EDIMessage.EM_ReceiveTransmitDescription),
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EDIMessage.EM_MessageSubTypeDescription),
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
			});
		}

		void ReOrderColumns()
		{
			MessagesGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
		}

		readonly string[] headerColumns =
		{
				nameof(EDIMessage.EM_MessageNum),
				nameof(EDIMessage.EM_StatusDescription),
				nameof(EDIMessage.EM_MessageType),
				nameof(EDIMessage.EM_MessageTypeDescription),
				nameof(EDIMessage.EM_MessageDateTime),
		};
	}
}
