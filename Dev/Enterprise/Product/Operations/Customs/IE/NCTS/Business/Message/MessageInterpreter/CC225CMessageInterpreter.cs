using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC225CMessageInterpreter : InboundMessageInterpreter<CC225CProvider>
	{
		public CC225CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC225CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("DCBE00CB-AFA5-4765-BFD9-75DFBB47632C", "A Guarantee Update Notification (IE225) message has been received for GRN, Job: {0}", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => Enumerable.Empty<(string Key, string Value)>();

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() => provider.GuaranteeReferences.Select(
			guaranteeReference => (
			summary: Res.GetString("01B89971-8571-4ABE-8491-09BB180BA579", "Guarantee Information:"),
			details: ExtractDetails(guaranteeReference)
		));

		IEnumerable<(string key, string value)> ExtractDetails(CC225CGuaranteeReferenceProvider guaranteeReference)
		{
			yield return (Res.GetString("B7C69922-3980-4331-AAB6-11A27ECA4259", "GRN"), guaranteeReference.GRN);
			yield return (Res.GetString("84FFA03A-E7B6-44FD-878F-F4CFC3979C78", "Currency"), guaranteeReference.Currency);
			yield return (Res.GetString("0A53F8B6-579A-45AB-A584-452404FDE0B1", "Reference amount"), guaranteeReference.ReferenceAmount.ToString());
			yield return (Res.GetString("9D332A37-8884-4989-A428-5F8DF06E44BD", "Percentage of reference amount"), guaranteeReference.PercentageOfReferenceAmount.ToString());
			yield return (Res.GetString("7C99A7D8-5293-4022-A85F-5961274AA009", "Guarantee amount"), guaranteeReference.GuaranteeAmount.ToString());
			yield return (Res.GetString("0CB25C1F-E94A-4FD3-83AD-0421C81982C1", "Number of certificates"), guaranteeReference.NumberOfCertificates.ToString());
			yield return (Res.GetString("0ACDB9E1-1AA5-450B-8DAB-80AA703254EE", "Validity date"), guaranteeReference.ValidityDate.ToShortDateString());
			yield return (Res.GetString("5C3DFA43-43A1-4119-9C96-2663E6D4D4CE", "Invalidity date"), guaranteeReference.InvalidityDate.ToShortDateString());
			yield return (Res.GetString("9DC42182-FFFD-4393-BBD8-7D1CC17E061B", "Invalidity reason code"), guaranteeReference.InvalidityReasonCode);
			yield return (Res.GetString("E1B1BB00-FCA2-41C4-AAC7-655D6B2FCC45", "Restricted use (suspended goods)"), guaranteeReference.RestrictedUseSuspendedGoods.ToString());
			yield return (Res.GetString("A211204C-208E-4FB2-9251-FAF9643580CA", "Custom office of Guarantee Reference number"), guaranteeReference.CustomOfficeOfGuaranteeReferenceNumber);
		}
	}
}
