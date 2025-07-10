using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC628MessageInterpreter : InboundMessageInterpreter<CC628CProvider>
	{
		public CC628MessageInterpreter(AESInboundEDIMessage message, CC628CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"3E35A903-1172-44D0-AAE8-88F396D69AD3",
			"An Exit Summary Declaration Acknowledgement message has been received from Customs for Job {0} through the IE628 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("A0E8B57E-3B91-47F1-BE6C-874254C2BB06", "Released for Export (REL)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AcceptanceDate, provider.DeclarationAcceptanceDate.ToShortDateString());
		}
	}
}
