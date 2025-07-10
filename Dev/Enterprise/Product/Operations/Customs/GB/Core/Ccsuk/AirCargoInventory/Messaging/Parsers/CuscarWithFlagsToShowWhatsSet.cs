using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarWithFlagsToShowWhatsSet : ICuscar
	{
		public CuscarWithFlagsToShowWhatsSet()
		{
			LinesOnlyForFcsResponses = new List<ICuscarLine>();
			CommunityHandlingCodes = new List<ZString>();
		}

		public ZBool AirportOfArrivalSet { get; set; }
		public ZString AirportOfArrival { get; set; }

		public ZBool CargoTerminalOperatorSet { get; set; }
		public ZString CargoTerminalOperator { get; set; }

		public ZBool AirlinePrefixSet { get; set; }
		public ZString AirlinePrefix { get; set; }

		public ZBool AirWaybillSerialNumberSet { get; set; }
		public ZString AirWaybillSerialNumber { get; set; }

		public ZBool HouseAirWaybillNumberSet { get; set; }
		public ZString HouseAirWaybillNumber { get; set; }

		public ZBool SplitReferenceSet { get; set; }
		public ZString SplitReference { get; set; }

		public ZBool DateTimeOfRecordCreationSet { get; set; }
		public ZDateTime DateTimeOfRecordCreation { get; set; }

		public ZBool AirportOfOriginSet { get; set; }
		public ZString AirportOfOrigin { get; set; }

		public ZBool AirportOfDestinationSet { get; set; }
		public ZString AirportOfDestination { get; set; }

		public ZBool ShipmentDescriptionCodeSet { get; set; }
		public ZString ShipmentDescriptionCode { get; set; }

		public ZBool NumberOfPiecesExpectedSet { get; set; }
		public ZShort NumberOfPiecesExpected { get; set; }

		public ZBool WeightCodeSet { get; set; }
		public ZString WeightCode { get; set; }

		public ZBool WeightSet { get; set; }
		public ZDecimal Weight { get; set; }

		public ZBool DescriptionOfGoodsSet { get; set; }
		public ZString DescriptionOfGoods { get; set; }

		public ZBool FlightNumberSet { get; set; }
		public ZString FlightNumber { get; set; }

		public ZBool DateOfFlightArrivalSet { get; set; }
		public ZDateTime DateOfFlightArrival { get; set; }

		public ZBool NumberOfPiecesReceivedSet { get; set; }
		public ZShort NumberOfPiecesReceived { get; set; }

		public ZBool Status2IndicatorSet { get; set; }
		public ZBool Status2Indicator { get; set; }

		public ZBool AgentBrokerConsolidatorCodeSet { get; set; }
		public ZString AgentBrokerConsolidatorCode { get; set; }

		public ZBool HarmonisedCommodityCodeSet { get; set; }
		public ZString HarmonisedCommodityCode { get; set; }

		public ZBool CarrierCodeSet { get; set; }
		public ZString CarrierCode { get; set; }

		public ZBool IsPrearrival { get { return false; } }

		public List<ICuscarLine> LinesOnlyForFcsResponses { get; set; }

		public ZString LineOrSplitNumber { get; set; }

		internal ZString WriteLinesForInterpretation()
		{
			var table = new HtmlTableCreator(new string[] { "Split Reference", "Pieces", "Weight" });
			foreach (var line in LinesOnlyForFcsResponses)
			{
				table.WriteRow(line.LineOrSplitNumber, line.NumberOfPiecesExpected, line.Weight.ToStringTrimZeros() + line.WeightCode);
			}
			return table.ToHtml();
		}

		public List<ZString> CommunityHandlingCodes { get; private set; }
		public bool CommunityHandlingCodesSet
		{
			get { return CommunityHandlingCodes.Count > 0; }
		}

		public ZBool Status1DateSet { get; set; }
		public ZDateTime Status1Date { get; set; }
	}
}
