using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public class MessageStatusManager
	{
		readonly EDIInterchange interchange;

		public MessageStatusManager(EDIInterchange interchange)
		{
			this.interchange = Argument.NotNull(interchange, "interchange");
		}

		public void Fail(string errorDescription)
		{
			FailInterchange(errorDescription);
			FailContainedMessages(errorDescription);
			FailApplicationMessage();
		}

		public void Update(ZString interchangeStatus)
		{
			UpdateInterchangeStatus(interchangeStatus);
		}

		public void Update(ZString interchangeStatus, ZString messageStatus)
		{
			UpdateInterchangeStatus(interchangeStatus);
			UpdateContainedMessagesStatus(messageStatus);
		}

		void FailApplicationMessage()
		{
			var handler = FailedMessageHandlerFactory.GetHandler(interchange.EI_ApplicationCode, interchange.Company?.GC_RN_NKCountryCode);
			handler?.UpdateFailedMessage(interchange);
		}

		void FailInterchange(string errorDescription)
		{
			UpdateInterchangeStatus(EDIInterchange.Status.Failed);
			interchange.Notes.AddNew(true, Res.GetString("702C44CC-7E60-4F35-BD4F-BF999C8F9F34", "eHub Server Error"), errorDescription);
		}

		void FailContainedMessages(string errorDescription)
		{
			UpdateContainedMessagesStatus(EDIMessage.Status.Failed);
			Array.ForEach<EDIMessage>(interchange.ContainedMessages.Cast<EDIMessage>().ToArray(), new Action<EDIMessage>((EDIMessage message) => { message.Notes.AddNew(true, Res.GetString("702C44CC-7E60-4F35-BD4F-BF999C8F9F34", "eHub Server Error"), errorDescription); }));
		}

		void UpdateContainedMessagesStatus(ZString messageStatus)
		{
			Array.ForEach<EDIMessage>(interchange.ContainedMessages.Cast<EDIMessage>().ToArray(), new Action<EDIMessage>((EDIMessage message) =>
			{
				message.EM_Status = messageStatus;
			}));
		}

		void UpdateInterchangeStatus(ZString interchangeStatus)
		{
			var newInterchangeStatus = interchange.NewInterchangeStatus(interchangeStatus);
			interchange.EI_Status = newInterchangeStatus;
		}
	}
}
