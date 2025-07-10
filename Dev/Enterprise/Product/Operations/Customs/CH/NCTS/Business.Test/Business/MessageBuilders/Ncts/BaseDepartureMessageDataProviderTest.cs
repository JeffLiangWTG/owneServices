using System;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseDepartureMessageDataProviderTest<TDataProvider, TMessageSendingObject> : BasePassarMessageDataProviderTest<TDataProvider>
	where TDataProvider : class, IPassarMessage
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	protected NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	protected NctsBill NctsBill => nctsBill ?? (nctsBill = CreateNctsBill());
	NctsBill nctsBill;

	NctsBill CreateNctsBill()
	{
		var nctsBill = NctsHeader.Bills.AddNew();
		nctsBill.GoodsItems.AddNew();
		return nctsBill;
	}

	protected NctsDepartureCargoDesc GoodsItem => NctsBill.GoodsItems.FirstOrDefault() ?? NctsBill.GoodsItems.AddNew();

	protected TMessageSendingObject MessageSendingObject => messageSendingObject ?? (messageSendingObject = CreateMessageSendingObject());
	TMessageSendingObject messageSendingObject;

	protected TMessageSendingObject CreateMessageSendingObject() => (TMessageSendingObject)Activator.CreateInstance(typeof(TMessageSendingObject), NctsHeader);
}
