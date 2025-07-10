namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class MessageSendingWrapperFactory : IMessageSendingWrapperFactory
{
	IHouseConsignmentCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewHouseConsignmentCustomsMessageWrapper(NctsBill bill)
	{
		return new HouseConsignmentCustomsMessageWrapper(bill);
	}

	IConsignmentItemCustomsMessageWrapper IMessageSendingWrapperFactory.GetNewConsignmentItemCustomsMessageWrapper(NctsDepartureCargoDesc goodsItem)
	{
		return new ConsignmentItemCustomsMessageWrapper(goodsItem);
	}

	INctsHeaderWrapper IMessageSendingWrapperFactory.GetNewNctsHeaderWrapper(NctsHeader header)
	{
		return new NctsHeaderWrapper(header);
	}
}
