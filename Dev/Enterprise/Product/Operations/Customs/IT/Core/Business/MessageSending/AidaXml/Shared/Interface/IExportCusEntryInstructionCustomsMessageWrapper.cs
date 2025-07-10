using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public interface IExportCusEntryInstructionCustomsMessageWrapper : ICusEntryInstructionCustomsMessageWrapper
{
	IReadOnlyCollection<IAdditionalInformation> AdditionalInformation { get; }
	IReadOnlyCollection<ITransportDocument> TransportDocuments { get; }
	IReadOnlyCollection<IAdditionalReference> AdditionalReferences { get; }
}
