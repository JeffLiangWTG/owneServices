using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR015MessageInterpreter : InboundMessageInterpreter<TR015VProvider>
	{
		public TR015MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR015VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("BE299997-BDAE-49C7-9F8B-279A35B12305", "A Transit Pre-lodged Declaration Acknowledgment message (TR015V) has been received. Customs has acknowledged receipt of a Pre-Lodged Transit Declaration for Job {0}", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (Res.GetString("44DB0276-1A55-495C-8630-D8504BBE2D99", "Declaration Acknowledgement Date"), provider.DeclarationAcknowledgementDate.ToLongTimeString());
		}
	}
}
