using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMNonWarehouseHeaderProcedureWrapper : IMHeaderWrapper
{
	public IMNonWarehouseHeaderProcedureWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override ZDecimal? DeliveryCostsCore => EntryHeader.CH_FreightAdjustment;

	protected override ZString ProvinceOfDestinationCore => JobDeclaration.FinalDestination?.CountryStates?.RW_Code ?? ZString.Empty;

	protected override IIMHeaderEntryCustomsOffice EntryCustomsOfficeCore => new IMHeaderEntryCustomsOfficeWrapper(JobDeclaration);

	protected override IMeansOfTransport MeansOfTransportOnArrivalCore => new SADMeansOfTransportWrapper(JobDeclaration.ZG_Box18TransportNationality, JobDeclaration.ZG_Box18TransportID);

	protected override ITermOfDeliveryGroup TermsOfDeliveryCore => new SADTermsOfDeliveryWrapper(EntryHeader);

	protected override IMeansOfTransport MeansOfTransportCrossingBorderCore
	{
		get
		{
			var nationality = JobDeclaration.TransportNationality?.Code ?? ZString.Empty;
			return new SADMeansOfTransportWrapper(nationality, JobDeclaration.MeansOfTransportCrossingBorderIdentity);
		}
	}

	protected override ITransactionData TransactionDataCore => new SADTransactionDataWrapper(EntryHeader);

	protected override ZString CountryOfDestinationCore => JobDeclaration.CountryOfDestinationCode;

	protected override ZBool? IsContainerizedTransportCore => CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(JobDeclaration.ContainerMode);

	protected override ZString TransportModeAtBorderCore => JobDeclaration.TransportModeTranslator.TranslateToWCOCode(JobDeclaration.JE_TransportMode);

	protected override ZString InlandTransportModeCore => JobDeclaration.TransportModeTranslator.TranslateToWCOCode(JobDeclaration.JE_TransportModeInland);
}
