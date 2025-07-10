using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC140CMessageInterpreter : InboundMessageInterpreter<CC140CProvider>
	{
		public CC140CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC140CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"587211E9-3B0A-4D51-AD6B-B830EC056D51",
			"A Request on Non-Arrived Movement Message (IE140) has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("0CB8D3E9-0EFC-46A7-A64D-6B6F67CF6560", "Request on Non-Arrived Movement Date"), provider.RequestOnNonArrivedMovementDate.ToShortDateString());
			yield return (Res.GetString("CC270744-8BD3-49BF-948F-C097D54A3FB4", "Limit for Response Date"), provider.LimitForResponseDate.ToShortDateString());
			yield return (Res.GetString("2C384166-AD77-4050-AA7D-A146FA303BB1", "Customs Office of Enquiry at Departure"), GetCodeAndDescription(provider.CustomsOfficeOfEnquiry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
		}
	}
}
