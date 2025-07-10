using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM484Provider
{
	ZString MovementReferenceNumber { get; }

	ZString LocalReferenceNumber { get; }

	ZDateTime RequestDate { get; }

	ZDateTime DateLimit { get; }

	IReadOnlyCollection<IDocumentAdditionalInformationProvider> AdditionalInformations { get; }
}
