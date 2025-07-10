using System.Collections;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Client.JAS.Business.Testing
{
	internal class DummyJASMailSender : JASMailSender
	{
		public override void SendEmail(ZGuid notificationGroupPK, IRegistryItem notificationGroup, string subject, string body, string attachment)
		{
			EmailSentList.Add(new EmailSent(notificationGroupPK, subject, body, attachment));
			this.LastNotificationGroupPK = notificationGroupPK;
			this.LastSubject = subject;
			this.LastBody = body;
			this.LastAttachmentFileName = attachment;
		}

		public struct EmailSent
		{
			public EmailSent(ZGuid notificationGroupPK, string subject, string body, string attachment)
			{
				this.NotificationGroupPK = notificationGroupPK;
				this.Subject = subject;
				this.Body = body;
				this.Attachment = attachment;
			}

			public ZGuid NotificationGroupPK;
			public string Subject;
			public string Body;
			public string Attachment;
		}

		readonly ArrayList EmailSentList = new ArrayList();
		public EmailSent[] EmailsSent
		{
			get
			{
				return (EmailSent[])EmailSentList.ToArray(typeof(EmailSent));
			}
		}

		public void PurgeEmailsSent()
		{
			EmailSentList.Clear();
		}

		public ZGuid LastNotificationGroupPK;
		public string LastSubject;
		public string LastBody;
		public string LastAttachmentFileName;
	}
}
