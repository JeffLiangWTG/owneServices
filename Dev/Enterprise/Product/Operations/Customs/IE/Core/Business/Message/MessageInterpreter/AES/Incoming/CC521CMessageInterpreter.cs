using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC521MessageInterpreter : InboundMessageInterpreter<CC521CProvider>
	{
		public CC521MessageInterpreter(AESInboundEDIMessage message, CC521CProvider provider)
			: base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"{1BBA2F1B-5760-46FC-B452-D28D94636238}",
			"An Export Diversion Rejection message has been received from Customs on Job {0} through the IE521 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("{7FC64C01-9AD0-4B60-A1C9-798EA3B7BCA6}", "REJ - Exit Released Rejected"));
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("{F2C44F15-A22A-4F4D-8228-91A04FF0F2B3}", "Diversion Rejection Reason Code"), GetCodeAndDescription(provider.DiversionRejectionReasonCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.DiversionRejectionCode));
			yield return (Res.GetString("{6EEF5AB5-692E-473A-A0E9-FF934A5F54EF}", "Diversion Rejection Text"), provider.DiversionRejectionText);
			yield return (Res.GetString("{DD1201B2-8342-49BA-B7CD-06AFDB8768C4}", "Customs office of Exit (Actual)"), provider.CustomsOfficeOfExitActual);
		}
	}
}
