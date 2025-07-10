using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class CADMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2025, 01, 03)]
		public void TestTemporaryImport()
		{
			importDec.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			invoice1.CA_TimeLimit = 4;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoiceLine1.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;

			importDec.DoMerge();
			var wrapper = new CADMessageWrapper(importDec.B3EntryHeader);
			var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Original);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Create_TemporaryImport.txt");
			var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message;
			var messageText = message.EM_MessageText.Replace("'", "'\r\n");
			AssertMultilineASCIIEquals("Empty CAD Message", expectedMessageText, messageText);
		}

		[TestDate(2021, 12, 17)]
		public void TestCreateOriginalForLVS()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress = consignor.Addresses.MainAddress;
			consignorMainAddress.Address1 = "Address 1";
			consignorMainAddress.Address2 = "Address 2";
			consignorMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			consignorMainAddress.StateCode = "ABC";
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(Factory, 5m);

			var helper = new DeclarationTestHelper(Factory, true);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var lvsJob = Factory.New<JobDeclaration>();
				lvsJob.JE_MessageType = Business.JobMessageTypeList.Codes.LowValueShipments;

				var invoiceHeader = lvsJob.Invoices.AddNew();
				invoiceHeader.JZ_OH_Supplier = consignor.PK;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.JZ_InvoiceCurrExRate = 1.1m;
				var invoiceLine = lvsJob.FilteredInvoiceLines.AddNew();
				invoiceLine.CA_IsCasualImport = true;
				invoiceLine.CA_CustomsValueOvr = true;
				invoiceLine.CA_CustomsValue = 100m;
				invoiceLine.JI_CustomsQuantity = 20;
				invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
				lvsJob.JE_CustomsOffice = "0301";
				invoiceLine.CA_CasualImportDestinationProvince = "QC";
				invoiceLine.CA_CasualImportCommodity = "SparklingWine";
				invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
				invoiceLine.JI_Description = "test Invoice Line1";
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = UnitOfWeightList.Codes.Kilogram;
				invoiceLine.JI_Tariff = "0789456125";
				invoiceLine.JI_RN_NKCountryOfExport = "US";
				invoiceLine.JI_LinePrice = 100m;
				invoiceLine.CA_99TariffCode = "7001";
				invoiceLine.CA_AuthorityNumber = "OICAUTH";
				invoiceLine.CA_ValueForDutyCode = "13";
				invoiceLine.CA_RemissionType = RemissionTypeList.Codes.ComplianceCaseNumber;

				lvsJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				lvsJob.DoMerge();

				var excise = invoiceLine.DutiesAndTaxes.AddNew();
				excise.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
				excise.C1_Code = "NO";
				excise.C1_Amount = 20.01m;

				var add = invoiceLine.DutiesAndTaxes.AddNew();
				add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
				add.C1_Code = "AD";
				add.C1_Amount = 11.11m;
				add.Quantity = 10m;
				add.C1_UnitOfMeasure = Core.Constants.Weight.Kilograms;

				var cvd = invoiceLine.DutiesAndTaxes.AddNew();
				cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
				cvd.C1_Code = "CD";
				cvd.C1_Amount = 14.21m;
				cvd.Quantity = 20m;

				var sur = invoiceLine.DutiesAndTaxes.AddNew();
				sur.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
				sur.C1_Code = "SR";
				sur.C1_Amount = 14.31m;
				sur.Quantity = 30m;
				sur.C1_UnitOfMeasure = Core.Constants.Weight.Tonnes;

				var gst = invoiceLine.DutiesAndTaxes.AddNew();
				gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
				gst.C1_ExemptCode = "GE";
				gst.C1_Amount = 16.21m;
				gst.Quantity = 21m;

				var saf = invoiceLine.DutiesAndTaxes.AddNew();
				saf.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
				saf.C1_Code = "SF";
				saf.C1_ExemptCode = SIMACodes.Codes.C50;
				saf.C1_Amount = 22.11m;

				var wrapper = new CADMessageWrapper(lvsJob.B3EntryHeader);
				var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Original);
				var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Create_LVS.txt");
				var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message;
				var messageText = message.EM_MessageText.Replace("'", "'\r\n");

				AssertMultilineASCIIEquals(expectedMessageText, messageText);

				lvsJob.B3EntryHeader.Messages.Add(message);
				var iData = new CADMessageWrapper(lvsJob.B3EntryHeader).CADDocumentMetaData;
				AssertEquals("00000000000001002", iData.CommunicationMetaData.ApplicationReferenceID);
			}
		}

		[TestDate(2021, 12, 17)]
		public void TestCreateOriginal()
		{
			var wrapper = new CADMessageWrapper(importDec.B3EntryHeader);
			var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Original);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Create.txt");
			var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message;
			var messageText = message.EM_MessageText.Replace("'", "'\r\n");
			AssertMultilineASCIIEquals(expectedMessageText, messageText);

			importDec.B3EntryHeader.Messages.Add(message);
			var iData = new CADMessageWrapper(importDec.B3EntryHeader).CADDocumentMetaData;
			AssertEquals("1234500006789700001002", iData.CommunicationMetaData.ApplicationReferenceID);
		}

		[TestDate(2024, 04, 16)]
		public void TestCreateOriginalForWarehouse()
		{
			importDec.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			invoice1.CA_TimeLimit = 3;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			importDec.DoMerge();
			var wrapper = new CADMessageWrapper(importDec.B3EntryHeader);
			var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Original);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Create_Warehouse.txt");
			var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message;
			var messageText = message.EM_MessageText.Replace("'", "'\r\n");
			AssertMultilineASCIIEquals(expectedMessageText, messageText);

			importDec.B3EntryHeader.Messages.Add(message);
			var iData = new CADMessageWrapper(importDec.B3EntryHeader).CADDocumentMetaData;
			AssertEquals("1234500006789700001001", iData.CommunicationMetaData.ApplicationReferenceID);
		}

		[TestDate(2021, 12, 17)]
		public void TestChange_ModifyExistingLines()
		{
			invoiceLine1.B3EntryLine.Header.CH_VersionID = 1;
			invoiceLine1.B3EntryLine.CL_GoodsShipmentSequence = 1;
			invoiceLine1.B3EntryLine.CL_CommoditySequence = 1;
			invoiceLine2.B3EntryLine.CL_GoodsShipmentSequence = 1;
			invoiceLine2.B3EntryLine.CL_CommoditySequence = 2;
			invoiceLine3.B3EntryLine.CL_GoodsShipmentSequence = 2;
			invoiceLine3.B3EntryLine.CL_CommoditySequence = 3;

			invoiceLine1.JI_Tariff = "0789456126";
			importDec.DoMerge();

			var actionWrapper = new CADCorrectionMessageSendingActionWrapper(importDec.B3EntryHeader);
			var actions = new CADCorrectionMessageSendingActionCollection(actionWrapper);
			var action = actions.AddNew();
			action.InvoiceSequence = 1;
			action.InvoiceLineSequence = 1;
			action.CSI_Code = "100";
			action.CSI_SubType = "1";
			action.CSI_Description = "Test change CAD message modify existing tariff number.";

			var wrapper = new CADMessageWrapper(importDec.B3EntryHeader, actions);
			var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Change);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Change.txt");
			var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
			AssertMultilineASCIIEquals(expectedMessageText, message);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(UniversalReferenceConstants.RefCusCodeListType.Codes.SLFCALCEXS, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Change_v2.txt");
				messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Change);
				message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
				AssertMultilineASCIIEquals(expectedMessageText, message);

				var exs = invoiceLine2.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
				exs.C1_Code = "C01";
				expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Change_v3.txt");
				messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Change);
				message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
				AssertMultilineASCIIEquals(expectedMessageText, message);
			}
		}

		[TestDate(2021, 12, 17)]
		public void TestChange_AddNewLines()
		{
			invoiceLine1.B3EntryLine.Header.CH_VersionID = 1;
			invoiceLine1.B3EntryLine.CL_GoodsShipmentSequence = 1;
			invoiceLine1.B3EntryLine.CL_CommoditySequence = 1;
			invoiceLine2.B3EntryLine.CL_GoodsShipmentSequence = 1;
			invoiceLine2.B3EntryLine.CL_CommoditySequence = 2;
			invoiceLine3.B3EntryLine.CL_GoodsShipmentSequence = 2;
			invoiceLine3.B3EntryLine.CL_CommoditySequence = 3;

			var invoiceLine4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine4.JI_Description = "Line Sequence should be 4";
			invoiceLine4.JI_Tariff = "0789456127";

			var invoice3 = importDec.Invoices.AddNew();
			var invoiceLine5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine5.JI_Description = "Line Sequence should be 5";
			invoiceLine5.JI_Tariff = "0789456128";
			importDec.DoMerge();

			var actionWrapper = new CADCorrectionMessageSendingActionWrapper(importDec.B3EntryHeader);
			var actions = new CADCorrectionMessageSendingActionCollection(actionWrapper);
			var action = actions.AddNew();
			action.InvoiceSequence = 1;
			action.InvoiceLineSequence = 1;
			action.CSI_Code = "100";
			action.CSI_SubType = "1";
			action.CSI_Description = "Test change CAD message adding new lines.";

			var wrapper = new CADMessageWrapper(importDec.B3EntryHeader, actions);
			var messageBuilder = new CADMessageBuilder(wrapper, IIDMessageSubTypeList.Codes.Change);
			var expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageBuilders.Import.CAD.TestFiles.CADTestMessage_Change_AddNew.txt");
			var message = ((IMessageBuilder)messageBuilder).PopulateMessages().GetBuilderResults().First().Message.EM_MessageText.Replace("'", "'\r\n");
			AssertMultilineASCIIEquals(expectedMessageText, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever();
			SetUpUniversalTariff();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				SetupDataWrapper();
			}
		}

		EmbeddedResourceRetriever resourceRetriever;
		JobDeclaration importDec;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		JobComInvoiceLine invoiceLine3;

		void SetupDataWrapper()
		{
			var broker = GlbBranch.GetCurrentBranch(Factory).OrgProxy;
			broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0002");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "842957342RM0001");
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyerMainAddress1 = buyer1.Addresses.MainAddress;
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress1 = consignor1.Addresses.MainAddress;
			consignorMainAddress1.Address1 = "Test too long address will be truncated, address 1";
			consignorMainAddress1.Address2 = "Test too long address will be truncated, address 2";
			importDec = Factory.NewWithValidTestData<JobDeclaration>();
			importDec.JE_DeclarationReference = "B00000001";
			importDec.TransactionNumber.AccountSecurityCode = "12345";
			importDec.TransactionNumber.SequentialNumber = "00006789";
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.JE_OH_Importer = importer.PK;
			importDec.JE_MessageSubType = B3EntryTypeList.Codes.AutomotiveP;
			importDec.GrossWeight = new ZWeight(200, "KG");
			importDec.JE_TransportMode = Core.Constants.TransportModes.Road;
			importDec.JE_CarrierCode = "0451";
			importDec.JE_EntryAuthorisationDate = ZDateTime.Now;
			importDec.CA_UnladingOffice = "1234";
			importDec.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			invoice1 = importDec.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(-7);
			invoice1.JZ_OH_Supplier = consignor1.PK;
			invoice1.CA_USPortOfExit = "2813";
			invoice1.CA_RN_NKExport = "US";
			invoice1.CA_USStateOfExport = "NY";
			invoice1.JZ_OH_Buyer = buyer1.PK;
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.CentralAfricanRepublic;

			invoiceLine1 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_Description = "test Invoice Line1";
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_CustomsUnitQty = UnitOfWeightList.Codes.Kilogram;
			invoiceLine1.JI_Tariff = "0789456125";
			invoiceLine1.JI_RN_NKCountryOfExport = "US";
			invoiceLine1.JI_LinePrice = 99m;
			invoiceLine1.JI_Model = "TESP";
			invoiceLine1.CA_TreatmentCode = "10";
			invoiceLine1.CA_AuthorityNumber = "OICAUTH";
			invoiceLine1.CA_RemissionType = "DRL";
			invoiceLine1.CA_99TariffCode = "7001";
			invoiceLine1.CA_ValueForDutyCode = "14";

			var gst = invoiceLine1.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_ExemptCode = "GE";
			gst.C1_Amount = 16.21m;
			gst.Quantity = 21m;

			invoiceLine2 = invoice1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_Description = "test Invoice Line2";
			invoiceLine2.JI_CustomsQuantity = 200.0004m;
			invoiceLine2.JI_CustomsUnitQty = UnitOfWeightList.Codes.Gram;
			invoiceLine2.JI_Tariff = "0789456123";
			invoiceLine2.JI_RN_NKCountryOfExport = "CA";
			invoiceLine2.JI_LinePrice = 9m;
			invoiceLine2.JI_Model = "TESP3";

			invoice2 = importDec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Afghanistan;
			invoiceLine3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine3.JI_Description = "test Invoice Line3 with too long description will be truncated, test Invoice Line3 with too long description will be truncated, test Invoice Line3 with too long description will be truncated";
			invoiceLine3.JI_CustomsQuantity = 200.0005m;
			invoiceLine3.JI_CustomsUnitQty = UnitOfWeightList.Codes.Gram;
			invoiceLine3.JI_CustomsThirdQuantity = 10m;
			invoiceLine3.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.AlcoholByVolume;
			invoiceLine3.JI_Tariff = "0789456124";
			invoiceLine3.JI_RN_NKCountryOfExport = "CA";
			invoiceLine3.JI_LinePrice = 9m;
			invoiceLine3.JI_Model = "TESP2";

			importDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			importDec.DoMerge();

			invoiceLine2.CA_IsCasualImport = true;
			invoiceLine2.CA_CasualImportDestinationProvince = "BC";
			var excise = invoiceLine2.DutiesAndTaxes.AddNew();
			excise.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			excise.C1_Code = "E01";
			excise.C1_Amount = 20.01m;

			var exciseDuty = invoiceLine2.DutiesAndTaxes.AddNew();
			exciseDuty.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			exciseDuty.C1_Amount = 25.38m;
			exciseDuty.C1_DutyType = DutyAndTaxManager.CombinedDuty.Excise;

			var add = invoiceLine2.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C40;
			add.C1_Code = "AD";
			add.C1_Amount = 11.11m;
			add.Quantity = 10m;
			add.C1_UnitOfMeasure = Core.Constants.Weight.Kilograms;

			var cvd = invoiceLine2.DutiesAndTaxes.AddNew();
			cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd.C1_Code = "CD";
			cvd.C1_Amount = 14.21m;
			cvd.Quantity = 20m;

			var sur = invoiceLine2.DutiesAndTaxes.AddNew();
			sur.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur.C1_Code = "SR";
			sur.C1_Amount = 14.31m;
			sur.Quantity = 30m;
			sur.C1_UnitOfMeasure = Core.Constants.Weight.Tonnes;

			var cpt = invoiceLine2.DutiesAndTaxes.AddNew();
			cpt.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			cpt.C1_Amount = 31.15m;

			var saf = invoiceLine2.DutiesAndTaxes.AddNew();
			saf.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf.C1_ExemptCode = SIMACodes.Codes.C40;
			saf.C1_Code = "HSP";
			cpt.C1_Amount = 11m;

			invoiceLine2.CA_CustomsValue = 1989m;
			invoiceLine2.CA_SIMADumpingNum = "AD1408";
		}

		void SetUpUniversalTariff()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			var taiwanTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(taiwanTradeGroup, Core.Constants.CountryCodes.Taiwan, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789", description: "TEST(TS1)");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingRateTW = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingApplicabilityTW = universalHelper.CreateCusApplicability(antiDumpingRateTW, taiwanTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456123", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(countervailingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var countervailingTariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123", description: "TEST(TS2)");
			var countervailingRelationShip1 = universalHelper.CreateTariffRelationship(countervailingTariff1.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate1 = universalHelper.CreateRate(countervailingTariff1, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability1 = universalHelper.CreateCusApplicability(countervailingRate1, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingTariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1409", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123", description: "TEST(TS3)");
			var countervailingRelationShip2 = universalHelper.CreateTariffRelationship(countervailingTariff2.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate2 = universalHelper.CreateRate(countervailingTariff2, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "120 * [KGM]");
			var countervailingApplicability2 = universalHelper.CreateCusApplicability(countervailingRate2, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456125", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "KG");
			var tariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456126", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff2, "CU1", "KG");
			var tariff3 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456124", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff3, "CU1", "G");
			Factory.Save();
		}
	}
}
