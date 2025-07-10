using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class MessageTextInterpretatorTest : TestCaseWithFactory
	{
		public void TestImpCIQFields()
		{
			instruction.CEI_CIQRequires = true;
			var interpretation = MessageTextInterpretator.GetInterpretionForCustomsEntry(entryHeader);

			AssertContains("进境关别", interpretation);
			AssertNotContains("出境关别", interpretation);
			AssertContains("进口日期", interpretation);
			AssertNotContains("出口日期", interpretation);
			AssertContains("入境口岸", interpretation);
			AssertNotContains("离境口岸", interpretation);
			AssertContains("启运港", interpretation);

			AssertContains("所需单证", interpretation);
			AssertContains("货物属性", interpretation);
			AssertContains("非危险货物", interpretation);
		}

		public void TestImpNonCIQFields()
		{
			instruction.CEI_CIQRequires = false;
			var interpretation = MessageTextInterpretator.GetInterpretionForCustomsEntry(entryHeader);

			AssertNotContains("所需单证", interpretation);
			AssertNotContains("货物属性", interpretation);
			AssertNotContains("危包规格", interpretation);
		}

		public void TestExpFields()
		{
			declaration.JE_MessageType = "EXP";
			var interpretation = MessageTextInterpretator.GetInterpretionForCustomsEntry(entryHeader);

			AssertContains("出境关别", interpretation);
			AssertNotContains("进境关别", interpretation);
			AssertContains("出口日期", interpretation);
			AssertNotContains("进口日期", interpretation);
			AssertContains("离境口岸", interpretation);
			AssertNotContains("入境口岸", interpretation);
			AssertNotContains("启运港", interpretation);
		}

		[TestDate(2020, 5, 19)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetInterpretationForCustomsEntry()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			instruction.CEI_CIQRequires = true;
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CUSOF", "OFC", "Test Customs Office");
			declaration.JE_CustomsOffice = "OFC";   // CustomsOfficeCode: OFC, CustomsOfficeName: Test Customs Office
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.China.PreEntryNumber, "100000000000000021"); // PreEntryNumber
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.China.DeclarationUnifiedNumber, "000000000000000002", ZDateTime.Today);  // DeclarationUnifiedNumber
			entryHeader.EntryNumber = "ENT00000001";    // EntryNumber
			declaration.JE_OfficeOfEntryExit = "OFC";   // OfficeOfEntryOrExitCode/OfficeOfEntryOrExitName
			instruction.CEI_ManualNo = "MN001";  // ManualNo
			invoiceHeader.ContractNumbers.AddNew().J2_ReferenceNumber = "0123"; // ContractNo
			declaration.JE_ExportDate = new ZDateTime(2020, 05, 18); // ImportOrExportDateString
			declaration.JE_DateOfArrival = new ZDateTime(2020, 05, 19); // ImportOrExportDateString
			entryHeader.SetMovementReferenceNumber("ENT00000001", new ZDateTime(2020, 05, 18)); // DeclarantDateString
			var importer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Importer Company", "ImpCode", "ImporterSocialCrd", "ImporterCIQCode", orgHeaderCode: "CO1").Header;
			declaration.JE_OH_Importer = importer.PK;   // TradeOrgUSCI:ImporterSocialCrd, TradeOrgCCD:ImpCode, TradeOrgCIQ:ImporterCIQCode, TradeOrgName: Importer Company
			var addressDoc = declaration.SupplierDocumentaryAddress;
			addressDoc.E2_AddressOverride = true;
			addressDoc.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			addressDoc.OverseasPartyCode = "DE001";   // OverseasOrgCode
			addressDoc.CompanyName = "OverseasOrg Name";    // OverseasOrgName
			var buyer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Buyer Company", "BuyerCus1", "BuyerSocial1", "BuyerCIQ1", orgHeaderCode: "CO2").Header;
			declaration.BuyerDocAddress.OrganisationPK = buyer.PK;  // {OwnerOrgUSCI BuyerSocial1} {OwnerOrgCCD BuyerCus1} {OwnerOrgCIQ BuyerCIQ1} {OwnerOrgName Buyer Company }
			var agent = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Agent Company", "AgentCus1", "AgentUSCICode", "AgentCIQCode", orgHeaderCode: "CO3").Header;
			declaration.Branch.GB_OH_OrgProxy = agent.PK;   // {AgentOrgUSCI AgentUSCICode}	{AgentOrgCCD AgentCus1}	{AgentOrgCIQ AgentCIQCode}	{AgentOrgName Agent Company}
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea; // TransportModeDesc 水路运输
			declaration.JE_VesselName = "BUNGA DELIMA";    // VesselName
			declaration.JE_VoyageFlightNo = "001Y"; // Voyage
			instruction.BillOfLading = "BILL001";   // BillOfLading
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_ZZZ_NKDataGrouping = "CN";
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_Description = "Procedure AB";
			instruction.CEI_Style = "AB";   // CustomsProcedureDesc:Procedure AB
			instruction.CEI_LevyType = "401";    // LevyTypeDesc: 科教用品
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "2301", "Customs Office DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var requiredDoc = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			requiredDoc.Attributes.AddNew("Import", ZString.Empty);
			requiredDoc.Attributes.AddNew("IsLicense", ZString.Empty);
			Factory.Save();
			invoiceLine.CusSupportingDocuments.AddNew("CD1", "NUM1");   // LicenseNo NUM1
			declaration.JE_RL_NKOrigin = "US";  // CountryOfLoadOrDischargeName 美国
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "USA264", "洛杉矶（美国）");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "PORT", "GBR003", "阿伯丁（英国）");
			var locoMapQuery = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.China);
			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, "CUS");
			locoMapQuery.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, "GBABD");
			var locoMap = Factory.LoadTop1<RefLocoMap>(locoMapQuery);
			if (locoMap == null)
			{
				var loco = Factory.New<RefLocoMap>();
				loco.RY_LocalPortCode = "GBR003";
				loco.RY_RL_NKLocoPort = "GBABD";
				loco.RY_SystemUsage = "CUS";
				Factory.Save(); // For some stupid DAT DBs that do not have correct RefLocoMap Data and therefore cause incorrect CountryOfLoadOrDischargeName.
			}
			else if (locoMap.RY_LocalPortCode != "GBR003")
			{
				locoMap.RY_LocalPortCode = "GBR003";
				Factory.Save(); // For some stupid DAT DBs that do not have correct RefLocoMap Data and therefore cause incorrect CountryOfLoadOrDischargeName.
			}
			declaration.JE_CNLastPortBeforeEntry = "USA264";    // PortOfStopoverName 洛杉矶（美国）
			declaration.JE_CNPortOfOrigin = "GBR003";   // PortOfOriginOrDestCode 阿伯丁（英国）
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;   // IncoTermDesc FOB
			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = "OFT";
			oft.J7_Amount = 4;
			oft.J7_RX_NKCurrency = "CNY";
			oft.J7_IsIncludedInITOT = true;
			oft.J7_IsGSTApplicable = true;
			oft.J7_Calc_IsIncludedInInvoiceAmount = true;
			oft.J7_DistributeBy = "VAL";    // {FreightFeeCurrencyCode CNY}/{FreightFeeAmount 4}/{FreightFeeMarkDesc 总价}
			var ons = invoiceHeader.Charges.AddNew();
			ons.J7_ChargeType = "ONS";
			ons.J7_Amount = 5;
			ons.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			ons.J7_IsIncludedInITOT = false;
			ons.J7_IsGSTApplicable = true;
			ons.J7_DistributeBy = "VAL";    // {InsuranceFeeCurrencyCode CNY}/{InsuranceFeeAmount 5}/{InsuranceFeeMarkDesc 总价}
			var ryt = invoiceHeader.Charges.AddNew();
			ryt.J7_ChargeType = "RYT";
			ryt.J7_Amount = 1;
			ryt.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			ryt.J7_DistributeBy = "VAL";    // {OtherFeeCurrencyCode CNY}/{OtherFeeAmount 1}/{OtherFeeMarkDesc 总价}
			instruction.CEI_Packages = 5;    // NoOfPacks
			instruction.CEI_PackageUQ = PackageType.Codes.Bag;    // PackTypeDesc
			instruction.OtherPackages.AddNew("00"); // OtherPackageDescs 散装
			invoiceLine.JI_NetWeight = 1.1M;
			invoiceLine.JI_NetWeightUQ = "KG";  // GrossWeightInKG 1.1
			invoiceLine.JI_Weight = 3.3M;
			invoiceLine.JI_WeightUQ = "KG"; // NetWeightInKG 3.3
			invoiceLine.FormulaPricingRecordNumber = "012021000001";
			var orgUS = Factory.New<OrgHeader>();
			orgUS.OH_Code = "COU";
			orgUS.MainAddress.OA_RN_NKCountryCode = "US";
			declaration.JE_OH_Supplier = orgUS.PK;  // CountryOfTradeName 美国
			var container = declaration.CusContainers.AddNew(); // NumberOfContainers 1
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "01", "1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			Factory.Save();
			code1.Attributes.AddNew("DisplayCode", "1");
			code1.Attributes.AddNew("Import", "");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "02", "2");
			Factory.Save();
			code2.Attributes.AddNew("DisplayCode", "2");
			code2.Attributes.AddNew("Export", "");
			Factory.Save();
			invoiceLine.CusSupportingDocuments.AddNew("01", "SUPDOC01").CSI_LineNo = 2;    // SupportingDocumentCodes 1
			invoiceLine.CusSupportingDocuments.AddNew("02", "SUPDOC02");    // SupportingDocumentCodes 2
																																			// {SupportingDocuments.DocumentType:1 } {SupportingDocuments.DocumentNumber:SUPDOC01} {SupportingDocuments.ItemNumber:2}
																																			// CL_LineNumber:1
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQPO", "211906", "旅顺新港");
			declaration.JE_CIQOfficeOfEntryExit = "211906"; // CIQOfficeOfEntryOrExitName 旅顺新港
			declaration.JE_LocationOfGoods = "货物存放地点1";  // LocationOfGoods 货物存放地点1
			instruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms; // DocumentSubmissionTypeDesc 通关无纸化
			instruction.CustomsMessageRemarks = "CustomsMessageRemarks";    // Remarks CustomsMessageRemarks
			declaration.JE_MarksAndNumbers = "Marks And Numbers";  // MarksAndNumbers: Marks And Numbers
			invoiceHeader.JZ_SpecialRelationshipConfirm = "0";  // SpecialRelationshipConfirmDesc 否
			invoiceHeader.JZ_PriceAffectConfirm = "1";  // PriceAffectConfirmDesc 是
			invoiceHeader.JZ_PaymentOfRoyaltyConfirm = "9"; // PaymentOfRoyaltyConfirmDesc 空
			instruction.CEI_RelatedMRN = "RELMRN";   // RelatedEntryNumber
			instruction.CEI_RelatedManualNo = "RELMN"; // RelatedManualNumber
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseCPW = warehouse.CustomsCodes.AddNew();
			warehouseCPW.OK_RN_NKCodeCountry = "CN";
			warehouseCPW.OK_CodeType = "CPW";
			warehouseCPW.OK_CustomsRegNo = "CPW1000001";
			warehouseCPW.OK_OA_PremisesAddress = warehouse.Addresses.First().PK;
			var warehouseContact = Factory.NewWithValidTestData<OrgContact>();
			warehouseContact.OC_OH = warehouse.PK;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			declaration.WarehouseDocAddress.ContactPK = warehouseContact.PK;    // BondedAreaCode CPW1000001
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var depotCPD = depot.CustomsCodes.AddNew();
			depotCPD.OK_RN_NKCodeCountry = "CN";
			depotCPD.OK_CodeType = "CPD";
			depotCPD.OK_CustomsRegNo = "CPD1000002";
			depotCPD.OK_OA_PremisesAddress = depot.Addresses.First().PK;
			var depotContact = Factory.NewWithValidTestData<OrgContact>();
			depotContact.OC_OH = depot.PK;
			declaration.DepotDocAddress.OrganisationPK = depot.PK;
			declaration.DepotDocAddress.ContactPK = depotContact.PK;    // FreightYardCode CPD1000002
			var eq1 = instruction.EnterpriseQualifications.AddNew();
			eq1.CY_Code = "100";
			eq1.CY_Data = "001";// EnterpriseQualificationsAsString 100:001
			declaration.JE_DateAtOrigin = new ZDateTime(2020, 05, 17);  // DepartureDateString
			declaration.JE_MasterBill = "Ocean Bill 1"; // BillNumber
			instruction.BillOfLading = "BILL001";
			declaration.OfficeOfDestination = "2301";  // DestinationCIQOfficeName: Customs Office DES
			instruction.CEI_CIQRelatedNum = "CIQRELNUM"; // CEI_CIQRelatedNum
			instruction.CEI_CIQRelatedReason = "1"; // RelatedReason 通关单超过有效期
			var buyerContact = buyer.Contacts.AddNew();
			buyerContact.OC_ContactName = "Buyer Contact";
			buyerContact.OC_Phone = "+86-156-0113-1981";
			declaration.BuyerDocAddress.ContactPK = buyerContact.PK;    // ConsumerContactName:Buyer Contact, ConsumerContactPhone:156 0113 1981
			declaration.JE_DateOfUnloadComplete = new ZDateTime(2020, 5, 16);   // JobDeclaration.JE_DateOfUnloadComplete
			invoiceLine.JI_OrigContainerFlag = ConfirmationTypeList.Codes.Yes;    // IsOriginalContainerLoading 是
			var sb1 = Factory.New<SpecialBusinessIdentifier>();
			sb1.CY_ParentTableCode = instruction.TablePrefix;
			sb1.CY_Code = "B01";
			sb1.CY_Type = "SBI";
			sb1.CY_ParentID = instruction.PK;   // SpecialBusinessIdentifiersAsString 国际赛事
			var rd1 = instruction.CIQRequiredDocuments.AddNew();
			rd1.XC_DocumentType = "11";
			rd1.XC_NumberOfOriginals = 1;
			rd1.XC_NumberOfCopies = 2;  // RequiredDocumentsAsString: 11:1/2
			container.CO_ContainerNumber = "CNT001";    // ContainerNumber CNT001
			var containerRef = Factory.New<RefContainer>();
			containerRef.RC_Code = "20PP";
			var containerMap = Factory.New<RefContainerCodeMap>();
			containerMap.RCM_RC_Container = containerRef.PK;
			containerMap.RCM_RN_NKCountry = "CN";
			containerMap.RCM_Code = "31";
			container.CO_RC = containerRef.PK;  // ContainerCodeDescription:其他标准箱（S）
			var jobContainer = Factory.NewWithValidTestData<CommonContainer>();
			container.CO_JC = jobContainer.PK;
			jobContainer.JC_RC = containerRef.PK;
			jobContainer.JC_GrossWeightUQ = "KG";
			containerRef.RC_TareWeight = 123456.457m;   // TareWeightInKG:123456.457
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;   // IsLessContainerDesc 是
																																			// LinkedEntryLineNosAsString 1
			invoiceLine.JI_ProductManualNo = 2; // ProductManualNo:2
			var tariff = helper.CreateCustomsTariff("2713200000", "00000", "99997", "00423", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("99997", "包装规格");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff).PK;
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "2009891200101", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "未混合芒果汁");
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "035", "千克", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "002", "座", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "003", "辆", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateTariffUOM(tariff.PK, "CU1", "002");  // CustomsUnitQtyDescription 座
			helper.CreateTariffUOM(tariff.PK, "CU2", "003");  // CustomsSecondUnitDesc 辆
			Factory.Save();

			invoiceLine.JI_Tariff = "2713200000";   // TariffCode
			invoiceLine.JI_CIQTariff = "2009891200101"; // CIQSupplementCode 101, CIQTariffDescription 未混合芒果汁
			invoiceLine.JI_NameOfGoods = "产品A"; // NameOfGoods
			invoiceLine.XC_GoodsSpecModel = "1千克/箱|XXXXX|无其他";  // GoodsSpecModel:1千克/箱|XXXXX|无其他
			invoiceLine.JI_TradeUnitQty = "035";    // TradeUnitQtyDesc 千克
			invoiceLine.JI_TradeQuantity = 1; // TradeQuantity
			invoiceLine.JI_LinePrice = 50.01m;  // UnitPrice 46.01m
																					// TotalPrice 46.01m
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CURR", "CNY", "人民币");
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CNY"; // CurrencyDesc 人民币
			invoiceLine.JI_CustomsQuantity = 12m;   // CustomsQuantity
			invoiceLine.JI_ProductVersion = "PRODVER";  // ProductVersion
			invoiceLine.JI_PartNo = "PROCUDTCD"; // ProductCode
			invoiceLine.JI_RN_NKCountryOfExport = "GB"; // GoodsDestName 英国
			invoiceLine.JI_CustomsSecondQuantity = 2m;  // CustomsSecondQuantity
			invoiceLine.JI_CountryOfOrigin = "US"; // GoodsOriginName 美国
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, "840004", "阿拉斯加（美国）");
			invoiceLine.JI_CIQOriginState = "840004";   // OriginStateName 阿拉斯加（美国）
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "DISTR", "NJ", "南京");
			invoiceLine.JI_DestinationDistrict = "NJ";  // DomesticDistrictName 南京
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CIQDT", "101011", "Region1");
			invoiceLine.JI_DestinationRegion = "101011";    // DomesticRegionName Region1
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;    // DutyModeDesc 照章征税
			instruction.CEI_CIQRequires = true;
			invoiceLine.CIQIngredient = "Ingredient1";  // Ingredient Ingredient1
			invoiceLine.JI_CIQExpiryDate = new ZDateTime(2020, 12, 2);  // ExpiryDateAsString 20191202
			invoiceLine.JI_CIQQualityGuaranteePeriod = 20;  // QGPByDays 20
			invoiceLine.JI_NDescription = "Spec1";  // Specification Spec1
			invoiceLine.JI_Model = "CIQ Model 1";   // Model: CIQ Model 1
			invoiceLine.JI_BrandName = "CIQ Brand 1";   // Brand: CIQ Brand 1
			var manufacturer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Manufacturer 1", "CU1", "SO1", "CIQ01", orgHeaderCode: "CO4"); // ManufacturerCIQNum CIQ01, ManufacturerName Manufacturer 1
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.PK;
			var pb11 = invoiceLine.ProductionBatch.AddNew();
			pb11.CY_Data = "PB1";   // BatchNumber: PB1
			pb11.CY_Date = new ZDateTime(2019, 10, 1);  // ManufactureDate: 20191001
			var ca11 = invoiceLine.CargoAttributes.AddNew();
			ca11.CY_Code = "11";
			ca11.CY_Type = "CAT";   // CargoAttribute 3C目录内
			invoiceLine.JI_CIQEndUse = "19";    // EndUse 食品包装材料
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "0000";
			substance.DG_Variant = "A";
			substance.DG_PSN = "I am very dangerous";
			substance.DG_FlashPoint = "-4 cc";
			substance.DG_PG = "III";
			substance.DG_Code = "000A";
			substance.DG_Class = "8";
			substance.DG_SubLabel1 = "3";
			substance.DG_SubLabel2 = "6.1";
			invoiceLine.JI_NonDangerousChemicalFlag = false;
			invoiceLine.JI_PackageTypeOfUNDG = "1D";
			var undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = substance.PK;
			var chsName = undg.Substance.Names.AddNew();
			chsName.DA_Language = Core.Constants.Languages.ChineseSimplified;
			chsName.DA_Descriptor = "Chinese 1";// UNDGName: Chinese 1
																					// NonDangerousChemical 否
																					// UNDGNumber 0000A
																					// UNDGPackingTypeDescription 胶合板圆桶
																					// UNDGPackingGroup III
			var pq1 = invoiceLine.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "106";
			pq1.CSI_ReferenceNumber = "001";
			pq1.CSI_LineNo = 1;
			pq1.CSI_Quantity = 3;
			pq1.CSI_UnitOfQuantity = "010"; // ProductQualificationsAsString "106:001/1/3 010"

			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CNPTA", "AU1", "CN-AU free trade agreement 1", ("ApplicableCountry", "AU"));
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CNPTA", "GB1", "CN-GB free trade agreement 1", ("ApplicableCountry", "GB"));
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.CertificateOfOriginType = "C";
			invoiceLine.CertificateOfOrigin = "C12345678";
			invoiceLine.TradeAgreementCode = "AU1";
			invoiceLine.ItemNoOnCertOfOrigin = 2;
			invoiceLine.CertificateOfOriginCountry = "AU";

			CombineAssertions(() =>
			{
				var interpretation = MessageTextInterpretator.GetInterpretionForCustomsEntry(entryHeader);
				using (var expectedStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.Business.Testing.MessageBuilders.MessageTextInterpretatorTestFiles.ImportEntry.html"))
				{
					AssertMultilineASCIIEquals("Import Entry Form", expectedStream.ConvertToUTF8StringAndCloseStream(), interpretation);
				}

				declaration.JE_MessageType = "EXP";
				declaration.JE_CNPortOfDestination = "GBR003";   // PortOfOriginOrDestCode 阿伯丁（英国）
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				invoiceLine.TradeAgreementCode = "GB1";
				declaration.DoMerge();
				interpretation = MessageTextInterpretator.GetInterpretionForCustomsEntry(entryHeader);

				using (var expectedStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.Business.Testing.MessageBuilders.MessageTextInterpretatorTestFiles.ExportEntry.html"))
				{
					AssertMultilineASCIIEquals("Export Entry Form", expectedStream.ConvertToUTF8StringAndCloseStream(), interpretation);
				}
			});
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			instruction = declaration.CustomsEntryInstructions.AddNew();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			entryHeader = declaration.CustomsEntryHeaders[0];
		}
	}
}
