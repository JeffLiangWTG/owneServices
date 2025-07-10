using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public interface IDocumentRequestMessageProvider
	{
		ZString MovementReferenceNumber { get; }

		ZString LRN { get; }

		ZDateTime RequestDate { get; }

		ZDateTime DateLimit { get; }

		IReadOnlyCollection<DocumentAdditionalInformationProvider> AdditionalInformations { get; }
	}
}
