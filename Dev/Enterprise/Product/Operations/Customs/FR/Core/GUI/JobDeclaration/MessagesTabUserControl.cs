
namespace Enterprise.Customs.FR.GUI
{
	public partial class MessagesTabUserControl : EU.GUI.MessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			SetupMessagesGrid();
		}

		void SetupMessagesGrid()
		{
			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("5D998411-3E95-478C-B149-19BAD2938620", "Held Until Date"),
				ColumnName = "EM_HeldUntilDate",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsReadOnly = true
			});
		}
	}
}
