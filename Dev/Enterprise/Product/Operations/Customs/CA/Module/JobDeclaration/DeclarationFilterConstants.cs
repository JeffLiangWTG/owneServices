namespace Enterprise.Customs.CA.Module
{
	#region SuppressResourceStringsCheckRegion 

	public class DeclarationFilterConstants : Customs.Module.DeclarationFilterConstants
	{
		public const string EntryType = "Entry Type";
		public const string MessageStatus = "Last Message";
		public const string ServiceOption = "Service Option";
		public const string B2Type = "B2 Type";
		public const string CountryofOrigin = "Country/Region of Origin";
		public const string ReleaseMessage = "Release Message Status";
		public const string ReleaseStatus = "Release Status";
		public const string EntryMessageStatus = "Entry Message Status";
		public const string OGDStatus = "OGD Status";
		public const string AccountingAge = "Days Since Release";
		public const string OurFault = "Broker Issue";
		public const string InitiatedBy = "Initiated By";
		public const string ExceptionCode = "Exception Code";
		public const string DIFMessageStatus = "eDocs DIF Message Status";
		public const string LPCODIFURN = "LPCO DIF URN";
		public const string LPCORefNo = "LPCO Ref No";
		public const string CSAReleaseOnly = "CSA Release Only";
		public const string B3NeedRemergeToCAD = "Legacy Merged B3C Needs Re-merge To CAD";
		public const string EXPStatus = "EXP Status";
		public const string BondType = "Bond Type";

		public new class PortFilterTypes : Customs.Module.DeclarationFilterConstants.PortFilterTypes
		{
			public const string PlaceOfReport = "Place Of Report";
			public const string PortOfExit = "Port Of Exit";
			public const string PortOfClearance = "Port Of Clearance";
			public const string PortOfUnlading = "Port Of Unlading";
			public const string SubLocation = "Sub-Location";
		}

		public new class DateFilterTypes : Customs.Module.DeclarationFilterConstants.DateFilterTypes
		{
			public const string ReleaseDate = "Release Date";
			public const string K84AccountingDate = "Accounting Date";
			public const string SubLocationETD = "Sub-Location ETD";
			public const string DirectShipmentDate = "Direct Shipment Date";
			public const string EstimatedPaymentDueDate = "Entry Due Date";
			public const string EntrySubmittedDate = "Submitted Date";
			public const string EntrySubmissionDate = "Entry Submission Date";
			public const string ReleaseSubmissionDate = "Release Submission Date";
			public const string ChequeDate = "Cheque Date";
			public const string SubmittedDate = "Date Submitted";
			public const string ConfirmedDate = "Date Confirmed";
			public const string DecisionDate = "Date Accepted";
			public const string EntryAcceptedDate = "Entry Accepted Date";
			public const string ExportDate = "Export Date (Export Declarations)";
		}

		public new class NumberFilterTypes : Customs.Module.DeclarationFilterConstants.NumberFilterTypes
		{
			public const string FormKeyNumber = "Form Key #";
			public const string TransactionNumber = "Transaction #";
			public const string CCN = "CCN";
			public const string ProofOfReportNumber = "Proof Of Report #";
			public const string OriginalTransactionNo = "Original Transaction #";
			public const string Period = "Period";
			public const string LVSID = "LVS ID";
			public const string ChequeNumber = "Cheque #";
			public const string DIFURN = "eDocs DIF URN";
			public const string BondSurety = "Bond Surety";
			public const string BondNumber = "Bond Number";
		}

		public new class OrgFilterTypes : Customs.Module.DeclarationFilterConstants.OrgFilterTypes
		{
			public const string Carrier = "Carrier";
			public const string CarrierServiceProvider = "Carrier/Service Provider";
			public const string Consignee = "Consignee";
			public const string ExporterConsignee = "Vendor (Exporter)/Importer (Consignee)";
			public const string Exporter = "Exporter";
			public const string ExportingCompany = "Exporting Company";
			public const string ServiceProvider = "Srv. Provider";
			public const string CarrierCode = "Carrier Code";
			public const string ImporterVendor = "Importer/Vendor";
			public const string Vendor = "Vendor";
			public const string ImporterOfRecord = "Importer Of Record";
			public const string BondedWarehouse = "Bonded W/H";
			public const string Originator = "Originator";
			public const string ExportSeller = "Export Seller";
			public const string Address = "Address";
			public const string InvoiceVendor = "Invoice Vendor";
			public const string InvoicePurchaser = "Invoice Purchaser";
			public const string InvoiceConsignee = "Invoice Consignee";
			public const string InvoiceShipper = "Invoice Shipper";
			public const string InvoiceExporter = "Invoice Exporter";
			public const string InvoiceManufacturer = "Invoice Manufacturer";
			public const string InvoiceOriginator = "Invoice Originator";
		}
	}

	#endregion
}
