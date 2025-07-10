namespace Enterprise.Customs.ES.GUI
{
	public partial class MessagesTabUserControl : EU.GUI.MessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();

			MessagesGrid.OnResendInterchange = MessagesGrid.DoResendInterchange;
		}
	}
}
