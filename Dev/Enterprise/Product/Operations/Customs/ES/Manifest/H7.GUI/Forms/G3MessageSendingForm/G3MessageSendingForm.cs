using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public partial class G3MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		[Obsolete("Do not call. Only for designer use.")]
		public G3MessageSendingForm()
		{
		}

		public G3MessageSendingForm(BaseMessageSendingObjectParent parent, bool isRevoke = false)
			: base(parent)
		{
			if (isRevoke)
			{
				MessageSendingObjectsGrid.RemoveFromAvailableColumns(AutoMessageSendingObject.Schema.LocalReferenceNumber);
				AddOverrideRevokeReasonControl();
			}
			else
			{
				MessageSendingObjectsGrid.RemoveFromAvailableColumns(AutoMessageSendingObject.Schema.MRN, AutoMessageSendingObject.Schema.RevokeReason, AutoMessageSendingObject.Schema.RevokeReasonDescription);
			}
		}

		protected override int SendMessageToCustoms(Action<int, int> updateProgressCallback)
		{
			var messageSender = new G3MessageSender(BusinessEntity as G3MessageSendingObjectParent);
			var messagesSent = messageSender.Send();

			if (messagesSent == 0)
			{
				Globals.Message.Show(CouldNotCreateMessageError);
			}

			return messagesSent;
		}

		static ZString CouldNotCreateMessageError => Res.GetString("da4c14c2-6082-44b6-9fcb-2a20e4a1ddc9", "Could not create G3 message for H7 Job.");

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new G3MessageSendingColumnLayout();

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override bool ShouldSendSingleMessageForMultipleObjects => true;

		protected override bool AllowOverrideMessageType => false;

		void AddOverrideRevokeReasonControl()
		{
			var control = new OverrideRevokeReasonUserControl();
			control.Dock = System.Windows.Forms.DockStyle.Bottom;
			WarningSplitContainer.Panel2.Controls.Add(control);
		}
	}
}
