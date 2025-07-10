using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class IMHeaderWrapper : SADHeaderCommonWrapper, IIMHeader
{
	public IMHeaderWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	public ZDecimal? DeliveryCosts => DeliveryCostsCore;
	protected abstract ZDecimal? DeliveryCostsCore { get; }

	public ZString ProvinceOfDestination => ProvinceOfDestinationCore;
	protected abstract ZString ProvinceOfDestinationCore { get; }

	public IIMHeaderEntryCustomsOffice EntryCustomsOffice => EntryCustomsOfficeCore;
	protected abstract IIMHeaderEntryCustomsOffice EntryCustomsOfficeCore { get; }

	public IMeansOfTransport MeansOfTransportOnArrival => MeansOfTransportOnArrivalCore;
	protected abstract IMeansOfTransport MeansOfTransportOnArrivalCore { get; }

	public IMeansOfTransport MeansOfTransportCrossingBorder => MeansOfTransportCrossingBorderCore;
	protected abstract IMeansOfTransport MeansOfTransportCrossingBorderCore { get; }

	public ZBool PreClearing => JobDeclaration.ZG_PreClearing;

	public IIMHeaderCompanyRegister CompanyRegister => new IMHeaderCompanyRegisterWrapper();

	public IIMHeaderLocationOfGoods LocationOfGoods => new IMHeaderLocationOfGoodWrapper(EntryHeader);

	protected override IDeclaration DeclarationCore => new IMDeclarationWrapper(JobDeclaration, EntryInstruction);

	protected override IWarehouseIdentification WarehouseIdentificationCore => new IMHeaderWarehouseIdentificationWrapper(EntryInstruction);
}
