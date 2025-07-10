using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IENSDeviationMessageDataProvider : IENSGenericMessageDataProvider
	{
		IENSDeviationHeader Header { get; }
		ZString ActualEntryCustomsOffice { get; }
		ZString FirstEntryCustomsOffice { get; }
		IENSAddressInformation RequestingTrader { get; }
		IReadOnlyCollection<IENSDeviationImportOperation> ImportOperations { get; }
	}

	public interface IENSDeviationHeader
	{
		ZString BorderTransportMode { get; }
		ZString FirstEntryOfficeCountry { get; }
		ZString InformationType { get; }
		ZString DeviationReferenceNumber { get; }
		ZString TransportId { get; }
		ZDateTime ExpectedArrivalDate { get; }
	}

	public interface IENSDeviationImportOperation
	{
		ZString ReferenceNumber { get; }
		IReadOnlyCollection<ZString> GoodsItems { get; }
	}
}
