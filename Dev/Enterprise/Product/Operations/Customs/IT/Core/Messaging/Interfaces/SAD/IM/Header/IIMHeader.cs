using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMHeader : IHeaderCommon
{
	IIMHeaderCompanyRegister CompanyRegister { get; }
	ZBool PreClearing { get; }
	ZDecimal? DeliveryCosts { get; }
	ZString ProvinceOfDestination { get; }
	IIMHeaderEntryCustomsOffice EntryCustomsOffice { get; }
	IIMHeaderLocationOfGoods LocationOfGoods { get; }
	IMeansOfTransport MeansOfTransportOnArrival { get; }
	IMeansOfTransport MeansOfTransportCrossingBorder { get; }
}
