using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MiscRequestMessagesUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public MiscRequestMessagesUserControl()
		{
			InitializeComponent();
			AddToUseColumnStyles();
			ChangeUsingColumnStyles();
			ReOrderColumns();
		}

		void ChangeUsingColumnStyles()
		{
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageType)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageType)).CaptionResourceString = Res.GetData("255D97B6-6A27-45DB-8B84-C3B1C8D35629", "Message Type");
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_User)).CaptionResourceString = Res.GetData("DB9B1F5A-97CF-490A-8129-4706E9970250", "Create By");
			MessagesGrid.GetColumnStyle(nameof(EDIMessage.EM_InterchangeStatus)).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

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
					ColumnName = nameof(EDIMessage.EM_ReceiveTransmitDescription),
					IsVisible = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
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
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EDIMessage.EM_MessageSubTypeDescription),
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				}
			});
		}

		void ReOrderColumns()
		{
			MessagesGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
		}

		readonly string[] headerColumns =
		{
			nameof(EDIMessage.EM_MessageNum),
			nameof(EDIMessage.EM_ReceiveTransmitDescription),
			nameof(EDIMessage.EM_StatusDescription),
			nameof(EDIMessage.EM_MessageType),
			nameof(EDIMessage.EM_MessageTypeDescription),
			nameof(EDIMessage.EM_MessageDateTime)
		};
	}
}
