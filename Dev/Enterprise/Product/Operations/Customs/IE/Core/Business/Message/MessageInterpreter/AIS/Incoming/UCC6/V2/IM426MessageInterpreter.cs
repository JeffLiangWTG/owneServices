using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM426MessageInterpreter : InboundMessageInterpreter<IM426Provider>
	{
		public IM426MessageInterpreter(AISInboundEDIMessage message, IM426Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("94F0DBA0-B4D3-4FD9-856D-6AB5116C1CC5", "A Registration Notification (IM426) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CustomsRegistrationNumber, provider.CustomsRegistrationNumber);
			yield return (Res.GetString("09546D38-88C7-40C2-B295-540148BBC24E", "Declaration Registration Date and Time"), provider.DeclarationRegistrationDateAndTime.ToLongTimeString());
			yield return (Res.GetString("3C79A5CB-9C37-4572-9F34-D69F10AB4C89", "Presentation Notification Due Date"), provider.PresentationNotificationDueDate.ToISO8601ShortDateString());
		}
	}
}
