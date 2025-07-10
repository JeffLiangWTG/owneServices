using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsSADHeaderCommonWrapper : IETHeader
{
	protected NctsSADHeaderCommonWrapper(NctsHeader nctsHeader)
	{
		NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		NctsMovementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}
	protected NctsHeader NctsHeader { get; }
	protected NctsDepartureMovementHeader NctsMovementHeader { get; }

	public ZBool HeaderDataDeclaredOnItems => NctsMovementHeader.ParticipantType == NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;

	public ZBool? SecurityData => NctsHeader.BH_FTZMove;

	public IMeansOfTransport MeansOfTransportAtDeparture => new SADMeansOfTransportWrapper(NctsMovementHeader.BM_RN_NKTransportAtDepartureCountry, NctsMovementHeader.BM_TransportAtDeparture);

	public IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorder => MeansOfTransportCrossingBorderCore;
	protected abstract IETHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorderCore { get; }

	public ZString DialogLanguageIndicatorAtDeparture => ZString.Empty;

	public IETHeaderSecurityBlock SecurityBlock => SecurityData.Value ? new NctsSADHeaderSafetyAndSecurityBlockWrapper(NctsHeader) : new NctsSADHeaderSecurityBlockWrapper(NctsHeader);

	public ZString ExitCustomsOffice => ZString.Empty;

	public IETHeaderAgreedLocationOfGoods AgreedLocationOfGoods => new NctsSADHeaderAgreedLocationOfGoodsWrapper(NctsHeader);

	public IETHeaderPrincipalTrader PrincipalTrader => PrincipalTraderCore;
	protected abstract IETHeaderPrincipalTrader PrincipalTraderCore { get; }

	public IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOffices => TransitCustomsOfficesCore;
	protected abstract IEnumerable<IETHeaderTransitCustomsOffice> TransitCustomsOfficesCore { get; }

	public IEnumerable<IETHeaderGuarantee> Guarantees => GuaranteesCore;
	protected abstract IEnumerable<IETHeaderGuarantee> GuaranteesCore { get; }

	public ZString DestinationCustomsOffice => NctsHeader.DestinationCustomsOfficeCode;

	public IEnumerable<ZString> Seals => NctsMovementHeader.BM_SealType == SealTypeList.Codes.PackageSeal ? NctsHeader.Seals.AllSeals.Distinct() : NctsHeader.DepartureHeaderContainers.AllSeals.Distinct();

	public IETHeaderControlResult ControlResult => new NctsSADHeaderControlResultWrapper(NctsMovementHeader);

	public ZString AnnualProgressiveNumber => Enterprise.Messaging.Business.EDIMessage.MessageNumberPlaceHolder;

	public ZString AuthorizationNo => AuthorizationNoCore;
	protected abstract ZString AuthorizationNoCore { get; }

	public ZString AuthorizationCIN => AuthorizationCINCore;
	protected abstract ZString AuthorizationCINCore { get; }

	public ZInt TotalItems => NctsMovementHeader.GoodsItems.Count;

	public ZDate AcceptanceDate => NctsMovementHeader.BM_EntryDate.Date;

	public IDeclaration Declaration => DeclarationCore;
	protected abstract IDeclaration DeclarationCore { get; }

	public ITrader Consignor => new SADTraderWrapper(NctsHeader.Consignor);

	public ITrader Consignee => new SADTraderWrapper(NctsHeader.Consignee);

	public IDeclarantTrader DeclarantTrader => new SADDeclarantTraderWrapper(NctsHeader);

	public ZString CountryOfDispatch => NctsHeader.BH_RL_NKImportLoadPort;

	public ITermOfDeliveryGroup TermsOfDelivery => new SADEmptyTermsOfDeliveryWrapper();

	public ITransactionData TransactionData => TransactionDataCore;
	protected abstract ITransactionData TransactionDataCore { get; }

	public IDeferredPayment DeferredPayment => new NctsSADHeaderDeferredPaymentWrapper(NctsMovementHeader);

	public IWarehouseIdentification WarehouseIdentification => new NctsSADHeaderWarehouseIdentificationWrapper(NctsMovementHeader);

	public ZString CountryOfDestination => NctsMovementHeader.BM_RL_NKDestinationPort;

	public ZBool? IsContainerizedTransport => NctsHeader.DepartureHeaderContainers.Any();

	public ZString TransportModeAtBorder => NctsMovementHeader.BM_ExportTransportMode;

	public ZString InlandTransportMode => NctsMovementHeader.BM_InlandTransportMode;

	public ZDate DateLimitOfTemporaryOperation => ZDate.Empty;
}
