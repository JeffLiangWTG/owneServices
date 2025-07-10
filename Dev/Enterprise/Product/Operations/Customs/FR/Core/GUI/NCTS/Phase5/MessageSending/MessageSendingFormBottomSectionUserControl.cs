using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class MessageSendingFormBottomSectionUserControl : ZUserControl
	{
		public MessageSendingFormBottomSectionUserControl()
		{
			InitializeComponent();
			AfterFirstBinding += MessageSendingFormBottomSectionUserControl_AfterFirstBinding;
		}

		void MessageSendingFormBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			dataSource = (TP5MessageSendingObjectParent)DataSource;
			foreach (TP5MessageSendingObject sendingObject in dataSource.SendingObjectsCollection)
			{
				sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
				MessageTypeChanged(sendingObject);
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			MessageTypeChanged((TP5MessageSendingObject)sender);
		}

		internal void MessageTypeChanged(TP5MessageSendingObject sendingObject)
		{
			SetControlsVisibility(sendingObject);
		}

		void SetControlsVisibility(TP5MessageSendingObject sendingObject)
		{
			if (sendingObject != null)
			{
				var isMessageTypeCC013C = sendingObject.MessageType == TP5MessageTypeList.Codes.CC013C;
				var isMessageTypeCC034C = sendingObject.MessageType == TP5MessageTypeList.Codes.CC034C;
				var isMessageTypeCC141C = sendingObject.MessageType == TP5MessageTypeList.Codes.CC141C;

				JustificationTextBox.Visible = sendingObject.MessageType == TP5MessageTypeList.Codes.CC013C || sendingObject.MessageType == TP5MessageTypeList.Codes.CC014C;
				QueryIdentifierDropEdit.Visible = isMessageTypeCC034C;
				QueryPeriodFromDateEdit.Visible = isMessageTypeCC034C;
				QueryPeriodToDateEdit.Visible = isMessageTypeCC034C;
				RequesterIDTextBox.Visible = isMessageTypeCC034C;
				RequesterRoleDropEdit.Visible = isMessageTypeCC034C;
				TC11DeliveryDate.Visible = isMessageTypeCC141C;
				QueryInformationTextBox.Visible = isMessageTypeCC141C;
				ActualConsigneeLabel.Visible = isMessageTypeCC141C;
				ActualConsigneeDocAddressControl.Visible = isMessageTypeCC141C;
				ActualOfficeOfDestinationFindBox.Visible = isMessageTypeCC141C;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				dataSource = (TP5MessageSendingObjectParent)DataSource;
				foreach (TP5MessageSendingObject sendingObject in dataSource.SendingObjectsCollection)
				{
					sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		TP5MessageSendingObjectParent dataSource;
	}
}
