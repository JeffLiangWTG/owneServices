using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM431MessageInterpreter : InboundMessageInterpreter<IM431Provider>
	{
		public IM431MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM431Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("40FF50C4-A6D7-45E7-986A-6E94F5414516", "An Expiration of Timer for Supplementary Declaration (IM431) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("DD5C9733-1DDE-46AD-9527-D33FEA746F58", "Lodgement of Supplementary Declaration Start Date"), provider.LodgementOfSupplementaryDeclarationStartDate.ToShortDateString());
			yield return (Res.GetString("204FCAD6-54C6-4F5E-8C98-5412348EDB33", "Lodgement of Supplementary Declaration Expiry Date"), provider.LodgementOfSupplementaryDeclarationExpiryDate.ToShortDateString());
			yield return (Res.GetString("52A48369-F850-4695-911D-48CFCCD70CD0", "Timer Expiry Information"), provider.TimerExpiryInformation);
		}
	}
}
