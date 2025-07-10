using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class SRATAXProvider : ISRATAX
	{
		public SRATAXProvider(NSTAXK message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly NSTAXK message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier?.Replace("-", string.Empty);

		public string ReferenceNumber => message.Header?.ReferenceNumber;

		public string MRN => message.Header?.MRN;

		public string LocalReferenceNumber => message.Header?.LocalReferenceNumber;

		public string TaxChangeAssessmentType => message.Header?.TaxChangeAssessmentType;

		public DateTime? TaxAssessmentCreationDate => message.Header?.TaxAssessmentCreationDate;

		public DateTime? MaturityDate => message.Header?.MaturityDate;

		public string InterchangeRecipientEBS => message.MetaData?.InterchangeRecipient?.Identification?.SubsidiaryNumber;
	}
}
