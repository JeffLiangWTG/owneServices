using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class DocumentRequestForm : Customs.GUI.MessageSendingFormWithValidationDetails
	{
		public DocumentRequestForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SelectAllButton.AllowOverlap(messageSendingObjectsGroupBox);
			UpdateTabIndex();
		}

		public override string FormHeading => Res.GetString("e18bb566-e8db-4e01-a78d-1629f42a7e8c", "Request Documents");

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		protected override bool PreviewMessageCheckboxVisible => false;

		protected void UpdateTabIndex()
		{
			SendButton.TabIndex = 9;
			CancelButton2.TabIndex = 10;
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			var shouldSelectAll = MessageSendingObjectParent.SendingObjectsCollection.Count != MessageSendingObjectParent.SelectedSendingObjects.Count();

			foreach (var messageSendingObject in MessageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>())
			{
				messageSendingObject.ShouldSend = shouldSelectAll;
			}
		}

		protected override void SendButton_ClickCore()
		{
			base.SendButton_ClickCore();

			var messageSentCount = SendMessageToCustoms();
			if (messageSentCount > 0)
			{
				TrySaveAndShowMessage(BusinessEntity.Factory, messageSentCount);
			}
		}

		protected virtual int SendMessageToCustoms()
		{
			var messagesSent = 0;

			foreach (MessageSendingObject sendingObject in BusinessEntity.SelectedSendingObjects)
			{
				var sender = sendingObject.CreateSender();
				if (sender?.Send() != null)
				{
					messagesSent++;
				}
			}

			return messagesSent;
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				var message = Res.GetString("9b02493e-7653-4925-80d0-ebf7fe46aee0", "{0} message(s) queued for sending.", messagesCreated);
				Globals.Message.ShowInformation(message);
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = GetNewColumnLayoutProvider());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;

		protected virtual IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new DocumentRequestGridColumnLayout();
	}
}
