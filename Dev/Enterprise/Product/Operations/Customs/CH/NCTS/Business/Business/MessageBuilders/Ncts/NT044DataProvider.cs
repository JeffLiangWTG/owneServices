using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT044DataProvider : BaseNctsMessageDataProvider<NctsHeaderArrivalMessageSendingObject>, INT044
{
	public NT044DataProvider(NctsHeaderArrivalMessageSendingObject sendingObject) : base(sendingObject)
	{
		movementHeader = nctsHeader.ArrivalMovementHeader;
	}
	readonly NctsArrivalMovementHeader movementHeader;

	public string TraderAtDestinationIdentificationNumber => nctsHeader.DestinationTrader.Organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);

	public bool UnloadingConform => movementHeader.BM_NoChangesToReport;

	public DateTime UnloadingDate => movementHeader.BM_UnloadingDate.Date.ToDateTime();

	public string UnloadingRemarkText => movementHeader.BM_UnloadingRemarks.ReturnNullIfEmpty();

	public IBaseTransitOperation TransitOperation => transitOperation ?? (transitOperation = BaseTransitOperationDataProvider.New(nctsHeader));
	IBaseTransitOperation transitOperation;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => movementHeader.BM_NoChangesToReport ? null : transportEquipments ?? (transportEquipments = InventoryTransportEquipmentDataProvider.NewCollection(nctsHeader.ArrivalHeaderContainers).ToArray());
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public IReadOnlyCollection<IHouseConsignment> HouseConsignments => houseConsignments ??= movementHeader.BM_NoChangesToReport ? null : InventoryHouseConsignmentDataProvider.NewCollection(nctsHeader.Bills).ToArray();
	IReadOnlyCollection<IHouseConsignment> houseConsignments;

	protected override string GetCorrelationIdentifier()
	{
		var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, nctsHeader.PK);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CHCustomsPassar);
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeCodeList.Codes.MSG);
		query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageSubTypeCodeList.Codes.PassarInventoryRequest);
		query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);
		query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;

		return nctsHeader.Factory.LoadTop1<EDIMessage>(query)?.EM_ApplicationReference ?? string.Empty;
	}
}
