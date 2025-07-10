using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE802MessageInterpreter : InboundMessageInterpreter<IIE802>
	{
		public IE802MessageInterpreter(EMCSInboundEDIMessage message, IIE802 provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("94C68423-A391-4D7B-83F8-007B12C3F998", "A reminder message IE802 has been received for the job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
				yield return (CommonResStrings.MessageType, provider.ReminderMessageType);
				yield return (Res.GetString("6923BD56-597C-4903-B5DD-A80794C9A2B9", "Date and Time of Issuance of Reminder"), provider.DateAndTimeOfIssuanceOfReminder.ToLongTimeString());
				yield return (Res.GetString("E36ECE3D-991E-4264-8ADE-05C9BCEA636F", "Limit Date and Time"), provider.LimitDateTime.ToLongTimeString());
				yield return (Res.GetString("F6C72C8B-EFB0-4B21-9A0D-D7105A38E322", "Reminder Information"), provider.ReminderInformation);
		}
	}
}
