using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDUACompleteImportMessageDataProvider : IDUAImportDataProvider
	{
		ZDateTime ProcedureDate { get; }
		IDUACompleteImportHeader Header { get; }
		IReadOnlyCollection<IDUACompleteImportLine> Lines { get; }
	}

	public interface IDUACompleteImportHeader : IDUAImportCommonHeader
	{
		IDUACompleteImportExporterProvider Exporter { get; }
		ZString DestinationCountry { get; }
		ZString DestinationState { get; }
		ZString ArrivalTransportId { get; }
		IDUACompleteImportDeliveryConditions DeliveryConditions { get; }
		ZString FrontierTransportCountry { get; }
		ZDecimal InvoiceAmount { get; }
		ZString TransactionNature { get; }
		ZString FrontierTransportMode { get; }
		ZString InteriorTransportMode { get; }
		ZString CustomsOfficeOfEntry { get; }
		ZString DepositId { get; }
	}

	public interface IDUACompleteImportExporterProvider : IPartyProvider
	{
		ZString SimplifiedProcedureType { get; }
	}

	public interface IDUACompleteImportDeliveryConditions
	{
		ZString Code { get; }
		ZString Place { get; }
		ZString ZoneIndicator { get; }
	}

	public interface IDUACompleteImportLine : IDUAImportCommonLine
	{
		ZString REACode { get; }
		ZDecimal PositiveAdjustment { get; }
		ZDecimal NegativeAdjustment { get; }
		ZDecimal StatisticalValue { get; }
	}
}
