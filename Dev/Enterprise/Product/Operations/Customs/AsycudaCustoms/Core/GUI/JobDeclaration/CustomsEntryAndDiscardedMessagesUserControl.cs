namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class CustomsEntryAndDiscardedMessagesUserControl : Customs.GUI.CustomsEntryAndDiscardedMessagesUserControl
	{
		public CustomsEntryAndDiscardedMessagesUserControl()
		{
			InitializeComponent();
			DiscardedTabPage.TabVisible = false;
		}
		protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();
	}
}
