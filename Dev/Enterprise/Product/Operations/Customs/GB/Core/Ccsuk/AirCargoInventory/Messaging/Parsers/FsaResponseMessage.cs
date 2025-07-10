using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class FsaResponseMessage
	{
		public ZString CargoWise_CommonAccessReference { get; set; }

		/// <summary>
		/// AN..3	Report type if a report, eg. E0
		/// </summary>
		public ZString Header_ReportType { get; set; }

		/// <summary>
		/// AN..70	Any text associated with the report. Up to 5 lines of 70 chars.
		/// </summary>
		public ZString Header_ReportText { get; set; }

		#region Group 1 Consignment identifiers - identify the object that was queried.  Group 1 DOCs show child objects

		/// <summary>
		/// A3 or N3	Identifies the issuer of the waybill .... plus N8
		/// </summary>
		public ZString Header_AirwaybillPrefixAndAirwaybillNumber { get; set; }

		/// <summary>
		/// AN8	If a consolidation
		/// </summary>
		public ZString Header_HouseWaybillNumber { get; set; }

		/// <summary>
		/// N2	If waybill has been split for entry
		/// </summary>
		public ZString Header_SplitReference { get; set; }

		#endregion

		// One for each Group 1
		public List<FsaChildConsignment> ChildConsignments { get; set; }

		bool IsReportP5
		{
			get { return Header_ReportType == "P5"; }
		}

		internal bool IsReportP5Insert
		{
			get { return IsReportP5 && !GetAirportAndShed("NEW").IsEmpty && !GetAirportAndShed("OLD").IsEmpty && GetAirportAndShed("").IsEmpty; }
		}

		internal bool IsReportP5Delete
		{
			get { return IsReportP5 && GetAirportAndShed("NEW").IsEmpty && GetAirportAndShed("OLD").IsEmpty && !GetAirportAndShed("").IsEmpty; }
		}

		public ZString GetAirportAndShed(string oldOrNewIndicator)
		{
			var airportAndShed = "";
			var consignmentWithNewIndicator = (from FsaChildConsignment c in ChildConsignments where c.OldOrNewDataIndicator == oldOrNewIndicator select c).FirstOrDefault();
			if (consignmentWithNewIndicator != null)
			{
				var arrivalAirport = consignmentWithNewIndicator.InwardLeg.Locations.AirportOfArrival;
				airportAndShed = arrivalAirport.LocationCode + arrivalAirport.ShedOperator;
			}
			return airportAndShed;
		}

		internal ZInt GetNewNPX()
		{
			var consignmentWithNewIndicator = (from FsaChildConsignment c in ChildConsignments where c.OldOrNewDataIndicator == "NEW" select c).FirstOrDefault();
			return consignmentWithNewIndicator?.NPX ?? 0;
		}

		public ZString GetJobNumber() => Header_AirwaybillPrefixAndAirwaybillNumber
								+ (Header_HouseWaybillNumber.IsEmpty ? "" : ("-" + Header_HouseWaybillNumber))
								+ (Header_SplitReference.IsEmpty ? "" : ("/" + Header_SplitReference));

		public ZString GetJobNumberIncludingShed(string oldOrNewIndicator) => GetAirportAndShed(oldOrNewIndicator) + "-" + GetJobNumber();
	}
}
