using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC228CMessageInterpreter : InboundMessageInterpreter<CC228CProvider>
	{
		public CC228CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC228CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("0FC18B30-4156-4A73-A982-1E17ACA469E5", "A Comprehensive Guarantee Cancellation Liability Liberation Message (IE228) message has been received.");

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => Enumerable.Empty<(string Key, string Value)>();

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() => provider.GuaranteeReferences.Select(
			guaranteeReference => (
			summary: Res.GetString("58D7CAA3-2734-453F-B947-63C837BD2919", "Guarantee Information:"),
			details: ExtractDetails(guaranteeReference, provider.Guarantor)
		));

		IEnumerable<(string key, string value)> ExtractDetails(CC228CGuaranteeReferenceProvider guaranteeReference, CC228CGuarantorProvider guarantor)
		{
			yield return (Res.GetString("39A67EE8-C218-4F57-A86E-9586A0D5FC1A", "Guarantor’s. identification number"), guarantor.IdentificationNumber);
			yield return (Res.GetString("EDBCE79A-E132-46D8-8510-DB19D5306F22", "Guarantor’s. Name"), guarantor.Name);
			yield return (Res.GetString("8F340BD1-C702-4249-86B3-159C336D2699", "Guarantor’s. Address "), guarantor.Address.Postcode + " " + guarantor.Address.City + " " + guarantor.Address.Country);
			yield return (Res.GetString("8CAE7947-525C-4B95-B7DC-8CA5FF5CDA88", "GRN"), guaranteeReference.GRN);
			yield return (Res.GetString("E2C30E8D-903F-4BFB-A390-2F82EBB3BAA5", "Currency"), guaranteeReference.Currency);
			yield return (Res.GetString("B2E51BBA-BF58-47C9-A218-1794E90F3737", "Guarantee amount"), guaranteeReference.GuaranteeAmount.ToString());
			yield return (Res.GetString("FCA7BB5B-4188-4C34-8F87-C2FFB4F178FA", "Invalidity date"), guaranteeReference.InvalidityDate.ToShortDateString());
			yield return (Res.GetString("A122F57D-4FD0-4E11-83E8-721A4C713F16", "Liability liberation date "), guaranteeReference.LiabilityLiberationDate.ToShortDateString());
			yield return (Res.GetString("1C10D8A3-08C6-4CAB-A185-A1D8A2974179", "Customs Office Of Guarantee"), guaranteeReference.CustomOfficeOfGuaranteeReferenceNumber);
		}
	}
}
