using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT007DataProvider : BaseNctsMessageDataProvider<NctsHeaderArrivalMessageSendingObject>, INT007
{
	public NT007DataProvider(NctsHeaderArrivalMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public string ArrivalOperationTraderAtDestinationReferenceNumber => nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum;

	public ITrader TraderAtDestination => traderAtDestination ??= TraderDataProvider.New(nctsHeader);
	ITrader traderAtDestination;

	public string ApprovedLocationOfGoodsIdentificationNumber => nctsHeader.ArrivalMovementHeader.GoodsLocation.Address.E2_GovRegNum;

	public ITransportMeans TransportMeans => transportMeans ??= TransportMeansAtArrivalDataProvider.New(nctsHeader.ArrivalMovementHeader);
	ITransportMeans transportMeans;

	public IReadOnlyCollection<IGoodsDeclaration> GoodsDeclarations => goodsDeclarations ??= GoodsDeclarationDataProvider.NewCollection(nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers).ToArray();
	IReadOnlyCollection<IGoodsDeclaration> goodsDeclarations;

	public IReadOnlyCollection<ISupernumeraryGoods> SupernumeraryGoods => supernumeraryGoods ??= SupernumeraryGoodsDataProvider.NewCollection(nctsHeader.ArrivalMovementHeader.SupernumeraryGoods).ToArray();
	IReadOnlyCollection<ISupernumeraryGoods> supernumeraryGoods;

	public IReadOnlyCollection<IAdditionalTransitOperation> AdditionalTransitOperations => additionalTransitOperations ??= AdditionalTransitOperationDataProvider.NewCollection(nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations).ToArray();
	IReadOnlyCollection<IAdditionalTransitOperation> additionalTransitOperations;

	protected override IOppositeInformation GetOppositeInformationData(NctsHeader nctsHeader, ZString? referenceNumberInput = null) => OppositeInformationDataProvider.New(nctsHeader, MessageIdentification);
}
