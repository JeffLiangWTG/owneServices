using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC055CMessageInterpreter : InboundMessageInterpreter<CC055CProvider>
	{
		public CC055CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC055CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"C76171A1-EA82-4BA7-9902-6E3AEEC03BB5",
			"A Guarantee Not Valid Message (IE055) has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("4ADBC4AB-E0A4-4F74-9744-56362B521206", "Declaration Acceptance Date"), provider.DeclarationAcceptanceDate.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var guaranteeReference in provider.GuaranteeReferences)
			{
				var guaranteeReferences = new List<(string, string)>();
				guaranteeReferences.Add((CommonResStrings.SequenceNumber, guaranteeReference.SequenceNumber.ToString()));
				guaranteeReferences.Add((Res.GetString("88E01F7F-C8B9-4869-ABA1-A1AB7E3579B2", "Guarantee Reference Number"), guaranteeReference.GRN));

				foreach (var invalidGuaranteeReason in guaranteeReference.InvalidGuaranteeReasons)
				{
					guaranteeReferences.Add((Res.GetString("25388847-7275-416B-8260-40ED223B2280", "Invalid Guarantee Reason Sequence Number"), invalidGuaranteeReason.SequenceNumber.ToString()));
					guaranteeReferences.Add((Res.GetString("AD435D4A-6A84-46F1-98B0-3AB2E875B40E", "Invalid Guarantee Reason Code"), invalidGuaranteeReason.InvalidGuaranteeReasonCode));
					guaranteeReferences.Add((Res.GetString("91F2C6BD-58EB-4E2B-9634-41633FB45FCE", "Invalid Guarantee Reason Text"), invalidGuaranteeReason.InvalidGuaranteeReasonText));
				}

				yield return ((string summary, IReadOnlyCollection<(string Key, string Value)>))(null, guaranteeReferences.AsReadOnly());
			}
		}
	}
}
