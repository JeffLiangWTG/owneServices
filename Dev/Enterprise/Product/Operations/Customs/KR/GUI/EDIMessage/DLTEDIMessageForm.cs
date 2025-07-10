using Enterprise.Customs.KR.Business;
using Enterprise.Messaging.GUI;
using static Enterprise.Messaging.GUI.MessageSaveActionHandler;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DLTEDIMessageForm : EDIMessageForm
	{
		public DLTEDIMessageForm(EDIMessage message)
			: base(message)
		{
			ReOrderTabPage();

			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 485, true);
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 485, true);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("35CB6184-F536-4096-9F33-57FC5F1B0F43", "Message : DLT"); }
		}

		void ReOrderTabPage()
		{
			MainTabControl.Controls.Clear();
			MainTabControl.Controls.Add(MainTabPage);
			MainTabControl.Controls.Add(MessageContentsTabPage);
			MainTabControl.Controls.Add(NotesTabPage);
			MainTabControl.Controls.Add(LogsTabPage);
		}

		protected override EDIMessageStandAloneUserControl GetNewMessageDetailUserControl()
		{
			return new EDIMessageMainUserControl();
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			new MessageSaveActionHandler(Message).SaveMessageAction(FileNameType.Formatted);
		}
	}
}
