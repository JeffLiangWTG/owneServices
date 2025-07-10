using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSConsignmentHeaderMasterLevelProvider : IConsignmentHeaderMasterLevel
{
	public PNTSConsignmentHeaderMasterLevelProvider(TemporaryStorageHeader temporaryStorageHeader)
	{
		this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		temporaryStorageBill = Argument.NotNull(temporaryStorageHeader.MasterBill, nameof(temporaryStorageBill));
	}
	readonly TemporaryStorageHeader temporaryStorageHeader;
	readonly EU.Business.CusTempStorage.TemporaryStorageBill temporaryStorageBill;

	public string PlaceOfUnloadingUNLocode => temporaryStorageBill.ABL_RL_NKPortOfDischarge;

	public IConsignmentMasterLevel ConsignmentMasterLevel => null;

	public IReadOnlyCollection<IConsignmentHouseLevel> ConsignmentHouseLevels => Array.Empty<IConsignmentHouseLevel>();

	public IPNTSLocationOfGoods LocationOfGoods => locationOfGoods ??= temporaryStorageHeader.GoodsLocation != null ? new PNTSLocationOfGoodsProvider((CusGoodsLocation)temporaryStorageHeader.GoodsLocation) : null;
	IPNTSLocationOfGoods locationOfGoods;

	public IArrivalTransportMeans ArrivalTransportMeans => transportMeans ??= temporaryStorageHeader.ArrivalTransportMeans != null ? new PNTSArrivalTransportMeansProvider(temporaryStorageHeader.ArrivalTransportMeans) : null;
	IArrivalTransportMeans transportMeans;

	public IWarehouse Warehouse => warehouse ??= temporaryStorageHeader.AuthorizationUsage != null ? new PNTSWarehouseProvider(temporaryStorageHeader.AuthorizationUsage) : null;
	IWarehouse warehouse;

	public IPNTSPartyWithName Carrier => carrier ??= temporaryStorageHeader.Carrier != null ? new PNTSCarrierProvider(temporaryStorageHeader.Carrier) : null;
	IPNTSPartyWithName carrier;
}
