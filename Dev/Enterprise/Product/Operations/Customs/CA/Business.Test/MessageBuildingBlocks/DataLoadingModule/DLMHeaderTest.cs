using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DLMHeaderTest : TestCase
	{
		public void TestSerialise()
		{
			DLMHeader header = new DLMHeader();
			header.ExporterAuthorizationId = "SC0001";
			header.FormKey = "20050100007";

			header.ExporterName = "Exporter Name";
			header.ExporterBusinessNumber = "123456789RM0001";
			header.ExporterStreet = "Exporter Street";
			header.ExporterCity = "Exporter City";
			header.ExporterProvinceState = "Quebec";
			header.ExporterCountry = "Canada";
			header.ExporterPostalZipCode = "J1J1J1";
			header.ExporterTelephone = "1111111111";
			header.ExporterTelephoneExtension = "2222";
			header.ExporterFax = "3333333333";

			header.ConsigneeName = "Consignee Name";
			header.ConsigneeStreet = "Consignee Street";
			header.ConsigneeCity = "Consignee City";
			header.ConsigneeProvinceState = "Consignee Province State";
			header.ConsigneeCountry = "Cambodia";

			header.ServiceProviderAuthorizationId = "SA0001";
			header.ServiceProviderName = "Service Provider Name";
			header.ServiceProviderStreet = "Service Provider Street";
			header.ServiceProviderCity = "Service Provider City";
			header.ServiceProviderProvinceState = "SP State";
			header.ServiceProviderCountry = "Saint Lucia";
			header.ServiceProviderPostalZipCode = "A9A9A9";
			header.ServiceProviderTelephone = "4444444444";
			header.ServiceProviderTelephoneExtension = "5555";

			header.CertifierName = "Certifier Name";
			header.CertifierStreet = "Certifier Street";
			header.CertifierCity = "Certifier City";
			header.CertifierProvinceState = "Alberta";
			header.CertifierCountry = "Canada";
			header.CertifierPostalZipCode = "K4K4K4";
			header.CertifierTelephone = "6666666666";
			header.CertifierTelephoneExtension = "7777";
			header.CertifierFax = "8888888888";
			header.CertifierCompanyName = "Certifier Company Name";
			header.CertifierStatus = "2";

			header.CommodityGrossWeight = 67.8m;
			header.CommodityGrossWeightUnitOfMeasure = "Kilogram";
			header.FreightCharges = 34.5m;
			header.CommodityCurrencyOfDeclaredValue = "Canadian Dollar";
			header.ModeOfTransport = "Water";
			header.ReasonForExport = "Reason For Export";
			header.VesselName = "Vessel Name";
			header.CountryOfFinalDestination = "Afghanistan";
			header.DateOfExportation = new ZDate(2005, 1, 18);
			header.PortOfExit = "AB-Aden";
			header.PlaceOfReport = "AB-Coutts";
			header.NumberOfPackages = 4;
			header.KindOfPackages = "paquets";
			header.NameOfExportingCompany = "Exporting Company";
			header.TransportationDocumentNumber = "Transp. doc #1";

			string expectedMessage = "H" +
				new string(' ', 6) +
				"SC0001".PadRight(6) +
				new string(' ', 4) +
				"20050100007".PadRight(11) +
				"Exporter Name".PadRight(70) +
				"123456789RM0001".PadRight(15) +
				"Exporter Street".PadRight(70) +
				"Exporter City".PadRight(35) +
				new string(' ', 3) +
				"Quebec".PadRight(30) +
				new string(' ', 2) +
				"Canada".PadRight(20) +
				"J1J1J1".PadRight(15) +
				"1111111111".PadRight(10) +
				"2222".PadRight(4) +
				"3333333333".PadRight(10) +
				"Consignee Name".PadRight(70) +
				"Consignee Street".PadRight(70) +
				"Consignee City".PadRight(35) +
				"Consignee Province State".PadRight(30) +
				new string(' ', 2) +
				"Cambodia".PadRight(20) +
				"SA0001".PadRight(6) +
				"Service Provider Name".PadRight(70) +
				"Service Provider Street".PadRight(70) +
				"Service Provider City".PadRight(35) +
				new string(' ', 3) +
				"SP State".PadRight(30) +
				new string(' ', 2) +
				"Saint Lucia".PadRight(20) +
				"A9A9A9".PadRight(15) +
				"4444444444".PadRight(10) +
				"5555".PadRight(4) +
				"Certifier Name".PadRight(70) +
				"Certifier Street".PadRight(70) +
				"Certifier City".PadRight(35) +
				new string(' ', 3) +
				"Alberta".PadRight(30) +
				new string(' ', 2) +
				"Canada".PadRight(20) +
				"K4K4K4".PadRight(15) +
				"6666666666".PadRight(10) +
				"7777".PadRight(4) +
				"8888888888".PadRight(10) +
				"Certifier Company Name".PadRight(70) +
				"2".PadRight(1) +
				"67.8".PadRight(10) +
				new string(' ', 3) +
				"Kilogram".PadRight(50) +
				new string(' ', 16) +
				"34.5".PadRight(16) +
				new string(' ', 3) +
				"Canadian Dollar".PadRight(50) +
				new string(' ', 1) +
				"Water".PadRight(20) +
				new string(' ', 2) +
				"Reason For Export".PadRight(50) +
				"Vessel Name".PadRight(30) +
				new string(' ', 2) +
				"Afghanistan".PadRight(20) +
				"20050118".PadRight(8) +
				new string(' ', 4) +
				"AB-Aden".PadRight(50) +
				new string(' ', 4) +
				"AB-Coutts".PadRight(50) +
				"4".PadRight(4) +
				new string(' ', 3) +
				"paquets".PadRight(20) +
				"Exporting Company".PadRight(35) +
				"Transp. doc #1".PadRight(35);

			AssertMultilineASCIIEquals("Header", expectedMessage, header.Serialise());
		}
	}
}
