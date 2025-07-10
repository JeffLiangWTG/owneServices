using System;
using System.IO;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed public class TestDataSetupHelper
	{
		public TestDataSetupHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public EDIMessage Create830EDIMessage()
		{
			var entry = GetExportEntryWithFullData();
			var exportEntryHeader = new ExportEntryHeaderCreator().Create(entry);
			var result = new GOVCBR830MessageBuilder(exportEntryHeader).GenerateMessage();
			var message = entry.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				message.SetEM_MessageTextOrDataSource(stream);
				factory.Save();
			}
			return message;
		}

		public CusEntryHeader GetEntryHas830Snapshot()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "99999999", "서브", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			factory.Save();

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "USED EXCAVATOR");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "899999999", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "품명2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8523491020", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "DISCS FOR LASER READING SYSTEMS FOR REPRODUCING SO");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4910001000", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "PAPER CALENDARS");

			#region OrgHeader
			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김환태", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA1", "레디코리아1");
			var sellerCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCode);

			var sellerAddressCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00000"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller.MainAddress, sellerAddressCode);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA2", "레디코리아2");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "1234561243567" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);
			var supplierAddresssCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier.MainAddress, supplierAddresssCodes);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA3", "제조미상");
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "", "", "04784");
			var manufacturerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "제조미상9999000" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);
			var manufacturerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001" },
				new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "999" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer.MainAddress, manufacturerAddressCodes);

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001A" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			var terminalOperator = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA5", "");
			TestOrgDataSetUpHelper.AddOrgAddress(terminalOperator.MainAddress, "", "", "");
			var terminalOperatorCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = OrgCusCode.CodeTypes.ControlledPremisesID, Number = "55555", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(terminalOperator.MainAddress, terminalOperatorCodes);

			var forwarder = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA6", "");
			TestOrgDataSetUpHelper.AddOrgContact(forwarder, "나대표", true);

			var shippingLine = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA7", "대한항공");
			factory.Save();
			#endregion

			#region declaration
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.DocAddresses.AddNew(terminalOperator.MainAddress, MasterFiles.Integration.DocAddressType.CustomsContainerTerminalOperatorAddress);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator.MainAddress.PK;
			declaration.JE_ExportGoodsType = "11";
			declaration.JE_MessageSubType = "B";
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_GoodsDestination = "HK";
			declaration.JE_RL_NKPortOfLoading = "KRINC";
			declaration.JE_TradeIDWithKP = "02";
			declaration.JE_TradeIndicatorWithKP = "N";
			declaration.JE_ExportDate = new ZDateTime(2014, 01, 01);
			declaration.JE_ExporterType = "C";
			declaration.JE_OutOfHoursDecInd = "B";
			declaration.JE_ReturnReason = "";
			declaration.JE_ReturnType = "A";
			declaration.JE_GoodsCondition = "O";
			declaration.JE_SimpleDRWApp = "NO";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_UCR = "99999999999999999";
			declaration.JE_CarrierCode = "KE";
			declaration.JE_VesselName = "AA9999";
			declaration.JE_VoyageFlightNo = "AA9998";
			declaration.JE_TotalNoOfPacksPackType = "OU";
			declaration.Branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_LocationQualifier = "08500";
			declaration.JE_LocationOfGoods = "서울 금천구 가산디지털1로 119(SK트윈테크타워)";
			declaration.JE_LocationOtherInformation = "99999999";
			declaration.JE_LocationIDInBondedArea = "9999999999";
			declaration.UnderbondMovementArrivalDate = new ZDateTime(2014, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2014, 01, 01);

			declaration.JE_ProcedureType = "H";

			declaration.InspectionDate = new ZDateTime(2021, 03, 08);

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;
			billRef.J3_ReferenceNumber = "16HJSC0686I";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0008";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			houseBill.CU_BillSeqNo = "0001";
			#endregion

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_PaymentTerms = "TT";
			invoice1.JZ_LetterOfCreditNumber = "1234567";
			invoice1.JZ_IncoTerm = "CFR";
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_InvoiceAmount = 27000m;
			invoice1.JZ_InvoiceCurrExRate = 1084.25m;
			invoice1.JZ_InvoiceNumber = "999999999";
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_NoOfPacks = 1;
			invoice1.JZ_Remarks = @"1. 보세구역 반입 후 수출신고건은 보세구역 운영인, 컨테이너 작업업체 연락처 등 기재
