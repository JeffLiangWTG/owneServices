using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CADAsLodgedDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestCADAsLodgedDocumentWrapper()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType("CCRC", "Canada Change Reason Codes");
			helper.CreateNewOrGetExistingCusCodeType("CAPC", "Canada Appeals Program Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", "014", "Description 014", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", "015", "Description 015", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", "1", "Description 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", "2", "Description 2", yesterday, tomorrow);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress = importer.MainAddress;
			importerAddress.CompanyName = "IMP COM1";
			importerAddress.Address1 = "IMP ADDRESS1";
			importerAddress.City = "IMP CITY";
			importerAddress.StateCode = "19";
			importerAddress.Postcode = "239074";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			importer.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "793914086RM0001");
			var importerDetails = @"IMP COM1
IMP ADDRESS1
IMP CITY 19 239074";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var orgProxy = declaration.EffectiveBranch.OrgProxy;
			orgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "102891009RM0004");
			var orgProxyAddress = orgProxy.MainAddress;
			orgProxyAddress.CompanyName = "ORG COM2";
			orgProxyAddress.Address1 = "ORG ADDRESS2";
			orgProxyAddress.City = "ORG CITY";
			orgProxyAddress.StateCode = "21";
			orgProxyAddress.Postcode = "239000";
			orgProxyAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var brokerOrAgentDetails = @"ORG COM2
