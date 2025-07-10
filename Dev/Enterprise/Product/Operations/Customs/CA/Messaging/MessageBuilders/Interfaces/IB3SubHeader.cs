using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Messaging
{
	public struct VendorStateAndZipStruct
	{
		public readonly ZString State;
		public readonly ZString Zip;

		public VendorStateAndZipStruct(ZString state, ZString zip)
		{
			State = state;
			Zip = zip;
		}
	}

	public interface IB3SubHeader
	{
		ZInt B3SubHeaderNumber { get; }
		ZDecimal FreightCharges { get; }
		IDocAddress Vendor { get; }
		IDocAddress Exporter { get; }
		ZDateTime DateOfDirectShipment { get; }
		ZString CountryOfOrigin { get; }
		ZString PlaceOfExport { get; }
		ZString USPortOfExit { get; }
		ZString TariffTreatmentCode { get; }
		ZString TimeLimitUnit { get; }
		ZInt B3TimeLimits { get; }
		ZString CurrencyCode { get; }

		ZString TradeZone { get; }

		//B3 Document
		ZString InvoiceNumber { get; }
		ZDecimal ExchangeRate { get; }

		VendorStateAndZipStruct VendorStateAndZip { get; }

		IB3Header B3Header { get; }
	}
}
