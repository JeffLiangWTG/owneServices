using System.Collections;
using Enterprise.Interop.OutlookIntegration;

namespace Enterprise.Client.EDI.IncidentManager.Testing
{
	public class DummyOutlookMailItem : IOutlookMailItem
	{
		public DummyOutlookMailItem()
		{
		}

		public void SendMail()
		{
			fMailItemSend(null);
		}

		#region IOutlookMailItem Members
		public void AddReplyRecipient(string replyRecipient)
		{
			this.ReplyRecipient = replyRecipient;
		}

		public void AddAttachment(string fileName)
		{
		}

		public void Display(bool modal)
		{
			DisplayCalled = true;
			DisplayModal = modal;
		}

		public void AddRecipient(string recipient)
		{
			if (Recipients != null)
			{
				ArrayList recipientsArrayList = new ArrayList(Recipients);
				recipientsArrayList.Add(recipient);
				Recipients = (string[])recipientsArrayList.ToArray(typeof(string));
			}
			else
			{
				Recipients = new string[] { recipient };
			}
		}

		public bool DisplayCalled;
		public bool DisplayModal;
		public string ReplyRecipient;
		public string[] Recipients;
		#region Properties
		public object CustomData
		{
			get
			{
				return null;
			}
		}

		public string FileNameToSaveOnSend
		{
			get
			{
				return null;
			}

			set
			{
			}
		}

		#region Subject
		public string Subject
		{
			get
			{
				return fSubject;
			}

			set
			{
				fSubject = value;
			}
		}

		string fSubject;
		#endregion
		#region Body
		public string Body
		{
			get
			{
				return fBody;
			}

			set
			{
				fBody = value;
			}
		}

		string fBody;
		#endregion
		#endregion
		#region MailItemSend
		public event OutlookMailItemSendEventHandler MailItemSend
		{
			add
			{
				fMailItemSend += value;
			}

			remove
			{
				fMailItemSend -= value;
			}
		}

		event OutlookMailItemSendEventHandler fMailItemSend;
		#endregion
		#endregion
	}
}
