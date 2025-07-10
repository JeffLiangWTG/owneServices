using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common.MessageBuilders;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class SupplementaryCargoReportMessageBuilderTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSupplementaryCargoReportMessageBuilder()
		{
			var mock = GetMock(Factory);
			var supplementaryCargoReport = mock.Object;

			var builder = new SupplementaryCargoReportMessageBuilder(supplementaryCargoReport, MessageSubTypes.Create);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			AssertMultilineASCIIEquals("Message text", ExpectedResult, msg.EM_FormattedMessageText);

			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\SupplementaryCargoReportMessageInterpretation.html");
			AssertMultilineASCIIEquals("Supplementary Cargo Report Message Interpretation", expectedInterpretation, msg.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		static Mock<ISupplementaryCargoReport> GetMock(BusinessObjectFactory factory)
		{
			var messages = new EDIMessageCollection(factory.New<JobDeclaration>());

			var mock = new Mock<ISupplementaryCargoReport>();
			mock.Setup(m => m.Factory).Returns(factory);
			mock.Setup(m => m.Messages).Returns(messages);
			mock.Setup(m => m.DocumentMessageNumber).Returns("ABCD1234");
			mock.Setup(m => m.ServiceOption).Returns("687");
			mock.Setup(m => m.ModeOfTransport).Returns("1");
			mock.Setup(m => m.CarrierCode).Returns("8080");
			mock.Setup(m => m.OriginalCargoControlNumber).Returns("ORIGCCN");
			mock.Setup(m => m.UniqueConsignmentReference).Returns("UCR");
			mock.Setup(m => m.SupplementaryReferenceNumber).Returns("8080SRN123456");
			mock.Setup(m => m.DestinationCountryCode).Returns("US");
			mock.Setup(m => m.DestinationCityName).Returns("NEWYORK890123456789012345X");
			mock.Setup(m => m.DestinationPortName).Returns("JFK INTERNATIONAL AIRPORT");
			mock.Setup(m => m.CustomsProcedureCode).Returns("23");
			mock.Setup(m => m.SpecialInstructions).Returns("FRAGILE GLASS HANDLE WITH CAUTION");
			mock.Setup(m => m.BillOfLading).Returns(" BILL OF LADING NUMBER ");

			var consignor = new AddressForACI("CONSIGNOR NAME LINE 1", "CONSIGNOR ADDRESS LINE 1",
											  "", "PARIS", "", "", "FR", "0129337218", "GILLES");

			var consignee = new AddressForACI("CONSIGNEE NAME WHICH IS MORE THAN 35 CHARACTERS",
											  "CONSIGNEE ADDRESS LINE 1", "CONSIGNEE ADDRESS LINE 2", "MANHATTAN", "NY", "12986", "US", "2125555212", "FRANK");

			var delivery = new AddressForACI("DELIVERED TO 1", "DELIVERY 1 ADDRESS LINE 1", "",
											 "MANHATTAN", "NY", "12783", "US", "212-555 1212", "ELIZABETH");

			var notify = new AddressForACI("NOTIFY PÄRTY 1", "ÄDDRESS LINE 1 678901234567890123456", "",
										   "NEW YORK 012345678901234567890123456", "NY", "12345", "US", "6475551212", "SUZÄNNE");

			mock.Setup(m => m.Consignor).Returns(consignor);
			mock.Setup(m => m.Consignee).Returns(consignee);
			mock.Setup(m => m.DeliveryParty).Returns(delivery);
			mock.Setup(m => m.NotifyParty).Returns(notify);

			var containers = new List<ISCRContainer>();
			var mockContainer1 = new Mock<ISCRContainer>();
			mockContainer1.Setup(m => m.ContainerNumber).Returns("OCLU1111111");
			mockContainer1.Setup(m => m.CountryOfRegistration).Returns("US");
			mockContainer1.Setup(m => m.ContainerSizeCode).Returns("22G0");
			mockContainer1.Setup(m => m.IsEmpty).Returns(false);
			var container1 = mockContainer1.Object;
			containers.Add(container1);
			var mockContainer2 = new Mock<ISCRContainer>();
			mockContainer2.Setup(m => m.ContainerNumber).Returns("OCLU22222222");
			mockContainer2.Setup(m => m.CountryOfRegistration).Returns("CA");
			mockContainer2.Setup(m => m.ContainerSizeCode).Returns("4G01");
			mockContainer2.Setup(m => m.IsEmpty).Returns(true);
			var container2 = mockContainer2.Object;
			containers.Add(container2);
			mock.Setup(m => m.Containers).Returns(containers);

			var goodsLines = new List<ISCRLine>();
			var mockLine1 = new Mock<ISCRLine>();
			mockLine1.Setup(m => m.NumberOfPackages).Returns(2400);
			mockLine1.Setup(m => m.TypeOfPackages).Returns("PCS");
			mockLine1.Setup(m => m.GoodsDescription).Returns("CARTRIDGES SMALL ARMS BLANK\n\rWHITE DOVES");
			mockLine1.Setup(m => m.GrossWeight).Returns(12345678.1234);
			mockLine1.Setup(m => m.WeightUnits).Returns(Constants.Weight.Kilograms);
			mockLine1.Setup(m => m.Volume).Returns(200.075);
			mockLine1.Setup(m => m.VolumeUnits).Returns(Constants.Volume.Litre);
			mockLine1.Setup(m => m.ContainerNumber).Returns("IJKL4065400");
			mockLine1.Setup(m => m.DGCodes).Returns("0327a, MHB, MHB, MHB");
			mockLine1.Setup(m => m.ShippingMarks).Returns("                                        MARKS LINE 1\r\nMARKS LINE 2\r\nMARKS LINE 3\r\nMARKS LINE 4\r\nMARKS LINE 5\r\nMARKS LINE 6\r\nMARKS LINE 7\r\nMARKS LINE 8\r\nMARKS LINE 9\r\nMARKS LINE 10");
			mockLine1.Setup(m => m.TariffNumbers).Returns("6601.10.00 00, 6602001000,6602001001 , 6602001002, 6602001003");
			var line1 = mockLine1.Object;
			goodsLines.Add(line1);
			var mockLine2 = new Mock<ISCRLine>();
			mockLine2.Setup(m => m.NumberOfPackages).Returns(200);
			mockLine2.Setup(m => m.TypeOfPackages).Returns("BOX");
			mockLine2.Setup(m => m.GoodsDescription).Returns("FRENCH DARK CHOCOLATE\n\rA LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER TWO FTX SEGMENTS");
			mockLine2.Setup(m => m.GrossWeight).Returns(2000.375);
			mockLine2.Setup(m => m.WeightUnits).Returns(Constants.Weight.Pounds);
			mockLine2.Setup(m => m.Volume).Returns(0m);
			mockLine2.Setup(m => m.VolumeUnits).Returns("");
			mockLine2.Setup(m => m.ContainerNumber).Returns("");
			mockLine2.Setup(m => m.DGCodes).Returns("");
			mockLine2.Setup(m => m.ShippingMarks).Returns("A LONG MARKS LINE THAT SHOULD BE SPLIT INTO TWO SEGMENTS");
			mockLine2.Setup(m => m.TariffNumbers).Returns("");
			var line2 = mockLine2.Object;
			goodsLines.Add(line2);
			var mockLine3 = new Mock<ISCRLine>();
			mockLine3.Setup(m => m.NumberOfPackages).Returns(200);
			mockLine3.Setup(m => m.TypeOfPackages).Returns("BOX");
			mockLine3.Setup(m => m.GoodsDescription).Returns("DESCRIPTION 1\r\nA LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER TWO FTX SEGMENTS\r\nDESCRIPTION 4\r\nDESCRIPTION 5\r\nDESCRIPTION 6                                                  \r\nDESCRIPTION 7\r\nDESCRIPTION 8\r\nDESCRIPTION 9\r\nDESCRIPTION NOT TO BE QUOTED");
			mockLine3.Setup(m => m.GrossWeight).Returns(2000.375);
			mockLine3.Setup(m => m.WeightUnits).Returns(Constants.Weight.Pounds);
			mockLine3.Setup(m => m.Volume).Returns(0m);
			mockLine3.Setup(m => m.VolumeUnits).Returns("");
			mockLine3.Setup(m => m.ContainerNumber).Returns("");
			mockLine3.Setup(m => m.DGCodes).Returns("");
			mockLine3.Setup(m => m.ShippingMarks).Returns("MARKS AND NUMBERS");
			mockLine3.Setup(m => m.TariffNumbers).Returns("");
			var line3 = mockLine3.Object;
			goodsLines.Add(line3);
			var mockLine4 = new Mock<ISCRLine>();
			mockLine4.Setup(m => m.NumberOfPackages).Returns(200);
			mockLine4.Setup(m => m.TypeOfPackages).Returns("BOX");
			mockLine4.Setup(m => m.GoodsDescription).Returns("GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX1GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX2GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX3GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX4GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX5GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX6GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX7GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX8GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX9SUPERFLUOUS CHARACTERS ARE DISCARDED");
			mockLine4.Setup(m => m.GrossWeight).Returns(2000.375);
			mockLine4.Setup(m => m.WeightUnits).Returns(Constants.Weight.Pounds);
			mockLine4.Setup(m => m.Volume).Returns(0m);
			mockLine4.Setup(m => m.VolumeUnits).Returns("");
			mockLine4.Setup(m => m.ContainerNumber).Returns("");
			mockLine4.Setup(m => m.DGCodes).Returns("");
			mockLine4.Setup(m => m.ShippingMarks).Returns("MARKS AND NUMBERS");
			mockLine4.Setup(m => m.TariffNumbers).Returns("");
			var line4 = mockLine4.Object;
			goodsLines.Add(line4);
			mock.Setup(m => m.GoodsLines).Returns(goodsLines);
			mock.Setup(m => m.Authentication).Returns("12345678");
			return mock;
		}

		const string ExpectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+GSMCAR:D:00A:UN:SUPRPT
BGM+85+ABCD1234+9
CST++687::96
TDT+20++1++8080
CNI+1
DOC+704+ORIGCCN
DOC+701+UCR
RFF+ABE:8080SRN123456
LOC+8+US:::NEWYORK890123456789012345+JFK INTERNATIONAL AIRPORT
GEI+6+:::23
FTX+SIN+++FRAGILE GLASS HANDLE WITH CAUTION
TDT+12
RFF+AIJ:BILL OF LADING NUMBER
NAD+CN+++CONSIGNEE NAME WHICH IS MORE THAN 3:5 CHARACTERS+CONSIGNEE ADDRESS LINE 1:CONSIGNEE ADDRESS LINE 2+MANHATTAN+NY+12986+US
CTA+CN+:FRANK
COM+2125555212:TE
NAD+CZ+++CONSIGNOR NAME LINE 1+CONSIGNOR ADDRESS LINE 1+PARIS+++FR
CTA+CO+:GILLES
COM+0129337218:TE
NAD+DP+++DELIVERED TO 1+DELIVERY 1 ADDRESS LINE 1+MANHATTAN+NY+12783+US
CTA+DL+:ELIZABETH
COM+2125551212:TE
NAD+NI+++NOTIFY PARTY 1+ADDRESS LINE 1 67890123456789012345:6+NEW YORK 01234567890123456789012345+NY+12345+US
CTA+NT+:SUZANNE
COM+6475551212:TE
EQD+CN+OCLU1111111US22G0::5++++5
EQD+CN+OCLU2222222CA4G01::5++++4
GID+1
PAC+2400++PCS
FTX+AAA+++CARTRIDGES SMALL ARMS BLANK
FTX+AAA+++WHITE DOVES
MEA+WT+AAE+KGM:12345678.1234
MEA+VOL+:::V+WSD:200.075
SGP+IJKL4065400
DGS+++0327
DGS+++MHB
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3:MARKS LINE 4:MARKS LINE 5:MARKS LINE 6:MARKS LINE 7:MARKS LINE 8:MARKS LINE 9
PCI++MARKS LINE 10
CST++6601100000+6602001000+6602001001+6602001002+6602001003
GID+2
PAC+200++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+AAA+++A LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER
FTX+AAA+++TWO FTX SEGMENTS
MEA+WT+AAE+LBR:2000.375
PCI++A LONG MARKS LINE THAT SHOULD BE SP:LIT INTO TWO SEGMENTS
GID+3
PAC+200++BOX
FTX+AAA+++DESCRIPTION 1
FTX+AAA+++A LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER
FTX+AAA+++TWO FTX SEGMENTS
FTX+AAA+++DESCRIPTION 4
FTX+AAA+++DESCRIPTION 5
FTX+AAA+++DESCRIPTION 6
FTX+AAA+++DESCRIPTION 7
FTX+AAA+++DESCRIPTION 8
FTX+AAA+++DESCRIPTION 9
MEA+WT+AAE+LBR:2000.375
PCI++MARKS AND NUMBERS
GID+4
PAC+200++BOX
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX1
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX2
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX3
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX4
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX5
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX6
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX7
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX8
FTX+AAA+++GOODS DESCRIPTION FILLING 50 CHARACTERS IN SEGFTX9
MEA+WT+AAE+LBR:2000.375
PCI++MARKS AND NUMBERS
AUT+12345678
UNT+74+<<MSGNO PLACEHOLDER>>";
	}
}
