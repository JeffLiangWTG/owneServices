using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC531MessageInterpreter : InboundMessageInterpreter<CC531Provider>
	{
		public CC531MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC531Provider provider) : base(message, provider)
		{
			linkedEntryHeader = message.EM_LinkedObject as CusEntryHeader;
		}

		readonly CusEntryHeader linkedEntryHeader;

		protected override string Summary => Res.GetString("58F32D2C-BE73-4B95-9DF5-38C56D5C1B73",
			"An Expiry Of Timer message has been received for supplementary declaration from Customs for Job {0} through the IE531 message.",
			linkedEntryHeader?.Declaration?.JE_DeclarationReference ?? string.Empty);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("77A30542-59B4-4F9D-A2E3-7AE57C857C5F", "Lodgement of Supplementary Declaration start date"), provider.SupplementaryDeclarationLodgementStart.ToShortDateString());
			yield return (Res.GetString("282E3CCD-5D6A-4E0A-A2C9-8B4E9817ED56", "Lodgement of Supplementary Declaration expiry date"), provider.SupplementaryDeclarationLodgementEnd.ToShortDateString());
			yield return (Res.GetString("E00CACFF-8DFD-4EF9-8BF5-C576A7B8BEF4", "Timer Expiry Information"), provider.TimerExpiryInformation);
		}
	}
}
