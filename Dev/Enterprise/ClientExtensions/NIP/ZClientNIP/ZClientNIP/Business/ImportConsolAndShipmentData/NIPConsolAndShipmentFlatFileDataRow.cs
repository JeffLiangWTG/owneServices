using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport
{
	public class NIPConsolAndShipmentFlatFileDataRow : FlatFileDataRow
	{
		public NIPConsolAndShipmentFlatFileDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		#region Schema

		static class Schema
		{
			public const int TransportMode = 0;
			public const int ExportImport = 1;
			public const int DataKind = 2;
			public const int MasterBBPPort = 3;
			public const int MasterBLNo = 4;
			public const int ManifestNo = 5;
			public const int ContainerMode = 6;
			public const int ContainerNo = 7;
			public const int SealNo = 8;
			public const int BLNo = 9;
			public const int DepartureCountryCode = 10;
			public const int DeparturePort = 11;
			public const int DepartureCity = 12;
			public const int DestinationCountryCode = 13;
			public const int DestinationPort = 14;
			public const int DestinationCity = 15;
			public const int CarrierCode = 16;
			public const int FinalFlightVoyage = 17;
			public const int FlightDate = 18;
			public const int EstimatedDateOfDeparture = 19;
			public const int EstimatedTimeOfDeparture = 20;
			public const int EstimatedDateOfArrival = 21;
			public const int EstimatedTimeOfArrival = 22;
			public const int SailingDate = 23;
			public const int LloydsNo = 24;
			public const int VesselName = 25;
			public const int PortOfLoadingCountry = 26;
			public const int PortOfLoadingCity = 27;
			public const int PortOfDischargeCountry = 28;
			public const int PortOfDischargeCity = 29;
			public const int PortOfDeilveryCountry = 30;
			public const int PortOfDeliveryCity = 31;
			public const int ShippersName = 32;
			public const int ShippersAddress1 = 33;
			public const int ShippersAddress2 = 34;
			public const int ShippersAddress3 = 35;
			public const int ShippersAddress4 = 36;
			public const int ShippersAddress5 = 37;
			public const int ConsigneeName = 38;
			public const int ConsigneeAddress1 = 39;
			public const int ConsigneeAddress2 = 40;
			public const int ConsigneeAddress3 = 41;
			public const int ConsigneeAddress4 = 42;
			public const int ConsigneeAddress5 = 43;
			public const int NotifyName = 44;
			public const int NotifyAddress1 = 45;
			public const int NotifyAddress2 = 46;
			public const int NotifyAddress3 = 47;
			public const int NotifyAddress4 = 48;
			public const int NotifyAddress5 = 49;
			public const int Volume = 50;
			public const int GrossWeight = 51;
			public const int ChargeableWeight = 52;
			public const int Goods1 = 53;
			public const int Goods2 = 54;
			public const int OuterPackingType = 55;
			public const int TotalOuterPiecesNo = 56;
			public const int InnerPackingType = 57;
			public const int TotalInnerPiecesNo = 58;
			public const int CollectFreight = 59;
			public const int CurrencyCodeForFreight = 60;
			public const int DeclaredValue = 61;
			public const int CurrencyCodeForValuation = 62;
			public const int ContainerFeet = 63;
			public const int ContainerHigh = 64;
			public const int ContainerType = 65;
			public const int ServiceType = 66;
			public const int JobNo = 67;
			public const int ENDExemptCode = 68;
			public const int CustomsEntryOnly = 69;
		}

		#endregion

		#region Fields

		public ZString TransportMode
		{
			get { return GetField(Schema.TransportMode).Trim(); }
		}

		public ZString ExportImport
		{
			get { return GetField(Schema.ExportImport).Trim(); }
		}

		public ZString DataKind
		{
			get { return GetField(Schema.DataKind).Trim(); }
		}

		public ZString MasterBBPPort
		{
			get { return GetField(Schema.MasterBBPPort).Trim(); }
		}

		public ZString MasterBLNo
		{
			get { return GetField(Schema.MasterBLNo).Trim(); }
		}

		public ZString ManifestNo
		{
			get { return GetField(Schema.ManifestNo).Trim(); }
		}

		public ZString ContainerMode
		{
			get { return GetField(Schema.ContainerMode).Trim(); }
		}

		public ZString ContainerNo
		{
			get { return GetField(Schema.ContainerNo).Trim(); }
		}

		public ZString SealNo
		{
			get { return GetField(Schema.SealNo).Trim(); }
		}

		public ZString BLNo
		{
			get { return GetField(Schema.BLNo).Trim(); }
		}

		public ZString DepartureCountryCode
		{
			get { return GetField(Schema.DepartureCountryCode).Trim(); }
		}

		public ZString DeparturePort
		{
			get { return GetField(Schema.DeparturePort).Trim(); }
		}

		public ZString DepartureCity
		{
			get { return GetField(Schema.DepartureCity).Trim(); }
		}

		public ZString DestinationCountryCode
		{
			get { return GetField(Schema.DestinationCountryCode).Trim(); }
		}

		public ZString DestinationPort
		{
			get { return GetField(Schema.DestinationPort).Trim(); }
		}

		public ZString DestinationCity
		{
			get { return GetField(Schema.DestinationCity).Trim(); }
		}

		public ZString CarrierCode
		{
			get { return GetField(Schema.CarrierCode).Trim(); }
		}

		public ZString FinalFlightVoyage
		{
			get { return GetField(Schema.FinalFlightVoyage).Trim(); }
		}

		public ZDateTime FlightDate
		{
			get { return GetFieldAsZDateTime(Schema.FlightDate, "yyyy-MM-dd"); }
		}

		public ZDateTime EstimatedDateOfDeparture
		{
			get { return GetFieldAsZDateTime(Schema.EstimatedDateOfDeparture, "yyyy-MM-dd"); }
		}

		public ZString EstimatedTimeOfDeparture
		{
			get { return GetField(Schema.EstimatedTimeOfDeparture).Trim(); }
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get { return GetFieldAsZDateTime(Schema.EstimatedDateOfArrival, "yyyy-MM-dd"); }
		}

		public ZString EstimatedTimeOfArrival
		{
			get { return GetField(Schema.EstimatedTimeOfArrival).Trim(); }
		}

		public ZDateTime SailingDate
		{
			get { return GetFieldAsZDateTime(Schema.SailingDate, "yyyy-MM-dd"); }
		}

		public ZString LloydsNo
		{
			get { return GetField(Schema.LloydsNo).Trim(); }
		}

		public ZString VesselName
		{
			get { return GetField(Schema.VesselName).Trim(); }
		}

		public ZString PortOfLoadingCountry
		{
			get { return GetField(Schema.PortOfLoadingCountry).Trim(); }
		}

		public ZString PortOfLoadingCity
		{
			get { return GetField(Schema.PortOfLoadingCity).Trim(); }
		}

		public ZString PortOfDischargeCountry
		{
			get { return GetField(Schema.PortOfDischargeCountry).Trim(); }
		}

		public ZString PortOfDischargeCity
		{
			get { return GetField(Schema.PortOfDischargeCity).Trim(); }
		}

		public ZString PortOfDeliveryCountry
		{
			get { return GetField(Schema.PortOfDeilveryCountry).Trim(); }
		}

		public ZString PortOfDeliveryCity
		{
			get { return GetField(Schema.PortOfDeliveryCity).Trim(); }
		}

		public ZString ShippersName
		{
			get { return GetField(Schema.ShippersName).Trim(); }
		}

		public ZString ShippersAddress1
		{
			get { return GetField(Schema.ShippersAddress1).Trim(); }
		}

		public ZString ShippersAddress2
		{
			get { return GetField(Schema.ShippersAddress2).Trim(); }
		}

		public ZString ShippersAddress3
		{
			get { return GetField(Schema.ShippersAddress3).Trim(); }
		}

		public ZString ShippersAddress4
		{
			get { return GetField(Schema.ShippersAddress4).Trim(); }
		}

		public ZString ShippersAddress5
		{
			get { return GetField(Schema.ShippersAddress5).Trim(); }
		}

		public ZString ConsigneeName
		{
			get { return GetField(Schema.ConsigneeName).Trim(); }
		}

		public ZString ConsigneeAddress1
		{
			get { return GetField(Schema.ConsigneeAddress1).Trim(); }
		}

		public ZString ConsigneeAddress2
		{
			get { return GetField(Schema.ConsigneeAddress2).Trim(); }
		}

		public ZString ConsigneeAddress3
		{
			get { return GetField(Schema.ConsigneeAddress3).Trim(); }
		}

		public ZString ConsigneeAddress4
		{
			get { return GetField(Schema.ConsigneeAddress4).Trim(); }
		}

		public ZString ConsigneeAddress5
		{
			get { return GetField(Schema.ConsigneeAddress5).Trim(); }
		}

		public ZString NotifyName
		{
			get { return GetField(Schema.NotifyName).Trim(); }
		}

		public ZString NotifyAddress1
		{
			get { return GetField(Schema.NotifyAddress1).Trim(); }
		}

		public ZString NotifyAddress2
		{
			get { return GetField(Schema.NotifyAddress2).Trim(); }
		}

		public ZString NotifyAddress3
		{
			get { return GetField(Schema.NotifyAddress3).Trim(); }
		}

		public ZString NotifyAddress4
		{
			get { return GetField(Schema.NotifyAddress4).Trim(); }
		}

		public ZString NotifyAddress5
		{
			get { return GetField(Schema.NotifyAddress5).Trim(); }
		}

		public ZDecimal Volume
		{
			get { return GetFieldAsZDecimal(Schema.Volume); }
		}

		public ZDecimal GrossWeight
		{
			get { return GetFieldAsZDecimal(Schema.GrossWeight); }
		}

		public ZDecimal ChargeableWeight
		{
			get { return GetFieldAsZDecimal(Schema.ChargeableWeight); }
		}

		public ZString Goods1
		{
			get { return GetField(Schema.Goods1).Replace('"', ' ').Trim(); }
		}

		public ZString Goods2
		{
			get { return GetField(Schema.Goods2).Replace('"', ' ').Trim(); }
		}

		public ZString OuterPackingType
		{
			get { return GetField(Schema.OuterPackingType).Trim(); }
		}

		public ZDecimal TotalOuterPiecesNo
		{
			get { return GetFieldAsZDecimal(Schema.TotalOuterPiecesNo); }
		}

		public ZString InnerPackingType
		{
			get { return GetField(Schema.InnerPackingType).Trim(); }
		}

		public ZDecimal TotalInnerPiecesNo
		{
			get { return GetFieldAsZDecimal(Schema.TotalInnerPiecesNo); }
		}

		public ZDecimal CollectFreight
		{
			get { return GetFieldAsZDecimal(Schema.CollectFreight); }
		}

		public ZString CurrencyCodeForFreight
		{
			get { return GetField(Schema.CurrencyCodeForFreight).Trim(); }
		}

		public ZDecimal DeclaredValue
		{
			get { return GetFieldAsZDecimal(Schema.DeclaredValue); }
		}

		public ZString CurrencyCodeForValuation
		{
			get { return GetField(Schema.CurrencyCodeForValuation).Trim(); }
		}

		public ZString ContainerFeet
		{
			get { return GetField(Schema.ContainerFeet).Trim(); }
		}

		public ZString ContainerHigh
		{
			get { return GetField(Schema.ContainerHigh).Trim(); }
		}

		public ZString ContainerType
		{
			get { return GetField(Schema.ContainerType).Trim(); }
		}

		public ZString ServiceType
		{
			get { return GetField(Schema.ServiceType).Trim(); }
		}

		public ZString JobNo
		{
			get { return GetField(Schema.JobNo).Trim(); }
		}

		public ZString ENDExemptCode
		{
			get { return GetField(Schema.ENDExemptCode).Trim(); }
		}

		public ZString CustomsEntryOnly
		{
			get { return GetField(Schema.CustomsEntryOnly).Trim(); }
		}

		#endregion
	}
}
