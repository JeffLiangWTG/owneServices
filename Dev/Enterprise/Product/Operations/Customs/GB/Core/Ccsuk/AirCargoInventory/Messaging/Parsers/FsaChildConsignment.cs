using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class FsaChildConsignment
	{
		public FsaChildConsignment()
		{
			CommunityHandlingCodes = new List<ZString>();
		}

		public ZString ConsignmentReferenceNumber { get; set; }
		public ZString ConsignmentReferenceNumberType { get; set; }
		public ZString OldOrNewDataIndicator { get; set; }

		public ConsignmentLegFlight InwardLeg { get; set; }
		public ConsignmentLegFlight OnwardLeg { get; set; }

		#region MISC - to be tidied 
		/// <summary>
		///  N10	Format – YYMMDDHHMM
		/// </summary>
		public ZString ReportDateTime { get; set; }

		/// <summary>
		/// N10	Date time on which waybill details were created
		/// </summary>
		public ZDateTime DatetimeOfRecordCreation { get; set; }

		/// <summary>
		/// A	If arriving on a single flight / multiple flights, or if Community goods (don't care how many flights)
		/// </summary>
		public ZString ShipmentDescriptionCode { get; set; }

		/// <summary>
		/// A	Import, transit, transhipment
		/// </summary>
		public ZString ConsignmentType { get; set; }

		/// <summary>
		/// AN2..3	Prefix of flight number on which consignment (or first part) arrived
		/// </summary>
		public ZString InboundCarrierCode { get { return InwardLeg != null ? InwardLeg.Carrier : ZString.Empty; } }

		/// <summary>
		/// AN3..5	Flight number on which consignment (or first part) arrived
		/// </summary>
		public ZString InboundFlightNumber { get { return InwardLeg != null ? InwardLeg.FlightNumber : ZString.Empty; } }

		/// <summary>
		/// N6 or N10	Date on which consignment (or first part) arrived. Format - YYMMDD or YYMMDDHHMM
		/// </summary>
		public ZDateTime DateOfArrival { get { return InwardLeg != null ? InwardLeg.Date : ZDateTime.Empty; } }

		/// <summary>
		/// 	A..6	Id of customs user making entry
		/// </summary>
		public ZString CustomsUserID { get; set; }

		/// <summary>
		/// AN2..3	
		/// </summary>
		public ZString CurrencyCode { get; set; }

		#endregion

		#region Group 2 Status flags and idicators

		/// <summary>
		/// AN..70	Text explanation of error
		/// </summary>
		public ZString IndicatorErrorText { get; set; }

		/// <summary>
		/// A
		/// </summary>
		public ZBool IndicatorLicenceRestricted { get; set; }

		/// <summary>
		/// A 	Up to 9 verification indicators(up to 9 in total)
		/// </summary> 
		public ZBool VerificationIndicatorOne { get; set; }
		public ZBool VerificationIndicatorTwo { get; set; }
		public ZBool VerificationIndicatorThree { get; set; }
		public ZBool VerificationIndicatorFour { get; set; }
		public ZBool VerificationIndicatorFive { get; set; }
		public ZBool VerificationIndicatorSix { get; set; }
		public ZBool VerificationIndicatorSeven { get; set; }
		public ZBool VerificationIndicatorEight { get; set; }
		public ZBool VerificationIndicatorNine { get; set; }

		/// <summary>
		/// A	if consignment has not yet arrived
		/// </summary>
		public ZBool PreArrivalIndicator { get; set; }

		/// <summary>
		/// A
		/// </summary>
		public ZBool CurrentStatusIndicator { get; set; }

		/// <summary>
		/// A
		/// </summary>
		public ZBool DetainedTextIndicator { get; set; }

		/// <summary>
		/// A
		/// </summary>
		public ZBool SpecialActionIndicator { get; set; }

		public List<ZString> CommunityHandlingCodes { get; private set; }

		#endregion

		#region Group 3 Onward details
		/// <summary>
		/// A3 or N3
		/// </summary>
		public ZString OnwardAirWaybillNumberAndPrefix { get { return OnwardLeg != null ? OnwardLeg.Awb : ZString.Empty; } }

		/// <summary>
		/// AN2..3	Prefix of flight number on which consignment will depart
		/// </summary>
		public ZString OnwardCarrierCode { get { return OnwardLeg != null ? OnwardLeg.Carrier : ZString.Empty; } }

		/// <summary>
		/// AN3..5	Flight number on which consignment will depart
		/// </summary>
		public ZString OnwardFlightNumber { get { return OnwardLeg != null ? OnwardLeg.FlightNumber : ZString.Empty; } }

		/// <summary>
		/// N2	If not by air
		/// </summary>
		public ZString OnwardTransportMeans { get { return OnwardLeg != null ? OnwardLeg.Mode : ZString.Empty; } }

		#endregion

		#region Group 4 Agent details

		/// <summary>
		/// A3	Nominated agent for this consignment
		/// </summary>
		public ZString AgentCode { get; set; }

		/// <summary>
		/// AN..35	Text explanation of code
		/// </summary>
		public ZString AgentName { get; set; }

		/// <summary>
		/// A6
		/// </summary>
		public ZString PrintLocation1 { get; set; }

		/// <summary>
		/// A6
		/// </summary>
		public ZString PrintLocation2 { get; set; }

		#endregion

		#region Group 5 Weights & measures

		/// <summary>
		/// N..18
		/// </summary>
		public ZDecimal ValueOfGoods { get; set; }

		/// <summary>
		/// AN..15	Text description
		/// </summary>
		public ZString DescriptionOfGoods { get; set; }

		/// <summary>
		/// N..4 Total packages expected
		/// </summary>
		public ZInt NPX { get; set; }

		/// <summary>
		/// N..4	Total packages received
		/// </summary>
		public ZInt NPR { get; set; }

		/// <summary>
		/// N..7	Gross weight of goods expected
		/// </summary>
		public ZDecimal Weight { get; set; }

		/// <summary>
		/// AN3	Indicates unit of Measure for consignment weight (KGM for Kilogrames)
		/// </summary>
		public ZString WeightCode { get; set; }

		/// <summary>
		/// N10	Date time when NPX first equalled NPR
		/// </summary>
		public ZDateTime Status1Date { get; set; }

		/// <summary>
		/// N8 Date time, Format – YYYYMMDD
		/// </summary>
		public ZDateTime TemporaryStorageEndDate { get; set; }

		/// <summary>
		/// N4..8	may occur twice
		/// </summary>
		public ZString HarmonisedCommodityCode1 { get; set; }
		public ZString HarmonisedCommodityCode2 { get; set; }

		#endregion

		#region Group 6 - customs statuses

		/// <summary>
		/// EPU N3
		/// </summary>
		public ZString EntryProcessingUnit { get; set; }

		/// <summary>
		/// N..3	IEVR-NO
		/// </summary>
		public ZInt InventoryVersionNo { get; set; }

		/// <summary>
		/// AN7
		/// </summary>
		public ZString EntryNumber { get; set; }

		/// <summary>
		/// N8	Format CCYYMMDD
		/// </summary>
		public ZDateTime EntryDate { get; set; }

		/// <summary>
		/// AN..8	Agent's reference to entry
		/// </summary> 
		public ZString AgentsReferenceNumber { get; set; }

		/// <summary>
		/// N..4
		/// </summary>
		public ZInt NumPackagesEntered { get; set; }

		/// <summary>
		/// AN..2	
		/// </summary>
		public ZString Route { get; set; }

		/// <summary>
		/// AN..2
		/// </summary>
		public ZString CustomsClearanceStatus { get; set; }

		/// <summary>
		/// A2 or N2
		/// </summary>
		public ZString CustomsActionCode { get; set; }

		/// <summary>
		/// AN..20	
		/// </summary>
		public ZString CustomsActionText { get; set; }

		/// <summary>
		/// 	N10	
		/// </summary>
		public ZDateTime DateOfCustomsAction { get; set; }

		/// <summary>
		/// N3	Result of inventory check
		/// </summary>

		public ZString InventoryReturnCodeIRC
		{
			get { return inventoryReturnCodeIRC; }
			set
			{
				inventoryReturnCodeIRC = value;
				InventoryReturnCodeMeaning = new Business.CodeDescriptionPairLists.InventoryReturnCodesCCS().GetDescriptionFromCode(value);
			}
		}
		public ZString InventoryReturnCodeMeaning { get; private set; }
		ZString inventoryReturnCodeIRC;

		/// <summary>
		/// AN..70	Up to 2 lines of 70 chars
		/// </summary>
		public ZString IRCText { get; set; }

		#endregion

	}
}
