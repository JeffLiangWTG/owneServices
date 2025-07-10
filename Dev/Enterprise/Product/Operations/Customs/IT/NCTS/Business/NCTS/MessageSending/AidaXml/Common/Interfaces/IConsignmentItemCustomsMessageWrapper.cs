using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public interface IConsignmentItemCustomsMessageWrapper
{
	string DeclarationType { get; }

	int GoodsItemNumber { get; }

	int DeclarationGoodsItemNumber { get; }

	IReadOnlyCollection<IConsignmentItemPreviousDocument> PreviousDocuments { get; }

	IReadOnlyCollection<IAdditionalInformation> AdditionalInformation { get; }

	IReadOnlyCollection<ISupportingDocument> SupportingDocuments { get; }

	IReadOnlyCollection<ITransportDocument> TransportDocuments { get; }

	IReadOnlyCollection<IAdditionalReference> AdditionalReferences { get; }

	string Ucr { get; }

	ITrader Consignee { get; }

	IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors { get; }

	string TransportChargesMethodOfPayment { get; }

	string CountryOfDispatch { get; }

	string CountryOfDestination { get; }

	decimal? NetMass { get; }

	decimal? GrossMass { get; }

	decimal? SupplementaryUnits { get; }

	string DescriptionOfGoods { get; }

	IReadOnlyCollection<IPackage> Packages { get; }

	string CusCode { get; }

	string HsTariffCode { get; }

	string NcTariffCode { get; }

	IReadOnlyCollection<string> DangerousGoodsCodes { get; }
}