ORG ADDRESS2 ALBION  QLD
ORG CITY 21 239000
Australia";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var message1 = Factory.New<CADMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message1.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.TestFiles.CADMessage1.xml");
			}
			entryHeader.Messages.Add(message1);
			Factory.Save();

			var cadHeader = new CADAsLodgedDocumentWrapper(message1) as ICADHeader;
			CombineAssertions(() =>
			{
				AssertEquals("TypeCode_Box1", "AB", cadHeader.TypeCode_Box1);
				AssertEquals("WsSType_Box2", ZString.Empty, cadHeader.WsSType_Box2);
				AssertEquals("AccountingDate_Box3", new ZDateTime(2022, 4, 13, 14, 54, 1), cadHeader.AccountingDate_Box3);
				AssertEquals("AccountSecurityCode_Box4", "13284", cadHeader.AccountSecurityCode_Box4);
				AssertEquals("CADTransactionNo_Box4", "009000123", cadHeader.CADTransactionNo_Box4);
				AssertEquals("OfficeNo_Box5", "0453", cadHeader.OfficeNo_Box5);
				AssertEquals("ModeOfTransport_Box6", "02", cadHeader.ModeOfTransport_Box6);
				AssertEquals("ReleaseDate_Box7", new ZDateTime(2022, 3, 15), cadHeader.ReleaseDate_Box7);
				AssertEquals("GrossWeightKg_Box8", new ZDecimal(100), cadHeader.GrossWeightKg_Box8);
				AssertEquals("CarrierCodeAtImportation_Box9", "8041", cadHeader.CarrierCodeAtImportation_Box9);
				AssertEquals("Pre_CARM_Box10", "1", cadHeader.Pre_CARM_Box10);
				AssertEquals("Z1_RPP", ZString.Empty, cadHeader.Z1_RPP);
				AssertEquals("ImporterBN_Box11", "793914086RM0001", cadHeader.ImporterBN_Box11);
				AssertEquals("ImporterDetails_Box12", importerDetails, cadHeader.ImporterDetails_Box12);
				AssertEquals("BrokerOrAgentBN_Box13", "102891009RM0004", cadHeader.BrokerOrAgentBN_Box13);
				AssertEquals("BrokerOrAgentDetails_Box14", brokerOrAgentDetails, cadHeader.BrokerOrAgentDetails_Box14);
				AssertEquals("CargoControlNo_Box15", "8041DCTCE1", cadHeader.CargoControlNo_Box15);
				AssertEquals("RecordOfIntentNo_Box16", ZString.Empty, cadHeader.RecordOfIntentNo_Box16);
				AssertEquals("PreviousTransactionNo_Box17", ZString.Empty, cadHeader.PreviousTransactionNo_Box17);
				AssertEquals("AcceptedDate_Box18", new ZDateTime(2022, 4, 13, 14, 54, 1), cadHeader.AcceptedDate_Box18);
				AssertEquals("OriginalTransactionNo_Box19", ZString.Empty, cadHeader.OriginalTransactionNo_Box19);
				AssertEquals("PrevTransNoWarehouse_Box20", ZString.Empty, cadHeader.PrevTransNoWarehouse_Box20);
				AssertEquals("PortOfUnlading_Box21", "9952", cadHeader.PortOfUnlading_Box21);
				AssertEquals("Notes_Box35", ZString.Empty, cadHeader.Notes_Box35);
				AssertEquals("TotalValueForDuty_Box113", 112481.78m, cadHeader.TotalValueForDuty_Box113);
				AssertEquals("TotalPSTAndHST_Box114", 112.01m, cadHeader.TotalPSTAndHST_Box114);
				AssertEquals("TotalPSTCannabisAmount_Box115", 55.03m, cadHeader.TotalPSTCannabisAmount_Box115);
				AssertEquals("TotalProvAlcoholTaxAmount_Box116", 37.11m, cadHeader.TotalProvAlcoholTaxAmount_Box116);
				AssertEquals("TotalProvTobaccoAmount_Box117", 38.12m, cadHeader.TotalProvTobaccoAmount_Box117);
				AssertEquals("TotalDeclarationRelieved_Box118", 101.12m, cadHeader.TotalDeclarationRelieved_Box118);
				AssertEquals("TotalAmount_Box119", 8754.90m, cadHeader.TotalAmount_Box119);
				AssertEquals("TotalCustomsDuties_Box120", 3568.77m, cadHeader.TotalCustomsDuties_Box120);
				AssertEquals("TotalExciseDuties_Box121", 39.13m, cadHeader.TotalExciseDuties_Box121);
				AssertEquals("TotalExciseTaxes_Box122", 40.14m, cadHeader.TotalExciseTaxes_Box122);
				AssertEquals("TotalGST_Box123", 4936.20m, cadHeader.TotalGST_Box123);
				AssertEquals("TotalAnti_Dumping_Box124", 41.15m, cadHeader.TotalAnti_Dumping_Box124);
				AssertEquals("TotalCountervailing_Box125", 42.16m, cadHeader.TotalCountervailing_Box125);
				AssertEquals("TotalSurtaxes_Box126", 43.17m, cadHeader.TotalSurtaxes_Box126);
				AssertEquals("TotalSafeguards_Box127", 44.18m, cadHeader.TotalSafeguards_Box127);
				AssertEquals("TotalInterest_Box128", 45.19m, cadHeader.TotalInterest_Box128);
				AssertEquals("TotalDutiesAndTaxesWithInterest_Box129", 8550.16m, cadHeader.TotalDutiesAndTaxesWithInterest_Box129);
				AssertEquals("TotalDutiesAndTaxes_Box130", 8504.97m, cadHeader.TotalDutiesAndTaxes_Box130);

				var cadSubHeaders = cadHeader.CADSubHeaders.ToArray();
				AssertEquals("Count of CADSubHeaders", 2, cadSubHeaders.Length);
				var cadSubHeader1 = cadSubHeaders[0];
				AssertEquals("VendorDetails_Box36", "HENKEL CORP. 14351 HIGHWAY 221 WOODRUFF PLANT ENOREE SC 29335 US", cadSubHeader1.VendorDetails_Box36);
				AssertEquals("PurchaserDetails_Box37", "BUYER ADDRESS1 HONGKONG HK 239000 CN", cadSubHeader1.PurchaserDetails_Box37);
				AssertEquals("InvoiceNo_Box38", "DCTCEINV1", cadSubHeader1.InvoiceNo_Box38);
				AssertEquals("InvoiceValue_Box39", 21110.03m, cadSubHeader1.InvoiceValue_Box39);
				AssertEquals("InvoiceCurrencyCode_Box40", "CAD", cadSubHeader1.InvoiceCurrencyCode_Box40);
				AssertEquals("PurchaseOrderNo_Box41", ZString.Empty, cadSubHeader1.PurchaseOrderNo_Box41);
				AssertEquals("FreightCharges_Box42", new ZDecimal(200), cadSubHeader1.FreightCharges_Box42);
				AssertEquals("USPortOfExit_Box43", "3801", cadSubHeader1.USPortOfExit_Box43);

				var cadLines = cadSubHeader1.CADLines.ToArray();
				AssertEquals("Count of CADLines", 2, cadLines.Length);
				var cadLine1 = cadLines[0];
				AssertEquals("CADLineNo_Box56", (ZShort)1, cadLine1.CADLineNo_Box56);
				AssertEquals("PreviousLineNoWarehouse_Box57", ZString.Empty, cadLine1.PreviousLineNoWarehouse_Box57);
				AssertEquals("ClassificationNo_Box58", "1701139000", cadLine1.ClassificationNo_Box58);
				AssertEquals("ClassificationDescription_Box59", "RAW SUGAR NOT CONTAINING ADDED FLAVOURING OR COLOURING MATTER", cadLine1.ClassificationDescription_Box59);
				AssertEquals("NarrativeDescription_Box60", "RAW SUGAR NOT CONTAINING ADDED FLAVOURING OR COLOURING MATTER", cadLine1.NarrativeDescription_Box60);
				AssertEquals("Quantity_Box61", 5m, cadLine1.Quantity_Box61);
				AssertEquals("UnitOfMeasure_Box62", "TNE", cadLine1.UnitOfMeasure_Box62);
				AssertEquals("TimeLimitType_Box63", ZString.Empty, cadLine1.TimeLimitType_Box63);
				AssertEquals("ExtensionDate_Box64", ZDateTime.Empty, cadLine1.ExtensionDate_Box64);
				AssertEquals("CountryOfOrigin_Box65", "ES", cadLine1.CountryOfOrigin_Box65);
				AssertEquals("USState_Box66", ZString.Empty, cadLine1.USState_Box66);
				AssertEquals("PlaceOfExport_Box67", "US", cadLine1.PlaceOfExport_Box67);
				AssertEquals("PlaceOfExportCodeState_Box68", "SC", cadLine1.PlaceOfExportCodeState_Box68);
				AssertEquals("DirectShipmentDate_Box69", new ZDateTime(2022, 3, 15), cadLine1.DirectShipmentDate_Box69);
				AssertEquals("TariffTreatment_Box70", "002", cadLine1.TariffTreatment_Box70);
				AssertEquals("TariffCode_Box71", "99038801", cadLine1.TariffCode_Box71);
				AssertEquals("TimeLimitFrom_Box72", "20241028", cadLine1.TimeLimitFrom_Box72);
				AssertEquals("TimeLimitTo_Box73", "20241104", cadLine1.TimeLimitTo_Box73);
				AssertEquals("DestinationProvince_Box74", "TW", cadLine1.DestinationProvince_Box74);
				AssertEquals("ValueForCurrencyConversion_Box75", 555.75m, cadLine1.ValueForCurrencyConversion_Box75);
				AssertEquals("Currency_Box76", "USD", cadLine1.Currency_Box76);
				AssertEquals("ExchangeRate_Box77", 0.99m, cadLine1.ExchangeRate_Box77);
				AssertEquals("ValueForDuty_Box78", 3150.75m, cadLine1.ValueForDuty_Box78);
				AssertEquals("DRPLicense_Box79", ZString.Empty, cadLine1.DRPLicense_Box79);
				AssertEquals("SpecialAuthOIC_Box80", "HXUTEST03", cadLine1.SpecialAuthOIC_Box80);
				AssertEquals("SpecialAuthorityPermit_Box81", ZString.Empty, cadLine1.SpecialAuthorityPermit_Box81);
				AssertEquals("CustomsDuty_Box82", 110.25m, cadLine1.CustomsDuty_Box82);
				AssertEquals("ExciseTax_Box83", 11.22m, cadLine1.ExciseTax_Box83);
				AssertEquals("ExciseDuty_Box84", 22.33m, cadLine1.ExciseDuty_Box84);
				AssertEquals("Surtax_Box85", 33.44m, cadLine1.Surtax_Box85);
				AssertEquals("Anti_Dumping_Box86", 44.55m, cadLine1.Anti_Dumping_Box86);
				AssertEquals("Safeguard_Box87", 55.66m, cadLine1.Safeguard_Box87);
				AssertEquals("Countervailing_Box88", 66.77m, cadLine1.Countervailing_Box88);
				AssertEquals("ValueForTax_Box89", 3261.00m, cadLine1.ValueForTax_Box89);
				AssertEquals("GST_Box90", 163.05m, cadLine1.GST_Box90);
				AssertEquals("PSTAndHSTAmount_Box91", 77.88m, cadLine1.PSTAndHSTAmount_Box91);
				AssertEquals("ProvincialAlcoholTax_Box92", 88.99m, cadLine1.ProvincialAlcoholTax_Box92);
				AssertEquals("ProvincialTobaccoAmount_Box93", 99.00m, cadLine1.ProvincialTobaccoAmount_Box93);
				AssertEquals("AlcohosPercent_Box94", cadLine1.AlcohosPercent_Box94, cadLine1.AlcohosPercent_Box94);
				AssertEquals("ProvincialCannabisExciseDuty_Box95", 111.12m, cadLine1.ProvincialCannabisExciseDuty_Box95);
				AssertEquals("CBSACaseNo_Box96", ZString.Empty, cadLine1.CBSACaseNo_Box96);
				AssertEquals("RulingNo_Box97", "HXU023", cadLine1.RulingNo_Box97);
				AssertEquals("AppealsCaseNo_Box98", ZString.Empty, cadLine1.AppealsCaseNo_Box98);
				AssertEquals("ComplianceCaseNo_Box99", ZString.Empty, cadLine1.ComplianceCaseNo_Box99);
				AssertEquals("LineTotalDutiesAndTaxes_Box100", 273.30m, cadLine1.LineTotalDutiesAndTaxes_Box100);
				AssertEquals("1-1 CommodityReason1_Box101", "014 DESCRIPTION 014", cadLine1.CommodityReason1_Box101);
				AssertEquals("1-1 Authority1_Box102", "1 DESCRIPTION 1", cadLine1.Authority1_Box102);
				AssertEquals("1-1 CommodityRemark1_Box103", "Change to TT not CIFTA, NAFTA, CCRFTA or CUSMA", cadLine1.CommodityRemark1_Box103);
				AssertEquals("1-1 CommodityReason1_Box105", "015 DESCRIPTION 015", cadLine1.CommodityReason2_Box105);
				AssertEquals("1-1 Authority2_Box106", "1 DESCRIPTION 1", cadLine1.Authority2_Box106);
				AssertEquals("1-1 CommodityRemark2_Box107", "Remark 2", cadLine1.CommodityRemark2_Box107);
				AssertEquals("1-1 CommodityReason3_Box109", "014 DESCRIPTION 014", cadLine1.CommodityReason3_Box109);
				AssertEquals("1-1 Authority3_Box110", "2 DESCRIPTION 2", cadLine1.Authority3_Box110);
				AssertEquals("1-1 CommodityRemark3_Box111", "Remark 3", cadLine1.CommodityRemark3_Box111);

				var cadLine12 = cadLines[1];
				AssertEquals("1-2 CommodityReason1_Box101", "014 DESCRIPTION 014", cadLine12.CommodityReason1_Box101);
				AssertEquals("1-2 Authority1_Box102", "1 DESCRIPTION 1", cadLine12.Authority1_Box102);
				AssertEquals("1-2 CommodityRemark1_Box103", "Change to TT not CIFTA, NAFTA, CCRFTA or CUSMA", cadLine12.CommodityRemark1_Box103);
				AssertEquals("1-2 CommodityReason1_Box105", "014 DESCRIPTION 014", cadLine12.CommodityReason2_Box105);
				AssertEquals("1-2 Authority2_Box106", "2 DESCRIPTION 2", cadLine12.Authority2_Box106);
				AssertEquals("1-2 CommodityRemark2_Box107", "Remark 3", cadLine12.CommodityRemark2_Box107);
				AssertEquals("1-2 CommodityReason3_Box109", ZString.Empty, cadLine12.CommodityReason3_Box109);
				AssertEquals("1-2 Authority3_Box110", ZString.Empty, cadLine12.Authority3_Box110);
				AssertEquals("1-2 CommodityRemark3_Box111", ZString.Empty, cadLine12.CommodityRemark3_Box111);

				var cadSubHeader2 = cadSubHeaders[1];
				var cadLines2 = cadSubHeader2.CADLines.ToArray();
				AssertEquals("2 Count of CADLines", 3, cadLines2.Length);
				var cadLine23 = cadLines2[0];
				AssertEquals("2-3 TimeLimitFrom_Box72", ZString.Empty, cadLine23.TimeLimitFrom_Box72);
				AssertEquals("2-3 TimeLimitTo_Box73", ZString.Empty, cadLine23.TimeLimitTo_Box73);
				AssertEquals("2-3 CommodityReason1_Box101", "014 DESCRIPTION 014", cadLine23.CommodityReason1_Box101);
				AssertEquals("2-3 Authority1_Box102", "1 DESCRIPTION 1", cadLine23.Authority1_Box102);
				AssertEquals("2-3 CommodityRemark1_Box103", "Change to TT not CIFTA, NAFTA, CCRFTA or CUSMA", cadLine23.CommodityRemark1_Box103);
				AssertEquals("2-3 CommodityReason1_Box105", ZString.Empty, cadLine23.CommodityReason2_Box105);
				AssertEquals("2-3 Authority2_Box106", ZString.Empty, cadLine23.Authority2_Box106);
				AssertEquals("2-3 CommodityRemark2_Box107", ZString.Empty, cadLine23.CommodityRemark2_Box107);
				AssertEquals("2-3 CommodityReason1_Box109", ZString.Empty, cadLine23.CommodityReason3_Box109);
				AssertEquals("2-3 Authority3_Box110", ZString.Empty, cadLine23.Authority3_Box110);
				AssertEquals("2-3 CommodityRemark3_Box111", ZString.Empty, cadLine23.CommodityRemark3_Box111);

				var cadLine24 = cadLines2[1];
				AssertEquals("2-4 CommodityReason1_Box101", ZString.Empty, cadLine24.CommodityReason1_Box101);
				AssertEquals("2-4 Authority1_Box102", ZString.Empty, cadLine24.Authority1_Box102);
				AssertEquals("2-4 CommodityRemark1_Box103", ZString.Empty, cadLine24.CommodityRemark1_Box103);
				AssertEquals("2-4 CommodityReason1_Box105", ZString.Empty, cadLine24.CommodityReason2_Box105);
				AssertEquals("2-4 Authority2_Box106", ZString.Empty, cadLine24.Authority2_Box106);
				AssertEquals("2-4 CommodityRemark2_Box107", ZString.Empty, cadLine24.CommodityRemark2_Box107);
				AssertEquals("2-4 CommodityReason1_Box109", ZString.Empty, cadLine24.CommodityReason3_Box109);
				AssertEquals("2-4 Authority3_Box110", ZString.Empty, cadLine24.Authority3_Box110);
				AssertEquals("2-4 CommodityRemark3_Box111", ZString.Empty, cadLine24.CommodityRemark3_Box111);
			});
		}

		public void TestImporterDetails_Box12WhenMultiOrgsHaveSameBusinessNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CCRC", "Canada Change Reason Codes");
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress1 = importer1.MainAddress;
			importerAddress1.CompanyName = "IMP COM1";
			importerAddress1.Address1 = "IMP ADDRESS1";
			importerAddress1.City = "IMP CITY";
			importerAddress1.StateCode = "19";
			importerAddress1.Postcode = "239074";
			importer1.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "793914086RM0001");

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress2 = importer2.MainAddress;
			importerAddress2.CompanyName = "IMP COM1";
			importerAddress2.Address1 = "IMP ADDRESS1";
			importer2.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "793914086RM0001");

			var importer3 = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress3 = importer3.MainAddress;
			importerAddress3.CompanyName = "IMP COM1";
			importerAddress3.Address1 = "IMP ADDRESS1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer3.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var message1 = Factory.New<CADMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.CAIMP;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message1.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.DocWrappers.TestFiles.CADMessage1.xml");
			}
			entryHeader.Messages.Add(message1);
			Factory.Save();

			var cadHeader = new CADAsLodgedDocumentWrapper(message1) as ICADHeader;
			AssertEquals("ImporterDetails_Box12", ZString.Empty, cadHeader.ImporterDetails_Box12);

			var importerDetails = @"IMP COM1
IMP ADDRESS1
IMP CITY 19 239074";
			declaration.JE_OH_Importer = importer1.PK;
			Factory.Save();
			var cadHeader2 = new CADAsLodgedDocumentWrapper(message1) as ICADHeader;
			AssertEquals("ImporterDetails_Box12", importerDetails, cadHeader2.ImporterDetails_Box12);
		}
	}
}
