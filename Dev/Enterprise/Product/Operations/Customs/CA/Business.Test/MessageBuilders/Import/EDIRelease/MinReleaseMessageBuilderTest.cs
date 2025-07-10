using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class MinReleaseMessageBuilderTest : TestCaseWithFactory
	{
		public void TestRoundingGrossWeight()
		{
			var mock = GetMock(0m, 0.5m, 0.1m);
			var rmdDRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, rmdDRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++257:105+2:117+987654321RM001:58++2:110
LOC+22+0497+14:::BONDED WAREHOUSE
DTM+232:200801202200:203
DTM+133:200801212335:203
MEA+WT+AAD+KGM:1
MEA+WT+AAC+KGM:1
EQD+CN+AAA987654321
EQD+CN+BBBB1234567890
EQD+CN+CCC43215678
RFF+CN:2ITN12345678987654321
RFF+CN:CCN2
PAC+1++:::BOX
PAC+5++:::PAL
PAC+1++:::CRT
NAD+IM+++ABC IMPORTING CO. A DIVISION OF ANO:THER COMPANY+123 IMPORTER ST.XXXXXXXXXXXXXX12345:6+OTTAWA+ONT+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++XXX BROKER INC.
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.XXXXXXXXXXX12345:601 CONSIGNEE BLVD.XXXXXXXXXXX12345+OTTAWA+ONT+K2B2B2+CA
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862
LOC+27+VAR+UNY
LIN+1++0303030000
QTY+PCE:100
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
LIN+1++0707070000
QTY+DZN:20
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+1
QTY+XYZ:50.5
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+UNY+UNY
LIN+1++0303030000
QTY+PCE:100
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+55+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);

			mock = GetMock(0m, 0.49m, 1.2m);
			rmdDRelease = mock.Object;

			builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, rmdDRelease);
			builder.PopulateMessages();

			query = new ZQuery();
			msg = Factory.LoadTop1<EDIMessage>(query);
			expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++257:105+2:117+987654321RM001:58++2:110
