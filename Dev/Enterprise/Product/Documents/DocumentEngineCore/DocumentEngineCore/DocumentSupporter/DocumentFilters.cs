namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public enum DocumentFilters
	{
		/// <summary>
		/// Supplier, Consignor or Exporter
		/// </summary>
		SUP,
		/// <summary>
		/// Buyer, Consignee or Importer
		/// </summary>
		BUY,
		/// <summary>
		/// Client Contact for Transport services
		/// </summary>
		TRC,
		/// <summary>
		/// Warehouse, Distribution Client
		/// </summary>
		WHS,
		/// <summary>
		/// Sales updates, quotes, special offers
		/// </summary>
		SAL,
		/// <summary>
		/// Forwarding Agent Contact
		/// </summary>
		FWD,
		/// <summary>
		/// Shipping Line, Airline, Carrier Contact
		/// </summary>
		SHP,
		/// <summary>
		/// Local Transport, Cartage Contractor
		/// </summary>
		TRN,
		/// <summary>
		/// Warehouse Client Contact
		/// </summary>
		TPL,
		/// <summary>
		/// Transit Warehouse
		/// </summary>
		TWH,
		/// <summary>
		/// Transport Mode
		/// </summary>
		MOD,
		/// <summary>
		/// Commodity Code (GEN/HAZ/PER/REF)
		/// </summary>
		COM,
		/// <summary>
		/// Container Mode (FCL/LCL/BCN etc.)
		/// </summary>
		CNT,
		/// <summary>
		/// Container Mode (FCL or BCN returns true)
		/// </summary>
		FCLBCN,
		/// <summary>
		/// BCN Type 
		/// </summary>
		BCN,
		/// <summary>
		/// Miscellaneous Contact (no Automation)
		/// </summary>
		MSC,
		/// <summary>
		/// Country Code
		/// </summary>
		CTY,
		/// <summary>
		/// Country Code And Brokerage
		/// </summary>
		CTYBKR,
		/// <summary>
		/// Country of currency company plus 'direction' (IMPort/EXPort) of job, e.g. GBEXP, USIMP. 
		/// </summary>
		CountryDirection,
		/// <summary>
		/// Brokerage
		/// </summary>
		BKR,
		/// <summary>
		/// Country and Transport Mode
		/// </summary>
		CTYMOD,
		/// <summary>
		/// Brokerage Message Type
		/// </summary>
		MSGBKR,
		/// <summary>
		/// Brokerage Message Type and Country
		/// </summary>
		MSGBKRCTY,
		/// <summary>
		/// Brokerage Message Type and Country and Transport Mode
		/// </summary>
		MSGBKRCTYMOD,
		/// <summary>
		/// Brokerage Message Type and Country and Application 
		/// </summary>
		MSGBKRCTYAPP,
		///
		/// Entry Message Type and Country and Application
		/// 
		MSGENTCTRYAPP,
		/// <summary>
		/// Export Docs for Brokerage
		/// </summary>
		EXPBKRLIC,
		/// <summary>
		/// Ledger (AR/AP/CB/GL)
		/// </summary>
		LGR,
		/// <summary>
		/// MY Delivery Order
		/// </summary>
		MYDO,
		/// <summary>
		/// Australian Brokerage
		/// </summary>
		AUBKR,
		/// <summary>
		/// Declaration type for Bill of Entries
		/// </summary>
		DECTP,
		/// <summary>
		/// MYPEN Port with Sea Rail or Road mode
		/// </summary>
		MYPENSRR,
		/// <summary>
		/// Shipment is Air and Country is Singapore
		/// </summary>
		SGAIRSHP,
		/// <summary>
		/// Consol is Air and Country is Singapore
		/// </summary>
		SGAIRCON,
		/// <summary>
		/// Current Company's Code
		/// </summary>
		CO,
		/// <summary>
		/// Electronic HBL to Consignor
		/// </summary>
		EBL,
		/// <summary>
		/// To hide Menu items used as Child Menu's 
		/// </summary>
		HIDE,
		/// <summary>
		/// For Export Receival Advice, Only AU, blk/bkk/lqd 
		/// </summary>
		AUBLK,
		/// <summary>
		/// For Export Receival Advice, Only AU, not (blk/bkk/lqd)
		/// </summary>
		AUNOTBLK,
		/// <summary>
		/// International Air Mode
		/// </summary>
		INTAIR,
		/// <summary>
		/// Domestic Air Mode
		/// </summary>
		DOMAIR,
		/// <summary>
		/// Domestic Air Or Road Mode
		/// </summary>
		DOMAIRORROAD,
		/// <summary>
		/// Current Country Economic Grouping Code
		/// </Summary>
		CTYEG,
		/// <summary>
		/// Current Country Economic Grouping Code - SADH documents supporting
		/// </Summary>
		CTYEGSADH,
		/// <summary>
		/// Import Declaration Document
		/// </Summary>
		CTYEGIDD,
		/// <summary>
		/// Can Print B3 or CAD As Accounted Document
		/// </Summary>
		CAAsAccountedDataSupport,
		/// <summary>
		/// Brokerage Message Type and Landed Costing Type
		/// </summary>
		MSGBKRLCO,
		/// <summary>
		/// Brokerage Country and Application
		/// </summary>
		BKRCTYAPP,
		/// <summary>
		/// Brokerage Country
		/// </summary>
		BKRCTY,
		/// <summary>
		/// Compliance Sequence Module Visibility
		/// </summary>
		CMP,
		/// <summary>
		/// Enable Report Setups in registry
		/// </summary>
		ERS,
		/// <summary>
		/// Which NCTS country are we talking about?  EU+EFTA+friends
		/// </summary>
		NCTS,
		/// <summary>
		/// Is Consol Eligable ForJPAFR messaging, MOD=SEA and IsGoingViaIgnoringDomesticRoute(JP) = true
		/// </summary>
		JPAFR,
		/// <summary>
		/// Company Cash Basis Feature Enabled
		/// </summary>
		CSHB,
		/// <summary>
		/// Can Print B3 or CAD Current Data Document
		/// </Summary>
		CACurrentDataSupport,
		/// <summary>
		/// Canada Import
		/// </Summary>
		CAIMP,
		/// <summary>
		/// Is Integrated Country
		/// </Summary>
		ISINTEGRATED,
		/// <summary>
		/// Compliance Sub Type Include AP Ledger
		/// </Summary>
		COMAP,
		/// <summary>
		/// Compliance Sub Type NOT Include AP Ledger
		/// </Summary>
		COMNOAP,
		/// <summary>
		/// B2 Adjustment Support
		/// </Summary>
		CAIM2SUPPORT,
		/// <summary>
		/// Login Name
		/// </Summary>
		LoginName,
		/// <summary>
		/// EU including United Kingdom
		/// </summary>
		EUGB,
		/// <summary>
		/// TW MessageType, GoodsType
		/// </Summary>
		MSGGDSCTRY,
		/// <summary>
		/// Is Enable Compliance Document Module
		/// </Summary>
		IsComplianceDocumentModuleEnabled,
		/// <summary>
		/// Is Declaration (Non-Condensed)
		/// </Summary>
		IsNonCondensedDeclaration,
		/// <summary>
		/// Additional Match using setting from filter
		/// </Summary>
		AdditionalMatch,
		/// <summary>
		/// Is Tax Framework configured for the login company
		/// </summary>
		TF,
		/// <summary>
		/// Is Asycuda Country
		/// </Summary>
		ASYCUDA,
		/// <summary>
		/// CA eManifest Forwarder
		/// </Summary>
		CAeManifest,
		/// <summary>
		/// NEXDOCS activate status, Non-AQS/AQS/AQS NEXDOCS Activated
		/// </summary>
		AUMSGNXD,
		/// <summary>
		/// Has HVLV Clearance
		/// </summary>
		HasHVLVClearance,
		/// <summary>
		/// Are NCTS transit documents supported?
		/// </summary>
		NCTSTransitDocumentsSupport,
		/// <summary>
		/// Is T2L Document supported?
		/// </summary>
		T2LDocumentSupport,
		/// <summary>
		/// Is T2LF Document supported?
		/// </summary>
		T2LFDocumentSupport,
		/// <summary>
		/// PrintSocialSecurityNumberAllowed
		/// </summary>
		PrintSocialSecurityNumberAllowed,
		/// <summary>
		/// Temporary Storage Declaration
		/// </summary>
		TSD,
		/// <summary>
		/// Is JP Export Permit Message Document supported?
		/// </summary>
		IsJPExportPermitMessageDocumentSupport,
		/// <summary>
		/// Is JP Inspection Notice Document supported?
		/// </summary>
		IsJPInspectionNoticeDocumentSupport,
		/// <summary>
		/// Is JP Import Permit Message Document supported?
		/// </summary>
		IsJPImportPermitMessageDocumentSupport,
		/// <summary>
		/// Is JP Discrepancy Notice Document supported?
		/// </summary>
		IsJPDiscrepancyNoticeDocumentSupport,
	}
}
