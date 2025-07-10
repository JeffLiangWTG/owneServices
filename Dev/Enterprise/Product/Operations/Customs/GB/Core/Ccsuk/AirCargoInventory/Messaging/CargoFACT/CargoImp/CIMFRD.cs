using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CIMFRD : CIMFCS
	{
		public CIMFRD(ZString airportOfArrival, ZString cargoTerminalOperator, ZString airWaybillNumberFormatted, ZString houseAirWaybillNumber, NonPersistentSplitsAndFlightData npSplitsAndFlightData, ErrorCollector ec, ZString agentBadge)
			: base(airportOfArrival, cargoTerminalOperator, airWaybillNumberFormatted, houseAirWaybillNumber, npSplitsAndFlightData.SplitLines, ec)
		{
			this.agentBadge = agentBadge;
			this.npSplitsAndFlightData = npSplitsAndFlightData;
		}

		public override ZString CargoImpCode
		{
			get { return Code; }
		}
		public const string Code = "FRD";

		protected override ZString GetHandlingDetail(NonPersistentSplitLine split)
		{
			return split.HandlingDetail.IsEmpty ? "" : "/" + split.HandlingDetail;
		}

		protected override void AddFlightArrivalDetails(List<string> result)
		{
			if (npSplitsAndFlightData.SendFlightInfoToo && !npSplitsAndFlightData.FlightNumber.IsEmpty && !npSplitsAndFlightData.FlightArrivalDate.IsEmpty)
			{
				result.Add("ARR" +                                                      //4.1
							slash +                                                     //4.2.1
							npSplitsAndFlightData.FlightNumber +                        //4.2.2 + 4.2.3
							slash +                                                     //4.2.4
							npSplitsAndFlightData.FlightArrivalDate.ToString("ddMMM")   //4.2.5 + 4.2.6
							);
			}
		}

		protected override void AddAgentBadge(List<string> result)
		{
			result.Add("AGT" +                  //5.1
						slash +                 //5.3
						agentBadge);            //5.4
		}

		ZString agentBadge;
		readonly NonPersistentSplitsAndFlightData npSplitsAndFlightData;
	}
}
