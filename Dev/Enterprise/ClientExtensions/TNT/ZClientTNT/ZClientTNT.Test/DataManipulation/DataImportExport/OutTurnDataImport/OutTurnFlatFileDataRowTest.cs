using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.TNT.OutTurnDataImport.Testing
{
	public class OutTurnFlatFileDataRowTest : TestCase
	{
		public void TestConstructor()
		{
			OutTurnFlatFileDataRow dataRow = new OutTurnFlatFileDataRow();
			AssertEquals("FieldCount", OutTurnFlatFileDataRow.Schema.FieldCount, dataRow.FieldCount);
		}

		public void TestFieldProperty()
		{
			OutTurnFlatFileDataRow dataRow = new OutTurnFlatFileDataRow("QF|1680|08196106301 |TYO|SYD|060805|A| 153630584 |TYO|HM3|N|00002|000003");
			AssertEquals("FieldCount", OutTurnFlatFileDataRow.Schema.FieldCount, dataRow.FieldCount);
			AssertEquals("SectorInformation", "QF168008196106301 TYOSYD060805A", dataRow.SectorInformation);
			AssertEquals("FlightNo", "QF1680", dataRow.FlightNo);
			AssertEquals("CarrierCode", "QF", dataRow.CarrierCode);
			AssertEquals("CarrierNumber", "1680", dataRow.CarrierNumber);
			AssertEquals("MAWB", "08196106301", dataRow.MAWB);
			AssertEquals("LoadPort", "TYO", dataRow.LoadPort);
			AssertEquals("DiscPort", "SYD", dataRow.DiscPort);
			AssertEquals("ArrivalDate", new ZDateTime(2005, 8, 6), dataRow.ArrivalDate);
			AssertEquals("Mode", "A", dataRow.Mode);
			AssertEquals("HAWB", "153630584", dataRow.HAWB);
			AssertEquals("ConsignmentOrigin", "TYO", dataRow.ConsignmentOrigin);
			AssertEquals("ConsignmentDestination", "HM3", dataRow.ConsignmentDestination);
			AssertEquals("DocumentIndicator", "N", dataRow.DocumentIndicator);
			AssertEquals("ManifestPieces", 2, dataRow.ManifestPieces);
			AssertEquals("LandedPieces", 3, dataRow.LandedPieces);
			dataRow = new OutTurnFlatFileDataRow("Q|168|0819610630|TY|SY|060805||153630584|TYO|HM|N|2|3");
			AssertEquals("SectorInformation", "Q 168 0819610630  TY SY 060805 ", dataRow.SectorInformation);
			AssertEquals("FlightNo", "Q168", dataRow.FlightNo);
			AssertEquals("CarrierCode", "Q", dataRow.CarrierCode);
			AssertEquals("CarrierNumber", "168", dataRow.CarrierNumber);
			AssertEquals("MAWB", "0819610630", dataRow.MAWB);
			AssertEquals("LoadPort", "TY", dataRow.LoadPort);
			AssertEquals("DiscPort", "SY", dataRow.DiscPort);
			AssertEquals("ArrivalDate", new ZDateTime(2005, 8, 6), dataRow.ArrivalDate);
			AssertEquals("Mode", "", dataRow.Mode);
			AssertEquals("HAWB", "153630584", dataRow.HAWB);
			AssertEquals("ConsignmentOrigin", "TYO", dataRow.ConsignmentOrigin);
			AssertEquals("ConsignmentDestination", "HM", dataRow.ConsignmentDestination);
			AssertEquals("DocumentIndicator", "N", dataRow.DocumentIndicator);
			AssertEquals("ManifestPieces", 2, dataRow.ManifestPieces);
			AssertEquals("LandedPieces", 3, dataRow.LandedPieces);
		}
	}
}
