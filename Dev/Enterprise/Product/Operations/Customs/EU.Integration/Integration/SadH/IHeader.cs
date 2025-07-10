using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IHeader
	{
		List<ZString> CountriesOfRouting { get; }

		/// <summary>
		/// Declaration types: SDE
		/// Screen names: Tax Point date (and) time
		/// EDI data element: ACPTNC-DTM
		/// This is the date and time when CHIEF legally accepted (i.e. not the date and time of submission of the
		/// declaration) the associated PSA, as shown on the X2 print (see ‘Entry accepted’ field on the X2), or its
		/// CUSRES equivalent (data element ACPTNC-DTM).
		/// Under LCP this will be the same date and time that the goods were available for inspection at LCP
		/// premises (see above).
		/// For Memorandum Of Understanding (MOU) traders authorised for LCP or SDP the tax point will be the
		/// date and time the goods were formally entered into their records.
		/// For traders authorised to use CAP victualling simplified procedures, the date and time to be entered is
		/// 2359 hours on the last day of the month to which the claim refers. For example, for goods delivered
		/// during October 2006, the date and time to be entered would be 31/10/06 at 2359 hours.
		/// </summary>
		ZDateTime TaxPointDateAndTime { get; }
		ZString InvoiceCurrency { get; }    // INV-CRRN
		ZString DeclarationType { get; }    // DECLN-TYPE
		IOrganisation ConsignorExporter { get; }
		IOrganisation ConsigneeImporter { get; }
		IOrganisation Warehouse { get; } // PREM-ID
		IOrganisation NotifyParty { get; } // NOTIFY-TID, NOTIFY-NAME

		ZString TypeOfRepresentation { get; }   // DECLT-REP
		IOrganisation DeclarantOrRepresentative { get; } // DECLT-TID ++
		ZString CountryOfExport { get; }    // DISP-CNTRY
		ZString TransportIdentityOnDepartureBox18 { get; }  // TRPT-ID-INLD
		ZString TransportIdentityAtTheBorderBox21 { get; }  // TRPD-ID
		ZString TransportNationalityOnDepartureBox18 { get; }   // ??
		ZString TransportNationalityAtTheBorderBox21 { get; }   // TRPD-CNTRY
		ZString TransportModeAtTheBorderBox25 { get; }  // TRPT-MODE-CODE
		ZString TransportModeInlandBox26 { get; }   //	TRPT-MODE-INLD
		ZString LocationOfGoods { get; }    // GDS-LOCN-CODE

		ZString TradersOwnReference { get; } // TDR-OWN-REF-ENT
		ZString JobReferenceGemsOnly { get; } // Gems only

		IEnumerable<ILine> Lines { get; }

		IEnumerable<IPreviousDocument> PreviousDocuments { get; }   // faked down onto line for convenience
		IEnumerable<IStatement> Statements { get; }

		ZString DeclarationCurrency { get; }    // DECLN-CRRN
		IEnumerable<ISupportingDocument> SupportingDocuments { get; }

		ZString DeclarationUniqueConsignmentReference { get; }  // DECLN-UCR
		ZString DeclarationUniqueConsignmentReferencePartSuffix { get; }    // DECLN-PART-NO

		IOrganisation SupervisingOffice { get; }

		ZString MasterUniqueConsignmentReference { get; } // MASTER-UCR

		// Not allowed for SADH
		//		ZString PlaceOfLoading { get; } // PLA-LDG-CODE
		//		ZString PlaceOfUnloadingInTheEU { get; } // PLA-ULDG-CODE

		ZString TransportChargesMethodOfPayment { get; } // TRPT-CHGE-MOP
		IOrganisation Premises { get; } // PREM-NAME +++

		IEnumerable<ISeal> Seals { get; }

		ZBool FECAcceptanceReport { get; }   // PROC-INST "ACC"
		ZBool FECExpectedTransportNationalityAtTheBorder { get; } // PROC-INST "FLG"
		ZBool FECRouteF { get; } // PROC-INST "RTF"

		ZString EntryMessageType { get; }

		ZInt TotalPackages { get; } // TOT-PKGS
		ZString AmendmentReason { get; }
	}
}
