using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class ExpectedB3SubHeaderForTesting : IB3SubHeader
	{
		#region Implementation of IB3SubHeader

		public ZInt B3SubHeaderNumber { get; set; }
		public ZDecimal FreightCharges { get; set; }
		public IDocAddress Vendor { get; set; }
		public IDocAddress Exporter { get; set; }
		public ZDateTime DateOfDirectShipment { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public ZString PlaceOfExport { get; set; }
		public ZString USPortOfExit { get; set; }
		public ZString TariffTreatmentCode { get; set; }
		public ZString TimeLimitUnit { get; set; }
		public ZInt B3TimeLimits { get; set; }
		public ZString CurrencyCode { get; set; }
		public ZString InvoiceNumber { get; set; }
		public ZDecimal ExchangeRate { get; set; }
		public VendorStateAndZipStruct VendorStateAndZip { get; set; }
		public IB3Header B3Header { get; }
		public ZString TradeZone { get; set; }

		#endregion
	}
}