LOC+22+0497+14:::BONDED WAREHOUSE
DTM+232:200801202200:203
DTM+133:200801212335:203
MEA+WT+AAD+KGM:1
MEA+WT+AAC+KGM:1
EQD+CN+AAA987654321
EQD+CN+BBBB1234567890
EQD+CN+CCC43215678
RFF+CN:2ITN12345678987654321
RFF+CN:CCN2
PAC+1++:::BOX
PAC+5++:::PAL
PAC+1++:::CRT
NAD+IM+++ABC IMPORTING CO. A DIVISION OF ANO:THER COMPANY+123 IMPORTER ST.XXXXXXXXXXXXXX12345:6+OTTAWA+ONT+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++XXX BROKER INC.
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.XXXXXXXXXXX12345:601 CONSIGNEE BLVD.XXXXXXXXXXX12345+OTTAWA+ONT+K2B2B2+CA
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862
LOC+27+VAR+UNY
LIN+1++0303030000
QTY+PCE:100
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
LIN+1++0707070000
QTY+DZN:20
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+1
QTY+XYZ:50.5
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+UNY+UNY
LIN+1++0303030000
QTY+PCE:100
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+55+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public void TestMinReleaseMessageBuilder()
		{
			var mock = GetMock(0m);
			var rmdDRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, rmdDRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++257:105+2:117+987654321RM001:58++2:110
LOC+22+0497+14:::BONDED WAREHOUSE
DTM+232:200801202200:203
DTM+133:200801212335:203
MEA+WT+AAD+KGM:550
MEA+WT+AAC+KGM:500
EQD+CN+AAA987654321
EQD+CN+BBBB1234567890
EQD+CN+CCC43215678
RFF+CN:2ITN12345678987654321
RFF+CN:CCN2
PAC+1++:::BOX
PAC+5++:::PAL
PAC+1++:::CRT
NAD+IM+++ABC IMPORTING CO. A DIVISION OF ANO:THER COMPANY+123 IMPORTER ST.XXXXXXXXXXXXXX12345:6+OTTAWA+ONT+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++XXX BROKER INC.
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.XXXXXXXXXXX12345:601 CONSIGNEE BLVD.XXXXXXXXXXX12345+OTTAWA+ONT+K2B2B2+CA
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862
LOC+27+VAR+UNY
LIN+1++0303030000
QTY+PCE:100
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
LIN+1++0707070000
QTY+DZN:20
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+1
QTY+XYZ:50.5
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+UNY+UNY
LIN+1++0303030000
QTY+PCE:100
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+55+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public void TestNotAQReleaseMessageBuilder()
		{
			var mock = GetMock(1000m);
			var rmdDRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, rmdDRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++257:105+2:117+987654321RM001:58++2:110
LOC+22+0497+14:::BONDED WAREHOUSE
DTM+232:200801202200:203
DTM+133:200801212335:203
MEA+WT+AAD+KGM:550
MEA+WT+AAC+KGM:500
EQD+CN+AAA987654321
EQD+CN+BBBB1234567890
EQD+CN+CCC43215678
RFF+CN:2ITN12345678987654321
RFF+CN:CCN2
PAC+1++:::BOX
PAC+5++:::PAL
PAC+1++:::CRT
NAD+IM+++ABC IMPORTING CO. A DIVISION OF ANO:THER COMPANY+123 IMPORTER ST.XXXXXXXXXXXXXX12345:6+OTTAWA+ONT+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++XXX BROKER INC.
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
MOA+39:600.00:CAD
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.XXXXXXXXXXX12345:601 CONSIGNEE BLVD.XXXXXXXXXXX12345+OTTAWA+ONT+K2B2B2+CA
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862
LOC+27+VAR+UNY
LIN+1++0303030000
QTY+PCE:100
MOA+146:1.00
MOA+38:100.00:CAD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
LIN+1++0707070000
QTY+DZN:20
MOA+146:10.00
MOA+38:200.00:CAD
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+1
QTY+XYZ:50.5
MOA+146:5.9406
MOA+38:300.00:CAD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
MOA+39:400.00:CAD
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+UNY+UNY
LIN+1++0303030000
QTY+PCE:100
MOA+146:4.00
MOA+38:400.00:CAD
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+65+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public Mock<IEDIReleaseOGD> GetMock(ZDecimal invoiceAmount, decimal grossweight = 550m, decimal netweight = 500m)
		{
			var mock = new Mock<IEDIReleaseOGD>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New<JobDeclaration>()));
			mock.Setup(m => m.TransactionNumber).Returns("12345987654321");
			mock.Setup(m => m.ServiceOptionID).Returns("257");
			mock.Setup(m => m.AssessmentOption).Returns("2");
			mock.Setup(m => m.ImporterNumber).Returns("987654321RM001");
			mock.Setup(m => m.PriorityIndicator).Returns("2");
			mock.Setup(m => m.PortOfClearance).Returns("0497");
			mock.Setup(m => m.GoodsLocationCode).Returns("14");
			mock.Setup(m => m.GoodsLocationName).Returns("BONDED WAREHOUSE");
			mock.Setup(m => m.DateOfArrival).Returns(new ZDateTime(2008, 1, 20, 22, 0, 0));
			mock.Setup(m => m.DateOfDeparture).Returns(new ZDateTime(2008, 1, 21, 23, 35, 30));
			mock.Setup(m => m.GrossWeight).Returns(grossweight);
			mock.Setup(m => m.GrossWeightUnits).Returns("KG");
			mock.Setup(m => m.NetWeight).Returns(netweight);
			mock.Setup(m => m.NetWeightUnits).Returns("KG");
			mock.Setup(m => m.ContainerNumbers).Returns(new ZString[] { "AAA987654321", "BBBB1234567890", "CCC43215678" });
			mock.Setup(m => m.CargoControlNumbers).Returns(new ZString[] { "2ITN12345678987654321", "CCN2" });
			mock.Setup(m => m.NumberOfPackages).Returns(new ZInt[] { 1, 5, 1 });
			mock.Setup(m => m.TypeOfPackages).Returns(new ZString[] { "BOX", "PAL", "CRT" });
			var importer = Factory.New<JobDocAddress>();
			importer.E2_AddressOverride = true;
			importer.E2_CompanyName = "ABC IMPORTING CO. A DIVISION OF ANOTHER COMPANY";
			importer.E2_Address1 = "123 IMPORTER ST.XXXXXXXXXXXXXX123456";
			importer.E2_City = "OTTAWA";
			importer.E2_State = "ONT";
			importer.E2_Postcode = "K1A1A1";
			importer.E2_RN_NKCountryCode = "CA";
			var carrier = Factory.New<JobDocAddress>();
			carrier.E2_AddressOverride = true;
			carrier.E2_CompanyName = "CUSTOM TRUCKING CO.";
			carrier.E2_Address1 = "XXXX";
			carrier.E2_City = "CHICARGO";
			carrier.E2_State = "IL";
			carrier.E2_Postcode = "K1A1A1";
			carrier.E2_RN_NKCountryCode = "US";
			var broker = Factory.New<JobDocAddress>();
			broker.E2_AddressOverride = true;
			broker.E2_CompanyName = "XXX BROKER INC.";
			broker.E2_Address1 = "XXX";
			broker.E2_Address2 = "XXX";
			broker.E2_City = "MONTREAL";
			broker.E2_State = "QC";
			broker.E2_Postcode = "123456";
			broker.E2_RN_NKCountryCode = "CA";
			mock.Setup(m => m.Importer).Returns(importer);
			mock.Setup(m => m.Carrier).Returns(carrier);
			mock.Setup(m => m.Broker).Returns(broker);
			mock.Setup(m => m.DeliveryInstructions).Returns("THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70 CHARACTERS IN LENGTHY");
			mock.Setup(m => m.TotalValueForDuty).Returns(7000m);
			var invoices = new List<IEDIInvoiceOGD>();
			var mockInvoice1 = new Mock<IEDIInvoiceOGD>();
			mockInvoice1.Setup(m => m.InvoiceNumber).Returns("INV987654321");
			mockInvoice1.Setup(m => m.InvoiceAmount).Returns(invoiceAmount * 0.6m);
			mockInvoice1.Setup(m => m.InvoiceCurrency).Returns("CAD");
			var vendor = Factory.New<JobDocAddress>();
			vendor.E2_AddressOverride = true;
			vendor.E2_CompanyName = "XYZ VENDING CO. A DIVISION OF U.S. INC.";
			vendor.E2_Address1 = "987 VENDOR AVE.";
			vendor.E2_City = "NEW YORK";
			vendor.E2_State = "NY";
			vendor.E2_Postcode = "54321";
			vendor.E2_RN_NKCountryCode = "US";
			var purchaser = Factory.New<JobDocAddress>();
			purchaser.E2_AddressOverride = true;
			purchaser.E2_CompanyName = "GENERAL PURCHASING CO.";
			purchaser.E2_Address1 = "500 PURCHASING DR.";
			purchaser.E2_City = "OTTAWA";
			purchaser.E2_State = "ONT";
			purchaser.E2_Postcode = "K1C1C1";
			purchaser.E2_RN_NKCountryCode = "CA";
			var consignee = Factory.New<JobDocAddress>();
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "CANADIAN RECEIVING CO.";
			consignee.E2_Address1 = "600 CONSIGNEE BLVD.XXXXXXXXXXX123456";
			consignee.E2_Address2 = "601 CONSIGNEE BLVD.XXXXXXXXXXX123456";
			consignee.E2_City = "OTTAWA";
			consignee.E2_State = "ONT";
			consignee.E2_Postcode = "K2B2B2";
			consignee.E2_RN_NKCountryCode = "CA";
			var exporter = Factory.New<JobDocAddress>();
			exporter.E2_AddressOverride = true;
			exporter.E2_CompanyName = "U.S. EXPORTING CO.";
			exporter.E2_Address1 = "800 EXPORTER WAYXXXXXXXXXXXXXX123456";
			exporter.E2_City = "NEW YORK";
			exporter.E2_State = "NY";
			exporter.E2_Postcode = " 123456789";
			exporter.E2_RN_NKCountryCode = "US";
			mockInvoice1.Setup(m => m.Vendor).Returns(vendor);
			mockInvoice1.Setup(m => m.Purchaser).Returns(purchaser);
			mockInvoice1.Setup(m => m.Consignee).Returns(consignee);
			mockInvoice1.Setup(m => m.Exporter).Returns(exporter);
			mockInvoice1.Setup(m => m.HeaderOrigin).Returns("");
			mockInvoice1.Setup(m => m.CommonCountryOfOrigin).Returns("VAR");
			mockInvoice1.Setup(m => m.CommonCountryOfExport).Returns("UNY");
			var invoice1 = mockInvoice1.Object;
			invoices.Add(invoice1);
			var invoiceLines = new List<IEDIInvoiceLineOGD>();
			var mockInvoiceLine1 = new Mock<IEDIInvoiceLineOGD>();
			var mockInvoiceLine2 = new Mock<IEDIInvoiceLineOGD>();
			var mockInvoiceLine3 = new Mock<IEDIInvoiceLineOGD>();
			mockInvoiceLine1.Setup(m => m.TariffNumber).Returns("0303030000");
			mockInvoiceLine1.Setup(m => m.Quantity).Returns(100m);
			mockInvoiceLine1.Setup(m => m.QuantityUnits).Returns("PCE");
			mockInvoiceLine1.Setup(m => m.CountryOfOrigin).Returns("UNY");
			mockInvoiceLine1.Setup(m => m.ItemDescription).Returns("COMMODITY 1 DESCRIPTION");
			mockInvoiceLine1.Setup(m => m.UnitPrice).Returns(invoiceAmount * 0.1m / 100m);
			mockInvoiceLine1.Setup(m => m.LinePrice).Returns(invoiceAmount * 0.1m);
			mockInvoiceLine1.Setup(m => m.LinePriceCurrency).Returns("CAD");
			mockInvoiceLine2.Setup(m => m.TariffNumber).Returns("0707070000");
			mockInvoiceLine2.Setup(m => m.Quantity).Returns(20m);
			mockInvoiceLine2.Setup(m => m.QuantityUnits).Returns("DZN");
			mockInvoiceLine2.Setup(m => m.CountryOfOrigin).Returns("GT");
			mockInvoiceLine2.Setup(m => m.ItemDescription).Returns("COMMODITY 2 DESCRIPTION");
			mockInvoiceLine2.Setup(m => m.UnitPrice).Returns(invoiceAmount * 0.2m / 20m);
			mockInvoiceLine2.Setup(m => m.LinePrice).Returns(invoiceAmount * 0.2m);
			mockInvoiceLine2.Setup(m => m.LinePriceCurrency).Returns("CAD");
			mockInvoiceLine3.Setup(m => m.TariffNumber).Returns("");
			mockInvoiceLine3.Setup(m => m.Quantity).Returns(50.5m);
			mockInvoiceLine3.Setup(m => m.QuantityUnits).Returns("XYZ");
			mockInvoiceLine3.Setup(m => m.CountryOfOrigin).Returns("UNY");
			mockInvoiceLine3.Setup(m => m.ItemDescription).Returns("COMMODITY 3 DESCRIPTION WHICH SHOULD BE SPLIT OVER 2 LINES");
			mockInvoiceLine3.Setup(m => m.UnitPrice).Returns(0m);
			mockInvoiceLine3.Setup(m => m.LinePrice).Returns(invoiceAmount * 0.3m);
			mockInvoiceLine3.Setup(m => m.LinePriceCurrency).Returns("CAD");
			var invoiveLine1 = mockInvoiceLine1.Object;
			var invoiveLine2 = mockInvoiceLine2.Object;
			var invoiveLine3 = mockInvoiceLine3.Object;
			invoiceLines.Add(invoiveLine1);
			invoiceLines.Add(invoiveLine2);
			invoiceLines.Add(invoiveLine3);
			mockInvoice1.Setup(m => m.InvoiceLines).Returns(invoiceLines);
			var mockInvoice2 = new Mock<IEDIInvoiceOGD>();
			mockInvoice2.Setup(m => m.InvoiceNumber).Returns("INV123456789");
			mockInvoice2.Setup(m => m.InvoiceAmount).Returns(invoiceAmount * 0.4m);
			mockInvoice2.Setup(m => m.InvoiceCurrency).Returns("CAD");
			var vendor2 = Factory.New<JobDocAddress>();
			vendor2.E2_AddressOverride = true;
			vendor2.E2_CompanyName = "XYZ VENDING CO.";
			vendor2.E2_Address1 = "987 VENDOR AVE.";
			vendor2.E2_City = "NEW YORK";
			vendor2.E2_State = "NY";
			vendor2.E2_Postcode = "54321";
			vendor2.E2_RN_NKCountryCode = "US";
			var consignee2 = Factory.New<JobDocAddress>();
			consignee2.E2_AddressOverride = true;
			consignee2.E2_CompanyName = "CANADIAN RECEIVING CO.";
			consignee2.E2_Address1 = "600 CONSIGNEE BLVD.";
			consignee2.E2_City = "OTTAWA";
			consignee2.E2_State = "ONT";
			consignee2.E2_Postcode = "K2B2B2";
			consignee2.E2_RN_NKCountryCode = "CA";
			mockInvoice2.Setup(m => m.Vendor).Returns(vendor2);
			mockInvoice2.Setup(m => m.Consignee).Returns(consignee2);
			mockInvoice2.Setup(m => m.HeaderOrigin).Returns("UNY");
			mockInvoice2.Setup(m => m.CommonCountryOfOrigin).Returns("UNY");
			mockInvoice2.Setup(m => m.CommonCountryOfExport).Returns("UNY");
			var invoice2 = mockInvoice2.Object;
			invoices.Add(invoice2);
			var invoiceLines2 = new List<IEDIInvoiceLineOGD>();
			var mockInvoiceLine4 = new Mock<IEDIInvoiceLineOGD>();
			mockInvoiceLine4.Setup(m => m.TariffNumber).Returns("0303030000");
			mockInvoiceLine4.Setup(m => m.Quantity).Returns(100m);
			mockInvoiceLine4.Setup(m => m.QuantityUnits).Returns("PCE");
			mockInvoiceLine4.Setup(m => m.ItemDescription).Returns("COMMODITY 4 DESCRIPTION WITH A DESCRIPTION THAT IS MORE THEN 70 CHARACTERS IN LENGTH");
			mockInvoiceLine4.Setup(m => m.UnitPrice).Returns(0m);
			mockInvoiceLine4.Setup(m => m.LinePrice).Returns(invoiceAmount * 0.4m);
			mockInvoiceLine4.Setup(m => m.LinePriceCurrency).Returns("CAD");
			var invoiveLine4 = mockInvoiceLine4.Object;
			invoiceLines2.Add(invoiveLine4);
			mockInvoice2.Setup(m => m.InvoiceLines).Returns(invoiceLines2);
			mock.Setup(m => m.Invoices).Returns(invoices);
			return mock;
		}
	}
}