2. 보세구역 반입 후 수출신고건은 보세구역 운영인, 컨테이너 작업업체 연락처 등 기재";
			invoice1.JZ_Weight = 29600m;
			invoice1.JZ_DRWApplicantType = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_PaymentTerms = "TT";
			invoice2.JZ_LetterOfCreditNumber = "1234567";
			invoice2.JZ_IncoTerm = "CFR";
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_InvoiceAmount = 670m;
			invoice2.JZ_InvoiceCurrExRate = 1084.25m;
			invoice2.JZ_InvoiceNumber = "899999999";
			invoice2.JZ_NoOfPacks = 2;
			invoice2.JZ_Weight = 0m;
			invoice2.JZ_DRWApplicantType = "1";
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "8429521022";

			invoiceLine.JI_Model = "HYUNDAI ROBEX3000LC-7A";
			invoiceLine.JI_BrandName = "상표명";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_COOLabelLocation = "N";
			invoiceLine.JI_PreviousEntryNumber = "A";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.JI_SkipManifestReport = "N";

			invoiceLine.JI_Weight = 29000m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 300m;
			invoiceLine.JI_NetWeightUQ = "KG";

			invoiceLine.JI_Description = "2007 N81011141";
			invoiceLine.JI_Ingredient = "dfewe";
			invoiceLine.JI_LotNumber = "AbcdZZZ";
			invoiceLine.JI_InvoiceUQ = "U";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_CustomsUnitQty = "CT";
			invoiceLine.JI_CustomsQuantity = 35m;
			invoiceLine.UnitPrice = 27670m;
			invoiceLine.JI_LinePrice = 27670m;
			invoiceLine.JI_PackType = "CT";
			invoiceLine.JI_NoOfPacks = 3;

			invoiceLine.CertificateOfOriginIssueStatus = "Y";
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_RN_NKCountryCode = "KR";
			certificate.CSI_LineNo = 1;

			var approvalDocument11 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument11.CSI_LineNo = 1;
			approvalDocument11.CSI_RN_NKCountryCode = "KR";
			approvalDocument11.CSI_Procedure = "05";
			approvalDocument11.CSI_SubType = "1";
			approvalDocument11.CSI_DateOfIssue = new ZDateTime(2021, 02, 26);
			approvalDocument11.CSI_Code = "A";
			approvalDocument11.CSI_ReferenceNumber = "NO";
			approvalDocument11.CSI_Description = "식품등의 수입신고확인증";
			approvalDocument11.CSI_AdditionalDescription = "테스트";
			approvalDocument11.CSI_ReferenceNumber2 = "TEST";

			var approvalDocument12 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument12.CSI_LineNo = 2;
			approvalDocument12.CSI_RN_NKCountryCode = "KR";
			approvalDocument12.CSI_Procedure = "11";
			approvalDocument12.CSI_SubType = "Y";
			approvalDocument12.CSI_DateOfIssue = new ZDateTime(2015, 01, 01);
			approvalDocument12.CSI_Code = "A";
			approvalDocument12.CSI_ReferenceNumber = "899999999";
			approvalDocument12.CSI_Description = "발급서류명2";
			approvalDocument12.CSI_AdditionalDescription = "사유2";
			approvalDocument12.CSI_ReferenceNumber2 = "586455632";

			var vehicleNumber1 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber1.CY_Order = 1;
			vehicleNumber1.CY_Data = "KN3HNP6N18K283119";

			var vehicleNumber2 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber2.CY_Order = 2;
			vehicleNumber2.CY_Data = "CCCCZZZ";

			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1000m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 2000m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			#endregion

			#region invoiceLine2
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_Tariff = "8429521022";

			invoiceLine2.JI_Weight = 600m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_NetWeight = 60m;
			invoiceLine2.JI_NetWeightUQ = "KG";

			invoiceLine2.JI_Model = "모델규격2";
			invoiceLine2.JI_Ingredient = "성분2";
			invoiceLine2.JI_LotNumber = "ZZZZEEE";
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_InvoiceQuantity = 1234m;
			invoiceLine2.JI_CustomsUnitQty = "CT";
			invoiceLine2.JI_CustomsQuantity = 1200m;
			invoiceLine2.UnitPrice = 4564m;
			invoiceLine2.JI_LinePrice = 899999.25m;

			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 300m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 500m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			#endregion

			#region invoiceLine3
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 1;
			invoiceLine3.JI_Tariff = "899999999";

			invoiceLine3.JI_Model = "거래품명2";
			invoiceLine3.JI_BrandName = "상표명2";
			invoiceLine3.JI_CountryOfOrigin = "KR";
			invoiceLine3.JI_COOLabelLocation = "N";
			invoiceLine3.JI_PreviousEntryNumber = "A";
			invoiceLine3.JI_PreviousEntryLineNumber = 1;
			invoiceLine3.JI_SkipManifestReport = "N";

			invoiceLine3.JI_Weight = 0m;
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_NetWeight = 299999.99m;
			invoiceLine3.JI_NetWeightUQ = "KG";

			invoiceLine3.JI_Model = "모델규격3";
			invoiceLine3.JI_Ingredient = "성분3";
			invoiceLine3.JI_LotNumber = "ZZZZEEEK";
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_InvoiceQuantity = 12345m;
			invoiceLine3.JI_CustomsUnitQty = "CT";
			invoiceLine3.JI_CustomsQuantity = 1235m;
			invoiceLine3.UnitPrice = 45648m;
			invoiceLine3.JI_LinePrice = 199999.25m;
			invoiceLine3.JI_PackType = "CT";
			invoiceLine3.JI_NoOfPacks = 5;

			invoiceLine3.CertificateOfOriginIssueStatus = "Y";
			invoiceLine3.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate3 = invoiceLine3.CertificateOfOriginData;
			certificate3.CSI_RN_NKCountryCode = "KR";
			certificate3.CSI_LineNo = 1;
			#endregion

			#region entry
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "EXP";
			entry.EntryNumber = "6N00221000025X";
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			#endregion

			#region entryLine

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8429521022";
			entryLine.CL_CustomsValue = 25987293m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "899999999";
			entryLine2.CL_CustomsValue = 8599999.99m;
			invoiceLine3.JI_CL = entryLine2.PK;
			#endregion

			#region container
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "999999999999999";

			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 1;
			containerLink.CCE_Status = "";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "899999999999999";

			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;
			containerLink2.CCE_Status = "";

			declaration.JE_ContainerPackMode = "AA";
			#endregion

			var export830 = new ExportEntryHeaderCreator().Create(entry);
			export830.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				factory.Save();
			}
			return entry;
		}

		public CusEntryHeader GetEntryHas5BASnapshot()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0712311000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "양송이 버섯");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2106903021", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "홍삼차");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8523292991", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "비디오 녹화된 것");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0712200000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "양파");

			var broker = factory.NewWithValidTestData<OrgHeader>();
			broker.OH_FullName = "(주)유한상사";
			broker.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "1234567890", Core.Constants.CountryCodes.KoreaSouth);
			broker.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1122334455", Core.Constants.CountryCodes.KoreaSouth);
			broker.CustomsCodes.AddNew(IdentificationType.OfficeID, "5693456", Core.Constants.CountryCodes.KoreaSouth);
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김유한", true);
			var brokerAddress = broker.Addresses.AddNew();
			TestOrgDataSetUpHelper.AddOrgAddress(brokerAddress, "서울시 마포구 망원동 64", "3층 21호", "821043", "101010", "32253");
			brokerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.Branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "21";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BA;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "4062001070010U";
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_IssueDate = new ZDateTime(2021, 7, 15);
			var entryNum5BA = entry.EntryNumbers.AddNew();
			entryNum5BA.CE_EntryType = ElectronicDocumentTypeList.Codes._5BA;
			entryNum5BA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_AgreedDutyRate = 8.25m;
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "C1";
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "0712311000";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0712311000";
			invoiceLine1.JI_Description = "Cultivated mushrooms (Agaricus bisporus)";
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "2106903021";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2106903021";
			invoiceLine2.JI_Description = "Red ginseng tea";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var import5BA = new Import5BAHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import5BA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA, stream);
				factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5BA);
			factory.Save();

			return entry;
		}

		public void SetUpExportEntryTariffData(GlbCompany company)
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var carrierCode = helper.CreateCarrierCode("KE", "KOREAN AIR", Core.Constants.CountryCodes.KoreaSouth);
			factory.Save();

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "USED EXCAVATOR");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "899999999", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "품명2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8523491020", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "DISCS FOR LASER READING SYSTEMS FOR REPRODUCING SO");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4910001000", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "PAPER CALENDARS");

			var usdCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			new TestDataSetupHelper(factory).SetExchangeRate(company, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1100.5m, usdCurrency);
			new TestDataSetupHelper(factory).SetExchangeRate(company, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1300.75m, usdCurrency);
			#endregion
		}

		public RefExchangeRate SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			factory.Save();

			return result;
		}

		public CusEntryHeader GetExportEntryWithFullData()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "99999999", "서브", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			factory.Save();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";

			new TestDataSetupHelper(factory).SetUpExportEntryTariffData(company);

			#region OrgHeader
			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA0", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김환태", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			branch.GB_OH_OrgProxy = broker.PK;

			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA1", "레디코리아");
			var sellerCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCode);

			var sellerAddressCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00000"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller.MainAddress, sellerAddressCode);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA2", "레디코리아");
			//supplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "1234561243567" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);

			var supplierAddressCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier.MainAddress, supplierAddressCode);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA3", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "", "", "04784");
			var manufacturerCode = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리아999000" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCode);

			var manufacturerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001" },
				new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "888" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer.MainAddress, manufacturerAddressCodes);

			var buyer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			var buyerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001A" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(buyer, buyerCodes);

			var terminalOperator = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA5", "");
			TestOrgDataSetUpHelper.AddOrgAddress(terminalOperator.MainAddress, "", "", "");
			var terminalOperatorCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = OrgCusCode.CodeTypes.ControlledPremisesID, Number = "55555", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(terminalOperator.MainAddress, terminalOperatorCodes);

			var forwarder = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA6", "");
			TestOrgDataSetUpHelper.AddOrgContact(forwarder, "나대표", true);

			var shippingLine = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA7", "대한항공");
			#endregion

			#region declaration
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.DocAddresses.AddNew(terminalOperator.MainAddress, MasterFiles.Integration.DocAddressType.CustomsContainerTerminalOperatorAddress);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator.MainAddress.PK;
			declaration.JE_ExportGoodsType = "11";
			declaration.JE_MessageSubType = "B";
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_GoodsDestination = "HK";
			declaration.JE_RL_NKPortOfLoading = "KRINC";
			declaration.JE_TradeIDWithKP = "02";
			declaration.JE_TradeIndicatorWithKP = "N";
			declaration.JE_ExportDate = new ZDateTime(2014, 01, 01);
			declaration.JE_ExporterType = "C";
			declaration.JE_OutOfHoursDecInd = "B";
			declaration.JE_ReturnReason = "ZZ";
			declaration.JE_ReturnType = "A";
			declaration.JE_GoodsCondition = "O";
			declaration.JE_SimpleDRWApp = "NO";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_UCR = "99999999999999999";
			declaration.JE_CarrierCode = "KE";
			declaration.JE_VesselName = "AA9999";
			declaration.JE_VoyageFlightNo = "AA9998";
			declaration.JE_TotalNoOfPacksPackType = "OU";
			declaration.JE_GB = branch.PK;
			declaration.JE_LocationQualifier = "08589";
			declaration.JE_LocationOfGoods = "서울 금천구 가산디지털1로 119";
			declaration.JE_LocationOtherInformation = "99999999";
			declaration.JE_EntryDate = new ZDate(2014, 01, 01);

			declaration.JE_LocationIDInBondedArea = "9999999999";
			declaration.UnderbondMovementArrivalDate = new ZDateTime(2014, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2014, 01, 01);

			declaration.JE_ProcedureType = "H";

			declaration.InspectionDate = new ZDateTime(2021, 03, 08);

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;
			billRef.J3_ReferenceNumber = "16HJSC0686I";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0008";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			houseBill.CU_BillSeqNo = "0001";
			#endregion

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_DRWApplicantType = "1";
			invoice1.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice1.JZ_OH_Buyer = buyer.PK;
			invoice1.JZ_PaymentTerms = "TT";
			invoice1.JZ_LetterOfCreditNumber = "1234567";
			invoice1.JZ_IncoTerm = "CFR";
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_InvoiceAmount = 27000m;
			invoice1.JZ_InvoiceCurrExRate = 1084.25m;
			invoice1.JZ_InvoiceNumber = "999999999";
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_NoOfPacks = 1;

			invoice1.JZ_Remarks = @"신고인";
			invoice1.JZ_ImportCargoManagementNumber = "16HJSC0686I00080001";
			invoice1.JZ_Weight = 29000m;
			invoice1.JZ_WeightUQ = "KG";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_DRWApplicantType = "1";
			invoice2.JZ_Remarks = @"기재란";
			invoice2.JZ_PaymentTerms = "TT";
			invoice2.JZ_LetterOfCreditNumber = "1234567";
			invoice2.JZ_IncoTerm = "CFR";
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_InvoiceAmount = 670m;
			invoice2.JZ_InvoiceCurrExRate = 1084.25m;
			invoice2.JZ_InvoiceNumber = "899999999";
			invoice2.JZ_NoOfPacks = 2;
			invoice2.JZ_Weight = 600m;
			invoice2.JZ_WeightUQ = "KG";
			#endregion

			#region invoiceLine
			JobComInvoiceLine invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "8429521022";

			invoiceLine.JI_Model = "HYUNDAI ROBEX3000LC-7A";
			invoiceLine.JI_BrandName = "상표명";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_COOLabelLocation = "N";
			invoiceLine.JI_PreviousEntryNumber = "A";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.JI_SkipManifestReport = "N";
			invoiceLine.JI_NetWeight = 300m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Description = "2007 N81011141";
			invoiceLine.JI_Ingredient = "dfewe";
			invoiceLine.JI_LotNumber = "AbcdZZZ";
			invoiceLine.JI_InvoiceUQ = "U";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_CustomsUnitQty = "CT";
			invoiceLine.JI_CustomsQuantity = 35m;
			invoiceLine.UnitPrice = 27670m;
			invoiceLine.JI_LinePrice = 27670m;
			invoiceLine.JI_PackType = "CT";
			invoiceLine.JI_NoOfPacks = 3;
			invoiceLine.JI_PrimaryPreference = "106";

			invoiceLine.CertificateOfOriginIssueStatus = "Y";
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_RN_NKCountryCode = "KR";
			certificate.CSI_LineNo = 1;

			invoiceLine.PRA_ReferenceNumber = "KR00101010101";
			invoiceLine.PRA_DateOfIssue = new ZDateTime(2018, 01, 01);
			invoiceLine.PRA_DateOfExpiry = new ZDateTime(2018, 09, 01);

			var approvalDocument11 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument11.CSI_LineNo = 1;
			approvalDocument11.CSI_RN_NKCountryCode = "KR";
			approvalDocument11.CSI_Procedure = "05";
			approvalDocument11.CSI_SubType = RequirementTypeCodeList.Codes._3;
			approvalDocument11.CSI_DateOfIssue = new ZDateTime(2021, 02, 26);
			approvalDocument11.CSI_Code = "A";
			approvalDocument11.CSI_ReferenceNumber = "NO";
			approvalDocument11.CSI_Description = "식품등의 수입신고확인증";
			approvalDocument11.CSI_AdditionalDescription = "테스트";
			approvalDocument11.CSI_ReferenceNumber2 = "TEST";
			approvalDocument11.CSI_Status = "01";

			var approvalDocument12 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument12.CSI_LineNo = 2;
			approvalDocument12.CSI_RN_NKCountryCode = "KR";
			approvalDocument12.CSI_Procedure = "11";
			approvalDocument12.CSI_SubType = RequirementTypeCodeList.Codes._3;
			approvalDocument12.CSI_DateOfIssue = new ZDateTime(2015, 01, 01);
			approvalDocument12.CSI_Code = "A";
			approvalDocument12.CSI_ReferenceNumber = "899999999";
			approvalDocument12.CSI_Description = "발급서류명2";
			approvalDocument12.CSI_AdditionalDescription = "사유2";
			approvalDocument12.CSI_ReferenceNumber2 = "586455632";
			approvalDocument12.CSI_Status = "02";

			var vehicleNumber1 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber1.CY_Order = 1;
			vehicleNumber1.CY_Data = "KN3HNP6N18K283119";

			var vehicleNumber2 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber2.CY_Order = 2;
			vehicleNumber2.CY_Data = "CCCCZZZ";

			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 899999100m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 59m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 799999000m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 39m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			#endregion

			#region invoiceLine2
			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_Tariff = "8429521022";
			invoiceLine2.JI_NetWeight = 60m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_Description = "모델규격2";
			invoiceLine2.JI_Ingredient = "성분2";
			invoiceLine2.JI_LotNumber = "ZZZZEEE";
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_InvoiceQuantity = 1234m;
			invoiceLine2.JI_CustomsUnitQty = "CT";
			invoiceLine2.JI_CustomsQuantity = 1200m;
			invoiceLine2.UnitPrice = 4564m;
			invoiceLine2.JI_LinePrice = 899999.25m;

			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 800m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 40m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 900m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			((IChargeApportionee)invoiceLine2).ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 60m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			#endregion

			#region invoiceLine3
			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 1;
			invoiceLine3.JI_Tariff = "899999999";

			invoiceLine3.JI_Model = "거래품명2";
			invoiceLine3.JI_BrandName = "상표명2";
			invoiceLine3.JI_CountryOfOrigin = "KR";
			invoiceLine3.JI_COOLabelLocation = "N";
			invoiceLine3.JI_PreviousEntryNumber = "A";
			invoiceLine3.JI_PreviousEntryLineNumber = 1;
			invoiceLine3.JI_SkipManifestReport = "N";

			invoiceLine3.JI_Weight = 0m;
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_NetWeight = 1299999.99m;
			invoiceLine3.JI_NetWeightUQ = "KG";

			invoiceLine3.JI_Description = "모델규격3";
			invoiceLine3.JI_Ingredient = "성분3";
			invoiceLine3.JI_LotNumber = "ZZZZEEEK";
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_InvoiceQuantity = 12345m;
			invoiceLine3.JI_CustomsUnitQty = "CT";
			invoiceLine3.JI_CustomsQuantity = 1235m;
			invoiceLine3.UnitPrice = 45648m;
			invoiceLine3.JI_LinePrice = 199999.25m;
			invoiceLine3.JI_PackType = "CT";
			invoiceLine3.JI_NoOfPacks = 5;
			invoiceLine3.JI_PrimaryPreference = "105";

			invoiceLine3.CertificateOfOriginIssueStatus = "Y";
			invoiceLine3.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate3 = invoiceLine3.CertificateOfOriginData;
			certificate3.CSI_RN_NKCountryCode = "KR";
			certificate3.CSI_LineNo = 1;

			invoiceLine3.PRA_ReferenceNumber = "KR01231313";
			invoiceLine3.PRA_DateOfIssue = new ZDateTime(2019, 01, 01);
			invoiceLine3.PRA_DateOfExpiry = new ZDateTime(2019, 07, 01);
			#endregion

			#region entry
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "EXP";
			entry.EntryNumber = "6N00221000025X";
			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
			entry.CusEntryNumber.CE_ExpiryDate = ZDate.Today.AddDays(365);
			entry.CH_EntryReleaseDate = ZDateTime.Today;
			entry.CH_CustomsMessageRemarks = "ABCDEF";
			entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, "AAAA", "BBB", ZDate.Empty);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			#endregion

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8429521022";
			entryLine.CL_CustomsValue = 25987293m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "899999999";
			entryLine2.CL_CustomsValue = 8599999.99m;
			invoiceLine3.JI_CL = entryLine2.PK;
			#endregion

			#region container
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "999999999999999";

			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 1;
			containerLink.CCE_Status = "";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "899999999999999";

			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;
			containerLink2.CCE_Status = "";

			declaration.JE_ContainerPackMode = "AA";
			#endregion

			return entry;
		}

		public CusEntryHeader GetLocalExportEntry5DPWithFullData()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01023010", "보세구역이름", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			factory.Save();

			#region Supplier
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "5DP Company";
			supplier.MainAddress.Address1 = "5DP Address1";
			supplier.MainAddress.Address2 = "5DP Address2";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DP CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplier.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			supplier.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Manufacturer
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			manufacturer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Exporter
			var exporter = factory.NewWithValidTestData<OrgHeader>();
			exporter.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1234567890", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Importer
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1200020212", Core.Constants.CountryCodes.KoreaSouth);
			importer.OH_FullName = "무한상사";

			var importerAddress = importer.MainAddress;
			importerAddress.OA_CompanyNameOverride = "무한상사";
			importerAddress.CustomsCodes.AddNew(IdentificationType.RoadNameCode, "110001");
			importerAddress.CustomsCodes.AddNew(IdentificationType.BuildingNumber, "121200");
			importerAddress.OA_Address1 = "기본주소";
			importerAddress.OA_Address2 = "상세주소";
			importerAddress.OA_PostCode = "32012";

			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "김대표";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;
			#endregion

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			declaration.JE_EntryDate = new ZDate(2013, 01, 01);
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ExportGoodsType = "1";
			declaration.JE_LocationOfGoods = "";
			declaration.JE_LocationOtherInformation = "01023010";
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Manufacturer = manufacturer.PK;
			declaration.JE_OH_Exporter = exporter.PK;
			invoice.JZ_OH_Buyer = importer.PK;
			invoice.JZ_OA_BuyerAddress = importer.MainAddress.PK;
			invoice.JZ_Weight = 900;
			invoice.JZ_WeightUQ = DefaultWeightUnit;
			invoice.JZ_NoOfPacks = 100;
			invoice.JZ_DRWApplicantType = "1";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_BGMReference = "010D8210003501";
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum.CE_EntryNum = "1083699012345";
			entryNum.CE_IssueDate = ZDateTime.Today;

			#region ILocalExportEntryLine
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "1234567890";
			entryLine1.CL_Description = "STAINLESS STEEL";
			entryLine1.CL_CustomsValue = 1000m;

			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_InvoiceQuantity = 9999m;
			invoiceLine1.JI_InvoiceUQ = DefaultWeightUnit;
			invoiceLine1.JI_NoOfPacks = 99;
			invoiceLine1.JI_PackType = "VL";
			invoiceLine1.JI_NetWeight = 9999m;
			invoiceLine1.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine1.JI_LinePrice = 9999m;
			invoiceLine1.JI_InboundDate = new ZDateTime(2013, 01, 01);
			invoiceLine1.JI_PreviousEntryNumber = "010151234567001999";
			invoiceLine1.JI_OriginalStateDocType = "01";
			invoiceLine1.JI_SerialNumber = "000000000";
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_Ingredient = "ABCD9589375";

			JobComInvoiceLine invoiceLine1_2 = invoice.InvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_NetWeight = 1m;
			invoiceLine1_2.JI_NetWeightUQ = DefaultWeightUnit;

			invoiceLine1.SupportingDocumentReferenceNumber = "L172770912345";
			invoiceLine1.SupportingDocumentCode = "1";

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0987654321";
			entryLine2.CL_Description = "STAINLESS STEEL2";
			entryLine2.CL_CustomsValue = 3333m;

			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_InvoiceQuantity = 1111m;
			invoiceLine2.JI_InvoiceUQ = DefaultWeightUnit;
			invoiceLine2.JI_NoOfPacks = 11;
			invoiceLine2.JI_PackType = "VL";
			invoiceLine2.JI_NetWeight = 2222m;
			invoiceLine2.JI_NetWeightUQ = DefaultWeightUnit;
			invoiceLine2.JI_LinePrice = 3333m;
			invoiceLine2.JI_InboundDate = new ZDateTime(2020, 02, 02);
			invoiceLine2.JI_PreviousEntryNumber = "999100765432151010";
			invoiceLine2.JI_OriginalStateDocType = "02";
			invoiceLine2.JI_SerialNumber = "1111111111";
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_Ingredient = "5739859DCBA";

			invoiceLine2.SupportingDocumentReferenceNumber = "L172770925459";
			invoiceLine2.SupportingDocumentCode = "2";
			#endregion

			return entry;
		}

		public CusEntryHeader GetLocalExportEntry5DQWithFullData()
		{
			var vessels = new RefVesselCollection(factory);
			var vessel1 = vessels.AddNew();
			vessel1.RV_Code = "CY_Code Test1";
			vessel1.RV_MalaysiaVesselId = "1";
			factory.Save();

			#region Exporter
			var exporter = factory.NewWithValidTestData<OrgHeader>();
			exporter.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1234567890", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region OrgHeader
			#region Supplier
			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "5DQ Company";
			supplier.MainAddress.Address1 = "5DQ Address1";
			supplier.MainAddress.Address2 = "5DQ Address2";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DQ CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplier.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "통관고유부호", Core.Constants.CountryCodes.KoreaSouth);
			supplier.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "사업자등록번호", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Manufacturer
			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "통관고유부호", Core.Constants.CountryCodes.KoreaSouth);
			manufacturer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "사업자등록번호", Core.Constants.CountryCodes.KoreaSouth);
			#endregion

			#region Importer
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "무한상사";
			importer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1200020212", Core.Constants.CountryCodes.KoreaSouth);

			var importerAddress = importer.MainAddress;
			importerAddress.OA_CompanyNameOverride = "무한상사";
			importerAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "110001");
			importerAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "121200");
			importerAddress.OA_Address1 = "기본주소";
			importerAddress.OA_Address2 = "상세주소";
			importerAddress.OA_PostCode = "32012";

			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "김대표";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CEOForKRCustoms;
			#endregion
			#endregion

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ExportGoodsType = "1";
			declaration.JE_TotalNoOfPacksPackType = "BOX";
			declaration.JE_NoOfCrew = 15;
			declaration.JE_VoyageDuration = 24;
			declaration.JE_VesselName = "M/V MARIA";
			declaration.JE_MRNType = MRNTypeList.Codes.Normal;
			declaration.DeclarationRefs.AddNew("MRN", "20GLKO0080I");
			declaration.JE_EntryDate = ZDate.Today;
			declaration.JE_LocationOfGoods = "";
			declaration.JE_SubLocationOfGoods = "경남창고";
			declaration.JE_OH_Exporter = exporter.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var transportMean = declaration.TransportMeans.AddNew();
			transportMean.CY_Order = 1;
			transportMean.CY_Code = "CY_Code Test1";
			transportMean.CY_Data = "1";

			#region Stevedores
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			var person1 = declaration.Persons.AddNew();
			var glbPerson1 = factory.New<GlbPerson>();
			glbPerson1.PER_FullName = "홍길동";
			glbPerson1.PER_BirthDate = new ZDate(1991, 05, 06);
			person1.CPN_PER_Person = glbPerson1.PK;

			var person2 = declaration.Persons.AddNew();
			var glbPerson2 = factory.New<GlbPerson>();
			glbPerson2.PER_FullName = "Hong-Gil-Dong";
			glbPerson2.PER_BirthDate = new ZDate(1991, 06, 06);
			person2.CPN_PER_Person = glbPerson2.PK;

			var stevedorer = declaration.StevedoreCompany;
			stevedorer.E2_OA_Address = orgHeader.Addresses.AddNew().PK;
			stevedorer.Address.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "110001");
			stevedorer.Address.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "121200");
			stevedorer.Address.OA_PostCode = "43012";
			stevedorer.Address.OA_Address1 = "기본주소";
			stevedorer.Address.OA_Address2 = "상세주소";
			#endregion

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 900;
			invoice.JZ_WeightUQ = DefaultWeightUnit;
			invoice.JZ_NoOfPacks = 100;
			invoice.JZ_OH_Manufacturer = manufacturer.PK;
			invoice.JZ_OH_Buyer = importer.PK;
			invoice.JZ_OA_BuyerAddress = importer.MainAddress.PK;
			invoice.JZ_DRWApplicantType = "1";

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum.CE_EntryNum = "4177721000030";
			entryNum.CE_IssueDate = new ZDateTime(2013, 01, 01);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "1234567891";
			entryLine2.CL_Description = "STAINLESS STEEL2";
			entryLine2.CL_CustomsValue = 999m;

			ZString ingredient = "";
			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_InvoiceQuantity = 999m;
			invoiceLine2.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_NoOfPacks = 9;
			invoiceLine2.JI_PackType = "VL";
			invoiceLine2.JI_NetWeight = 999000m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine2.JI_LinePrice = 999m;
			invoiceLine2.JI_InboundDate = new ZDateTime(2013, 01, 02);
			invoiceLine2.JI_PreviousEntryNumber = "010151234567001990";
			invoiceLine2.JI_OriginalStateDocType = "02";
			invoiceLine2.JI_SerialNumber = "물품식별번호2";
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_Ingredient = ingredient;

			invoiceLine2.SupportingDocumentReferenceNumber = "L172770925458";
			invoiceLine2.SupportingDocumentCode = "2";

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "1234567890";
			entryLine1.CL_Description = "STAINLESS STEEL";
			entryLine1.CL_CustomsValue = 1000m;

			JobComInvoiceLine invoiceLine1_1 = invoice.InvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_1.JI_InvoiceQuantity = 5555m;
			invoiceLine1_1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_1.JI_NoOfPacks = 55;
			invoiceLine1_1.JI_PackType = "VL";
			invoiceLine1_1.JI_NetWeight = 5555m;
			invoiceLine1_1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_1.JI_LinePrice = 5555m;
			invoiceLine1_1.JI_InboundDate = new ZDateTime(2013, 01, 01);
			invoiceLine1_1.JI_PreviousEntryNumber = "010151234567001999";
			invoiceLine1_1.JI_OriginalStateDocType = "01";
			invoiceLine1_1.JI_SerialNumber = "물품식별번호";
			invoiceLine1_1.JI_SequenceNumber = 1;
			invoiceLine1_1.JI_Ingredient = ingredient;

			invoiceLine1_1.SupportingDocumentReferenceNumber = "L172770912345";
			invoiceLine1_1.SupportingDocumentCode = "1";

			JobComInvoiceLine invoiceLine1_2 = invoice.InvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_InvoiceQuantity = 4444m;
			invoiceLine1_2.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_2.JI_NoOfPacks = 44;
			invoiceLine1_2.JI_PackType = "VL";
			invoiceLine1_2.JI_NetWeight = 4444m;
			invoiceLine1_2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_2.JI_LinePrice = 4444m;
			invoiceLine1_2.JI_InboundDate = new ZDateTime(2013, 01, 01);
			invoiceLine1_2.JI_PreviousEntryNumber = "010151234567001999";
			invoiceLine1_2.JI_OriginalStateDocType = "01";
			invoiceLine1_2.JI_SerialNumber = "물품식별번호";
			invoiceLine1_2.JI_SequenceNumber = 2;
			invoiceLine1_2.JI_Ingredient = ingredient;

			entry.CH_BGMReference = "010D8210003501";
			return entry;
		}

		public CusEntryHeader GetLocalExportEntry5DPWithPartialData()
		{
			var entry = GetLocalExportEntry5DPWithFullData();

			entry.Declaration.JE_EntryDate = ZDate.Invalid;
			entry.Declaration.JE_LocationOfGoods = ZString.Empty;
			entry.Declaration.JE_SubLocationOfGoods = ZString.Empty;
			entry.Declaration.JE_LocationOtherInformation = ZString.Empty;
			var invoice = entry.Declaration.Invoices[0];
			invoice.JZ_OH_Manufacturer = ZGuid.Empty;

			var invoiceLine1 = invoice.InvoiceLines[0];
			invoiceLine1.JI_InboundDate = ZDateTime.Invalid;
			invoiceLine1.JI_SerialNumber = ZString.Empty;
			invoiceLine1.JI_Ingredient = ZString.Empty;
			invoiceLine1.SupportingDocumentReferenceNumber = ZString.Empty;
			invoiceLine1.SupportingDocumentCode = ZString.Empty;

			entry.Declaration.JE_OH_Exporter = ZGuid.Empty;

			#region Importer
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1200020212", Core.Constants.CountryCodes.KoreaSouth);
			importer.OH_FullName = "무한상사";
			importer.MainAddress.OA_Address1 = null;
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "김대표";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;
			#endregion

			return entry;
		}

		public CusEntryHeader GetLocalExportEntry5DQWithPartialData()
		{
			var entry = GetLocalExportEntry5DQWithFullData();

			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "무한상사";

			var contact = importer.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = ContactAllocationType.CEOForKRCustoms;

			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "통관고유부호", Core.Constants.CountryCodes.KoreaSouth);

			var importerAddress = importer.MainAddress;

			importerAddress.OA_CompanyNameOverride = ZString.Empty;
			importerAddress.CustomsCodes.RemoveAll(x => true);
			importerAddress.OA_Address1 = ZString.Empty;
			importerAddress.OA_Address2 = ZString.Empty;
			importerAddress.OA_PostCode = ZString.Empty;

			var declaration = entry.Declaration;
			declaration.JE_NoOfCrew = ZInt.Zero;
			declaration.JE_VoyageDuration = ZInt.Zero;
			declaration.JE_VesselName = ZString.Empty;
			declaration.DeclarationRefs.RemoveAll(x => true);
			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.JE_SubLocationOfGoods = ZString.Empty;
			declaration.JE_OH_Exporter = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			var transportMean = declaration.TransportMeans[0];
			transportMean.CY_Order = 1;
			transportMean.CY_Code = ZString.Empty;
			transportMean.CY_Data = ZString.Empty;

			var stevedoreAddress = declaration.StevedoreCompany.Address;
			stevedoreAddress.CustomsCodes.RemoveAll(x => true);
			stevedoreAddress.OA_PostCode = ZString.Empty;
			stevedoreAddress.OA_Address2 = ZString.Empty;

			declaration.Invoices[0].JZ_OH_Manufacturer = ZGuid.Empty;

			foreach (JobComInvoiceLine invoiceLine in declaration.Invoices[0].InvoiceLines)
			{
				invoiceLine.JI_InboundDate = ZDate.Empty;
				invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
				invoiceLine.JI_OriginalStateDocType = ZString.Empty;
				invoiceLine.JI_SerialNumber = ZString.Empty;
			}

			return entry;
		}

		public CusEntryHeader GetEntryHas5DQSnapshot()
		{
			var entry = GetLocalExportEntry5DQWithFullData();
			var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ);
				factory.Save();
			}

			return entry;
		}
		public CusEntryHeader GetEntryHas5DPSnapshot()
		{
			var entry = GetLocalExportEntry5DPWithFullData();
			var originalHeader = new LocalExport5DPEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DP, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DP);
				factory.Save();
			}

			return entry;
		}

		public CusEntryHeader GetLocalExportHasAmendSnapShot()
		{
			var entry = GetLocalExportEntry5DQWithFullData();
			var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ);
				factory.Save();
			}
			var snapShot = entry.Snapshots.GetFirstSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, "LDG");
			snapShot.CES_VersionNumber = 1;

			entry.CH_BGMReference = "01010230000010";
			var amendedItems = new LocalExportAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5DS).AmendedItems;
			var amendmentHeader = new LocalExportAmendmentHeaderCreator().Create(entry, amendedItems.ToArray());
			using (var stream = KRXmlObjectSerializer.Serialize(amendmentHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5DQ);
				factory.Save();
			}
			snapShot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, "LDG");
			snapShot.CES_VersionNumber = 2;

			return entry;
		}

		public void SetEntry929Tariff()
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			factory.Save();

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", new ZDateTime(2013, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "TACKS");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208122222", new ZDateTime(2013, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "TACKS2");
			#endregion
		}

		public CusEntryHeader GetEntry929FullData(ZString paymentMethod, ZBool paidByBlank)
		{
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";

			company.GC_IsReciprocal = false;
			var usdCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);

			RefExchangeRate result = factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = usdCurrency.RX_Code;
			result.RE_StartDate = ZDateTime.Today;
			result.RE_ExpiryDate = ZDateTime.Today;
			result.RE_SellRate = 1.39m;
			result.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			factory.Save();

			#region OrgHeader
			var broker = factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "상호";
			broker.OH_IsBroker = true;

			var orgContact = TestOrgDataSetUpHelper.AddOrgContact(broker, "신고인", true);
			orgContact.OC_PhoneExtension = "0000";
			var brokeraddress = broker.MainAddress;
			brokeraddress.OA_Email = "id@nnnn";
			brokeraddress.OA_Phone = "0000000000";

			company.GC_OH_OrgProxy = broker.PK;
			#endregion

			#region Importer1
			var importer1 = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA1", "");
			TestOrgDataSetUpHelper.AddOrgContact(importer1, "조인성", true);
			var importerrCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "관세상사1234561" },
				new IDNumberAndType() { Type = IdentificationType.AuthorizedImporterRegNo,  Number = "1234567890" }
			};
			var importerAddress = importer1.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			TestOrgDataSetUpHelper.AddOrgAddress(importerAddress, "서울시 강남구 논현동 235", "7층 101호", "11087", "012345678912", "1234567891234567891234567");
			TestOrgDataSetUpHelper.AddCustomsCode(importer1, importerrCodes);
			#endregion

			#region Payer
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA2", "모나리자(주)");
			payer.OH_IsBroker = false;
			var payerCustomsAddress = payer.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			TestOrgDataSetUpHelper.AddOrgContact(payer, "홍나리", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payerCustomsAddress, "서울시 강남구 논현동 235", "7층 101호", "11087", "012345678912", "1234567891234567891234567");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "모나리자1771025" },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "6510071645915" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			var payerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "0001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer.MainAddress, payerAddressCodes);

			payerCustomsAddress.OA_Email = "id@domain.com";
			payerCustomsAddress.OA_Phone = "0000000000";
			#endregion

			#region Seller
			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA3", "OMR ENGR");
			TestOrgDataSetUpHelper.AddOrgAddress(seller.MainAddress, "", "", "", "", "", "JP");
			var sellerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "CNTOSHIN12347" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);
			#endregion

			#region FreightForwarder
			var freightForwarder = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA4", "세관무역(주)");
			var freightForwarderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode,  Number = "ABCD" },
				new IDNumberAndType() { Type = IdentificationType.CourierCompanyID,  Number = "HJSC" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(freightForwarder, freightForwarderCodes);
			#endregion

			#region Header
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer1.PK;
			declaration.JE_OA_ImporterAddress = importer1.MainAddress.PK;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "20";
			declaration.JE_DateOfArrival = new ZDateTime(2012, 01, 01);
			declaration.JE_GC = company.PK;
			declaration.JE_ProcedureType = "L";
			if (paymentMethod.IsEmpty)
			{
				declaration.JE_PaymentMethod = "13";
			}
			else
			{
				declaration.JE_PaymentMethod = paymentMethod;
			}
			if (paidByBlank)
			{
				declaration.JE_PaidBy = "";
			}
			else
			{
				declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			}
			declaration.JE_OH_Forwarder = freightForwarder.PK;
			declaration.JE_DeclarationPlan = "D";
			declaration.JE_MessageSubType = "A";
			declaration.JE_TradeType = "11";
			declaration.JE_TotalNoOfPacksPackType = "CT";
			declaration.JE_RL_NKPortOfArrival = "KRPUS";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ContainerPackMode = "ETC";
			declaration.JE_CustomsLoadPort = "PR";
			declaration.JE_GB = branch.PK;
			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "KE1098";
			declaration.JE_LocationOtherInformation = "13011013";
			declaration.JE_OA_ImporterAddress = importer1.MainAddress.PK;
			declaration.JE_LocationIDInBondedArea = "1A123";

			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "KE1098";
			vessel.RV_RN_NKCountryOfReg = "KR";

			var airline = factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "KE";
			airline.RM_RN_NKAirlineCountry = "KR";

			declaration.JE_CarrierCode = "HJSC";
			declaration.JE_OwnerRef = "123456789";
			declaration.JE_TradeIndicatorWithKP = "Y";
			declaration.JE_GoldTrade = "Y";

			var referenceNumber1 = declaration.DeclarationRefs.AddNew();
			referenceNumber1.J3_ReferenceType = Constants.AdditionalInformationStatementCodes_929._257;
			referenceNumber1.J3_ReferenceNumber = "A";

			var referenceNumber2 = declaration.DeclarationRefs.AddNew();
			referenceNumber2.J3_ReferenceType = Constants.AdditionalInformationStatementCodes_929._258;
			referenceNumber2.J3_ReferenceNumber = "A";

			var referenceNumber3 = declaration.DeclarationRefs.AddNew();
			referenceNumber3.J3_ReferenceType = Constants.AdditionalInformationStatementCodes_929._259;
			referenceNumber3.J3_ReferenceNumber = "A";
			#endregion

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0010";
			masterBill.CU_BillNum = "DBSC96100123AB01";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;
			houseBill.CU_BillSeqNo = "003";
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			houseBill.CU_HBSplitDecReasonCode = "A";
			houseBill.HBSplitDecReasonRemark = "a";
			houseBill.CU_BillNum = "HJSC98100123AB01";

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = seller.PK;
			invoice1.JZ_IncoTerm = "CFR";
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_PaymentTerms = "DA";
			invoice1.JZ_InvoiceCurrExRate = 1210.12m;
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_ImportCargoManagementNumber = "01KE0766SS200100003";
			invoice1.JZ_ValuationDecAttachCode = "Y";
			invoice1.JZ_BlanketValuationDeclarationNumber = "AAAAAAAAAAAA";
			invoice1.JZ_Remarks = @"기재사항1";
			invoice1.JZ_COOStatus = "Y";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "CFR";
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_PaymentTerms = "DA";
			invoice2.JZ_InvoiceCurrExRate = 1210.12m;
			invoice2.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice2.JZ_BlanketValuationDeclarationNumber = "AAAAAAAAAAAA";
			invoice2.JZ_COOStatus = "Y";
			#endregion

			#region invoiceLine1
			JobComInvoiceLine invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;

			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_Weight = 30.2m;

			invoiceLine.JI_Model = "RABBIT MEAT";
			invoiceLine.JI_BrandCode = "ZZZZ";
			invoiceLine.JI_BrandName = "상표명";
			invoiceLine.JI_Tariff = "0208100000";
			invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = "123456789";

			var hsExtensionCode1 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode1.CY_Order = 0;
			hsExtensionCode1.CY_Code = "01";

			var hsExtensionCode2 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode2.CY_Order = 1;
			hsExtensionCode2.CY_Code = "101";

			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_ReferenceNumber = "12345678";
			certificate.CSI_Procedure = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			certificate.CSI_DateOfIssue = new ZDateTime(2013, 01, 01);
			certificate.CSI_RN_NKCountryCode = "KR";
			certificate.CSI_Description = "발행기관명";
			certificate.CSI_AdditionalDescription = "발급지역명";
			certificate.CSI_ReferenceNumber2 = "발급담당자명";
			certificate.CSI_Status = Constants.YesNo.Yes;
			certificate.CSI_Quantity = 10000m;
			certificate.CSI_Quantity2 = 10000m;
			certificate.CSI_Quantity3 = 10000m;

			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_COOLabelLocation = "Y";
			invoiceLine.JI_COOLabelType = "A";
			invoiceLine.JI_COOExemptionReason = "14";
			invoiceLine.JI_ProductTypeCode = "7";
			invoiceLine.JI_ParentLine = 123;
			invoiceLine.JI_MightRequireInspection = YesNo.Yes;
			invoiceLine.JI_PostClearanceProcedureGA1 = "023";
			invoiceLine.JI_PostClearanceProcedureGA2 = "019";
			invoiceLine.JI_PostClearanceProcedureGA3 = "020";
			invoiceLine.JI_NetWeight = 99999.9m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CourierCargoSelectivityIndicator = CourierCargoSelectivityIndicatorCodeList.Codes.Y;
			invoiceLine.JI_PrimaryPreference = "x";
			invoiceLine.JI_AdditionalDutyRate = 99.99M;
			invoiceLine.JI_AdditionalDutyType = "I";
			invoiceLine.JI_DomesticTaxExemptionCode = "0000001";
			invoiceLine.JI_DomesticTaxCode = "AA-";
			invoiceLine.JI_CustomsUnitQty = "DZ";
			invoiceLine.JI_CustomsQuantity = 30m;
			invoiceLine.JI_ZZF_NKTaxType = VATRateTypeCodeList.Codes.VATReduction;
			invoiceLine.JI_VATReductionCode = "11";

			#endregion
			#region invoiceLine2
			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;

			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_Weight = 30.2m;
			#endregion

			#region invoiceLine3
			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 1;

			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_Weight = 65.2m;

			invoiceLine3.JI_Model = "RABBIT MEAT2";
			invoiceLine3.JI_BrandCode = "XXXX";
			invoiceLine3.JI_BrandName = "상표명2";
			invoiceLine3.JI_Tariff = "0208122222";
			invoiceLine3.JI_SpecificUseCodeDutyRatePermitNo = "987654321";

			var hsExtensionCode3 = invoiceLine3.HSExtensionCodeCollection.AddNew();
			hsExtensionCode3.CY_Order = 0;
			hsExtensionCode3.CY_Code = "02";

			var hsExtensionCode4 = invoiceLine3.HSExtensionCodeCollection.AddNew();
			hsExtensionCode4.CY_Order = 1;
			hsExtensionCode4.CY_Code = "102";

			invoiceLine3.CriteriaForDeterminingCountryOfOrigin = "1";
			var certificate3 = invoiceLine3.CertificateOfOriginData;
			certificate3.CSI_ReferenceNumber = "87654321";
			certificate3.CSI_Procedure = CountryOfOriginDeterminationRuleCodeList.Codes.B;
			certificate3.CSI_DateOfIssue = new ZDateTime(2013, 02, 02);
			certificate3.CSI_RN_NKCountryCode = "KR";
			certificate3.CSI_Description = "발행기관명2";
			certificate3.CSI_AdditionalDescription = "발급지역명2";
			certificate3.CSI_ReferenceNumber2 = "발급담당자명2";
			certificate3.CSI_Status = Constants.YesNo.Yes;
			certificate3.CSI_Quantity = 20000m;
			certificate3.CSI_Quantity2 = 20000m;
			certificate3.CSI_Quantity3 = 20000m;

			invoiceLine3.JI_CountryOfOrigin = "CN";
			invoiceLine3.JI_COOLabelLocation = "Y";
			invoiceLine3.JI_COOLabelType = "D";
			invoiceLine3.JI_COOExemptionReason = "12";
			invoiceLine3.JI_ProductTypeCode = "8";
			invoiceLine3.JI_ParentLine = 123;
			invoiceLine3.JI_MightRequireInspection = YesNo.Yes;
			invoiceLine3.JI_PostClearanceProcedureGA1 = "024";
			invoiceLine3.JI_PostClearanceProcedureGA2 = "020";
			invoiceLine3.JI_PostClearanceProcedureGA3 = "021";
			invoiceLine3.JI_NetWeight = 99999.9m;
			invoiceLine3.JI_NetWeightUQ = "KG";
			invoiceLine3.JI_CourierCargoSelectivityIndicator = CourierCargoSelectivityIndicatorCodeList.Codes.Y;
			invoiceLine3.JI_PrimaryPreference = "x";
			invoiceLine3.JI_AdditionalDutyRate = 99.99M;
			invoiceLine3.JI_AdditionalDutyType = "I";
			invoiceLine3.JI_DomesticTaxExemptionCode = "0000002";
			invoiceLine3.JI_DomesticTaxCode = "AA-";
			invoiceLine3.JI_CustomsUnitQty = "DZ";
			invoiceLine3.JI_CustomsQuantity = 30m;
			invoiceLine3.JI_ZZF_NKTaxType = VATRateTypeCodeList.Codes.VATReduction;
			invoiceLine3.JI_VATReductionCode = "11";
			#endregion

			#region entry
			var entryInstruction = factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_PackQty = 2;
			entryInstruction.CEI_BondedFactoryUseCode = "B";
			entryInstruction.CEI_BondedFactoryArrivalDate = new ZDateTime(2012, 02, 10, 10, 12, 00);
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "Y";
			entryInstruction.CEI_ApplyDutyPenaltyReduction = "Y";

			var amendmentSessionalData = entryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.TaxPenaltyCause = "01";
			amendmentSessionalData.DutyPenaltyCause = "02";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			CreateCharge("DTY", 1999999999990m);
			CreateCharge("VAT", 1999999999990m);

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;
			billRef.J3_ReferenceNumber = "01KE0766SS2";

			declaration.UnderbondMovementArrivalDate = new ZDateTime(2013, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2012, 01, 01);

			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_IssueDate = new ZDateTime(2013, 01, 01);
			entryNum1.CE_ExpiryDate = new ZDateTime(2012, 01, 01);
			entryNum1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum1.CE_EntryNum = "4062001070010U";

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_IssueDate = new ZDateTime(2013, 01, 01);
			entryNum2.CE_EntryType = "5UA";
			entryNum2.CE_EntryLineReference = "1";

			var entryNum3 = entry.EntryNumbers.AddNew();
			entryNum3.CE_IssueDate = new ZDateTime(2013, 01, 01);
			entryNum3.CE_EntryType = "5UL";
			entryNum3.CE_EntryNum = "2";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "40620");
			#endregion

			#region entryLine

			var entryLine = entry.MergedLines.AddNew();

			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0208100000";
			entryLine.CL_CustomsValue = 999999999990m;
			entryLine.CL_ValueForVAT = 11m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			CreateFee(entryLine, "DTY", 999999999999m);
			CreateFee(entryLine, "VAT", 999999999999m);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0208122222";
			entryLine2.CL_CustomsValue = 9m;
			entryLine2.CL_ValueForVAT = 11m;
			invoiceLine3.JI_CL = entryLine2.PK;
			CreateFee(entryLine2, "DTY", 999999999999m);
			CreateFee(entryLine2, "VAT", 999999999999m);
			#endregion

			void CreateFee(CusEntryLine entryLine, string chargeType, decimal chargeAmount)
			{
				var fee = entryLine.Fees.AddNew();
				fee.CF_ChargeType = chargeType;
				fee.CF_ChargeAmount = chargeAmount;
			}

			void CreateCharge(string chargeType, decimal chargeAmount)
			{
				var charge = entry.Charges.AddNew();
				charge.C1_ChargeType = chargeType;
				charge.C1_ChargeAmount = chargeAmount;
			}

			return entry;
		}

		public CusEntryHeader GetFTAEntry()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();

			var entryInstruction = factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_FTARelationArticleCode = "4";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_FTASequenceNumber = 1;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.CusEntryLine.CL_LineNumber = 1;
			invoiceLine1.CusEntryLine.CL_FTASequenceNumber = 1;
			invoiceLine1.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine1.JI_COOSupportingDocType = "1";
			invoiceLine1.JI_SequenceNumber = 1;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_FTASequenceNumber = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.CusEntryLine.CL_FTASequenceNumber = 2;
			invoiceLine2.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine2.JI_COOSupportingDocType = "1";
			invoiceLine2.JI_SequenceNumber = 2;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_FTASequenceNumber = 3;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine.PK;
			invoiceLine3.CusEntryLine.CL_FTASequenceNumber = 3;
			invoiceLine3.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine3.JI_COOSupportingDocType = "1";
			invoiceLine3.JI_SequenceNumber = 3;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 4;
			entryLine.CL_FTASequenceNumber = 4;
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_CL = entryLine.PK;
			invoiceLine4.CusEntryLine.CL_FTASequenceNumber = 4;
			invoiceLine4.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine4.JI_COOSupportingDocType = "1";
			invoiceLine4.JI_SequenceNumber = 4;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.CL_FTASequenceNumber = 5;
			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_CL = entryLine.PK;
			invoiceLine5.CusEntryLine.CL_FTASequenceNumber = 5;
			invoiceLine5.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine5.JI_COOSupportingDocType = "1";
			invoiceLine5.JI_SequenceNumber = 5;

			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 6;
			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine.PK;

			return entry;
		}

		public CusEntryHeader GetFTAEntrySnapshot(bool dhr = false)
		{
			var entry = GetFTAEntry();
			var entryLine6 = entry.MergedLines[5];
			entryLine6.CL_FTASequenceNumber = 6;
			var invoiceLine6 = entryLine6.RandomLine;
			invoiceLine6.CusEntryLine.CL_FTASequenceNumber = 6;
			invoiceLine6.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine6.JI_COOSupportingDocType = "1";
			invoiceLine6.JI_SequenceNumber = 6;

			var declaration = entry.Declaration;

			var broker = factory.NewWithValidTestData<OrgHeader>();
			broker.OH_FullName = "상호";
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김환태", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			declaration.Branch.GB_OH_OrgProxy = broker.PK;
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ExportDate = new ZDateTime(2023, 07, 18);

			Stream stream = null;
			if (dhr)
			{
				ImportDHRHeader header = new ImportDHRCreator().Create(entry);
				stream = KRXmlObjectSerializer.Serialize(header);
			}
			else
			{
				ImportFTAHeader header = new ImportFTACreator().Create(entry);
				stream = KRXmlObjectSerializer.Serialize(header);
			}
			var originalMessageType = dhr ? ElectronicDocumentTypeList.Codes._DHR : ElectronicDocumentTypeList.Codes._5SC;
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, originalMessageType, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, originalMessageType);

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = originalMessageType;
			entryNum.CE_EntryLineReference = "2";
			factory.Save();
			return entry;
		}

		public ImportDHRHeader GetImportDHRHeader()
		{
			var entry = GetFTAEntry();
			var header = new ImportDHRCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR);
				factory.Save();
			}

			return header;
		}

		public CusEntryHeader GetEntryWithFullGOVCBR934Data()
		{
			#region OrgHeader
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA2", "레디코리아2");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120", "KR");
			supplier.OH_RL_NKClosestPort = "KRSEL";

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "홍길동", true);
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "서울 강남구 테헤란로 129", "8층", "06133", "129", "020120");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			var payer = factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "Test";
			#endregion

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_PaidBy = "";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_OA_SellerAddress = supplier.MainAddress.PK;
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_AuthorName = "Staff 1";
			declaration.JE_AuthorPhone = "130";
			declaration.JE_AuthorJobTitle = "Manager";

			declaration.JE_AuditorName = "Staff 2";
			declaration.JE_AuditorPhone = "131";
			declaration.JE_AuditorJobTitle = "Leader";

			#region entry
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1235621434585M";
			#endregion

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_CustomsValue = 25987293m;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_CustomsValue = 8599999.99m;
			#endregion

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "899999999";
			invoice1.JZ_InvoiceDate = new ZDateTime(2022, 07, 06);
			invoice1.JZ_ProvPricingYN = YesNoList.Codes.No;
			invoice1.JZ_ProvAdditionalRate = 10.3m;
			invoice1.JZ_ImpContractExpiryDate = new ZDateTime(2022, 12, 31);
			invoice1.JZ_ProvAdditionalAmount = 50000;
			invoice1.JZ_EstimatedDateOfFinalPrice = new ZDateTime(2022, 02, 02);
			invoice1.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice1.JZ_OH_Supplier = importer.PK;

			invoice1.ContractNumber = "Contract No. 347582";
			invoice1.ContractDate = new ZDateTime(2022, 01, 01);

			invoice1.ProvisionalPricingReason101 = true;
			invoice1.ProvisionalPricingReason119 = "기타비용";

			invoice1.PurchaseOrderNumber = "Purchase No. 123456";
			invoice1.PurchaseOrderDate = new ZDateTime(2022, 07, 07);
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			#endregion

			return entry;
		}

		public CusEntryHeader GetDF3Entry()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "030";
			declaration.JE_CustomsDivision = "33";
			declaration.JE_EntryDate = new ZDate(2021, 04, 06);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "03033151234562";

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var stevedorerAddress = orgHeader.Addresses.AddNew();
			stevedorerAddress.OA_Address1 = "Address";
			stevedorerAddress.CompanyName = "군장유업";
			stevedorerAddress.OA_Phone = "0514621354";

			var stevedorer = declaration.DocAddresses.AddNew(stevedorerAddress, DocAddressType.Stevedore);
			stevedorer.E2_OA_Address = stevedorerAddress.PK;

			var person1 = declaration.Persons.AddNew();
			var glbPerson1 = factory.New<GlbPerson>();
			glbPerson1.PER_FullName = "김태경";
			glbPerson1.PER_MobilePhone = "01057855226";
			person1.CPN_PER_Person = glbPerson1.PK;

			var person2 = declaration.Persons.AddNew();
			var glbPerson2 = factory.New<GlbPerson>();
			glbPerson2.PER_FullName = "신태영";
			glbPerson2.PER_MobilePhone = "01046587745";
			person2.CPN_PER_Person = glbPerson2.PK;

			factory.Save();

			return entry;
		}

		readonly BusinessObjectFactory factory;

		public void Create5ULSnapshot(CusEntryHeader entry)
		{
			var entryNumber = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
			if (entryNumber == null)
			{
				entryNumber = entry.EntryNumbers.AddNew();
				entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
				entryNumber.CE_EntryNum = "AAA111";
			}
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var header = new Import5ULCreator().Create(entry, refundDetails);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5UL, stream);
				factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5UL);
			factory.Save();
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Outgoing";

		public JobDeclaration SetLocalExportEntryData(JobDeclaration declaration, string entryType, string entryNum, string messageType, string fileName)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = entryType;
			entry.CH_VersionID = 1;
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNumber.CE_EntryNum = entryNum;
			var message = entry.Messages.AddNew();
			message.EM_MessageType = messageType;
			message.EM_ApplicationReference = "2";
			using (var stream = new MemoryStream(new TestFileReader(typeof(TestDataSetupHelper)).GetEmbeddedFileData(TestFilesPath, fileName)))
			{
				message.SetEM_MessageTextOrDataSource(stream);
				factory.Save();
			}
			return declaration;
		}

		public EDIMessage SetLocalExportMessageData(CusEntryHeader entry, string messageType, string fileName)
		{
			var message = entry.Messages.AddNew();
			message.EM_MessageType = messageType;
			using (var stream = new MemoryStream(new TestFileReader(typeof(TestDataSetupHelper)).GetEmbeddedFileData(TestFilesPath, fileName)))
			{
				message.SetEM_MessageTextOrDataSource(stream);
				factory.Save();
			}
			return message;
		}

		public CusEntryHeader Create929SnapShot(ZString bondedFactoryUseCode)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime("2023-10-26 12:30:00");
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_BondedFactoryArrivalDate = new ZDateTime("2023-10-28 12:30:00");
			instruction.CEI_BondedFactoryUseCode = bondedFactoryUseCode;

			var onlineOrders1 = instruction.OnlineOrders.AddNew();
			onlineOrders1.CY_Order = 1;
			var onlineOrders2 = instruction.OnlineOrders.AddNew();
			onlineOrders2.CY_Order = 2;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "2";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = JobMessageTypeList.Codes.Import;

			var containerLink1 = entry.PivotsToContainers.AddNew();
			containerLink1.CCE_CO_Container = container1.PK;
			containerLink1.CCE_SequenceNumber = 1;
			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;

			entry.Declaration.UnderbondMovementArrivalDate = new ZDateTime("2023-10-27 12:30:00");

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_SequenceNumber = 1;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2_1 = invoice1.InvoiceLines.AddNew();
			invoiceLine2_1.JI_CL = entryLine2.PK;
			invoiceLine2_1.JI_LineNo = 1;
			invoiceLine2_1.JI_SequenceNumber = 2;

			var invoiceLine2_2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2_2.JI_CL = entryLine2.PK;
			invoiceLine2_2.JI_LineNo = 2;
			invoiceLine2_2.JI_SequenceNumber = 3;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var immediateDeliveries1 = entryLine3.ImmediateDeliveries.AddNew();
			immediateDeliveries1.CY_Order = 1;
			var immediateDeliveries2 = entryLine3.ImmediateDeliveries.AddNew();
			immediateDeliveries2.CY_Order = 2;

			var invoiceLine3 = invoice1.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_LineNo = 1;
			invoiceLine3.CreateCertificateOfOriginDataIfRequired();
			invoiceLine3.CertificateOfOriginData.CSI_DateOfIssue = new ZDateTime("2023-10-29 12:30:00");
			invoiceLine3.JI_SequenceNumber = 4;

			var nonGaDetails1 = entryLine3.NonGADetailCollection.AddNew();
			nonGaDetails1.CSI_LineNo = 1;
			var nonGaDetails2 = entryLine3.NonGADetailCollection.AddNew();
			nonGaDetails2.CSI_LineNo = 2;

			var previousExpDecLine1 = entryLine3.PreviousExpDecLineCollection.AddNew();
			previousExpDecLine1.CSI_ReferenceNumber = "1111111";
			previousExpDecLine1.CSI_ReferenceNumber2 = "111";
			previousExpDecLine1.CSI_ItemNumber = 1;
			previousExpDecLine1.CSI_LineNo = 1;
			var previousExpDecLine2 = entryLine3.PreviousExpDecLineCollection.AddNew();
			previousExpDecLine2.CSI_ReferenceNumber = "2222222";
			previousExpDecLine2.CSI_ReferenceNumber2 = "222";
			previousExpDecLine2.CSI_ItemNumber = 2;
			previousExpDecLine2.CSI_LineNo = 2;

			var gAApprovalData1 = invoiceLine3.GAApprovalDataCollection.AddNew();
			gAApprovalData1.CSI_LineNo = 1;
			gAApprovalData1.CSI_DateOfIssue = new ZDateTime("2023-10-30 12:30:00");
			var gAApprovalData2 = invoiceLine3.GAApprovalDataCollection.AddNew();
			gAApprovalData2.CSI_LineNo = 2;
			gAApprovalData2.CSI_DateOfIssue = new ZDateTime("2023-10-30 12:30:00");

			var vat1 = entryLine1.Fees.AddNew();
			vat1.CF_ChargeAmount = 1000m;
			vat1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var vat2 = entryLine2.Fees.AddNew();
			vat2.CF_ChargeAmount = 2000m;
			vat2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var vat3 = entryLine3.Fees.AddNew();
			vat3.CF_ChargeAmount = 3000m;
			vat3.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var vat = entry.Charges.AddNew();
			vat.C1_ChargeAmount = 6000m;
			vat.C1_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				factory.Save();
			}
			return entry;
		}

		public void SetupRefDBDataForDutyReductionAndExemption()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(factory);

			var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var tradeGroup = referenceDataHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Australia, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffTypeDRE = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DutyReductionExemption);
			var rateTypeDRE = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Constants.ZZ.TariffTypes.DutyReductionExemption);
			rateTypeDRE.ZZR_CustomsValueFormula = "DTY";
			var rateCodeDRE = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, Constants.ZZ.TariffTypes.DutyReductionExemption, rateTypeDRE.PK);
			factory.Save();

			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101291000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "경주말");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariff, "CU2", "KG");
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			var rateAdValorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "8");
			referenceDataHelper.CreateCusApplicability(rateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDutyReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A095000101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제95조제1항제1호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyReduction);
			var tariffDutyExemption = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A088000101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제88조제1항제1호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.IsDutyExempt, "Y", tariffDutyExemption);
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyExemption);
			referenceDataHelper.CreateRate(tariffDutyReduction, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			referenceDataHelper.CreateRate(tariffDutyExemption, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "100");
			factory.Save();
		}

		public void SetupRefDBDataForDomesticTax()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var tradeGroup = referenceDataHelper.LoadOrCreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);

			var tariffTypeDRE = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.ZZ.TariffTypes.DutyReductionExemption);
			var rateTypeDRE = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "DRE");
			rateTypeDRE.ZZR_CustomsValueFormula = "DTY";
			var rateCodeDRE = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "DRE", rateTypeDRE.PK);

			var tariffTypeDMT = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			var rateTypeDMT = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeSCT = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "SCT", rateTypeDMT.PK);
			var rateCodeLQT = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "LQT", rateTypeDMT.PK);
			var rateCodeTRT = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "TRT", rateTypeDMT.PK);
			var rateTypeEDT = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "EDT");
			rateTypeEDT.ZZR_CustomsValueFormula = "DMT";
			var rateCodeEDT = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "EDT", rateTypeEDT.PK);

			var tariffTypeDTE = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var rateTypeDTE = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "DTE");
			rateTypeDTE.ZZR_CustomsValueFormula = "DMT";
			var rateCodeDTE = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "DTE", rateTypeDTE.PK);
			factory.Save();

			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "2206001010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "사과주");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "L");
			referenceDataHelper.CreateTariffUOM(tariff, "CU2", "KG");
			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "9003191000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "귀금속을 사용한 것");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU2", "KG");

			var dutyRateAdValorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			var dutyRateAdValorem2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "80");
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDutyReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A095000102", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제95조제1항제2호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyReduction);
			referenceDataHelper.CreateRate(tariffDutyReduction, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");

			var tariffDomesticTax = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "941220-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "과실주");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.LiquorTax, tariffDomesticTax);
			var tariffDomesticTax2 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "412000-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "귀금속제품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariffDomesticTax2);
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxBApplies, "Y", tariffDomesticTax2);
			var tariffDomesticTax3 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "779030-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "수입자동차");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.TransportationTax, tariffDomesticTax3);

			var taxRateLQT = referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeLQT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			referenceDataHelper.CreateCusApplicability(taxRateLQT, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, "A");
			var taxRateSCT = referenceDataHelper.CreateRate(tariffDomesticTax2, rateCodeSCT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "(VFD - 5000000*[U]) * 0.2", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "20");
			referenceDataHelper.CreateCusApplicability(taxRateSCT, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, "A");
			var taxRateTRT = referenceDataHelper.CreateRate(tariffDomesticTax3, rateCodeSCT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "80");
			referenceDataHelper.CreateCusApplicability(taxRateTRT, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, "A");
			referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeEDT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.1", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTax2, rateCodeEDT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping);

			var tariffDomesticTaxReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E109801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제109조 제8항 제1호 물품");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxReduction, "CU1", "U");
			referenceDataHelper.CreateRate(tariffDomesticTaxReduction, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 4000000", dataGrouping: dataGrouping);
			factory.Save();
		}

		public void SetupRefDBDataForDomesticTaxReductionOrExemption()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(factory);

			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);

			var tariffTypeDMT = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			var rateTypeDMT = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeSCT = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "SCT", rateTypeDMT.PK);

			var tariffTypeDTE = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var rateTypeDTE = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, "DTE");
			rateTypeDTE.ZZR_CustomsValueFormula = "DMT";
			var rateCodeDTE = referenceDataHelper.LoadOrCreateNewCusRateCode(factory, "DTE", rateTypeDTE.PK);
			factory.Save();

			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "8704211010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "신차");
			var dutyRateAdValorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.1", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "10");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDomesticTax = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "511100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "자동차 내국세");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariffDomesticTax);
			var taxRateSCT = referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeSCT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.05", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "5");
			referenceDataHelper.CreateCusApplicability(taxRateSCT, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, "A");

			var tariffDomesticTaxExemption = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E106211", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제106조의2제1항제1호 물품");
			var tariffDomesticTaxReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E109801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제109조 제8항 제1호 물품");
			var tariffDomesticTaxInstallationCost = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "L180131", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "개별소비세법 18조 1항 3호가목의 물품");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxReduction, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxInstallationCost, "CU1", "U");
			referenceDataHelper.CreateRate(tariffDomesticTaxExemption, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTaxReduction, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 4000000", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTaxInstallationCost, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 5000000", dataGrouping: dataGrouping);
			factory.Save();
		}

		public void SetupTaxOrFeeData()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			factory.Save();
			helper.CreateTaxOrFee("AGA", 0.2m, dataGrouping, 0m, 0m, "FLA", description: "Agriculture Tax A");
			helper.CreateTaxOrFee("AGB", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "Agriculture Tax B");
			helper.CreateTaxOrFee("VTA", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate A");
			helper.CreateTaxOrFee("VTB", 0m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate B");
			helper.CreateTaxOrFee("VTC", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate C");
			factory.Save();
		}

		public void SetTariffAdditionalCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var categoryType = helper.CreateNewOrGetExistingTariffAdditionalCodeCategory(Core.Constants.CountryCodes.KoreaSouth, "CAT", "Category");
			var subCategoryType = helper.CreateNewOrGetExistingTariffAdditionalCodeCategory(Core.Constants.CountryCodes.KoreaSouth, "SCA", "Sub-Category");
			factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0301929090", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Test1");
			CrateTariffAdditionalCodes(tariff1, "CAT", "01", "", "");
			CrateTariffAdditionalCodes(tariff1, "CAT", "02", "", "");
			CrateTariffAdditionalCodes(tariff1, "CAT", "03", "", "");
			CrateTariffAdditionalCodes(tariff1, "CAT", "04", "", "");

			CrateTariffAdditionalCodes(tariff1, "SCA", "1A", "01", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "1B", "01", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2C", "01", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2D", "01", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2E", "01", "CAT");

			CrateTariffAdditionalCodes(tariff1, "SCA", "1F", "02", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "1G", "02", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2H", "02", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2I", "02", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2J", "02", "CAT");

			CrateTariffAdditionalCodes(tariff1, "SCA", "1K", "03", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "1M", "03", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2L", "03", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2N", "03", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "2O", "03", "CAT");

			CrateTariffAdditionalCodes(tariff1, "SCA", "1P", "04", "CAT");
			CrateTariffAdditionalCodes(tariff1, "SCA", "1Q", "04", "CAT");

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0105949000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Test2");
			CrateTariffAdditionalCodes(tariff2, "CAT", "01", "", "");
			CrateTariffAdditionalCodes(tariff2, "SCA", "1R", "01", "CAT");
			CrateTariffAdditionalCodes(tariff2, "SCA", "2S", "01", "CAT");
			factory.Save();

			void CrateTariffAdditionalCodes(TariffView tariff, string category, string additionalCode, string parentAdditionalCode, string parentCategory)
			{
				var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, category, additionalCode);
				tariffAdditionalCode.ZY2_ParentAdditionalCode = parentAdditionalCode;
				tariffAdditionalCode.ZY2_ZY3_NKParentCategory = parentCategory;
			}
		}

		public CusReconDeclaration CreateRefundDeclarationWithOutgoingMessage(ZString entryNum)
		{
			var refundDeclaration = factory.New<CusReconDeclaration>();
			refundDeclaration.CRD_GS_NKCustomsAgent = "AG";
			refundDeclaration.CRD_ApplicationCode = "KRC";
			refundDeclaration.CRD_JobReferenceNumber = "5UL001";

			var entryNumber = factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = entryNum;
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = refundDeclaration.PK;
			entryNumber.CE_ParentTable = CusReconDeclaration.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;

			var outgoingMessage = factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusReconDeclaration.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = refundDeclaration.PK;
			outgoingMessage.EM_LinkedObject = refundDeclaration;

			return refundDeclaration;
		}

		public AmendmentSessionalData SetUpAmendmentSessionalData(CusEntryHeader entry, ZString messageNum, ZString applicationReference, ZString subType, string fileName = "GOVCBR5FK.xml")
		{
			var message5FE = entry.Messages.AddNew();
			message5FE.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_MessageNum = messageNum;
			message5FE.EM_ApplicationReference = applicationReference;
			message5FE.EM_MessageSubType = subType + "X";
			message5FE.EM_SystemCreateTimeUtc = ZDateTime.Today;

			var message5FK = entry.Messages.AddNew();
			message5FK.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_ApplicationReference = message5FE.EM_MessageNum;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			var testMsgFile = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming." + fileName);
			message5FK.EM_MessageData = testMsgFile;

			var sessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			sessionalData.CSI_Code = subType;
			sessionalData.CSI_LineNo = ZInt.ParseEmptyAsZero(applicationReference);

			return sessionalData;
		}
	}
}
