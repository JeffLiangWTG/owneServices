using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport.Testing
{
	public class NIPConsolAndShipmentFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string csvOrderHeader = @"SEA,E,HB,AUSYD  ,SYDYOK000055        ,SYYO02579    ,LCL,AAAAA11111   ,SEALNO,SYYOSRE07580    ,UA,IEV,KIEV,AU,SYD,SYDNEY,CSHK,46N,0001-01-01,2009-05-15,0000,2009-05-27,0000,0001-01-01,8616506,KAGA,AU,SYD,JP,YOK,JP,YOK,MS. AKIKO IWAI,452/3 HILTON TERRACE,addres2,NOOSAVILLE QLD,,addres5,MS. AKIKO IWAI,4-14-23-110 SYAKUJIIDAI,,NERIMA KU TOKYO JAPAN,addres4,addres5,MS.blah AKIKO IWAI,4-15-23-110 SYAKUJIIDAI,addres2,NERIMA KuuU TOKYO JAPAN,,,0000000000.800,00000147.0,0000000.0,HOUSEHOLD GOODS & PERSONAL EFFECTS,,PK,0000001,,0000,000000000000.00,USD,000000000000.000,AUD,40,20,FR,FCL,ShipRef,EXLV,Y";

			NIPConsolAndShipmentFlatFileDataRow consolRow = new NIPConsolAndShipmentFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues));
			AssertNotNull(consolRow);
			AssertEquals("SEA", consolRow.TransportMode);
			AssertEquals("E", consolRow.ExportImport);
			AssertEquals("HB", consolRow.DataKind);
			AssertEquals("AUSYD", consolRow.MasterBBPPort);
			AssertEquals("SYDYOK000055", consolRow.MasterBLNo);
			AssertEquals("SYYO02579", consolRow.ManifestNo);
			AssertEquals("LCL", consolRow.ContainerMode);
			AssertEquals("AAAAA11111", consolRow.ContainerNo);
			AssertEquals("SEALNO", consolRow.SealNo);
			AssertEquals("SYYOSRE07580", consolRow.BLNo);
			AssertEquals("UA", consolRow.DepartureCountryCode);
			AssertEquals("IEV", consolRow.DeparturePort);
			AssertEquals("KIEV", consolRow.DepartureCity);
			AssertEquals("AU", consolRow.DestinationCountryCode);
			AssertEquals("SYD", consolRow.DestinationPort);
			AssertEquals("SYDNEY", consolRow.DestinationCity);
			AssertEquals("CSHK", consolRow.CarrierCode);
			AssertEquals("46N", consolRow.FinalFlightVoyage);
			AssertEquals(ZDateTime.Invalid, consolRow.FlightDate);
			AssertEquals(new ZDateTime(2009, 5, 15), consolRow.EstimatedDateOfDeparture);
			AssertEquals("0000", consolRow.EstimatedTimeOfDeparture);
			AssertEquals(new ZDateTime(2009, 5, 27), consolRow.EstimatedDateOfArrival);
			AssertEquals("0000", consolRow.EstimatedTimeOfArrival);
			AssertEquals(ZDateTime.Invalid, consolRow.SailingDate);
			AssertEquals("8616506", consolRow.LloydsNo);
			AssertEquals("KAGA", consolRow.VesselName);
			AssertEquals("AU", consolRow.PortOfLoadingCountry);
			AssertEquals("SYD", consolRow.PortOfLoadingCity);
			AssertEquals("JP", consolRow.PortOfDischargeCountry);
			AssertEquals("YOK", consolRow.PortOfDischargeCity);
			AssertEquals("JP", consolRow.PortOfDeliveryCountry);
			AssertEquals("YOK", consolRow.PortOfDeliveryCity);

			AssertEquals("MS. AKIKO IWAI", consolRow.ShippersName);
			AssertEquals("452/3 HILTON TERRACE", consolRow.ShippersAddress1);
			AssertEquals("addres2", consolRow.ShippersAddress2);
			AssertEquals("NOOSAVILLE QLD", consolRow.ShippersAddress3);
			AssertEquals(ZString.Empty, consolRow.ShippersAddress4);
			AssertEquals("addres5", consolRow.ShippersAddress5);

			AssertEquals("MS. AKIKO IWAI", consolRow.ConsigneeName);
			AssertEquals("4-14-23-110 SYAKUJIIDAI", consolRow.ConsigneeAddress1);
			AssertEquals(ZString.Empty, consolRow.ConsigneeAddress2);
			AssertEquals("NERIMA KU TOKYO JAPAN", consolRow.ConsigneeAddress3);
			AssertEquals("addres4", consolRow.ConsigneeAddress4);
			AssertEquals("addres5", consolRow.ConsigneeAddress5);

			AssertEquals("MS.blah AKIKO IWAI", consolRow.NotifyName);
			AssertEquals("4-15-23-110 SYAKUJIIDAI", consolRow.NotifyAddress1);
			AssertEquals("addres2", consolRow.NotifyAddress2);
			AssertEquals("NERIMA KuuU TOKYO JAPAN", consolRow.NotifyAddress3);
			AssertEquals(ZString.Empty, consolRow.NotifyAddress4);
			AssertEquals((ZDecimal)0.8, consolRow.Volume);
			AssertEquals((ZDecimal)147.0, consolRow.GrossWeight);
			AssertEquals(ZDecimal.Zero, consolRow.ChargeableWeight);
			AssertEquals("HOUSEHOLD GOODS & PERSONAL EFFECTS", consolRow.Goods1);
			AssertEquals(ZString.Empty, consolRow.Goods2);
			AssertEquals("PK", consolRow.OuterPackingType);
			AssertEquals((ZDecimal)1, consolRow.TotalOuterPiecesNo);
			AssertEquals(ZString.Empty, consolRow.InnerPackingType);
			AssertEquals(ZDecimal.Zero, consolRow.TotalInnerPiecesNo);
			AssertEquals(ZDecimal.Zero, consolRow.CollectFreight);
			AssertEquals("USD", consolRow.CurrencyCodeForFreight);
			AssertEquals(ZDecimal.Zero, consolRow.DeclaredValue);
			AssertEquals("AUD", consolRow.CurrencyCodeForValuation);
			AssertEquals("40", consolRow.ContainerFeet);
			AssertEquals("20", consolRow.ContainerHigh);
			AssertEquals("FR", consolRow.ContainerType);
			AssertEquals("FCL", consolRow.ServiceType);
			AssertEquals("ShipRef", consolRow.JobNo);
			AssertEquals("EXLV", consolRow.ENDExemptCode);
			AssertEquals("Y", consolRow.CustomsEntryOnly);
		}
	}
}
