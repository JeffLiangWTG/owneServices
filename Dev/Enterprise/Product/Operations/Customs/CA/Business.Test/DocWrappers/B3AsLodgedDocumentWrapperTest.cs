using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B3AsLodgedDocumentWrapperTest : TestCaseWithFactory
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestGetImporter()
		{
			#region InterchangeText

			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696
UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT
UNH+679+CUSDEC:S:99B:UN
BGM+:::AB+419+9
CST++I
LOC+41+497
LOC+11+423
LOC+18+12345
RFF+TN:400004228
RFF+AEA:0495
RFF+ARA:123241838RM0001
TDT+11++2++3713
DOC+785+3713882555
DTM+204:20110603:102
MOA+43:27587
UNS+D
DMS+1
MOA+64:1023
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110429:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02+66::D:14
MOA+6::USD
DMS+2
MOA+64:1450
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110501:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02
MOA+6::USD
CST+1+POS+1+8536509111+23+9902
MOA+40:1200000
MOA+43:1141320
MOA+125:1141320
RFF+ABG:123-456789
RFF+ABA:123456789:1
RFF+MF::1
GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS
RFF+LI:1:2
MOA+38:1200000
TAX+1+ADD++31
MOA+46:40200
TAX+1+EXC++5.0
MOA+161:1234
TAX+7+VAT++5.0
MOA+1:57066
GIR+1+1
MEA+AAR++NMB:1200000
MEA+AAA++KGM:9000
TAX+5+++8.0
MOA+155:1234
CST+2+POS+2+8207130010+23
MOA+40:1700500
MOA+43:1617346
MOA+125:1617346
RFF+LI:1:1
MOA+38:1700500
TAX+7+VAT++48
MOA+1:000
GIR+1+2
MEA+AAR++NMB:100000
TAX+5+++0.0678
MOA+155:34567
UNS+S
TAX+5+:::K90
MOA+155:1247200
TAX+1+:::K90
MOA+105:49200
TAX+3+:::K90
MOA+4:19500
TAX+7+:::K90
MOA+1:266366
TAX+4+:::K90
MOA+176:1582266
TAX+5+:::K92
MOA+155:247200
TAX+1+:::K92
MOA+105:9200
TAX+3+:::K92
MOA+4:9500
TAX+7+:::K92
MOA+1:66366
TAX+4+:::K92
MOA+176:582266
UNT+57+679
UNE+1+700
UNZ+1+7696
";

			#endregion

			var helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);
			var importer2 = helper.CreateOrganisation("IMP", "IMPORTER NAME2", "CATOR", "IMPORTER ADDRESS2", "IMPORTER CITY2", "123 45672");
			importer2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);
			var importer3 = helper.CreateOrganisation("IMP", "IMPORTER NAME3", "CATOR", "IMPORTER ADDRESS3", "IMPORTER CITY3", "123 45673");
			importer3.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "LINE1 DESCRIPTION";
			line1.JI_Tariff = "1111111111";
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Description = "LINE2 DESCRIPTION";
			line2.JI_Tariff = "2222222222";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			Factory.Save();

			var message = CreateMessageFromInterchangeString(Factory, interchangeText);
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;

			var wrapper = (IB3Header)new B3AsLodgedDocumentWrapper(message);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNull(wrapper.Importer);
		}

		public void TestB3AsLodgedDocumentWrapper()
		{
			#region InterchangeText

			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696
UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT
UNH+679+CUSDEC:S:99B:UN
BGM+:::AB+419+9
CST++I
LOC+41+497
LOC+11+423
LOC+18+12345
RFF+TN:400004228
RFF+AEA:0495
RFF+ARA:123241838RM0001
TDT+11++2++3713
DOC+785+3713882555
DTM+204:20110603:102
MOA+43:27587
UNS+D
DMS+1
MOA+64:1023
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110429:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02+66::D:14
MOA+6::USD
DMS+2
MOA+64:1450
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110501:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02
MOA+6::USD
CST+1+POS+1+8536509111+23+9902
MOA+40:1200000
MOA+43:1141320
MOA+125:1141320
RFF+ABG:123-456789
RFF+ABA:123456789:1
RFF+MF::1
GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS
RFF+LI:1:2
MOA+38:1200000
TAX+1+ADD++31
MOA+46:40200
TAX+1+EXC++5.0
MOA+161:1234
TAX+7+VAT++5.0
MOA+1:57066
GIR+1+1
MEA+AAR++NMB:1200000
MEA+AAA++KGM:9000
TAX+5+++8.0
MOA+155:1234
CST+2+POS+2+8207130010+23
MOA+40:1700500
MOA+43:1617346
MOA+125:1617346
RFF+LI:1:1
MOA+38:1700500
TAX+7+VAT++48
MOA+1:000
GIR+1+2
MEA+AAR++NMB:100000
TAX+5+++0.0678
MOA+155:34567
UNS+S
TAX+5+:::K90
MOA+155:1247200
TAX+1+:::K90
MOA+105:49200
TAX+3+:::K90
MOA+4:19500
TAX+7+:::K90
MOA+1:266366
TAX+4+:::K90
MOA+176:1582266
TAX+5+:::K92
MOA+155:247200
TAX+1+:::K92
MOA+105:9200
TAX+3+:::K92
MOA+4:9500
TAX+7+:::K92
MOA+1:66366
TAX+4+:::K92
MOA+176:582266
UNT+57+679
UNE+1+700
UNZ+1+7696
";

			#endregion

			var helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);
			var importer2 = helper.CreateOrganisation("IMP", "IMPORTER NAME2", "CATOR", "IMPORTER ADDRESS2", "IMPORTER CITY2", "123 45672");
			importer2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);
			var importer3 = helper.CreateOrganisation("IMP", "IMPORTER NAME3", "CATOR", "IMPORTER ADDRESS3", "IMPORTER CITY3", "123 45673");
			importer3.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123241838RM0001", canada);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "LINE1 DESCRIPTION";
			line1.JI_Tariff = "1111111111";
			line1.JI_CustomsQuantity = 12;
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_InvoiceQuantity = 23;
			line1.JI_InvoiceUQ = "M3";
			line1.JI_LinePrice = 45;
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Description = "LINE2 DESCRIPTION";
			line2.JI_Tariff = "2222222222";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			Factory.Save();

			var message = CreateMessageFromInterchangeString(Factory, interchangeText);
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;

			var wrapper = (IB3Header)new B3AsLodgedDocumentWrapper(message);

			AssertEquals("BatchNumber", "419", wrapper.BatchNumber);
			AssertEquals("B3TypeCode", "AB", wrapper.B3TypeCode);
			AssertEquals("PaymentCode", "I", wrapper.PaymentCode);
			AssertEquals("CBSAOffice", "497", wrapper.CBSAOffice);
			AssertEquals("PortOfUnlading", "423", wrapper.PortOfUnlading);
			AssertEquals("WarehouseNumber", "12345", wrapper.WarehouseNumber);
			AssertEquals("TransactionNumber", "400004228", wrapper.TransactionNumber);
			AssertEquals("AccountSecurityCode", "10207", wrapper.AccountSecurityCode);
			AssertEquals("BusinessNumber", "123241838RM0001", wrapper.BusinessNumber);
			AssertEquals("Importer", "IMPORTER NAME", wrapper.Importer.E2_CompanyName);
			AssertEquals("GSTNumber", "0495", wrapper.GSTNumber);
			AssertEquals("TransportMode", "2", wrapper.TransportMode);
			AssertEquals("CarrierCodeAtImportation", "3713", wrapper.CarrierCodeAtImportation);
			var release = wrapper.B3BInputReleases.First();
			AssertEquals("CargoControlNumber", "3713882555", release.CargoControlNumber);
			AssertEquals("DateOfRelease", new ZDateTime(2011, 06, 03), release.DateOfRelease);
			AssertEquals("TotalValueForDuty", 27587m, wrapper.TotalValueForDuty);
			B3ImportMessageWrapperTest.AssertTotalAmounts(wrapper.PositiveTotalAmounts, 195m, 2663.66m, 492m, 12472m, 15822.66m);
			B3ImportMessageWrapperTest.AssertTotalAmounts(wrapper.NegativeTotalAmounts, 95m, 663.66m, 92m, 2472m, 5822.66m);

			declaration.JE_OH_Importer = importer2.PK;
			wrapper = new B3AsLodgedDocumentWrapper(message);
			AssertEquals("Importer", "IMPORTER NAME2", wrapper.Importer.E2_CompanyName);

			declaration.ImporterOfRecordAddress.OrganisationPK = importer3.PK;
			wrapper = new B3AsLodgedDocumentWrapper(message);
			AssertEquals("Importer", "IMPORTER NAME3", wrapper.Importer.E2_CompanyName);

			//Sub Header
			AssertEquals("PositiveB3SubHeaders count", 2, wrapper.PositiveB3SubHeaders.Count());
			var subHeader = wrapper.PositiveB3SubHeaders.First();
			AssertEquals("B3SubHeaderNumber", 1, subHeader.B3SubHeaderNumber);
			AssertEquals("FreightCharges", 1023m, subHeader.FreightCharges);
			AssertEquals("Vendor Name", "MITSUBISHI MATERIALS USA CORP.", subHeader.Vendor.E2_CompanyName);
			AssertEquals("Vendor Address", ZString.Empty, subHeader.Vendor.E2_Address1);
			AssertEquals("Vendor Country/Region", Constants.CountryCodes.UnitedStates, subHeader.Vendor.CountryCode);
			AssertEquals("Vendor State Code", USStatesList.Codes.California, subHeader.Vendor.E2_State);
			AssertEquals("Vendor Zip Code", "92708", subHeader.Vendor.E2_Postcode);
			AssertEquals("DateOfDirectShipment", new ZDateTime(2011, 04, 29), subHeader.DateOfDirectShipment);
			AssertEquals("CountryOfOrigin", "JP", subHeader.CountryOfOrigin);
			AssertEquals("PlaceOfExport", "UCA", subHeader.PlaceOfExport);
			AssertEquals("USPortOfExit", "3801", subHeader.USPortOfExit);
			AssertEquals("TariffTreatmentCode", TariffTreatmentCodes.Codes.MostFavouredNation, subHeader.TariffTreatmentCode);
			AssertEquals("CurrencyCode", Constants.CurrencyCodes.UnitedStates, subHeader.CurrencyCode);
			AssertEquals("TimeLimitUnit", TimeLimitUnitCodes.Codes.Day, subHeader.TimeLimitUnit);
			AssertEquals("B3TimeLimits", 14, subHeader.B3TimeLimits);
			AssertEquals("InvoiceNumber", "INV001", subHeader.InvoiceNumber);
			AssertEquals("ExchangeRate", 1.39m, subHeader.ExchangeRate);
			AssertEquals("", subHeader.TradeZone);

			//Classification Line 1
			AssertEquals("PositiveClassificationLines count", 2, wrapper.PositiveClassificationLines.Count());
			var classLine = wrapper.PositiveClassificationLines.First();
			AssertEquals("B3LineNumber", (short)1, classLine.B3LineNumber);
			AssertEquals("RecordIdentifier", MessageConstants.B3RecordIdentifiers.Positive, classLine.RecordIdentifier);
			AssertEquals("B3SubHeaderNumber", 1, classLine.B3SubHeaderNumber);
			AssertEquals("CA_B3SubHeaderNumber", 1, line1.CA_B3SubHeaderNumber);
			AssertEquals("ClassificationNumber", "8536509111", classLine.ClassificationNumber);
			AssertEquals("ValueForDutyCode", "23", classLine.ValueForDutyCode);
			AssertEquals("TariffCode", "9902", classLine.TariffCode);
			AssertEquals("ValueForCurrency", 12000.00m, classLine.ValueForCurrency);
			AssertEquals("ValueForDuty", 11413.20m, classLine.ValueForDuty);
			AssertEquals("ValueForTax", 11413.20m, classLine.ValueForTax);
			AssertEquals("AuthorityNumber", "123-456789", classLine.AuthorityNumber);
			AssertEquals("TRSNumber", "123456789", classLine.TRSNumber);
			AssertEquals("PartNumberDescriptions", "NUMBER DESCRIPTION UP TO 39 CHARACTERS1NUMBER DESCRIPTION UP TO 39 CHARS", classLine.PartNumberDescriptions[0]);
			AssertEquals("InvoiceCrossReferences count", false, classLine.InvoiceCrossReferences.Any());
			AssertEquals("SIMACode", SIMACodes.Codes.C31, classLine.SIMACode);
			AssertEquals("SIMAStatementCode", "S", classLine.SIMAStatementCode);
			AssertEquals("SIMAAssessment", 402m, classLine.SIMAAssessment);
			AssertEquals("ExciseExemptionCode", ZString.Empty, classLine.ExciseExemptionCode); //Rate or Code
			AssertEquals("ExciseTaxAmount", 12.34m, classLine.ExciseTaxAmount);
			AssertEquals("ExciseTaxRate", 5m, classLine.ExciseTaxRate);
			AssertEquals("ExciseTaxRateType", RateTypes.Codes.AdValorem, classLine.ExciseTaxRateType);
			AssertEquals("GSTExemptionCode", ZString.Empty, classLine.GSTExemptionCode); //Rate or Code
			AssertEquals("GSTAmount", 570.66m, classLine.GSTAmount);
			AssertEquals("GSTRateType", RateTypes.Codes.AdValorem, classLine.GSTRateType);
			AssertEquals("RateOfGST", 5m, classLine.RateOfGST);
			AssertEquals("CustomsQuantity", 12m, classLine.CustomsQuantity);
			AssertEquals("CustomsUnitQty", "KG", classLine.CustomsUnitQty);
			AssertEquals("InvoiceQuantity", 23m, classLine.InvoiceQuantity);
			AssertEquals("InvoiceUQ", "M3", classLine.InvoiceUQ);
			AssertEquals("TotalLinePrice", 45m, classLine.TotalLinePrice.Amount);
			AssertEquals("CustomsValue", 62.55m, classLine.CustomsValue.Amount);
			AssertEquals("FOB.Amount", 45m, classLine.FOB.Amount);
			AssertEquals("FOB.Currency", "USD", classLine.FOB.Currency.Code);

			AssertEquals("CountOfConsolidatedLines", 0, classLine.CountOfConsolidatedLines);

			//Classification Line 2
			AssertEquals("ClassificationLines 2 count", 1, classLine.ClassificationLines.Count());
			var classLine2 = classLine.ClassificationLines.First();
			CusEntryLineBusinessObjectTest.AssertClassificationLine2(classLine2, 1, "NMB", 1200m, 9000m, 8m, 12.34m, RateTypes.Codes.AdValorem, 0m, ZString.Empty);

			AssertEquals("CA_B3SubHeaderNumber", 1, line2.CA_B3SubHeaderNumber);
			classLine = wrapper.PositiveClassificationLines.ElementAt(1);
			AssertEquals("ExciseExemptionCode", ZString.Empty, classLine.ExciseExemptionCode); //Rate or Code
			AssertEquals("ExciseTaxAmount", 0m, classLine.ExciseTaxAmount);
			AssertEquals("ExciseTaxRate", 0m, classLine.ExciseTaxRate);
			AssertEquals("ExciseTaxRateType", ZString.Empty, classLine.ExciseTaxRateType);
			AssertEquals("GSTExemptionCode", GSTStatusCodes.Codes.C48, classLine.GSTExemptionCode); //Rate or Code
			AssertEquals("GSTAmount", 0m, classLine.GSTAmount);
			AssertEquals("GSTRateType", RateTypes.Codes.Exempt, classLine.GSTRateType);
			AssertEquals("RateOfGST", 0m, classLine.RateOfGST);
			classLine2 = classLine.ClassificationLines.First();
			CusEntryLineBusinessObjectTest.AssertClassificationLine2(classLine2, 2, "NMB", 100m, 0m, 0.0678m, 345.67m, RateTypes.Codes.Specific, 0m, ZString.Empty);

			message = CreateMessageFromInterchangeString(Factory, interchangeText.Replace("8536509111", "0000999902").Replace("TAX+1+EXC++5.0", "TAX+1+EXC++86"));
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			wrapper = new B3AsLodgedDocumentWrapper(message);
			classLine = wrapper.PositiveClassificationLines.First();
			AssertEquals("ClassificationNumber", "0000999902", classLine.ClassificationNumber);
			classLine = wrapper.PositiveClassificationLines.ElementAt(1);
			AssertEquals("PartNumberDescriptions", "LINE2 DESCRIPTION (*)", classLine.PartNumberDescriptions[0]);

			declaration.Supplier.OH_FullName = "MITSUBISHI MATERIALS USA CORP.";
			declaration.Supplier.Addresses.MainAddress.OA_State = "CA";
			declaration.Supplier.Addresses.MainAddress.OA_PostCode = "92708";
			wrapper = new B3AsLodgedDocumentWrapper(message);
			subHeader = wrapper.PositiveB3SubHeaders.First();
			AssertEquals("Vendor Name", "MITSUBISHI MATERIALS USA CORP.", subHeader.Vendor.E2_CompanyName);
			AssertEquals("Vendor Address", "SUPPLIER ADDRESS", subHeader.Vendor.E2_Address1);
			AssertEquals("Vendor State Code", USStatesList.Codes.California, subHeader.Vendor.E2_State);
			AssertEquals("Vendor Zip Code", "92708", subHeader.Vendor.E2_Postcode);

			classLine = wrapper.PositiveClassificationLines.First();
			AssertEquals("ExciseExemptionCode", ExciseTaxExemptionCodes.Codes.C86, classLine.ExciseExemptionCode); //Rate or Code
			AssertEquals("ExciseTaxAmount", 12.34m, classLine.ExciseTaxAmount);
			AssertEquals("ExciseTaxRate", 0m, classLine.ExciseTaxRate);
			AssertEquals("ExciseTaxRateType", RateTypes.Codes.Exempt, classLine.ExciseTaxRateType);

			message.EM_MessageText = message.EM_MessageText.Replace("MITSUBISHI MATERIALS USA CORP.++++UCA+92708", "NON-US SUPPLIER");
			wrapper = new B3AsLodgedDocumentWrapper(message);
			subHeader = wrapper.PositiveB3SubHeaders.First();
			AssertEquals("Vendor Name", "NON-US SUPPLIER", subHeader.Vendor.E2_CompanyName);
			AssertEquals("Vendor Address", ZString.Empty, subHeader.Vendor.E2_Address1);
			AssertEquals("Vendor State Code", ZString.Empty, subHeader.Vendor.E2_State);
			AssertEquals("Vendor Zip Code", ZString.Empty, subHeader.Vendor.E2_Postcode);

			declaration.Supplier.OH_FullName = "NON-US SUPPLIER";
			declaration.Supplier.Addresses.MainAddress.OA_State = "NSW";
			declaration.Supplier.Addresses.MainAddress.OA_PostCode = "2017";
			wrapper = new B3AsLodgedDocumentWrapper(message);
			subHeader = wrapper.PositiveB3SubHeaders.First();
			AssertEquals("Vendor Name", "NON-US SUPPLIER", subHeader.Vendor.E2_CompanyName);
			AssertEquals("Vendor Address", "SUPPLIER ADDRESS", subHeader.Vendor.E2_Address1);
			AssertEquals("Vendor State Code", "NSW", subHeader.Vendor.E2_State);
			AssertEquals("Vendor Post Code", "2017", subHeader.Vendor.E2_Postcode);
		}

		public void TestIClassificationLine1Properties_DutiesAndTaxes()
		{
			#region Create Test Data
			const string interchangeText = @"UNB+UNOA:3+YUSAIRXPN+INETCECPT+110607:0915+7696
UNG+CUSDEC+U10207V1+KI+110607:0915+700+UN+S:99B+10207YUSENT
UNH+679+CUSDEC:S:99B:UN
BGM+:::AB+419+9
CST++I
LOC+41+497
LOC+11+423
LOC+18+12345
RFF+TN:400004228
RFF+AEA:0495
RFF+ARA:123241838RM0001
TDT+11++2++3713
DOC+785+3713882555
DTM+204:20110603:102
MOA+43:27587
UNS+D
DMS+1
MOA+64:1023
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110429:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02+66::D:14
MOA+6::USD
DMS+2
MOA+64:1450
NAD+SE++MITSUBISHI MATERIALS USA CORP.++++UCA+92708
DOC+935
DTM+129:20110501:102
LOC+27+JP+UCA+3801
PAT+1+CONSIGN:::02
MOA+6::USD
CST+1+POS+1+8536509111+23+9902
MOA+40:1200000
MOA+43:1141320
MOA+125:1141320
RFF+ABG:123-456789
RFF+ABA:123456789:1
RFF+MF::1
GIN+PN+NUMBER DESCRIPTION UP TO 39 CHARACT:ERS1+NUMBER DESCRIPTION UP TO 39 CHARS
RFF+LI:1:2
MOA+38:1200000
TAX+1+ADD++31
MOA+46:40200
TAX+1+EXC++5.0
MOA+161:1234
TAX+7+VAT++5.0
MOA+1:57066
GIR+1+1
MEA+AAR++NMB:1200000
MEA+AAA++KGM:9000
TAX+5+++8.0
MOA+155:1234
UNS+S
TAX+5+:::K90
MOA+155:1247200
TAX+1+:::K90
MOA+105:49200
TAX+3+:::K90
MOA+4:19500
TAX+7+:::K90
MOA+1:266366
TAX+4+:::K90
MOA+176:1582266
TAX+5+:::K92
MOA+155:247200
TAX+1+:::K92
MOA+105:9200
TAX+3+:::K92
MOA+4:9500
TAX+7+:::K92
MOA+1:66366
TAX+4+:::K92
MOA+176:582266
UNT+57+679
UNE+1+700
UNZ+1+7696
";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();

			var cpt1 = line1.DutiesAndTaxes.AddNew();
			cpt1.C1_Override = true;
			cpt1.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt1.C1_Amount = 33m;
			var cta1 = line1.DutiesAndTaxes.AddNew();
			cta1.C1_Override = true;
			cta1.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta1.C1_Amount = 34m;
			var dty1 = line1.DutiesAndTaxes.AddNew();
			dty1.C1_Override = true;
			dty1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty1.C1_Amount = 35m;
			dty1.C1_Code = "AB";
			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 36m;
			exs1.C1_Code = "BC";
			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "CD";
			sur1.C1_ExemptCode = SIMACodes.Codes.C51;
			var add1 = line1.DutiesAndTaxes.AddNew();
			add1.C1_Override = true;
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add1.C1_Amount = 39m;
			add1.Quantity = 40m;
			add1.C1_UnitOfMeasure = "M4";
			add1.C1_Code = "DE";
			var cvd1 = line1.DutiesAndTaxes.AddNew();
			cvd1.C1_Override = true;
			cvd1.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd1.C1_Amount = 43;
			cvd1.Quantity = 44;
			cvd1.C1_UnitOfMeasure = "M5";
			cvd1.C1_Code = "EF";
			var saf1 = line1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 45m;
			saf1.C1_Code = "FG";
			saf1.C1_ExemptCode = SIMACodes.Codes.C10;
			var ded1 = line1.Charges.AddNew();
			ded1.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			ded1.J7_Amount = 46m;
			ded1.J7_RX_NKCurrency = "USD";
			var exd1 = line1.DutiesAndTaxes.AddNew();
			exd1.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd1.C1_Amount = 47m;
			exd1.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var cpt2 = line2.DutiesAndTaxes.AddNew();
			cpt2.C1_Override = true;
			cpt2.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt2.C1_Amount = 33m;
			var cta2 = line2.DutiesAndTaxes.AddNew();
			cta2.C1_Override = true;
			cta2.C1_TaxType = DutyAndTaxTypes.Codes.CTA;
			cta2.C1_Amount = 34m;
			var dty2 = line2.DutiesAndTaxes.AddNew();
			dty2.C1_Override = true;
			dty2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dty2.C1_Amount = 35m;
			dty2.C1_Code = "AB";
			var exs2 = line2.DutiesAndTaxes.AddNew();
			exs2.C1_Override = true;
			exs2.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs2.C1_Amount = 36m;
			exs2.C1_Code = "BC";
			var sur2 = line2.DutiesAndTaxes.AddNew();
			sur2.C1_Override = true;
			sur2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur2.C1_Amount = 37m;
			sur2.Quantity = 38m;
			sur2.C1_UnitOfMeasure = "M3";
			sur2.C1_Code = "CD";
			sur2.C1_ExemptCode = SIMACodes.Codes.C51;
			var add2 = line2.DutiesAndTaxes.AddNew();
			add2.C1_Override = true;
			add2.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add2.C1_Amount = 39m;
			add2.Quantity = 40m;
			add2.C1_UnitOfMeasure = "M4";
			add2.C1_Code = "DE";
			var cvd2 = line2.DutiesAndTaxes.AddNew();
			cvd2.C1_Override = true;
			cvd2.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd2.C1_Amount = 43;
			cvd2.Quantity = 44;
			cvd2.C1_UnitOfMeasure = "M5";
			cvd2.C1_Code = "EF";
			var saf2 = line2.DutiesAndTaxes.AddNew();
			saf2.C1_Override = true;
			saf2.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf2.C1_Amount = 45m;
			saf2.C1_Code = "FG";
			saf2.C1_ExemptCode = SIMACodes.Codes.C10;
			var ded2 = line2.Charges.AddNew();
			ded2.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			ded2.J7_Amount = 46m;
			ded2.J7_RX_NKCurrency = "USD";
			var exd2 = line2.DutiesAndTaxes.AddNew();
			exd2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exd2.C1_Amount = 47m;
			exd2.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;

			var message = CreateMessageFromInterchangeString(Factory, interchangeText);
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			var wrapper = (IB3Header)new B3AsLodgedDocumentWrapper(message);
			#endregion

			AssertEquals("PositiveClassificationLines count", 1, wrapper.PositiveClassificationLines.Count());
			var classificationLine1 = wrapper.PositiveClassificationLines.First();
			AssertEquals("SalesTaxAmount", 66m, classificationLine1.SalesTaxAmount);
			AssertEquals("CTAAmount", 68m, classificationLine1.CTAAmount);
			AssertEquals("DeductionChargeAmountAndCurrency.Amount", 92m, classificationLine1.DeductionChargeAmountAndCurrency.Amount);
			AssertEquals("DeductionChargeAmountAndCurrency.Currency.RX_Code", "USD", classificationLine1.DeductionChargeAmountAndCurrency.Currency.RX_Code);
			AssertEquals("CustomsDutyCode", "AB", classificationLine1.CustomsDutyCode);
			AssertEquals("ExciseCode", "BC", classificationLine1.ExciseCode);
			AssertEquals("SurtaxQuantity", 76m, classificationLine1.SurtaxQuantity);
			AssertEquals("SurtaxUnitOfMeasure", "M3", classificationLine1.SurtaxUnitOfMeasure);
			AssertEquals("SurtaxCode", "CD", classificationLine1.SurtaxCode);
			AssertEquals("SurtaxStatementCode", "S", classificationLine1.SurtaxStatementCode);
			AssertEquals("HasSurtax", true, classificationLine1.HasSurtax);
			AssertEquals("ADDAmount", 78m, classificationLine1.ADDAmount);
			AssertEquals("ADDQuantity", 80m, classificationLine1.ADDQuantity);
			AssertEquals("ADDUnitOfMeasure", "M4", classificationLine1.ADDUnitOfMeasure);
			AssertEquals("ADDCode", "DE", classificationLine1.ADDCode);
			AssertEquals("ADDIsOverride", true, classificationLine1.ADDIsOverride);
			AssertEquals("HasADD", true, classificationLine1.HasADD);
			AssertEquals("CVDAmount", 86m, classificationLine1.CVDAmount);
			AssertEquals("CVDQuantity", 88m, classificationLine1.CVDQuantity);
			AssertEquals("CVDUnitOfMeasure", "M5", classificationLine1.CVDUnitOfMeasure);
			AssertEquals("CVDCode", "EF", classificationLine1.CVDCode);
			AssertEquals("CVDIsOverride", true, classificationLine1.CVDIsOverride);
			AssertEquals("HasCVD", true, classificationLine1.HasCVD);
			AssertEquals("SafeguardCode", "FG", classificationLine1.SafeguardCode);
			AssertEquals("HasSafeguard", true, classificationLine1.HasSafeguard);
			AssertEquals("SafeguardStatementCode", "N", classificationLine1.SafeguardStatementCode);
			AssertEquals("ExciseDutyAmount", 94m, classificationLine1.ExciseDutyAmount);
		}

		internal static B3Message CreateMessageFromInterchangeString(BusinessObjectFactory factory, string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(factory, interchangeString.Replace("\r\n", "'"), EDIMessage.ApplicationCodes.CAIMP, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			var message = (B3Message)interchange.ContainedMessages[0];
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			return message;
		}
	}
}
