using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IExportHeader : IHeader
	{
		/// <summary>
		/// Declaration types: Full and PSA.
		/// Screen name: Goods avail from
		/// EDI data element: GDS-ARR-DTM-INLD
		/// This information is only to be supplied by traders authorised for, and submitting declarations under, the
		/// Local Clearance Procedure (LCP). It is used to declare when the goods will be placed under Customs
		/// control at approved LCP premises and be available for examination at the location specified in Box 30.
		/// </summary>
		ZDateTime DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises { get; }

		/// <summary>
		/// Declaration types: Full and PSA.
		/// Screen name: (Goods avail)
		/// EDI data element: GDS-DEP-DTM-INLD
		/// This information is only to be supplied by traders authorised for, and submitting declarations under, the
		/// Local Clearance Procedure (LCP). It is used to declare when it is intended that the goods will be
		/// leaving the approved LCP premises. The goods must not leave the premises before this date and time
		/// unless specific Customs approval to do so has been given.
		/// The date/time must be later than the date/time it was declared the goods will be available for inspection
		/// at LCP premises (see above).
		/// </summary>
		ZDateTime DateAndTimeTheGoodsWillBeLeavingLCPPremises { get; }

		/// <summary>
		/// Declaration types: SDE
		/// Screen name: Exp Date
		/// EDI data element: GDS-DEP-DT
		/// Enter the date on which the goods left the UK.
		/// Note. For SDEs submitted by traders authorised to use the special victuallers scheduling scheme, enter
		/// the last day of the month that goods were supplied under the scheme.
		/// </summary>
		ZDate GoodsDepartureDate { get; }
		ZString OfficeOfExit { get; } // EXIT-OFFICE

		ZString CountryOfDestination { get; }   // DEST-CNTRY
		ZBool FECCountryOfDestination { get; } // PROC-INST "DST"
		ZBool FECProgressReport { get; } // PROC-INST "PRG"
		ZString CTStatusID { get; } // DECLN-TYPE "CTSTATUS"

		ZString RegisteredConsignorTurn { get; } // RCNSGR-TURN
	}
}
