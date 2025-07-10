using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class SubscriptionMessageSendingForm : MessageSendingObjectForm
	{
		public SubscriptionMessageSendingForm(SubscriptionMessageSendingObjectParent parent) : base(parent)
		{
			InitializeComponent();
			InitializeNewColumns();

			parent.SelectedSendingObjectsChanged += SendingObjectParent_SelectedSendingObjectsChanged;
			SendingObjectParent_SelectedSendingObjectsChanged(this, null);
		}

		public SubscriptionMessageSendingObjectParent MessageSendingObjectParent => BusinessEntity as SubscriptionMessageSendingObjectParent;

		void SendingObjectParent_SelectedSendingObjectsChanged(object sender, System.EventArgs e)
		{
			SendButton.Enabled = MessageSendingObjectParent.SelectedSendingObjects.Any();
		}

		void InitializeNewColumns()
		{
			var messageTypeColumn = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			messageTypeColumn.ColumnName = SubscriptionMessageSendingObject.Schema.MessageType;
			messageTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeColumn);

			var eventIdColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			eventIdColumn.ColumnName = SubscriptionMessageSendingObject.Schema.EventId;
			eventIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			MessageSendingObjectsGrid.ColumnStyles.Add(eventIdColumn);

			var submittedDateColumn = new ZArchitecture.ZDateEditColumnStyleInfo();
			submittedDateColumn.ColumnName = SubscriptionMessageSendingObject.Schema.SubmittedDate;
			submittedDateColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			MessageSendingObjectsGrid.ColumnStyles.Add(submittedDateColumn);

			var statusColumn = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			statusColumn.ColumnName = SubscriptionMessageSendingObject.Schema.Status;
			statusColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			MessageSendingObjectsGrid.ColumnStyles.Add(statusColumn);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (MessageSendingObjectParent != null)
				{
					MessageSendingObjectParent.SelectedSendingObjectsChanged -= SendingObjectParent_SelectedSendingObjectsChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override bool CheckIsOKToSend()
		{
			return base.CheckIsOKToSend() && MessageSendingEnviromentChecker.CheckIsOKToSend();
		}
	}
}
