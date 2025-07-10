using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class NexDocNotificationForm : ZTemplateForm
	{
		public NexDocNotificationForm(QuarantineNexDocNotification nexDocNotification) : base(nexDocNotification) { }

		QuarantineNexDocNotification QuarantineNexDocNotification => (QuarantineNexDocNotification)BusinessEntity;

		public override string FormCaption => QuarantineNexDocNotification.HumanReadableName;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			var queryInterchangeCreator = new QueryInterchangeCreator(MessagesGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();

			MessagesGrid.AfterBind -= MessagesGrid_AfterBind;
			MessagesGrid.AfterBind += MessagesGrid_AfterBind;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(Res.GetString("7882FCD2-9EBA-45AB-9125-74F884E1ABF1", "Acknowledge Forward/Transfer Request"), AcknowledgeMenuItem_Click));
		}

		void AcknowledgeMenuItem_Click(object sender, EventArgs e)
		{
			if (!QuarantineNexDocNotification.IsAcknowledgedStatusForSendingMessage)
			{
				Globals.Message.ShowInformation(QuarantineNexDocNotification.WrongAcknowledgedStatusForSendingMessage);
			}
			else
			{
				using (var form = new NEXDOCAcknowledgeForm(QuarantineNexDocNotification))
				{
					ZFormModaliser.ShowDialogAndDispose(form);
				}
			}
		}

		void MessagesGrid_AfterBind(object sender, EventArgs e)
		{
			MessagesGrid.ListManager.CurrentChanged -= MessagesGrid_ListManager_CurrentChanged;
			MessagesGrid.ListManager.CurrentChanged += MessagesGrid_ListManager_CurrentChanged;
			MessagesGrid_ListManager_CurrentChanged(null, null);
		}

		void MessagesGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var selectedMessage = MessagesGrid.ListManager?.GetCurrent() as EDIMessage;
			if (selectedMessage != null)
			{
				if (MessageSummaries.TryGetValue(selectedMessage.PK, out var summary))
				{
					MessageSummaryBox.Text = summary;
				}
				else
				{
					var newSummary = MessageSummaryGenerator.GetMessageSummary(selectedMessage);
					MessageSummaryBox.Text = newSummary;
					MessageSummaries.Add(selectedMessage.PK, newSummary);
				}
			}
		}

		Dictionary<ZGuid, string> MessageSummaries => fMessageSummaries ?? (fMessageSummaries = new Dictionary<ZGuid, string>());
		Dictionary<ZGuid, string> fMessageSummaries;
	}
}
