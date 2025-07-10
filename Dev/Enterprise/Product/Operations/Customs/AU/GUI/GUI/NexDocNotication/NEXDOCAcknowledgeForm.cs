using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class NEXDOCAcknowledgeForm : ZChildForm
	{
		public NEXDOCAcknowledgeForm(QuarantineNexDocNotification notification) : base(notification)
		{
			Notification = Argument.NotNull(notification, nameof(notification));
			SetLabelText();
		}

		QuarantineNexDocNotification Notification { get; }

		public override string FormHeading => Res.GetString("9F3F2210-71A7-495A-8C2D-17ECDE010F75", "Acknowledge {0}", Notification.ObjectName);

		void SetLabelText()
		{
			HeaderLable.CaptionResourceString = Res.GetData("FC959B9A-0DAF-4E64-8B5A-D5EE508A54E9", "Do you want to Accept or Reject this {0}");
			HeaderLable.CaptionResourceString = HeaderLable.CaptionResourceString.Format(Notification.ObjectName);
		}

		NEXDOCAcknowledgeMessageSender fmessageSender;
		NEXDOCAcknowledgeMessageSender MessageSender => fmessageSender ?? (fmessageSender = new NEXDOCAcknowledgeMessageSender());

		void AcceptButtonNew_Click(object sender, EventArgs e)
		{
			ShowResult(MessageSender.SendAcceptMessage(Notification.PK));
			Close();
		}

		void RejectButton_Click(object sender, EventArgs e)
		{
			ShowResult(MessageSender.SendRejectMessage(Notification.PK));
			Close();
		}

		void ShowResult(ZString result)
		{
			if (result.IsEmpty)
			{
				Globals.Message.ShowInformation(MessageQueuedMessage);
			}
			else
			{
				Globals.Message.ShowError(result);
			}
		}

		string MessageQueuedMessage => Res.GetString("FD25B56A-F0E1-4E9E-9656-F323F273BBFA", "Message queued for sending.");
	}
}
