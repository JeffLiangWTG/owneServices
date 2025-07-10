using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Client.TGE.PMS.Testing
{
	public class ConsolAndShipmentRecordTest : TestCaseWithFactory
	{
		public void TestCANReturns3LetterCode()
		{
			string[] lineRow1 = { "13", "20051212", "Agent2Agent", "LOOSE", "AU", "SYD", "NZ", "AKL", "20050325121200", "20050325000000", "20050325000000", "1212121", "bkref1", "QF", "123", "AKL", "8112349301", "NZ", "AKL", "13qantas", "1212121", "1313131", "AUD", "shr63761", "WIG GALLERY", "73-77 SACKVILLE ST", "", "COLLINGWOOD", "3066", "03 9419 8266", "ABN1", "Consignee", "CONSIGN-1", "CONSIGN ADDR1-1", "CONSIGN ADDR2-1", "CONSIGN ADDR3-1", "PCODE-1", "PHONE-1", "NZAKL", "63761", "12micon", "STD", "DM", "QTE1", "Y", "OR1", "7712349301", "NZ", "AKL", "JP", "TYO", "20050325", "1", "PK", "STD", "FOB", "FRA", "1000", "AUD", "Goods1", "1.1", "0.001", "2", "0", "Prepaid", "AUD", "10", "EXLV", "FD69N", "OT", "20050323", "TCC", "CIL", "20050323" };
			ConsolAndShipmentRecord testRecord = new ConsolAndShipmentRecord(lineRow1);
			AssertEquals("CAN", "XLV", testRecord.CAN);
			lineRow1[67] = CMRExportExemptionCodes.EXML.Code;
			testRecord = new ConsolAndShipmentRecord(lineRow1);
			AssertEquals("CAN", "XML", testRecord.CAN);
			lineRow1[67] = CANType.ContingencyCustomsAuthorityNumber.Code;
			testRecord = new ConsolAndShipmentRecord(lineRow1);
			AssertEquals("CAN", "CCN", testRecord.CAN);
		}

		public void TestShipmentProperty()
		{
			AssertEquals("HAWB", "7712349301", Record.HAWB);
			AssertEquals("Order References", "OR1", Record.OrderReferences[0]);
			AssertEquals("Goods Description", "Goods1", Record.GoodsDescriptions);
			AssertEquals("Number of Packs", 1, Record.NumberOfPacks);
			AssertEquals("CustomsValue", 1000m, Record.CustomsValue);
			AssertEquals("Currency", "AUD", Record.Currency);
			AssertEquals("CAN", "XLV", Record.CAN);
			AssertEquals("Weight", 1.1m, Record.Weight);
		}

		public void TestConsolProperty()
		{
			AssertEquals("MAWB", "8112349301", Record.MAWB);
			AssertEquals("ConsolDate", ConsolDate, Record.ConsolDate.ToString("yyyyMMdd"));
			AssertEquals("ConsolType", "Agent", Record.ConsolType.ToString());
			AssertEquals("ContainerMode", "LSE", Record.ContainerMode.ToString());
			AssertEquals("SendingAgent", "1212121", Record.SendingAgent);
			AssertEquals("PortOfLoading", "AUSYD", Record.PortOfLoading.Value.ToString());
			AssertEquals("PortOfDischarge", "NZAKL", Record.PortOfDischarge.Value.ToString());
			AssertEquals("ETD", new ZDateTime(2005, 3, 25, 12, 12, 0), Record.ETD);
			AssertEquals("ETA", new ZDateTime(2005, 3, 25, 0, 0, 0), Record.ETA);
			AssertEquals("FlightCarrier", "QF", Record.FlightCarrier);
			AssertEquals("FlightNo", "123", Record.FlightNo);
			AssertEquals("Creditor", "1212121", Record.CreditorCode);
			AssertEquals("ReceivingAgent", "1313131", Record.ReceivingAgent);
		}

		public void TestShipperProperty()
		{
			AssertEquals("Shipper", "SHR63761", Record.ShipperCode);
			AssertEquals("Shipper Name", "WIG GALLERY", Record.ShipperName);
			AssertEquals("ShipperAddressLine1", "73-77 SACKVILLE ST", Record.ShipperAddressLine1);
			AssertEquals("ShipperAddressLine2", "COLLINGWOOD", Record.ShipperAddressLine2);
			AssertEquals("ShipperAddressLine3", "CITY", Record.ShipperAddressLine3);
			AssertEquals("Shipper Phone No", "03 9419 8266", Record.ShipperPhoneNo);
			AssertEquals("Shipper Post Code", "3066", Record.ShipperPostCode);
			AssertEquals("Shipper ABN", "ABN1", Record.ShipperABN);
		}

		public void TestConsigneeProperty()
		{
			AssertEquals("Consignee", "CONSIGNEE", Record.ConsigneeCode);
			AssertEquals("Consignee Name", "CONSIGN-1", Record.ConsigneeName);
			AssertEquals("ConsigneeAddressLine1", "CONSIGN ADDR1-1", Record.ConsigneeAddressLine1);
			AssertEquals("ConsigneeAddressLine2", "CONSIGN ADDR2-1", Record.ConsigneeAddressLine2);
			AssertEquals("ConsigneeAddressLine3", "CONSIGN ADDR3-1", Record.ConsigneeAddressLine3);
			AssertEquals("Consignee Phone No", "PHONE-1", Record.ConsigneePhoneNo);
			AssertEquals("Consignee Post Code", "PCODE-1", Record.ConsigneePostCode);
			AssertEquals("Consignee ABN", "NZAKL", Record.ConsigneeCity.Value);
		}

		ZString ConsolDate;
		ConsolAndShipmentRecord Record
		{
			get
			{
				if (fRecord == null)
				{
					ConsolDate = ZDateTime.Now.ToString("yyyyMMdd");
					string[] lineRow1 = { "13", ConsolDate, "Agent2Agent", "LOOSE", "AU", "SYD", "NZ", "AKL", "20050325121200", "20050325000000", "20050325000000", "1212121", "bkref1", "QF", "123", "AKL", "8112349301", "NZ", "AKL", "13qantas", "1212121", "1313131", "AUD", "shr63761", "WIG GALLERY", "73-77 SACKVILLE ST", "COLLINGWOOD", "CITY", "3066", "03 9419 8266", "ABN1", "Consignee", "CONSIGN-1", "CONSIGN ADDR1-1", "CONSIGN ADDR2-1", "CONSIGN ADDR3-1", "PCODE-1", "PHONE-1", "NZAKL", "63761", "12micon", "STD", "DM", "QTE1", "Y", "OR1", "7712349301", "NZ", "AKL", "JP", "TYO", "20050325", "1", "PK", "STD", "FOB", "FRA", "1000", "AUD", "Goods1", "1.1", "0.001", "2", "0", "Prepaid", "AUD", "10", "EXLV", "FD69N", "OT", "20050323", "TCC", "CIL", "20050323" };
					fRecord = new ConsolAndShipmentRecord(lineRow1);
				}

				return fRecord;
			}
		}

		ConsolAndShipmentRecord fRecord;
	}
}
