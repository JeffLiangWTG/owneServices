using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class OGDReleaseMessageBuilderTest : TestCaseWithFactory
	{
		public void TestCFIAReleaseMessageBuilder()
		{
			CACustomsDataRegistry.Instance.AlwaysSendDeliveryAddressOnReleaseMessages.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var mock = GetMock(true, false, false, false);
			var ediRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, ediRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++471:105+1:117+987654321RM001:58++2:110
LOC+22+0497+14:::BONDED WAREHOUSE
DTM+232:200801202200:203
DTM+133:200801212335:203
GIS+18:::1
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
NAD+IM+++ABC IMPORTING CO. A DIVISION OF ANO:THER COMPANY+123 IMPORTER ST.XXXXXXXXXXXXXX12345:6+OTTAWA+ON+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++XXX BROKER INC.
NAD+DP+++DELIVERED TO NAME+DELIVERY ADDRESS LINE 1456789012345:DELIVERY ADDRESS LINE 2456789012345+MONTREAL901234567890+QC+123456
COM+2345678901:TE
COM+345679999:FX
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
DTM+3:20090630:102
MOA+39:1234.56:USD
MOA+144:100
MOA+105:200
MOA+107:301
MOA+145:400
MOA+209:500
MOA+106:600
TOD+++:::OTHER REFERENCE------------------------------------------------------>:MORE OTHER REFERENCE
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ON+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ON+K2B2B2+CA
NAD+OS+++U.S. SHIPPING CO.+800 SHIPPER WAY+NEW YORK+NY+123456789+US
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862:::DEPARTMENTAL RULINGS+::PLACE OF DIRECT SHIPMENT
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+11
LIN+1++0303030000+:1
QTY+PCE:100
MOA+146:10.00
MOA+38:1000.00:USD
GIR+1+REQID+VER+AIRS+ON+XXX
GIR+7+123
GIR+2+NUM1+C1
GIR+2+NUM2+C2
GIR+2+NUM3+IC
TOD+5
LOC+27+UNY+MX
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
LIN+1++0707070000+:2
QTY+DZN:20
MOA+146:5.50
MOA+38:12.34:USD
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+2+++:1
QTY+XYZ:50.5
MOA+146:1.2346
MOA+38:1000.00:USD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
DTM+3:20090701:102
MOA+39:0.00
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ON+K2B2B2+CA
DOC+862
LOC+27+AU+UNY
ALC+G+01
LIN+1++0303030000+:1
QTY+PCE:100
MOA+146:0.00
MOA+38:0.00
TOD+5
LOC+27+AU+NZ
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+90+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public void TestICReleaseMessageBuilder()
		{
			var mock = GetMock(false, true, false, false);
			var ediRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, ediRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++471:105+1:117+987654321RM001:58++2:110
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
NAD+DP+++DELIVERED TO NAME+DELIVERY ADDRESS LINE 1456789012345:DELIVERY ADDRESS LINE 2456789012345+MONTREAL901234567890+QC+123456
COM+2345678901:TE
COM+345679999:FX
TOD+++:::THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70:CHARACTERS IN LENGTHY
MOA+39:7000
UNS+D
DMS+INV987654321
DTM+3:20090630:102
MOA+39:1234.56:USD
MOA+144:100
MOA+105:200
MOA+107:301
MOA+145:400
MOA+209:500
MOA+106:600
TOD+++:::OTHER REFERENCE------------------------------------------------------>:MORE OTHER REFERENCE
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
NAD+OS+++U.S. SHIPPING CO.+800 SHIPPER WAY+NEW YORK+NY+123456789+US
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862:::DEPARTMENTAL RULINGS+::PLACE OF DIRECT SHIPMENT
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+11
LIN+1++0303030000+:1
PIA+1+MAKE:VN+MODEL:MF+MODELNUMBER:MN+BRANDNAME:MP+VCLASS:VS
QTY+PCE:100
MOA+146:10.00
MOA+38:1000.00:USD
GIR+2+NUM1+C1
GIR+2+NUM2+C2
GIR+2+NUM3+IC
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
IMD++5+01
LIN+1++0707070000+:2
QTY+DZN:20
MOA+146:5.50
MOA+38:12.34:USD
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+2+++:1
QTY+XYZ:50.5
MOA+146:1.2346
MOA+38:1000.00:USD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
DTM+3:20090701:102
MOA+39:0.00
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+AU+UNY
ALC+G+01
LIN+1++0303030000+:1
QTY+PCE:100
MOA+146:0.00
MOA+38:0.00
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+87+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public void TestNRCANReleaseMessageBuilder()
		{
			CACustomsDataRegistry.Instance.AlwaysSendDeliveryAddressOnReleaseMessages.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var mock = GetMock(false, false, true, false);
			var ediRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, ediRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++471:105+1:117+987654321RM001:58++2:110
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
DTM+3:20090630:102
MOA+39:1234.56:USD
MOA+144:100
MOA+105:200
MOA+107:301
MOA+145:400
MOA+209:500
MOA+106:600
TOD+++:::OTHER REFERENCE------------------------------------------------------>:MORE OTHER REFERENCE
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
NAD+OS+++U.S. SHIPPING CO.+800 SHIPPER WAY+NEW YORK+NY+123456789+US
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
DOC+862:::DEPARTMENTAL RULINGS+::PLACE OF DIRECT SHIPMENT
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+11
LIN+1++0303030000+:1
PIA+1+MAKE:VN+MODEL:MF+MODELNUMBER:MN+BRANDNAME:MP+VCLASS:VS
QTY+PCE:100
MEA+ABC+:::TYPESIZE
MOA+146:10.00
MOA+38:1000.00:USD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
IMD++5+01
LIN+1++0707070000+:2
QTY+DZN:20
MOA+146:5.50
MOA+38:12.34:USD
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+2+++:1
QTY+XYZ:50.5
MOA+146:1.2346
MOA+38:1000.00:USD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
DTM+3:20090701:102
MOA+39:0.00
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+AU+UNY
ALC+G+01
LIN+1++0303030000+:1
QTY+PCE:100
MOA+146:0.00
MOA+38:0.00
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+82+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public void TestTireReleaseMessageBuilder()
		{
			CACustomsDataRegistry.Instance.AlwaysSendDeliveryAddressOnReleaseMessages.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var mock = GetMock(false, false, false, true);
			var ediRelease = mock.Object;

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, ediRelease);
			builder.PopulateMessages();

			var query = new ZQuery();
			var msg = Factory.LoadTop1<EDIMessage>(query);
			ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345987654321+9
CST++471:105+1:117+987654321RM001:58++2:110
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
DTM+3:20090630:102
MOA+39:1234.56:USD
MOA+144:100
MOA+105:200
MOA+107:301
MOA+145:400
MOA+209:500
MOA+106:600
TOD+++:::OTHER REFERENCE------------------------------------------------------>:MORE OTHER REFERENCE
NAD+VN+++XYZ VENDING CO. A DIVISION OF U.S.:INC.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+BY+++GENERAL PURCHASING CO.+500 PURCHASING DR.+OTTAWA+ONT+K1C1C1+CA
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
NAD+OS+++U.S. SHIPPING CO.+800 SHIPPER WAY+NEW YORK+NY+123456789+US
NAD+EX+++U.S. EXPORTING CO.+800 EXPORTER WAYXXXXXXXXXXXXXX12345:6+NEW YORK+NY+123456789+US
NAD+MF+++MEXICO MANUFACTURING+800 MANUFACTURING WAYXXXXXXXXX12345:801 MANUFACTURING WAYXXXXXXXXX12345+MEXICO CITY+XX+9999+MX
DOC+862:::DEPARTMENTAL RULINGS+::PLACE OF DIRECT SHIPMENT
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+11
LIN+1++0303030000+:1
PIA+1+MAKE:VN+++BRANDNAME:MP+VCLASS:VS
QTY+PCE:100
MEA+ABC+:::TYPESIZE
MOA+146:10.00
MOA+38:1000.00:USD
GIR+3+3:AB+1:AC+TIIN:AD
GIR+4+VIN1:VV+200901:AN
GIR+4+VIN2:VV+200902:AN
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 1 DESCRIPTION
IMD++5+01
LIN+1++0707070000+:2
QTY+DZN:20
MOA+146:5.50
MOA+38:12.34:USD
TOD+5
LOC+27+GT
IMD+++TRDESC:::COMMODITY 2 DESCRIPTION
LIN+2+++:1
QTY+XYZ:50.5
MOA+146:1.2346
MOA+38:1000.00:USD
TOD+5
LOC+27+UNY
IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES
DMS+INV123456789
DTM+3:20090701:102
MOA+39:0.00
NAD+VN+++XYZ VENDING CO.+987 VENDOR AVE.+NEW YORK+NY+54321+US
NAD+UC+++CANADIAN RECEIVING CO.+600 CONSIGNEE BLVD.+OTTAWA+ONT+K2B2B2+CA
DOC+862
LOC+27+AU+UNY
ALC+G+01
LIN+1++0303030000+:1
QTY+PCE:100
MOA+146:0.00
MOA+38:0.00
IMD+++TRDESC:::COMMODITY 4 DESCRIPTION WITH A DESC:RIPTION THAT IS MORE THEN 70 CHARAC
FTX+AAA+++TERS IN LENGTH
UNS+S
UNT+86+<<MSGNO PLACEHOLDER>>";
			AssertMultilineASCIIEquals("Message text", expectedResult, msg.EM_FormattedMessageText);
		}

		public Mock<IEDIReleaseOGD> GetMock(ZBool oGDCFIA, ZBool oGDIC, ZBool oGDNR, ZBool oGDTC)
		{
			var mock = new Mock<IEDIReleaseOGD>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New<JobDeclaration>()));
			mock.Setup(m => m.TransactionNumber).Returns("12345987654321");
			mock.Setup(m => m.ServiceOptionID).Returns("471");
			mock.Setup(m => m.AssessmentOption).Returns("1");
			mock.Setup(m => m.ImporterNumber).Returns("987654321RM001");
			mock.Setup(m => m.PriorityIndicator).Returns("2");
			mock.Setup(m => m.PortOfClearance).Returns("0497");
			mock.Setup(m => m.GoodsLocationCode).Returns("14");
			mock.Setup(m => m.GoodsLocationName).Returns("BONDED WAREHOUSE");
			mock.Setup(m => m.DateOfArrival).Returns(new ZDateTime(2008, 1, 20, 22, 0, 0));
			mock.Setup(m => m.DateOfDeparture).Returns(new ZDateTime(2008, 1, 21, 23, 35, 30));
			mock.Setup(m => m.GrossWeight).Returns(550m);
			mock.Setup(m => m.GrossWeightUnits).Returns("KG");
			mock.Setup(m => m.NetWeight).Returns(500);
			mock.Setup(m => m.NetWeightUnits).Returns("KG");
			mock.Setup(m => m.ContainerNumbers).Returns(new ZString[] { "AAA987654321", "BBBB1234567890", "CCC43215678" });
			mock.Setup(m => m.CargoControlNumbers).Returns(new ZString[] { "2ITN12345678987654321", "CCN2" });
			mock.Setup(m => m.NumberOfPackages).Returns(new ZInt[] { 1, 5, 1 });
			mock.Setup(m => m.TypeOfPackages).Returns(new ZString[] { "BOX", "PAL", "CRT" });
			mock.Setup(m => m.OGDCFIA).Returns(oGDCFIA);
			mock.Setup(m => m.OGDIC).Returns(oGDIC);
			mock.Setup(m => m.OGDNR).Returns(oGDNR);
			mock.Setup(m => m.OGDTC).Returns(oGDTC);
			mock.Setup(m => m.DeliveryPhone).Returns("+1 (234) 567-8901");
			mock.Setup(m => m.DeliveryFax).Returns("(34) 567-9999");
			var importer = Factory.New<JobDocAddress>();
			importer.E2_AddressOverride = true;
			importer.E2_CompanyName = "ABC IMPORTING CO. A DIVISION OF ANOTHER COMPANY";
			importer.E2_Address1 = "123 IMPORTER ST.XXXXXXXXXXXXXX123456";
			importer.E2_City = "OTTAWA";
			importer.E2_State = "ONT";
			importer.E2_Postcode = "K1A1A1";
			importer.E2_RN_NKCountryCode = "CA";
			var delivery = Factory.New<JobDocAddress>();
			delivery.E2_AddressOverride = true;
			delivery.E2_CompanyName = "DELIVERED TO NAME";
			delivery.E2_Address1 = "DELIVERY ADDRESS LINE 14567890123456";
			delivery.E2_Address2 = "DELIVERY ADDRESS LINE 24567890123456";
			delivery.E2_City = "MONTREAL9012345678901";
			delivery.E2_State = "QC";
			delivery.E2_Postcode = "123456";
			delivery.E2_RN_NKCountryCode = "CA";
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
			mock.Setup(m => m.DeliveryAddress).Returns(delivery);
			mock.Setup(m => m.DeliveryInstructions).Returns("THESE ARE THE DELIVERY INSTRUCTIONS IN A LONG STRING WHICH IS OVER 70 CHARACTERS IN LENGTHY");
			mock.Setup(m => m.TotalValueForDuty).Returns(7000m);
			var invoices = new List<IEDIInvoiceOGD>();
			var mockInvoice1 = new Mock<IEDIInvoiceOGD>();
			mockInvoice1.Setup(m => m.InvoiceNumber).Returns("INV987654321");
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
			consignee.E2_Address1 = "600 CONSIGNEE BLVD.";
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
			exporter.E2_Postcode = "123456789";
			exporter.E2_RN_NKCountryCode = "US";
			var manufacturer = Factory.New<JobDocAddress>();
			manufacturer.E2_AddressOverride = true;
			manufacturer.E2_CompanyName = "MEXICO MANUFACTURING";
			manufacturer.E2_Address1 = "800 MANUFACTURING WAYXXXXXXXXX123456";
			manufacturer.E2_Address2 = "801 MANUFACTURING WAYXXXXXXXXX123456";
			manufacturer.E2_City = "MEXICO CITY";
			manufacturer.E2_State = "XX";
			manufacturer.E2_Postcode = "9999";
			manufacturer.E2_RN_NKCountryCode = "MX";
			var shipper = Factory.New<JobDocAddress>();
			shipper.E2_AddressOverride = true;
			shipper.E2_CompanyName = "U.S. SHIPPING CO.";
			shipper.E2_Address1 = "800 SHIPPER WAY";
			shipper.E2_City = "NEW YORK";
			shipper.E2_State = "NY";
			shipper.E2_Postcode = "123456789";
			shipper.E2_RN_NKCountryCode = "US";
			mockInvoice1.Setup(m => m.Vendor).Returns(vendor);
			mockInvoice1.Setup(m => m.Purchaser).Returns(purchaser);
			mockInvoice1.Setup(m => m.Consignee).Returns(consignee);
			mockInvoice1.Setup(m => m.Shipper).Returns(shipper);
			mockInvoice1.Setup(m => m.Exporter).Returns(exporter);
			mockInvoice1.Setup(m => m.Manufacturer).Returns(manufacturer);
			mockInvoice1.Setup(m => m.HeaderOrigin).Returns("");
			mockInvoice1.Setup(m => m.CommonCountryOfOrigin).Returns("VAR");
			mockInvoice1.Setup(m => m.CommonCountryOfExport).Returns("UNY");
			mockInvoice1.Setup(m => m.InvoiceDate).Returns(new ZDate(2009, 6, 30));
			mockInvoice1.Setup(m => m.InvoiceAmount).Returns(new ZDecimal(1234.56));
			mockInvoice1.Setup(m => m.InvoiceCurrency).Returns("USD");
			mockInvoice1.Setup(m => m.IncludedOFTAndONS).Returns(new ZDecimal(100));
			mockInvoice1.Setup(m => m.IncludedConstruction).Returns(new ZDecimal(200.49));
			mockInvoice1.Setup(m => m.IncludedPacking).Returns(new ZDecimal(300.50));
			mockInvoice1.Setup(m => m.ExcludedOFTAndONS).Returns(new ZDecimal(400));
			mockInvoice1.Setup(m => m.ExcludedCommission).Returns(new ZDecimal(500));
			mockInvoice1.Setup(m => m.ExcludedPacking).Returns(new ZDecimal(600));
			mockInvoice1.Setup(m => m.OtherReference).Returns("OTHER REFERENCE------------------------------------------------------>MORE OTHER REFERENCE");
			mockInvoice1.Setup(m => m.DepartmentRuling).Returns("DEPARTMENTAL RULINGS");
			mockInvoice1.Setup(m => m.LastPortName).Returns("PLACE OF DIRECT SHIPMENT");
			mockInvoice1.Setup(m => m.LastPortDate).Returns(new ZDate(2009, 4, 28));
			mockInvoice1.Setup(m => m.TranshipmentCountry).Returns("SG");
			mockInvoice1.Setup(m => m.ConditionsOfSale).Returns("CONDITIONS OF SALE");
			mockInvoice1.Setup(m => m.TermsOfPayment).Returns("TERMS OF PAYMENT");
			mockInvoice1.Setup(m => m.ServicesInd).Returns(ZBool.True);
			mockInvoice1.Setup(m => m.RoyaltyInd).Returns(ZBool.True);
			var invoice1 = mockInvoice1.Object;
			invoices.Add(invoice1);
			var invoiceLines = new List<IEDIInvoiceLineOGD>();
			var mockInvoiceLine1 = new Mock<IEDIInvoiceLineOGD>();
			var mockInvoiceLine2 = new Mock<IEDIInvoiceLineOGD>();
			var mockInvoiceLine3 = new Mock<IEDIInvoiceLineOGD>();
			mockInvoiceLine1.Setup(m => m.PageNumber).Returns(new ZInt(1));
			mockInvoiceLine1.Setup(m => m.LineNumber).Returns(new ZInt(1));
			mockInvoiceLine1.Setup(m => m.TariffNumber).Returns("0303030000");
			mockInvoiceLine1.Setup(m => m.Quantity).Returns(100m);
			mockInvoiceLine1.Setup(m => m.QuantityUnits).Returns("PCE");
			mockInvoiceLine1.Setup(m => m.UnitPrice).Returns(0m);
			mockInvoiceLine1.Setup(m => m.LinePrice).Returns(1000m);
			mockInvoiceLine1.Setup(m => m.LinePriceCurrency).Returns("USD");
			mockInvoiceLine1.Setup(m => m.CountryOfOrigin).Returns("UNY");
			mockInvoiceLine1.Setup(m => m.ItemDescription).Returns("COMMODITY 1 DESCRIPTION");
			mockInvoiceLine1.Setup(m => m.RequirementID).Returns("REQID");
			mockInvoiceLine1.Setup(m => m.RequirementVersion).Returns("VER");
			mockInvoiceLine1.Setup(m => m.AirsCode).Returns("AIRS");
			mockInvoiceLine1.Setup(m => m.DestinationProvince).Returns("ON");
			mockInvoiceLine1.Setup(m => m.EndUse).Returns("XXX");
			mockInvoiceLine1.Setup(m => m.MiscID).Returns("123");
			mockInvoiceLine1.Setup(m => m.CFIAOrigin).Returns("MX");
			mockInvoiceLine1.Setup(m => m.RegistrationNumbers).Returns(new ZString[] { "NUM1", "NUM2", "NUM3" });
			mockInvoiceLine1.Setup(m => m.RegistrationTypes).Returns(new ZString[] { "C1", "C2", "IC" });
			mockInvoiceLine1.Setup(m => m.ImportReasonCode).Returns("01");
			mockInvoiceLine1.Setup(m => m.Make).Returns("MAKE");
			mockInvoiceLine1.Setup(m => m.Model).Returns("MODEL");
			mockInvoiceLine1.Setup(m => m.ModelNumber).Returns("MODELNUMBER");
			mockInvoiceLine1.Setup(m => m.BrandName).Returns("BRANDNAME");
			mockInvoiceLine1.Setup(m => m.VehicleClass).Returns("VCLASS");
			mockInvoiceLine1.Setup(m => m.TypeSize).Returns("TYPESIZE");
			mockInvoiceLine1.Setup(m => m.VIN).Returns(new ZString[] { "VIN1", "VIN2" });
			mockInvoiceLine1.Setup(m => m.AssemblyMonth).Returns(new ZString[] { "200901", "200902" });
			mockInvoiceLine1.Setup(m => m.CompliantImportDateIndicator).Returns(ZBool.True);
			mockInvoiceLine1.Setup(m => m.CompliantCompletionIndicator).Returns(ZBool.True);
			mockInvoiceLine1.Setup(m => m.TIIN).Returns("TIIN");
			mockInvoiceLine2.Setup(m => m.PageNumber).Returns(new ZInt(1));
			mockInvoiceLine2.Setup(m => m.LineNumber).Returns(new ZInt(2));
			mockInvoiceLine2.Setup(m => m.TariffNumber).Returns("0707070000");
			mockInvoiceLine2.Setup(m => m.Quantity).Returns(20m);
			mockInvoiceLine2.Setup(m => m.QuantityUnits).Returns("DZN");
			mockInvoiceLine2.Setup(m => m.UnitPrice).Returns(5.5m);
			mockInvoiceLine2.Setup(m => m.LinePrice).Returns(12.34m);
			mockInvoiceLine2.Setup(m => m.LinePriceCurrency).Returns("USD");
			mockInvoiceLine2.Setup(m => m.CountryOfOrigin).Returns("GT");
			mockInvoiceLine2.Setup(m => m.ItemDescription).Returns("COMMODITY 2 DESCRIPTION");
			mockInvoiceLine2.Setup(m => m.RequirementID).Returns("");
			mockInvoiceLine2.Setup(m => m.RequirementVersion).Returns("");
			mockInvoiceLine2.Setup(m => m.AirsCode).Returns("");
			mockInvoiceLine2.Setup(m => m.DestinationProvince).Returns("");
			mockInvoiceLine2.Setup(m => m.EndUse).Returns("");
			mockInvoiceLine2.Setup(m => m.RegistrationNumbers).Returns(Array.Empty<ZString>());
			mockInvoiceLine2.Setup(m => m.RegistrationTypes).Returns(Array.Empty<ZString>());
			mockInvoiceLine2.Setup(m => m.MiscID).Returns("");
			mockInvoiceLine2.Setup(m => m.CFIAOrigin).Returns("");
			mockInvoiceLine2.Setup(m => m.ImportReasonCode).Returns("");
			mockInvoiceLine2.Setup(m => m.Make).Returns("");
			mockInvoiceLine2.Setup(m => m.Model).Returns("");
			mockInvoiceLine2.Setup(m => m.ModelNumber).Returns("");
			mockInvoiceLine2.Setup(m => m.BrandName).Returns("");
			mockInvoiceLine2.Setup(m => m.VehicleClass).Returns("");
			mockInvoiceLine2.Setup(m => m.TypeSize).Returns("");
			mockInvoiceLine2.Setup(m => m.VIN).Returns(Array.Empty<ZString>());
			mockInvoiceLine2.Setup(m => m.AssemblyMonth).Returns(Array.Empty<ZString>());
			mockInvoiceLine2.Setup(m => m.CompliantImportDateIndicator).Returns(ZBool.False);
			mockInvoiceLine2.Setup(m => m.CompliantCompletionIndicator).Returns(ZBool.False);
			mockInvoiceLine2.Setup(m => m.TIIN).Returns("");
			mockInvoiceLine3.Setup(m => m.PageNumber).Returns(new ZInt(2));
			mockInvoiceLine3.Setup(m => m.LineNumber).Returns(new ZInt(1));
			mockInvoiceLine3.Setup(m => m.TariffNumber).Returns("");
			mockInvoiceLine3.Setup(m => m.Quantity).Returns(50.5m);
			mockInvoiceLine3.Setup(m => m.QuantityUnits).Returns("XYZ");
			mockInvoiceLine3.Setup(m => m.UnitPrice).Returns(1.23456m);
			mockInvoiceLine3.Setup(m => m.LinePrice).Returns(1000m);
			mockInvoiceLine3.Setup(m => m.LinePriceCurrency).Returns("USD");
			mockInvoiceLine3.Setup(m => m.CountryOfOrigin).Returns("UNY");
			mockInvoiceLine3.Setup(m => m.ItemDescription).Returns("COMMODITY 3 DESCRIPTION WHICH SHOULD BE SPLIT OVER 2 LINES");
			mockInvoiceLine3.Setup(m => m.RequirementID).Returns("");
			mockInvoiceLine3.Setup(m => m.RequirementVersion).Returns("");
			mockInvoiceLine3.Setup(m => m.AirsCode).Returns("");
			mockInvoiceLine3.Setup(m => m.DestinationProvince).Returns("");
			mockInvoiceLine3.Setup(m => m.EndUse).Returns("");
			mockInvoiceLine3.Setup(m => m.MiscID).Returns("");
			mockInvoiceLine3.Setup(m => m.RegistrationNumbers).Returns(Array.Empty<ZString>());
			mockInvoiceLine3.Setup(m => m.RegistrationTypes).Returns(Array.Empty<ZString>());
			mockInvoiceLine3.Setup(m => m.CFIAOrigin).Returns("");
			mockInvoiceLine3.Setup(m => m.ImportReasonCode).Returns("");
			mockInvoiceLine3.Setup(m => m.Make).Returns("");
			mockInvoiceLine3.Setup(m => m.Model).Returns("");
			mockInvoiceLine3.Setup(m => m.ModelNumber).Returns("");
			mockInvoiceLine3.Setup(m => m.BrandName).Returns("");
			mockInvoiceLine3.Setup(m => m.VehicleClass).Returns("");
			mockInvoiceLine3.Setup(m => m.TypeSize).Returns("");
			mockInvoiceLine3.Setup(m => m.VIN).Returns(Array.Empty<ZString>());
			mockInvoiceLine3.Setup(m => m.AssemblyMonth).Returns(Array.Empty<ZString>());
			mockInvoiceLine3.Setup(m => m.CompliantImportDateIndicator).Returns(ZBool.False);
			mockInvoiceLine3.Setup(m => m.CompliantCompletionIndicator).Returns(ZBool.False);
			mockInvoiceLine3.Setup(m => m.TIIN).Returns("");
			var invoiveLine1 = mockInvoiceLine1.Object;
			var invoiveLine2 = mockInvoiceLine2.Object;
			var invoiveLine3 = mockInvoiceLine3.Object;
			invoiceLines.Add(invoiveLine1);
			invoiceLines.Add(invoiveLine2);
			invoiceLines.Add(invoiveLine3);
			mockInvoice1.Setup(m => m.InvoiceLines).Returns(invoiceLines);
			var mockInvoice2 = new Mock<IEDIInvoiceOGD>();
			mockInvoice2.Setup(m => m.InvoiceNumber).Returns("INV123456789");
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
			mockInvoice2.Setup(m => m.HeaderOrigin).Returns("AU");
			mockInvoice2.Setup(m => m.CommonCountryOfOrigin).Returns("AU");
			mockInvoice2.Setup(m => m.CommonCountryOfExport).Returns("UNY");
			mockInvoice2.Setup(m => m.InvoiceDate).Returns(new ZDate(2009, 7, 1));
			mockInvoice2.Setup(m => m.InvoiceAmount).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.InvoiceCurrency).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.IncludedOFTAndONS).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.IncludedConstruction).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.IncludedPacking).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.ExcludedOFTAndONS).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.ExcludedCommission).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.ExcludedPacking).Returns(ZDecimal.Zero);
			mockInvoice2.Setup(m => m.OtherReference).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.DepartmentRuling).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.LastPortName).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.LastPortDate).Returns(ZDate.Empty);
			mockInvoice2.Setup(m => m.TranshipmentCountry).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.ConditionsOfSale).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.TermsOfPayment).Returns(ZString.Empty);
			mockInvoice2.Setup(m => m.ServicesInd).Returns(ZBool.True);
			mockInvoice2.Setup(m => m.RoyaltyInd).Returns(ZBool.False);
			var invoice2 = mockInvoice2.Object;
			invoices.Add(invoice2);
			var invoiceLines2 = new List<IEDIInvoiceLineOGD>();
			var mockInvoiceLine4 = new Mock<IEDIInvoiceLineOGD>();
			mockInvoiceLine4.Setup(m => m.PageNumber).Returns(new ZInt(1));
			mockInvoiceLine4.Setup(m => m.LineNumber).Returns(new ZInt(1));
			mockInvoiceLine4.Setup(m => m.TariffNumber).Returns("0303030000");
			mockInvoiceLine4.Setup(m => m.Quantity).Returns(100m);
			mockInvoiceLine4.Setup(m => m.QuantityUnits).Returns("PCE");
			mockInvoiceLine4.Setup(m => m.UnitPrice).Returns(0m);
			mockInvoiceLine4.Setup(m => m.LinePrice).Returns(0m);
			mockInvoiceLine4.Setup(m => m.LinePriceCurrency).Returns(ZString.Empty);
			mockInvoiceLine4.Setup(m => m.ItemDescription).Returns("COMMODITY 4 DESCRIPTION WITH A DESCRIPTION THAT IS MORE THEN 70 CHARACTERS IN LENGTH");
			mockInvoiceLine4.Setup(m => m.RequirementID).Returns("");
			mockInvoiceLine4.Setup(m => m.RequirementVersion).Returns("");
			mockInvoiceLine4.Setup(m => m.AirsCode).Returns("");
			mockInvoiceLine4.Setup(m => m.DestinationProvince).Returns("");
			mockInvoiceLine4.Setup(m => m.EndUse).Returns("");
			mockInvoiceLine4.Setup(m => m.MiscID).Returns("");
			mockInvoiceLine4.Setup(m => m.RegistrationNumbers).Returns(Array.Empty<ZString>());
			mockInvoiceLine4.Setup(m => m.RegistrationTypes).Returns(Array.Empty<ZString>());
			mockInvoiceLine4.Setup(m => m.CountryOfOrigin).Returns("");
			mockInvoiceLine4.Setup(m => m.CFIAOrigin).Returns("NZ");
			mockInvoiceLine4.Setup(m => m.ImportReasonCode).Returns("");
			mockInvoiceLine4.Setup(m => m.Make).Returns("");
			mockInvoiceLine4.Setup(m => m.Model).Returns("");
			mockInvoiceLine4.Setup(m => m.ModelNumber).Returns("");
			mockInvoiceLine4.Setup(m => m.BrandName).Returns("");
			mockInvoiceLine4.Setup(m => m.VehicleClass).Returns("");
			mockInvoiceLine4.Setup(m => m.TypeSize).Returns("");
			mockInvoiceLine4.Setup(m => m.VIN).Returns(Array.Empty<ZString>());
			mockInvoiceLine4.Setup(m => m.AssemblyMonth).Returns(Array.Empty<ZString>());
			mockInvoiceLine4.Setup(m => m.CompliantImportDateIndicator).Returns(ZBool.False);
			mockInvoiceLine4.Setup(m => m.CompliantCompletionIndicator).Returns(ZBool.False);
			mockInvoiceLine4.Setup(m => m.TIIN).Returns("");
			var invoiveLine4 = mockInvoiceLine4.Object;
			invoiceLines2.Add(invoiveLine4);
			mockInvoice2.Setup(m => m.InvoiceLines).Returns(invoiceLines2);
			mock.Setup(m => m.Invoices).Returns(invoices);
			return mock;
		}
	}
}
