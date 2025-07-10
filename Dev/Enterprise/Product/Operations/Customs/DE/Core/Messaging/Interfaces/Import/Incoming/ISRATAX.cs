using System;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISRATAX : IDataProvider
	{
		string ReferenceNumber { get; }

		string MRN { get; }

		string LocalReferenceNumber { get; }

		string TaxChangeAssessmentType { get; }

		DateTime? TaxAssessmentCreationDate { get; }

		DateTime? MaturityDate { get; }

		string InterchangeRecipientEBS { get; }
	}
}
