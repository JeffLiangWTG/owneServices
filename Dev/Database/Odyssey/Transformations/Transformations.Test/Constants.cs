using System;

namespace Enterprise.Core.Constants
{
	static class Groups
	{
		public static Guid AllPK => new Guid("94755E71-A87A-4034-8DFA-785773A49607");
	}

	static class GteGateDirections
	{
		public const string Both = "BOTH";
		public const string In = "IN";
		public const string Out = "OUT";
	}

	static class WarehouseTypes
	{
		public const string ContainerYard = "CYD";
		public const string FreeTradeZone = "FTZ";
		public const string Product = "PRW";
		public const string Transit = "TRW";
	}

	static class CurrencyCodes
	{
		public const string UnitedKingdom = "GBP";
		public const string EuropeanUnion = "EUR";
		public const string SouthAfrica = "ZAR";
	}

	static class CountryCodes
	{
		public const string Australia = "AU";
		public const string Austria = "AT";
		public const string Belgium = "BE";
		public const string Canada = "CA";
		public const string France = "FR";
		public const string Germany = "DE";
		public const string Ireland = "IE";
		public const string Italy = "IT";
		public const string Poland = "PL";
		public const string SouthAfrica = "ZA";
		public const string Switzerland = "CH";
		public const string Taiwan = "TW";
		public const string UnitedKingdom = "GB";
		public const string UnitedStates = "US";
	}

	static class InvoiceTerms
	{
		public const string CashOnDelivery = "COD";
		public const string PaymentInAdvance = "PIA";
		public const string FromCustomsClearanceDate = "CUS";
		public const string FromInvoiceDate = "INV";
		public const string FromMonthEnd = "MTH";
		public const string FromWeekEnd = "EWK";
		public const string FromPeriodEnd = "PER";
		public const string FromShipmentDate = "SHP";
		public const string MonthsFromInvoiceCycleDate = "MIC";
		public const string TermDaysAndDebtorPaymentCycle = "DPC";
		public const string MultipleInstallments = "MLI";
		public const string LaterOfShipmentOrInvoiceDate = "LSI";
	}

	static class ReceivedAs
	{
		public const string Default = "";
		public const string ScannedIn = "SCN";
		public const string PackedPackage = "PKG";
		public const string PackedPacline = "PKL";
		public const string PackedNonTracked = "PNT";
		public const string Adhoc = "ADC";
	}
}
