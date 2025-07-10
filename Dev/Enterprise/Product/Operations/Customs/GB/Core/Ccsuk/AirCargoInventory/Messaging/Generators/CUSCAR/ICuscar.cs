using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public interface ICuscar : ICuscarLine
	{
		ZString AirportOfArrival { get; }
		ZString CargoTerminalOperator { get; }
		ZString AirlinePrefix { get; }
		ZString AirWaybillSerialNumber { get; }
		ZString HouseAirWaybillNumber { get; }
		ZString SplitReference { get; }
		ZDateTime DateTimeOfRecordCreation { get; }
		ZString AirportOfOrigin { get; }
		ZString AirportOfDestination { get; }
		ZString ShipmentDescriptionCode { get; }
		ZString CarrierCode { get; }
		ZString FlightNumber { get; }
		ZDateTime DateOfFlightArrival { get; }
		ZBool Status2Indicator { get; }
		ZString AgentBrokerConsolidatorCode { get; }
		ZBool IsPrearrival { get; }
		List<ZString> CommunityHandlingCodes { get; }
		ZDateTime Status1Date { get; }
	}

	public interface ICuscarLine : ISplitLine
	{
		ZString WeightCode { get; }
		ZShort NumberOfPiecesReceived { get; }
		ZString HarmonisedCommodityCode { get; }
	}

	public interface ISplitLine
	{
		ZString LineOrSplitNumber { get; }
		ZShort NumberOfPiecesExpected { get; }
		ZDecimal Weight { get; }
		ZString DescriptionOfGoods { get; }
	}
}

