using System;
using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Types;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public interface ICusEntryInstructionCustomsMessageWrapper
{
	ZString AdditionalDeclarationType { get; }
	IReadOnlyCollection<IAuthorization> Authorizations { get; }
	IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors { get; }
	IWarehouse Warehouse { get; }
	DateTime? AcceptanceDate { get; }
	IReadOnlyCollection<IGuarantee> Guarantees { get; }
	IReadOnlyCollection<string> GuaranteeTypes { get; }
	IReadOnlyCollection<IFiscalReference> FiscalReferences { get; }
	string GetGuaranteeHolderIdentificationNumber(IEoriTrader declarant);
	DateTime? GoodsPresentationDateTime { get; }
	int? InlandTransportMode { get; }
	IReadOnlyCollection<IMeansOfTransport> DepartureMeansOfTransports { get; }
	IReadOnlyCollection<IPreviousDocument> PreviousDocuments { get; }
	IReadOnlyCollection<MessageBuilder.ISupportingDocument> SupportingDocuments { get; }
}
