using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMWarehouseHeaderProcedureWrapper : IMHeaderWrapper
{
	public IMWarehouseHeaderProcedureWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	protected override ZDecimal? DeliveryCostsCore => null;

	protected override ZString ProvinceOfDestinationCore => ZString.Empty;

	protected override IIMHeaderEntryCustomsOffice EntryCustomsOfficeCore => new IMHeaderEmptyEntryCustomsOfficeWrapper();

	protected override IMeansOfTransport MeansOfTransportOnArrivalCore => new SADMeansOfTransportWrapper(ZString.Empty, ZString.Empty);

	protected override ITermOfDeliveryGroup TermsOfDeliveryCore => new SADEmptyTermsOfDeliveryWrapper();

	protected override IMeansOfTransport MeansOfTransportCrossingBorderCore => new SADMeansOfTransportWrapper(ZString.Empty, ZString.Empty);

	protected override ITransactionData TransactionDataCore => new SADEmptyTransactionDataWrapper();

	protected override ZString CountryOfDestinationCore
	{
		get
		{
			if (EntryInstruction.RandomProcedure?.IsIntoWarehouseForReExport ?? ZBool.False)
			{
				return JobDeclaration.CountryOfDestinationCode;
			}
			return ZString.Empty;
		}
	}

	protected override ZBool? IsContainerizedTransportCore => null;

	protected override ZString TransportModeAtBorderCore => ZString.Empty;

	protected override ZString InlandTransportModeCore => ZString.Empty;
}
