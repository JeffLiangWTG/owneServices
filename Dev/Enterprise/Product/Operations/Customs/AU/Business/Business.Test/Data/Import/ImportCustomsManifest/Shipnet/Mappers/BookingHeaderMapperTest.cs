using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BookingHeaderMapperTest : BaseMapperTest
	{
		public void TestFromBookingHeaderRow()
		{
			BookingHeaderDataRow row = new BookingHeaderDataRow("BHDJKTBNE05A5BB22                                                                       IDJK1IDJK2     AUBNEAUSYD                                             ID   Indonesia                          AUAustralia                          LH   LO   25                          N");
			Xsd.CusImportManifestOceanBill oceanBill = new Xsd.CusImportManifestOceanBill();
			Xsd.CusImportManifestArrivalCollection arrivals = new Xsd.CusImportManifestArrivalCollection();
			Mapper.FromBookingHeaderRow(row, oceanBill, arrivals);

			AssertEquals("JKTBNE05A5BB22", oceanBill.OceanBillNumber.Trim());
			AssertEquals("IDJK1", oceanBill.PortOfOrigin.Port.Value);
			AssertEquals("IDJK2", oceanBill.PortOfLoading.Port.Value);
			AssertEquals("AUBNE", oceanBill.PortOfDischarge.Port.Value);
			AssertEquals("AUSYD", oceanBill.PortOfDestination.Port.Value);
			AssertEquals("ID", oceanBill.CountryOfOrigin.Trim());
			AssertEquals("CC", oceanBill.MethodOfPayment);

			AssertEquals(1, arrivals.Count);
			AssertEquals("AUBNE", arrivals[0].ArrivalPort.Port.Value);
		}

		public void TestFromShipperDetailsRow()
		{
			PartyDetailsDataRow row = new PartyDetailsDataRow("SPRSTDBOATHKG     Standard Boat Agency (HKG)         Rm. 221, 2/F., Seven Seas CommerciaCenter, 121, King's Road, North PoiHong Kong                          China");
			Xsd.CusImportManifestOceanBill oceanBill = new Xsd.CusImportManifestOceanBill();
			Mapper.FromShipperDetailsRow(row, oceanBill);

			AssertEquals("STDBOATHKG", oceanBill.Consignor.EDICode.Trim());
			AssertEquals("Standard Boat Agency (HKG)", oceanBill.Consignor.OrganisationDetails.Name.Trim());
			AssertEquals("Rm. 221, 2/F., Seven Seas Commercia", oceanBill.Consignor.OrganisationDetails.Addresses[0].AddressLine1.Trim());
			AssertEquals("Center, 121, King's Road, North Poi", oceanBill.Consignor.OrganisationDetails.Addresses[0].AddressLine2);
		}

		public void TestFromConsigneeDetailsRow()
		{
			PartyDetailsDataRow row = new PartyDetailsDataRow("CNECARGONEBNE     Cargo Network International Pty Lt Unit 4, 1368 Kingsford Smith Drive Meeandah                           QLD 4008                           Australia");
			Xsd.CusImportManifestOceanBill oceanBill = new Xsd.CusImportManifestOceanBill();
			Mapper.FromConsigneeDetailsRow(row, oceanBill);

			AssertEquals("CARGONEBNE", oceanBill.Consignee.EDICode.Trim());
			AssertEquals("Cargo Network International Pty Lt", oceanBill.Consignee.OrganisationDetails.Name.Trim());
			AssertEquals("Unit 4, 1368 Kingsford Smith Drive", oceanBill.Consignee.OrganisationDetails.Addresses[0].AddressLine1.Trim());
			AssertEquals("Meeandah", oceanBill.Consignee.OrganisationDetails.Addresses[0].AddressLine2.Trim());
		}

		public void TestFromCargoGroupRow()
		{
			CargoGroupDataRow row = new CargoGroupDataRow("CGR0001         12210 4      15219.000      12899.000         20.000                      730       2320.000");
			Xsd.CusImportManifestOceanBill oceanBill = new Xsd.CusImportManifestOceanBill();
			Mapper.FromCargoGroupRow(row, oceanBill);

			AssertEquals("E", oceanBill.CargoType);

			row = new CargoGroupDataRow("CGR0001         12210 5      15219.000      12899.000         20.000                      730       2320.000");
			oceanBill = new Xsd.CusImportManifestOceanBill();
			Mapper.FromCargoGroupRow(row, oceanBill);

			AssertEquals(ZString.Empty, oceanBill.CargoType);
		}

		public void TestGetCargoType()
		{
			AssertEquals("I", Mapper.GetCargoType("nzakl", "ausyd"));
			AssertEquals("C", Mapper.GetCargoType("ausyd", "aumel"));
			AssertEquals("X", Mapper.GetCargoType("aumel", "aqmcm"));
			AssertEquals("I", Mapper.GetCargoType("nzakl", "aqmcm"));
		}

		BookingHeaderMapper mapper;
		BookingHeaderMapper Mapper => mapper ?? (mapper = new BookingHeaderMapper());
	}
}
