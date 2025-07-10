using CargoWise.Common;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.Integration
{
	[Immutable]
	public class BillingInterfaceName
	{
		BillingInterfaceName(string databaseValue, bool shouldSuspendValidation = true)
		{
			Argument.NotNullOrEmpty(databaseValue, "databaseValue");
			this.databaseValue = databaseValue;
			this.ShouldSuspendValidation = shouldSuspendValidation;
		}

		readonly ZString databaseValue;

		public override string ToString()
		{
			return this.databaseValue;
		}

		public readonly bool ShouldSuspendValidation = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName AccountBalancesXmlImport = new BillingInterfaceName("Account Balances Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName AccountingTransactionsCsvImport = new BillingInterfaceName("Accounting Transactions Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ACXmlImport = new BillingInterfaceName("AC Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ACXStatusXmlImport = new BillingInterfaceName("ACX Status Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName AgencyBillOfLadingCsvImport = new BillingInterfaceName("Agency Bill Of Lading Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName AgencyBillOfLadingXmlImport = new BillingInterfaceName("Agency Bill Of Lading Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName AirCargoCsvImport = new BillingInterfaceName("Air Cargo Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName APInvoiceNZCustomsImport = new BillingInterfaceName("AP Invoice NZ Customs Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName BankStatementImport = new BillingInterfaceName("Bank Statement Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName BIRDImport = new BillingInterfaceName("BIRD Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName BookingXmlImport = new BillingInterfaceName("Booking Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CartageXmlImport = new BillingInterfaceName("Cartage Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CashbookTransactionsXmlImport = new BillingInterfaceName("Cashbook Transactions Xml Import", false); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ClientRatesXmlImport = new BillingInterfaceName("Client Rates Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ClientSpecifiedImport = new BillingInterfaceName("Client Specified Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CommercialInvoiceXmlImport = new BillingInterfaceName("Commercial Invoice Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ConsolXmlImport = new BillingInterfaceName("Consol Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ContainerEventsXmlImport = new BillingInterfaceName("Container Events Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ContainerXmlImport = new BillingInterfaceName("Container Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CostingXmlImport = new BillingInterfaceName("Costing Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CSVAccountChartImport = new BillingInterfaceName("CSV Account Chart Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CSVAlternateGLAccountImport = new BillingInterfaceName("CSV Alternate GL Account Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName CsvOrderImport = new BillingInterfaceName("Csv Order Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName DataWizard = new BillingInterfaceName("Data Wizard"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName DeclarationXmlImport = new BillingInterfaceName("Declaration Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName eAdaptorOutbound = new BillingInterfaceName("eAdaptor Outbound"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName eHubOutbound = new BillingInterfaceName("eHub Outbound"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName EuroPacificManifestImport = new BillingInterfaceName("Euro Pacific Manifest Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName EventXmlImport = new BillingInterfaceName("Event Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName GLJournalCSVImport = new BillingInterfaceName("GL Journal CSV Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName GloblRatesXmlImport = new BillingInterfaceName("Globl Rates Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ImportCASS = new BillingInterfaceName("Import CASS"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ImporterSecurityFilingXMLImport = new BillingInterfaceName("Importer Security Filing XML Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ImportRemittance = new BillingInterfaceName("Import Remittance"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ISFXmlImport = new BillingInterfaceName("ISF Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName JournalXMLEventImport = new BillingInterfaceName("Journal XML Event Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName JournalXmlImport = new BillingInterfaceName("Journal Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName LoadListConsolXmlImport = new BillingInterfaceName("Load List Consol Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName LocalCartageImport = new BillingInterfaceName("Local Cartage Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName LocalCartageXmlImport = new BillingInterfaceName("Local Cartage Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName None = new BillingInterfaceName("None"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrderCsvImport = new BillingInterfaceName("Order Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrdersCsvImport = new BillingInterfaceName("Orders Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrdersXmlImport = new BillingInterfaceName("Orders Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrderXmlImport = new BillingInterfaceName("Order Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrganisationXmlImport = new BillingInterfaceName("Organisation Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName OrganisationXMLImport = new BillingInterfaceName("Organisation XML Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ProductXmlImport = new BillingInterfaceName("Product Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ProductXMLImport = new BillingInterfaceName("Product XML Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName QuotationsXmlImport = new BillingInterfaceName("Quotations Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName QuotedBookingXmlImport = new BillingInterfaceName("Quoted Booking Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName RateImport = new BillingInterfaceName("Rate Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName RCLShippingManifestImport = new BillingInterfaceName("RCL Shipping Manifest Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ReconInvoiceLinesImport = new BillingInterfaceName("Recon Invoice Lines Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SailingXmlImport = new BillingInterfaceName("Sailing Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ScheduleXmlImport = new BillingInterfaceName("Schedule Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SeaCargoCsvImport = new BillingInterfaceName("Sea Cargo Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ShipmentXmlImport = new BillingInterfaceName("Shipment Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName ShipNETImport = new BillingInterfaceName("ShipNET Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeClassificationsImport = new BillingInterfaceName("System Merge Classifications Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeEDocsImport = new BillingInterfaceName("System Merge EDocs Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeOrganisationImport = new BillingInterfaceName("System Merge Organisation Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeProductImport = new BillingInterfaceName("System Merge Product Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeRatingImport = new BillingInterfaceName("System Merge Rating Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeWarehouseInventoryImport = new BillingInterfaceName("System Merge Warehouse Inventory Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName SystemMergeWarehousesImport = new BillingInterfaceName("System Merge Warehouses Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName Test = new BillingInterfaceName("Test"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName TransactionsCsvImport = new BillingInterfaceName("Transactions Csv Import "); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName TransactionsXmlImport = new BillingInterfaceName("Transactions Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseCartageXmlImport = new BillingInterfaceName("Warehouse Cartage Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseIFSXmlImport = new BillingInterfaceName("Warehouse IFS Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseOrderCsvImport = new BillingInterfaceName("Warehouse Order Csv Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseOrderFlatFileImport = new BillingInterfaceName("Warehouse Order Flat File Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseOrderIFSImport = new BillingInterfaceName("Warehouse Order IFS Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseOrderLegacyXmlImport = new BillingInterfaceName("Warehouse Order Legacy Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseReceiveXMLImport = new BillingInterfaceName("Warehouse Receive XML Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName WarehouseXmlImport = new BillingInterfaceName("Warehouse Xml Import"); // BillingInterfaceName for the eye of internal only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant string")]
		public static readonly BillingInterfaceName XmlMultiTypeImport = new BillingInterfaceName("Xml Multi Type Import"); // BillingInterfaceName for the eye of internal only	}

		public static BillingInterfaceName ConstructorExposed(string databaseValue, bool shouldSuspendValidation = true)
		{
			return new BillingInterfaceName(databaseValue, shouldSuspendValidation);
		}
	}
}
