using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CADCurrentDataDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestCADCurrentDataDocumentWrapper()
		{
			#region Create Test Data

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("CCRC", "Canada Change Reason Codes");
			helper.CreateCusCodeType("CAPC", "Canada Appeals Program Codes");
			for (var i = 1; i < 4; i++)
			{
				var code = i.ToString();
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", code, $"CCRC {code}", yesterday, tomorrow);
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", code, $"CAPC {code}", yesterday, tomorrow);
			}
			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Long description.");
			helper.CreateTariffUOM(tariff, "CU1", "NMB");
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			CreateCusCodeListWithAttribute("Cannabis", CasualImportConstants.CasualImpCommodityType.Tobacco);
			CreateCusCodeListWithAttribute("Tobacco", CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress = importer.MainAddress;
			importerAddress.CompanyName = "IMP COM1";
			importerAddress.Address1 = "IMP ADDRESS1";
			importerAddress.City = "IMP CITY";
			importerAddress.StateCode = "19";
			importerAddress.Postcode = "239074";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			importer.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "123BRM456");
			var importerDetails = @"IMP COM1
IMP ADDRESS1
IMP CITY 19 239074";

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company1.GC_Code = "TC1";
			company1.GC_OH_OrgProxy = importer.PK;
			var brokerBranch = Factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";
			brokerBranch.GB_GC = company1.PK;
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;
			broker.SignatureImage = new Bitmap(1, 2);

			var vendor = Factory.NewWithValidTestData<OrgHeader>();
			var vendorAddress = vendor.MainAddress;
			vendorAddress.CompanyName = "VEN COM1";
			vendorAddress.Address1 = "VEN ADDRESS1";
			vendorAddress.City = "VEN CITY";
			vendorAddress.StateCode = "13";
			vendorAddress.Postcode = "239001";
			var vendorDetails = "VEN COM1 VEN ADDRESS1 VEN CITY 13 239001";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerAddress = buyer.MainAddress;
			buyerAddress.CompanyName = "BUY COM1";
			buyerAddress.Address1 = "BUY ADDRESS1";
			buyerAddress.City = "BUY CITY";
			buyerAddress.StateCode = "14";
			buyerAddress.Postcode = "239002";
			var buyerDetails = "BUY COM1 BUY ADDRESS1 BUY CITY 14 239002";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 8, 12);

			var orgProxy = declaration.EffectiveBranch.OrgProxy;
			orgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "456BRM456");
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

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveS;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_CustomsOffice = "0123";
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, 8, 11);
			declaration.JE_TotalWeight = 1;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;
			declaration.JE_CarrierCode = "123";
			declaration.CA_UnladingOffice = "0345";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description, "Note to be printed on CAD");

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = vendor.PK;
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			invoiceHeader.JZ_InvoiceNumber = "INV121";
			invoiceHeader.JZ_InvoiceAmount = 999m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoiceHeader.CA_USPortOfExit = "51";
			invoiceHeader.CA_TimeLimit = 10;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoiceHeader.CA_RN_NKExport = "CA";
			invoiceHeader.CA_USStateOfExport = "NY";
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2023, 8, 13);
			invoiceHeader.JZ_InvoiceCurrExRate = 0.99;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "1234567890";

			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.CA_TRSNumber = "5221";
			invoiceLine1.JI_InvoiceQuantity = 199;
			invoiceLine1.JI_InvoiceUQ = "KGM";
			invoiceLine1.JI_CountryOfOrigin = "CN";
			invoiceLine1.JI_StateOrRegionOfOrigin = "AH";
			invoiceLine1.CA_TreatmentCode = "51";
			invoiceLine1.CA_99TariffCode = "9902";
			invoiceLine1.CA_CasualImportDestinationProvince = "JS";
			invoiceLine1.CA_CVforCurrConv = 9999;
			invoiceLine1.CA_AuthorityNumber = "3234";
			invoiceLine1.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;
			invoiceLine1.CA_SIMADumpingNum = "6324";
			invoiceLine1.CA_CasualImportCommodity = "Alcohol";
			invoiceLine1.JI_Description = "Long description From Line";
			invoiceLine1.JI_CustomsQuantity = 99.6;
			invoiceLine1.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			invoiceLine1.JI_PartNo = "1234";
			var duty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 100m;
			var exciseTax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exciseTax.C1_Amount = 101m;
			var exciseDuty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			exciseDuty.C1_Amount = 102m;
			exciseDuty.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var surtax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			surtax.C1_Amount = 103m;
			var anti_Dumping = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			anti_Dumping.C1_Amount = 104m;
			var countervailing = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			countervailing.C1_Amount = 105m;
			var gst = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Amount = 106m;
			var cpt = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CPT);
			cpt.C1_Amount = 107m;
			var cta = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta.C1_Amount = 108m;
			var saf = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SAF);
			saf.C1_Amount = 109m;
			invoiceLine1.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 111m);
			invoiceLine1.Charges.AddNew(CAChargeTypeList.Codes.DeductionCharge, 112m);
			invoiceLine1.JI_CustomsSecondQuantity = 99.6m;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.CA_CasualImportCommodity = "Cannabis";
			invoiceLine2.JI_Description = "Long description From Line";
			var duty2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 112m;
			var cta2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta2.C1_Amount = 109m;

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			entryLine3.CL_LineNumber = 3;
			invoiceLine3.CA_CasualImportCommodity = "Tobacco";
			var cta3 = invoiceLine3.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta3.C1_Amount = 110m;
			invoiceLine3.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 112m);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var parentDeclaration = Factory.New<JobDeclaration>();
			parentDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			parentDeclaration.TransactionNumber.AccountSecurityCode = "10207";
			parentDeclaration.TransactionNumber.SequentialNumber = "50000000";
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = parentDeclaration.PK;
			pivot.XX_Relation2ID = declaration.PK;
			pivot.XX_RelationType = "PRE";
			Factory.Save();

			invoiceLine1.CA_CustomsValue = 9987;
			var b3EntryLine1 = invoiceLine1.B3EntryLine;
			GenerateCorrectionCore(b3EntryLine1, 1, "1");
			GenerateCorrectionCore(b3EntryLine1, 1, "2");
			GenerateCorrectionCore(b3EntryLine1, 1, "3");

			#endregion

			var cadHeader = new CADCurrentDataDocumentWrapper(entryHeader) as ICADHeader;
			CombineAssertions(() =>
			{
				AssertEquals("TypeCode_Box1", declaration.JE_MessageSubType, cadHeader.TypeCode_Box1);
				AssertEquals("WsSType_Box2", ZString.Empty, cadHeader.WsSType_Box2);
				AssertEquals("AccountingDate_Box3", entryHeader.CH_EntryReleaseDate, cadHeader.AccountingDate_Box3);
				AssertEquals("AccountSecurityCode_Box4", declaration.TransactionNumber.AccountSecurityCode, cadHeader.AccountSecurityCode_Box4);
				AssertEquals("CADTransactionNo_Box4", declaration.TransactionNumber.UniqueIdentifier, cadHeader.CADTransactionNo_Box4);
				AssertEquals("OfficeNo_Box5", declaration.JE_CustomsOffice.TrimStart('0'), cadHeader.OfficeNo_Box5);
				AssertEquals("ModeOfTransport_Box6", TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode), cadHeader.ModeOfTransport_Box6);
				AssertEquals("ReleaseDate_Box7", declaration.JE_EntryAuthorisationDate, cadHeader.ReleaseDate_Box7);
				AssertEquals("GrossWeightKg_Box8", new ZWeight(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit).InKilogramsSafe, cadHeader.GrossWeightKg_Box8);
				AssertEquals("CarrierCodeAtImportation_Box9", declaration.JE_CarrierCode, cadHeader.CarrierCodeAtImportation_Box9);
				AssertEquals("Pre_CARM_Box10", declaration.ParentRelatedDeclaration != null ? "1" : "0", cadHeader.Pre_CARM_Box10);
				AssertEquals("Z1_RPP", ZString.Empty, cadHeader.Z1_RPP);
				AssertEquals("ImporterBN_Box11", IB3HeaderHelper.GetBusinessNumber(declaration.IsExistingEffectiveCasualImport, declaration.ImporterOfRecord, declaration.Importer), cadHeader.ImporterBN_Box11);
				AssertEquals("ImporterDetails_Box12", importerDetails, cadHeader.ImporterDetails_Box12);
				AssertEquals("BrokerOrAgentBN_Box13", declaration.BrokerBusinessNumber, cadHeader.BrokerOrAgentBN_Box13);
				AssertEquals("BrokerOrAgentDetails_Box14", brokerOrAgentDetails, cadHeader.BrokerOrAgentDetails_Box14);
				AssertEquals("CargoControlNo_Box15", "123", cadHeader.CargoControlNo_Box15);
				AssertEquals("RecordOfIntentNo_Box16", ZString.Empty, cadHeader.RecordOfIntentNo_Box16);
				AssertEquals("PreviousTransactionNo_Box17", parentDeclaration.TransactionNumber.FormattedTransactionNumber, cadHeader.PreviousTransactionNo_Box17);
				AssertEquals("AcceptedDate_Box18", declaration.B3AcceptedDate, cadHeader.AcceptedDate_Box18);
				AssertEquals("OriginalTransactionNo_Box19", parentDeclaration.TransactionNumber.FormattedTransactionNumber, cadHeader.OriginalTransactionNo_Box19);
				AssertEquals("PrevTransNoWarehouse_Box20", ZString.Empty, cadHeader.PrevTransNoWarehouse_Box20);
				AssertEquals("PortOfUnlading_Box21", declaration.CA_UnladingOffice, cadHeader.PortOfUnlading_Box21);
				AssertEquals("Notes_Box35", declaration.CADComments, cadHeader.Notes_Box35);
				AssertEquals("TotalValueForDuty_Box113", 9987m, cadHeader.TotalValueForDuty_Box113);
				AssertEquals("TotalPSTAndHST_Box114", 107m, cadHeader.TotalPSTAndHST_Box114);
				AssertEquals("TotalPSTCannabisAmount_Box115", 109m, cadHeader.TotalPSTCannabisAmount_Box115);
				AssertEquals("TotalProvAlcoholTaxAmount_Box116", 108m, cadHeader.TotalProvAlcoholTaxAmount_Box116);
				AssertEquals("TotalProvTobaccoAmount_Box117", 110m, cadHeader.TotalProvTobaccoAmount_Box117);
				AssertEquals("TotalDeclarationRelieved_Box118", 112m, cadHeader.TotalDeclarationRelieved_Box118);
				AssertEquals("TotalAmount_Box119", 942m, cadHeader.TotalAmount_Box119);
				AssertEquals("TotalCustomsDuties_Box120", 212m, cadHeader.TotalCustomsDuties_Box120);
				AssertEquals("TotalExciseDuties_Box121", 102m, cadHeader.TotalExciseDuties_Box121);
				AssertEquals("TotalExciseTaxes_Box122", 101m, cadHeader.TotalExciseTaxes_Box122);
				AssertEquals("TotalGST_Box123", 106m, cadHeader.TotalGST_Box123);
				AssertEquals("TotalAnti_Dumping_Box124", 104m, cadHeader.TotalAnti_Dumping_Box124);
				AssertEquals("TotalCountervailing_Box125", 105m, cadHeader.TotalCountervailing_Box125);
				AssertEquals("TotalSurtaxes_Box126", 103m, cadHeader.TotalSurtaxes_Box126);
				AssertEquals("TotalSafeguards_Box127", 109m, cadHeader.TotalSafeguards_Box127);
				AssertEquals("TotalInterest_Box128", 0m, cadHeader.TotalInterest_Box128);
				AssertEquals("TotalDutiesAndTaxesWithInterest_Box129", 1376m, cadHeader.TotalDutiesAndTaxesWithInterest_Box129);
				AssertEquals("TotalDutiesAndTaxes_Box130", 1376m, cadHeader.TotalDutiesAndTaxes_Box130);

				var cadSubHeaders = cadHeader.CADSubHeaders;
				AssertEquals("Count of CADSubHeaders", 1, cadSubHeaders.Count());
				var cadSubHeader = cadSubHeaders.First();
				AssertEquals("VendorDetails_Box36", vendorDetails, cadSubHeader.VendorDetails_Box36);
				AssertEquals("PurchaserDetails_Box37", buyerDetails, cadSubHeader.PurchaserDetails_Box37);
				AssertEquals("InvoiceNo_Box38", invoiceHeader.JZ_InvoiceNumber, cadSubHeader.InvoiceNo_Box38);
				AssertEquals("InvoiceValue_Box39", invoiceHeader.JZ_InvoiceAmount, cadSubHeader.InvoiceValue_Box39);
				AssertEquals("InvoiceCurrencyCode_Box40", invoiceHeader.JZ_RX_NKInvoice_Currency, cadSubHeader.InvoiceCurrencyCode_Box40);
				AssertEquals("PurchaseOrderNo_Box41", ZString.Empty, cadSubHeader.PurchaseOrderNo_Box41);
				AssertEquals("FreightCharges_Box42", 223m, cadSubHeader.FreightCharges_Box42);
				AssertEquals("USPortOfExit_Box43", invoiceHeader.CA_USPortOfExit, cadSubHeader.USPortOfExit_Box43);

				var cadLines = cadSubHeader.CADLines.ToArray();
				AssertEquals("Count of CADLines", 3, cadLines.Length);
				var cadLine1 = cadLines[0];
				AssertEquals("CADLineNo_Box56", entryLine1.CL_LineNumber, cadLine1.CADLineNo_Box56);
				AssertEquals("PreviousLineNoWarehouse_Box57", ZString.Empty, cadLine1.PreviousLineNoWarehouse_Box57);
				AssertEquals("ClassificationNo_Box58", entryLine1.CL_AdValoremTariff, cadLine1.ClassificationNo_Box58);
				AssertEquals("ClassificationDescription_Box59", "Long description From Line", cadLine1.ClassificationDescription_Box59);
				AssertEquals("NarrativeDescription_Box60", "TRS #:5221;1234", cadLine1.NarrativeDescription_Box60);
				AssertEquals("Quantity_Box61", invoiceLine1.JI_CustomsQuantity, cadLine1.Quantity_Box61);
				AssertEquals("UnitOfMeasure_Box62", invoiceLine1.JI_CustomsUnitQty, cadLine1.UnitOfMeasure_Box62);
				AssertEquals("TimeLimitType_Box63", invoiceHeader.CA_TimeLimitCode, cadLine1.TimeLimitType_Box63);
				AssertEquals("ExtensionDate_Box64", ZDateTime.Empty, cadLine1.ExtensionDate_Box64);
				AssertEquals("CountryOfOrigin_Box65", invoiceLine1.JI_CountryOfOrigin, cadLine1.CountryOfOrigin_Box65);
				AssertEquals("USState_Box66", invoiceLine1.JI_StateOrRegionOfOrigin, cadLine1.USState_Box66);
				AssertEquals("PlaceOfExport_Box67", invoiceHeader.CA_RN_NKExport, cadLine1.PlaceOfExport_Box67);
				AssertEquals("PlaceOfExportCodeState_Box68", invoiceHeader.CA_USStateOfExport, cadLine1.PlaceOfExportCodeState_Box68);
				AssertEquals("DirectShipmentDate_Box69", invoiceHeader.JZ_ValuationDateOverride, cadLine1.DirectShipmentDate_Box69);
				AssertEquals("TariffTreatment_Box70", invoiceLine1.CA_TreatmentCode, cadLine1.TariffTreatment_Box70);
				AssertEquals("TariffCode_Box71", invoiceLine1.CA_99TariffCode, cadLine1.TariffCode_Box71);
				AssertEquals("TimeLimitFrom_Box72", "20230811", cadLine1.TimeLimitFrom_Box72);
				AssertEquals("TimeLimitTo_Box73", "20230821", cadLine1.TimeLimitTo_Box73);
				AssertEquals("DestinationProvince_Box74", invoiceLine1.CA_CasualImportDestinationProvince, cadLine1.DestinationProvince_Box74);
				AssertEquals("ValueForCurrencyConversion_Box75", invoiceLine1.CA_CVforCurrConv, cadLine1.ValueForCurrencyConversion_Box75);
				AssertEquals("Currency_Box76", invoiceHeader.JZ_RX_NKInvoice_Currency, cadLine1.Currency_Box76);
				AssertEquals("ExchangeRate_Box77", invoiceHeader.JZ_InvoiceCurrExRate, cadLine1.ExchangeRate_Box77);
				AssertEquals("ValueForDuty_Box78", 9987m, cadLine1.ValueForDuty_Box78);
				AssertEquals("DRPLicense_Box79", ZString.Empty, cadLine1.DRPLicense_Box79);
				AssertEquals("SpecialAuthOIC_Box80", invoiceLine1.CA_AuthorityNumber, cadLine1.SpecialAuthOIC_Box80);
				AssertEquals("SpecialAuthorityPermit_Box81", ZString.Empty, cadLine1.SpecialAuthorityPermit_Box81);
				AssertEquals("CustomsDuty_Box82", 100m, cadLine1.CustomsDuty_Box82);
				AssertEquals("ExciseTax_Box83", 101m, cadLine1.ExciseTax_Box83);
				AssertEquals("ExciseDuty_Box84", 102m, cadLine1.ExciseDuty_Box84);
				AssertEquals("Surtax_Box85", 103m, cadLine1.Surtax_Box85);
				AssertEquals("Anti_Dumping_Box86", 104m, cadLine1.Anti_Dumping_Box86);
				AssertEquals("Safeguard_Box87", 109m, cadLine1.Safeguard_Box87);
				AssertEquals("Countervailing_Box88", 105m, cadLine1.Countervailing_Box88);
				AssertEquals("ValueForTax_Box89", 0m, cadLine1.ValueForTax_Box89);
				AssertEquals("GST_Box90", 106m, cadLine1.GST_Box90);
				AssertEquals("PSTAndHSTAmount_Box91", 107m, cadLine1.PSTAndHSTAmount_Box91);
				AssertEquals("ProvincialAlcoholTax_Box92", 108m, cadLine1.ProvincialAlcoholTax_Box92);
				AssertEquals("ProvincialTobaccoAmount_Box93", 0m, cadLine1.ProvincialTobaccoAmount_Box93);
				AssertEquals("AlcohosPercent_Box94", invoiceLine1.JI_CustomsSecondQuantity, cadLine1.AlcohosPercent_Box94);
				AssertEquals("ProvincialCannabisExciseDuty_Box95", 0m, cadLine1.ProvincialCannabisExciseDuty_Box95);
				AssertEquals("CBSACaseNo_Box96", ZString.Empty, cadLine1.CBSACaseNo_Box96);
				AssertEquals("RulingNo_Box97", ZString.Empty, cadLine1.RulingNo_Box97);
				AssertEquals("AppealsCaseNo_Box98", ZString.Empty, cadLine1.AppealsCaseNo_Box98);
				AssertEquals("ComplianceCaseNo_Box99", ZString.Empty, cadLine1.ComplianceCaseNo_Box99);
				AssertEquals("LineTotalDutiesAndTaxes_Box100", 1045m, cadLine1.LineTotalDutiesAndTaxes_Box100);
				AssertEquals("CommodityReason1_Box101", "1 CCRC 1", cadLine1.CommodityReason1_Box101);
				AssertEquals("Authority1_Box102", "1 CAPC 1", cadLine1.Authority1_Box102);
				AssertEquals("CommodityRemark1_Box103", "CSI_Description 1", cadLine1.CommodityRemark1_Box103);
				AssertEquals("CommodityReason1_Box105", "2 CCRC 2", cadLine1.CommodityReason2_Box105);
				AssertEquals("Authority2_Box106", "2 CAPC 2", cadLine1.Authority2_Box106);
				AssertEquals("CommodityRemark2_Box107", "CSI_Description 2", cadLine1.CommodityRemark2_Box107);
				AssertEquals("CommodityReason1_Box109", "3 CCRC 3", cadLine1.CommodityReason3_Box109);
				AssertEquals("Authority3_Box110", "3 CAPC 3", cadLine1.Authority3_Box110);
				AssertEquals("CommodityRemark3_Box111", "CSI_Description 3", cadLine1.CommodityRemark3_Box111);

				var cadLine2 = cadLines[1];
				AssertEquals("2 ClassificationDescription_Box59", "Long description From Line", cadLine2.ClassificationDescription_Box59);
				AssertEquals("2 TimeLimitFrom_Box72", "20230811", cadLine2.TimeLimitFrom_Box72);
				AssertEquals("2 TimeLimitTo_Box73", "20230821", cadLine2.TimeLimitTo_Box73);
				AssertEquals("2 ProvincialAlcoholTax_Box92", 0m, cadLine2.ProvincialAlcoholTax_Box92);
				AssertEquals("2 ProvincialTobaccoAmount_Box93", 0m, cadLine2.ProvincialTobaccoAmount_Box93);
				AssertEquals("2 ProvincialCannabisExciseDuty_Box95", 109m, cadLine2.ProvincialCannabisExciseDuty_Box95);
				AssertEquals("2 CommodityReason1_Box101", ZString.Empty, cadLine2.CommodityReason1_Box101);
				AssertEquals("2 Authority1_Box102", ZString.Empty, cadLine2.Authority1_Box102);
				AssertEquals("2 CommodityRemark1_Box103", ZString.Empty, cadLine2.CommodityRemark1_Box103);
				AssertEquals("2 CommodityReason1_Box105", ZString.Empty, cadLine2.CommodityReason2_Box105);
				AssertEquals("2 Authority2_Box106", ZString.Empty, cadLine2.Authority2_Box106);
				AssertEquals("2 CommodityRemark2_Box107", ZString.Empty, cadLine2.CommodityRemark2_Box107);
				AssertEquals("2 CommodityReason1_Box109", ZString.Empty, cadLine2.CommodityReason3_Box109);
				AssertEquals("2 Authority3_Box110", ZString.Empty, cadLine2.Authority3_Box110);
				AssertEquals("2 CommodityRemark3_Box111", ZString.Empty, cadLine2.CommodityRemark3_Box111);
				AssertEquals("NarrativeDescription_Box60", "Long description From Line", cadLine2.NarrativeDescription_Box60);

				var cadLine3 = cadLines[2];
				AssertEquals("3 ClassificationDescription_Box59", "", cadLine3.ClassificationDescription_Box59);
				AssertEquals("3 ProvincialAlcoholTax_Box92", 0m, cadLine3.ProvincialAlcoholTax_Box92);
				AssertEquals("3 ProvincialTobaccoAmount_Box93", 110m, cadLine3.ProvincialTobaccoAmount_Box93);
				AssertEquals("3 ProvincialCannabisExciseDuty_Box95", 0m, cadLine3.ProvincialCannabisExciseDuty_Box95);
			});
		}

		public void TestCADCurrentDataDocumentWrapperForLVS()
		{
			#region Create Test Data

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("CCRC", "Canada Change Reason Codes");
			helper.CreateCusCodeType("CAPC", "Canada Appeals Program Codes");
			for (var i = 1; i < 4; i++)
			{
				var code = i.ToString();
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", code, $"CCRC {code}", yesterday, tomorrow);
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", code, $"CAPC {code}", yesterday, tomorrow);
			}
			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Long description.");
			helper.CreateTariffUOM(tariff, "CU1", "NMB");
			CreateCusCodeListWithAttribute("Alcohol", CasualImportConstants.CasualImpCommodityType.Alcohol);
			CreateCusCodeListWithAttribute("Cannabis", CasualImportConstants.CasualImpCommodityType.Tobacco);
			CreateCusCodeListWithAttribute("Tobacco", CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress = importer.MainAddress;
			importerAddress.CompanyName = "IMP COM1";
			importerAddress.Address1 = "IMP ADDRESS1";
			importerAddress.City = "IMP CITY";
			importerAddress.StateCode = "19";
			importerAddress.Postcode = "239074";
			importerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			importer.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "123BRM456");
			var importerDetails = @"IMP COM1
IMP ADDRESS1
IMP CITY 19 239074";

			var brokerBranch = Factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;
			broker.SignatureImage = new Bitmap(1, 2);

			var vendor = Factory.NewWithValidTestData<OrgHeader>();
			var vendorAddress = vendor.MainAddress;
			vendorAddress.CompanyName = "VEN COM1";
			vendorAddress.Address1 = "VEN ADDRESS1";
			vendorAddress.City = "VEN CITY";
			vendorAddress.StateCode = "13";
			vendorAddress.Postcode = "239001";
			var vendorDetails = "VEN COM1 VEN ADDRESS1 VEN CITY 13 239001";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerAddress = buyer.MainAddress;
			buyerAddress.CompanyName = "BUY COM1";
			buyerAddress.Address1 = "BUY ADDRESS1";
			buyerAddress.City = "BUY CITY";
			buyerAddress.StateCode = "14";
			buyerAddress.Postcode = "239002";
			var buyerDetails = "BUY COM1 BUY ADDRESS1 BUY CITY 14 239002";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 8, 12);

			var orgProxy = declaration.EffectiveBranch.OrgProxy;
			orgProxy.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "456BRM456");
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

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveS;
			declaration.CA_K84AccountingDate = new ZDateTime(2023, 8, 10);
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_CustomsOffice = "0123";
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2023, 8, 11);
			declaration.JE_TotalWeight = 1;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;
			declaration.JE_CarrierCode = "123";
			declaration.CA_UnladingOffice = "0345";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD.Description, "Note to be printed on CAD");

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = vendor.PK;
			invoiceHeader.JZ_OH_Buyer = buyer.PK;
			invoiceHeader.JZ_InvoiceNumber = "INV121";
			invoiceHeader.JZ_InvoiceAmount = 999m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoiceHeader.CA_USPortOfExit = "51";
			invoiceHeader.CA_TimeLimit = 10;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoiceHeader.CA_RN_NKExport = "CA";
			invoiceHeader.CA_USStateOfExport = "NY";
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2023, 8, 13);
			invoiceHeader.JZ_InvoiceCurrExRate = 0.99;

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "1234567890";

			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.CA_B3SubHeaderNumber = 1;
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.CA_TRSNumber = "5221";
			invoiceLine1.JI_InvoiceQuantity = 199;
			invoiceLine1.JI_InvoiceUQ = "KGM";
			invoiceLine1.JI_CountryOfOrigin = "CN";
			invoiceLine1.JI_StateOrRegionOfOrigin = "AH";
			invoiceLine1.CA_TreatmentCode = "51";
			invoiceLine1.CA_99TariffCode = "9902";
			invoiceLine1.CA_CasualImportDestinationProvince = "JS";
			invoiceLine1.CA_CVforCurrConv = 9999;
			invoiceLine1.CA_CustomsValue = 9987;
			invoiceLine1.CA_AuthorityNumber = "3234";
			invoiceLine1.CA_RemissionType = RemissionTypeList.Codes.DutiesReliefProgramLicense;
			invoiceLine1.CA_SIMADumpingNum = "6324";
			invoiceLine1.CA_CasualImportCommodity = "Alcohol";
			invoiceLine1.JI_Description = "Long description From Line";
			invoiceLine1.JI_CustomsQuantity = 99;
			invoiceLine1.JI_CustomsUnitQty = "PKG";
			invoiceLine1.JI_CustomsSecondQuantity = 99.7m;
			invoiceLine1.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			var duty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty.C1_Amount = 100m;
			var exciseTax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exciseTax.C1_Amount = 101m;
			var exciseDuty = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			exciseDuty.C1_Amount = 102m;
			exciseDuty.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var surtax = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			surtax.C1_Amount = 103m;
			var anti_Dumping = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			anti_Dumping.C1_Amount = 104m;
			var countervailing = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			countervailing.C1_Amount = 105m;
			var gst = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_Amount = 106m;
			var cpt = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CPT);
			cpt.C1_Amount = 107m;
			var cta = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta.C1_Amount = 108m;
			invoiceLine1.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 111m);

			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine2.CA_B3SubHeaderNumber = 1;
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.CA_TRSNumber = "5221";
			invoiceLine2.JI_InvoiceQuantity = 199;
			invoiceLine2.JI_InvoiceUQ = "KGM";
			invoiceLine2.JI_CountryOfOrigin = "CN";
			invoiceLine2.JI_StateOrRegionOfOrigin = "AH";
			invoiceLine2.CA_TreatmentCode = "51";
			invoiceLine2.CA_99TariffCode = "9902";
			invoiceLine2.CA_CasualImportDestinationProvince = "JS";
			invoiceLine2.CA_CVforCurrConv = 9999;
			invoiceLine2.CA_CustomsValue = 9987;
			invoiceLine2.CA_AuthorityNumber = "3234";
			invoiceLine2.CA_SIMADumpingNum = "6324";
			invoiceLine2.CA_CasualImportCommodity = "Alcohol";
			var duty2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 1;
			var exciseTax2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			exciseTax2.C1_Amount = 2m;
			var exciseDuty2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			exciseDuty2.C1_Amount = 3m;
			exciseDuty2.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;
			var surtax2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			surtax2.C1_Amount = 4m;
			var anti_Dumping2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			anti_Dumping2.C1_Amount = 5m;
			var countervailing2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
			countervailing2.C1_Amount = 6m;
			var gst2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst2.C1_Amount = 7m;
			var cpt2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CPT);
			cpt2.C1_Amount = 8m;
			var cta2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta2.C1_Amount = 9m;
			invoiceLine2.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 10m);

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.CA_B3SubHeaderNumber = 2;
			invoiceLine3.CA_TreatmentCode = "01";
			invoiceLine3.CA_CasualImportCommodity = "Cannabis";
			var duty3 = invoiceLine3.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty3.C1_Amount = 112m;
			var cta3 = invoiceLine3.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta3.C1_Amount = 109m;

			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine4 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine3.PK;
			entryLine3.CL_LineNumber = 3;
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.CA_B3SubHeaderNumber = 3;
			invoiceLine4.CA_TreatmentCode = "02";
			invoiceLine4.CA_CasualImportCommodity = "Tobacco";
			var cta4 = invoiceLine4.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CTA);
			cta4.C1_Amount = 110m;
			invoiceLine4.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 112m);

			GenerateCorrectionCore(entryLine1, 1, "1");
			GenerateCorrectionCore(entryLine1, 1, "2");
			GenerateCorrectionCore(entryLine1, 1, "3");

			#endregion

			var cadHeader = new CADCurrentDataDocumentWrapper(entryHeader) as ICADHeader;
			CombineAssertions(() =>
			{
				AssertEquals("TypeCode_Box1", declaration.JE_MessageSubType, cadHeader.TypeCode_Box1);
				AssertEquals("WsSType_Box2", ZString.Empty, cadHeader.WsSType_Box2);
				AssertEquals("AccountingDate_Box3", entryHeader.CH_EntryReleaseDate, cadHeader.AccountingDate_Box3);
				AssertEquals("AccountSecurityCode_Box4", declaration.TransactionNumber.AccountSecurityCode, cadHeader.AccountSecurityCode_Box4);
				AssertEquals("CADTransactionNo_Box4", declaration.TransactionNumber.UniqueIdentifier, cadHeader.CADTransactionNo_Box4);
				AssertEquals("OfficeNo_Box5", declaration.JE_CustomsOffice.TrimStart('0'), cadHeader.OfficeNo_Box5);
				AssertEquals("ModeOfTransport_Box6", TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode), cadHeader.ModeOfTransport_Box6);
				AssertEquals("ReleaseDate_Box7", declaration.JE_EntryAuthorisationDate, cadHeader.ReleaseDate_Box7);
				AssertEquals("GrossWeightKg_Box8", new ZWeight(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit).InKilogramsSafe, cadHeader.GrossWeightKg_Box8);
				AssertEquals("CarrierCodeAtImportation_Box9", declaration.JE_CarrierCode, cadHeader.CarrierCodeAtImportation_Box9);
				AssertEquals("Pre_CARM_Box10", declaration.ParentRelatedDeclaration != null ? "1" : "0", cadHeader.Pre_CARM_Box10);
				AssertEquals("Z1_RPP", ZString.Empty, cadHeader.Z1_RPP);
				AssertEquals("ImporterBN_Box11", IB3HeaderHelper.GetLVSBusinessNumber(declaration.Importer), cadHeader.ImporterBN_Box11);
				AssertEquals("ImporterDetails_Box12", importerDetails, cadHeader.ImporterDetails_Box12);
				AssertEquals("BrokerOrAgentBN_Box13", declaration.BrokerBusinessNumber, cadHeader.BrokerOrAgentBN_Box13);
				AssertEquals("BrokerOrAgentDetails_Box14", brokerOrAgentDetails, cadHeader.BrokerOrAgentDetails_Box14);
				AssertEquals("CargoControlNo_Box15", "123", cadHeader.CargoControlNo_Box15);
				AssertEquals("RecordOfIntentNo_Box16", ZString.Empty, cadHeader.RecordOfIntentNo_Box16);
				AssertEquals("PreviousTransactionNo_Box17", ZString.Empty, cadHeader.PreviousTransactionNo_Box17);
				AssertEquals("AcceptedDate_Box18", declaration.B3AcceptedDate, cadHeader.AcceptedDate_Box18);
				AssertEquals("OriginalTransactionNo_Box19", ZString.Empty, cadHeader.OriginalTransactionNo_Box19);
				AssertEquals("PrevTransNoWarehouse_Box20", ZString.Empty, cadHeader.PrevTransNoWarehouse_Box20);
				AssertEquals("PortOfUnlading_Box21", declaration.CA_UnladingOffice, cadHeader.PortOfUnlading_Box21);
				AssertEquals("Notes_Box35", declaration.CADComments, cadHeader.Notes_Box35);
				AssertEquals("TotalValueForDuty_Box113", ZDecimal.Zero, cadHeader.TotalValueForDuty_Box113);
				AssertEquals("TotalPSTAndHST_Box114", 115m, cadHeader.TotalPSTAndHST_Box114);
				AssertEquals("TotalPSTCannabisAmount_Box115", 109m, cadHeader.TotalPSTCannabisAmount_Box115);
				AssertEquals("TotalProvAlcoholTaxAmount_Box116", 117m, cadHeader.TotalProvAlcoholTaxAmount_Box116);
				AssertEquals("TotalProvTobaccoAmount_Box117", 110m, cadHeader.TotalProvTobaccoAmount_Box117);
				AssertEquals("TotalDeclarationRelieved_Box118", 0m, cadHeader.TotalDeclarationRelieved_Box118);
				AssertEquals("TotalAmount_Box119", 861m, cadHeader.TotalAmount_Box119);
				AssertEquals("TotalCustomsDuties_Box120", 213m, cadHeader.TotalCustomsDuties_Box120);
				AssertEquals("TotalExciseDuties_Box121", 105m, cadHeader.TotalExciseDuties_Box121);
				AssertEquals("TotalExciseTaxes_Box122", 103m, cadHeader.TotalExciseTaxes_Box122);
				AssertEquals("TotalGST_Box123", 113m, cadHeader.TotalGST_Box123);
				AssertEquals("TotalAnti_Dumping_Box124", 109m, cadHeader.TotalAnti_Dumping_Box124);
				AssertEquals("TotalCountervailing_Box125", 111m, cadHeader.TotalCountervailing_Box125);
				AssertEquals("TotalSurtaxes_Box126", 107m, cadHeader.TotalSurtaxes_Box126);
				AssertEquals("TotalSafeguards_Box127", 0m, cadHeader.TotalSafeguards_Box127);
				AssertEquals("TotalInterest_Box128", 0m, cadHeader.TotalInterest_Box128);
				AssertEquals("TotalDutiesAndTaxesWithInterest_Box129", 1312m, cadHeader.TotalDutiesAndTaxesWithInterest_Box129);
				AssertEquals("TotalDutiesAndTaxes_Box130", 1312m, cadHeader.TotalDutiesAndTaxes_Box130);

				var cadSubHeaders = cadHeader.CADSubHeaders.ToArray();
				AssertEquals("Count of CADSubHeaders", 3, cadSubHeaders.Length);
				var cadSubHeader1 = cadSubHeaders[0];
				AssertEquals("1 VendorDetails_Box36", vendorDetails, cadSubHeader1.VendorDetails_Box36);
				AssertEquals("1 PurchaserDetails_Box37", buyerDetails, cadSubHeader1.PurchaserDetails_Box37);
				AssertEquals("1 InvoiceNo_Box38", ((IClassificationLine1)entryLine1).B3SubHeaderNumber.ToString(), cadSubHeader1.InvoiceNo_Box38);
				AssertEquals("1 InvoiceValue_Box39", ZDecimal.Zero, cadSubHeader1.InvoiceValue_Box39);
				AssertEquals("1 InvoiceCurrencyCode_Box40", invoiceHeader.JZ_RX_NKInvoice_Currency, cadSubHeader1.InvoiceCurrencyCode_Box40);
				AssertEquals("1 PurchaseOrderNo_Box41", ZString.Empty, cadSubHeader1.PurchaseOrderNo_Box41);
				AssertEquals("1 FreightCharges_Box42", 121m, cadSubHeader1.FreightCharges_Box42);
				AssertEquals("1 USPortOfExit_Box43", invoiceHeader.CA_USPortOfExit, cadSubHeader1.USPortOfExit_Box43);

				var cadLines = cadSubHeader1.CADLines.ToArray();
				AssertEquals("Count of CADLines", 1, cadLines.Length);
				var cadLine1 = cadLines[0];
				AssertEquals("1 CADLineNo_Box56", entryLine1.CL_LineNumber, cadLine1.CADLineNo_Box56);
				AssertEquals("1 PreviousLineNoWarehouse_Box57", ZString.Empty, cadLine1.PreviousLineNoWarehouse_Box57);
				AssertEquals("1 ClassificationNo_Box58", ((IClassificationLine1)entryLine1).ClassificationNumber, cadLine1.ClassificationNo_Box58);
				AssertEquals("1 ClassificationDescription_Box59", "Long description From Line", cadLine1.ClassificationDescription_Box59);
				AssertEquals("1 NarrativeDescription_Box60", "TRS #:5221;Long description From Line", cadLine1.NarrativeDescription_Box60);
				AssertEquals("1 Quantity_Box61", 99m, cadLine1.Quantity_Box61);
				AssertEquals("1 UnitOfMeasure_Box62", "PKG", cadLine1.UnitOfMeasure_Box62);
				AssertEquals("1 TimeLimitType_Box63", invoiceHeader.CA_TimeLimitCode, cadLine1.TimeLimitType_Box63);
				AssertEquals("1 ExtensionDate_Box64", ZDateTime.Empty, cadLine1.ExtensionDate_Box64);
				AssertEquals("1 CountryOfOrigin_Box65", invoiceLine1.JI_CountryOfOrigin, cadLine1.CountryOfOrigin_Box65);
				AssertEquals("1 USState_Box66", invoiceLine1.JI_StateOrRegionOfOrigin, cadLine1.USState_Box66);
				AssertEquals("1 PlaceOfExport_Box67", invoiceHeader.CA_RN_NKExport, cadLine1.PlaceOfExport_Box67);
				AssertEquals("1 PlaceOfExportCodeState_Box68", invoiceHeader.CA_USStateOfExport, cadLine1.PlaceOfExportCodeState_Box68);
				AssertEquals("1 DirectShipmentDate_Box69", invoiceHeader.JZ_ValuationDateOverride, cadLine1.DirectShipmentDate_Box69);
				AssertEquals("1 TariffTreatment_Box70", invoiceLine1.CA_TreatmentCode, cadLine1.TariffTreatment_Box70);
				AssertEquals("1 TariffCode_Box71", invoiceLine1.CA_99TariffCode, cadLine1.TariffCode_Box71);
				AssertEquals("1 TimeLimitFrom_Box72", "20230811", cadLine1.TimeLimitFrom_Box72);
				AssertEquals("1 TimeLimitTo_Box73", "20230821", cadLine1.TimeLimitTo_Box73);
				AssertEquals("1 DestinationProvince_Box74", invoiceLine1.CA_CasualImportDestinationProvince, cadLine1.DestinationProvince_Box74);
				AssertEquals("1 ValueForCurrencyConversion_Box75", invoiceLine1.CA_CVforCurrConv, cadLine1.ValueForCurrencyConversion_Box75);
				AssertEquals("1 Currency_Box76", invoiceHeader.JZ_RX_NKInvoice_Currency, cadLine1.Currency_Box76);
				AssertEquals("1 ExchangeRate_Box77", invoiceHeader.JZ_InvoiceCurrExRate, cadLine1.ExchangeRate_Box77);
				AssertEquals("1 ValueForDuty_Box78", ((IClassificationLine1)entryLine1).ValueForDuty, cadLine1.ValueForDuty_Box78);
				AssertEquals("1 DRPLicense_Box79", invoiceLine1.CA_AuthorityNumber, cadLine1.DRPLicense_Box79);
				AssertEquals("1 SpecialAuthOIC_Box80", ZString.Empty, cadLine1.SpecialAuthOIC_Box80);
				AssertEquals("1 SpecialAuthorityPermit_Box81", ZString.Empty, cadLine1.SpecialAuthorityPermit_Box81);
				AssertEquals("1 CustomsDuty_Box82", 101m, cadLine1.CustomsDuty_Box82);
				AssertEquals("1 ExciseTax_Box83", 103m, cadLine1.ExciseTax_Box83);
				AssertEquals("1 ExciseDuty_Box84", 105m, cadLine1.ExciseDuty_Box84);
				AssertEquals("1 Surtax_Box85", 107m, cadLine1.Surtax_Box85);
				AssertEquals("1 Anti_Dumping_Box86", 109m, cadLine1.Anti_Dumping_Box86);
				AssertEquals("1 Safeguard_Box87", 0m, cadLine1.Safeguard_Box87);
				AssertEquals("1 Countervailing_Box88", 111m, cadLine1.Countervailing_Box88);
				AssertEquals("1 ValueForTax_Box89", 0m, cadLine1.ValueForTax_Box89);
				AssertEquals("1 GST_Box90", 113m, cadLine1.GST_Box90);
				AssertEquals("1 PSTAndHSTAmount_Box91", 115m, cadLine1.PSTAndHSTAmount_Box91);
				AssertEquals("1 ProvincialAlcoholTax_Box92", 117m, cadLine1.ProvincialAlcoholTax_Box92);
				AssertEquals("1 ProvincialTobaccoAmount_Box93", 0m, cadLine1.ProvincialTobaccoAmount_Box93);
				AssertEquals("1 AlcohosPercent_Box94", 99.7m, cadLine1.AlcohosPercent_Box94);
				AssertEquals("1 ProvincialCannabisExciseDuty_Box95", 0m, cadLine1.ProvincialCannabisExciseDuty_Box95);
				AssertEquals("1 CBSACaseNo_Box96", ZString.Empty, cadLine1.CBSACaseNo_Box96);
				AssertEquals("1 RulingNo_Box97", ZString.Empty, cadLine1.RulingNo_Box97);
				AssertEquals("1 AppealsCaseNo_Box98", ZString.Empty, cadLine1.AppealsCaseNo_Box98);
				AssertEquals("1 ComplianceCaseNo_Box99", ZString.Empty, cadLine1.ComplianceCaseNo_Box99);
				AssertEquals("1 LineTotalDutiesAndTaxes_Box100", 981m, cadLine1.LineTotalDutiesAndTaxes_Box100);
				AssertEquals("1 CommodityReason1_Box101", "1 CCRC 1", cadLine1.CommodityReason1_Box101);
				AssertEquals("1 Authority1_Box102", "1 CAPC 1", cadLine1.Authority1_Box102);
				AssertEquals("1 CommodityRemark1_Box103", "CSI_Description 1", cadLine1.CommodityRemark1_Box103);
				AssertEquals("1 CommodityReason1_Box105", "2 CCRC 2", cadLine1.CommodityReason2_Box105);
				AssertEquals("1 Authority2_Box106", "2 CAPC 2", cadLine1.Authority2_Box106);
				AssertEquals("1 CommodityRemark2_Box107", "CSI_Description 2", cadLine1.CommodityRemark2_Box107);
				AssertEquals("1 CommodityReason1_Box109", "3 CCRC 3", cadLine1.CommodityReason3_Box109);
				AssertEquals("1 Authority3_Box110", "3 CAPC 3", cadLine1.Authority3_Box110);
				AssertEquals("1 CommodityRemark3_Box111", "CSI_Description 3", cadLine1.CommodityRemark3_Box111);

				var cadSubHeader2 = cadSubHeaders[1];
				AssertEquals("2 VendorDetails_Box36", vendorDetails, cadSubHeader2.VendorDetails_Box36);
				AssertEquals("2 PurchaserDetails_Box37", buyerDetails, cadSubHeader2.PurchaserDetails_Box37);
				AssertEquals("2 InvoiceNo_Box38", ((IClassificationLine1)entryLine2).B3SubHeaderNumber.ToString(), cadSubHeader2.InvoiceNo_Box38);
				AssertEquals("2 InvoiceValue_Box39", ZDecimal.Zero, cadSubHeader2.InvoiceValue_Box39);
				AssertEquals("2 InvoiceCurrencyCode_Box40", invoiceHeader.JZ_RX_NKInvoice_Currency, cadSubHeader2.InvoiceCurrencyCode_Box40);
				AssertEquals("2 PurchaseOrderNo_Box41", ZString.Empty, cadSubHeader2.PurchaseOrderNo_Box41);
				AssertEquals("2 FreightCharges_Box42", ZDecimal.Zero, cadSubHeader2.FreightCharges_Box42);
				AssertEquals("2 USPortOfExit_Box43", invoiceHeader.CA_USPortOfExit, cadSubHeader2.USPortOfExit_Box43);

				var cadLines2 = cadSubHeader2.CADLines.ToArray();
				AssertEquals("Count of CADLines", 1, cadLines2.Length);
				var cadLine2 = cadLines2[0];
				AssertEquals("2 TimeLimitFrom_Box72", "20230811", cadLine2.TimeLimitFrom_Box72);
				AssertEquals("2 TimeLimitTo_Box73", "20230821", cadLine2.TimeLimitTo_Box73);
				AssertEquals("2 ProvincialAlcoholTax_Box92", 0m, cadLine2.ProvincialAlcoholTax_Box92);
				AssertEquals("2 ProvincialTobaccoAmount_Box93", 0m, cadLine2.ProvincialTobaccoAmount_Box93);
				AssertEquals("2 ProvincialCannabisExciseDuty_Box95", 109m, cadLine2.ProvincialCannabisExciseDuty_Box95);
				AssertEquals("2 CommodityReason1_Box101", ZString.Empty, cadLine2.CommodityReason1_Box101);
				AssertEquals("2 Authority1_Box102", ZString.Empty, cadLine2.Authority1_Box102);
				AssertEquals("2 CommodityRemark1_Box103", ZString.Empty, cadLine2.CommodityRemark1_Box103);
				AssertEquals("2 CommodityReason1_Box105", ZString.Empty, cadLine2.CommodityReason2_Box105);
				AssertEquals("2 Authority2_Box106", ZString.Empty, cadLine2.Authority2_Box106);
				AssertEquals("2 CommodityRemark2_Box107", ZString.Empty, cadLine2.CommodityRemark2_Box107);
				AssertEquals("2 CommodityReason1_Box109", ZString.Empty, cadLine2.CommodityReason3_Box109);
				AssertEquals("2 Authority3_Box110", ZString.Empty, cadLine2.Authority3_Box110);
				AssertEquals("2 CommodityRemark3_Box111", ZString.Empty, cadLine2.CommodityRemark3_Box111);

				var cadSubHeader3 = cadSubHeaders[2];
				AssertEquals("3 VendorDetails_Box36", vendorDetails, cadSubHeader3.VendorDetails_Box36);
				AssertEquals("3 PurchaserDetails_Box37", buyerDetails, cadSubHeader3.PurchaserDetails_Box37);
				AssertEquals("3 InvoiceNo_Box38", ((IClassificationLine1)entryLine3).B3SubHeaderNumber.ToString(), cadSubHeader3.InvoiceNo_Box38);
				AssertEquals("3 InvoiceValue_Box39", ZDecimal.Zero, cadSubHeader3.InvoiceValue_Box39);
				AssertEquals("3 InvoiceCurrencyCode_Box40", invoiceHeader.JZ_RX_NKInvoice_Currency, cadSubHeader3.InvoiceCurrencyCode_Box40);
				AssertEquals("3 PurchaseOrderNo_Box41", ZString.Empty, cadSubHeader3.PurchaseOrderNo_Box41);
				AssertEquals("3 FreightCharges_Box42", 112m, cadSubHeader3.FreightCharges_Box42);
				AssertEquals("3 USPortOfExit_Box43", invoiceHeader.CA_USPortOfExit, cadSubHeader3.USPortOfExit_Box43);

				var cadLines3 = cadSubHeader3.CADLines.ToArray();
				AssertEquals("Count of CADLines", 1, cadLines3.Length);
				var cadLine3 = cadLines3[0];
				AssertEquals("3 ProvincialAlcoholTax_Box92", 0m, cadLine3.ProvincialAlcoholTax_Box92);
				AssertEquals("3 ProvincialTobaccoAmount_Box93", 110m, cadLine3.ProvincialTobaccoAmount_Box93);
				AssertEquals("3 ProvincialCannabisExciseDuty_Box95", 0m, cadLine3.ProvincialCannabisExciseDuty_Box95);
			});
		}

		public void TestCADLine_QueryTopThreeAmendmentInfo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("CCRC", "Canada Change Reason Codes");
			helper.CreateCusCodeType("CAPC", "Canada Appeals Program Codes");
			for (var i = 1; i < 5; i++)
			{
				var code1 = i.ToString();
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", code1, $"CCRC {code1}", yesterday, tomorrow);
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", code1, $"CAPC {code1}", yesterday, tomorrow);
			}
			Factory.Save();

			AssertArrayEqualsByElements("entry line not exist", new[]
			{
				(ZString.Empty, ZString.Empty, ZString.Empty),
				(ZString.Empty, ZString.Empty, ZString.Empty),
				(ZString.Empty, ZString.Empty, ZString.Empty)
			}, CADCurrentDataDocumentWrapper.QueryTopThreeAmendmentInfo(null));

			var entryLine1 = Factory.New<CusEntryLine>();
			AssertArrayEqualsByElements("actions is empty", new[]
			{
				(ZString.Empty, ZString.Empty, ZString.Empty),
				(ZString.Empty, ZString.Empty, ZString.Empty),
				(ZString.Empty, ZString.Empty, ZString.Empty)
			}, CADCurrentDataDocumentWrapper.QueryTopThreeAmendmentInfo(entryLine1));

			var entryLine2 = Factory.New<CusEntryLine>();
			GenerateCorrectionCore(entryLine2, 1, "1");
			GenerateCorrectionCore(entryLine2, 1, "2");
			GenerateCorrectionCore(entryLine2, 1, "3");
			GenerateCorrectionCore(entryLine2, 1, "4");
			GenerateCorrectionCore(entryLine2, 1, "2", "22");
			GenerateCorrectionCore(entryLine2, 1, "3", "32");
			GenerateCorrectionCore(entryLine2, 1, "3", "33");

			AssertArrayEqualsByElements("three different actions", new[]
			{
				(new ZString("1 CCRC 1"), new ZString("1 CAPC 1"), new ZString("CSI_Description 1")),
				(new ZString("2 CCRC 2"), new ZString("2 CAPC 2"), new ZString("CSI_Description 2")),
				(new ZString("3 CCRC 3"), new ZString("3 CAPC 3"), new ZString("CSI_Description 3"))
			}, CADCurrentDataDocumentWrapper.QueryTopThreeAmendmentInfo(entryLine2));

			var entryLine3 = Factory.New<CusEntryLine>();
			GenerateCorrectionCore(entryLine3, 1, "1");
			GenerateCorrectionCore(entryLine3, 1, "2");
			GenerateCorrectionCore(entryLine3, 1, "3");
			GenerateCorrectionCore(entryLine3, 1, "4");
			GenerateCorrectionCore(entryLine3, 2, "1", "11");
			GenerateCorrectionCore(entryLine3, 2, "2", "21");
			GenerateCorrectionCore(entryLine3, 2, "3", "31");
			GenerateCorrectionCore(entryLine3, 2, "4", "41");
			GenerateCorrectionCore(entryLine3, 2, "2", "22");
			GenerateCorrectionCore(entryLine3, 2, "3", "32");
			GenerateCorrectionCore(entryLine3, 2, "3", "33");

			AssertArrayEqualsByElements("batch of max line No.", new[]
			{
				(new ZString("1 CCRC 1"), new ZString("1 CAPC 1"), new ZString("CSI_Description 11")),
				(new ZString("2 CCRC 2"), new ZString("2 CAPC 2"), new ZString("CSI_Description 21")),
				(new ZString("3 CCRC 3"), new ZString("3 CAPC 3"), new ZString("CSI_Description 31"))
			}, CADCurrentDataDocumentWrapper.QueryTopThreeAmendmentInfo(entryLine3));
		}

		void GenerateCorrectionCore(CusEntryLine entryLine, int lineNo, string code, string description = "")
		{
			var cADCorrection = Factory.New<CADCorrectionMessageSendingAction>();
			cADCorrection.CSI_Type = "CCM";
			cADCorrection.CSI_ParentID = entryLine.PK;
			cADCorrection.CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			cADCorrection.CSI_LineNo = lineNo;
			cADCorrection.CSI_Code = code;
			cADCorrection.CSI_SubType = code;
			cADCorrection.CSI_Description = "CSI_Description " + (description.IsNullOrEmpty() ? code : description);
		}

		void CreateCusCodeListWithAttribute(ZString code, ZString attribute)
		{
			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				code, code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, attribute);
		}
	}
}
