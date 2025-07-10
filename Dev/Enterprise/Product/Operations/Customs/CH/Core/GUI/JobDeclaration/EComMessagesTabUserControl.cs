using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EComMessagesTabUserControl : MessagesTabUserControl
{
	public EComMessagesTabUserControl()
	{
		InitializeComponent();
		MessagesGrid.ColumnLayoutContext = MessageTypeCodeList.Codes.EBD;
		SetupMessageColumns();

		interpretedMessageTextWebBrowser.Name = "EComInterpretedMessageTextWebBrowser";
		interpretedMessageTextWebBrowser.AllowOverlap(InterpretedMessageTextBox);
	}

	#region Messages Grid columns

	void SetupMessageColumns()
	{
		MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("CF3144F7-270C-4C92-B7D6-890B0FBB57C6", "Sub Type Description"),
			ColumnName = EDIMessage.Schema.EM_MessageSubTypeDescription,
			IsReadOnly = true,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
		});

		MessagesGrid.SetAllColumnsVisible(false);
		MessagesGrid.SetColumnVisible(true, defaultColumnsForMessagesGrid);
		MessagesGrid.ReOrderColumns(defaultColumnsForMessagesGrid);
	}

	readonly string[] defaultColumnsForMessagesGrid = new string[]
	{
			EDIMessage.Schema.EM_MessageSubTypeDescription,
			EDIMessage.Schema.EM_ReceiveTransmit,
			EDIMessage.Schema.EM_MessageDateTime,
			EDIMessage.Schema.EM_User,
			EDIMessage.Schema.EM_Status,
	};

	#endregion
}
