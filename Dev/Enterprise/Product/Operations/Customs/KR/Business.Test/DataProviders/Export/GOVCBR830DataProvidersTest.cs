using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR830DataProvidersTest : XMLMessageTestHelper<GOVCBR830DataProvidersTest>
	{
		JobDeclaration CreateEmptyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		CusEntryHeader SetUpRealData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "04077012", "서브", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";

			RefUNLOCO originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "AAAAA";
			originUNLOCO.RL_IATA = "ICN";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			new TestDataSetupHelper(Factory).SetUpExportEntryTariffData(company);

			#region OrgHeader
			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "인천효민관세사무소");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김지한", true);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			branch.GB_OH_OrgProxy = broker.PK;

			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
			var sellerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "씨앤엘뮤1941018" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
			supplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "최석구이태윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "2148167176" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "씨앤엘뮤1941018" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "(주)씨앤엘뮤직");
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "", "", "06636");
			var manufacturerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "씨앤엘뮤1941018" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);
			var manufacturerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "999" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer.MainAddress, manufacturerAddressCodes);

			var buyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "MUSIC PLAZA");
			var buyerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "USMUSICP0001Y" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(buyer, buyerCodes);

			var terminalOperator = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "");
			var terminalOperatorCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = OrgCusCode.CodeTypes.ControlledPremisesID, Number = "04077012", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(terminalOperator.MainAddress, terminalOperatorCodes);

			var forwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "");
			TestOrgDataSetUpHelper.AddOrgContact(forwarder, "", true);
			#endregion

			#region declaration
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.DocAddresses.AddNew(terminalOperator.MainAddress, MasterFiles.Integration.DocAddressType.CustomsContainerTerminalOperatorAddress);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator.MainAddress.PK;
			declaration.JE_ExportGoodsType = "11";
			declaration.JE_MessageSubType = "B";
			declaration.JE_CustomsOffice = "040";
			declaration.JE_CustomsDivision = "15";
			declaration.JE_GoodsDestination = "US";
			declaration.JE_ContainerPackMode = "ETC";
			declaration.JE_TradeIDWithKP = "";
			declaration.JE_ExporterType = "A";
			declaration.JE_ReturnReason = "";
			declaration.JE_ReturnType = "";
			declaration.JE_GoodsCondition = "N";
			declaration.JE_SimpleDRWApp = "NO";
			declaration.JE_TradeIndicatorWithKP = "";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_UCR = "";
			declaration.JE_CarrierCode = "";
			declaration.JE_VesselName = "";
			declaration.JE_VoyageFlightNo = "";
			declaration.JE_TotalNoOfPacksPackType = "CT";
			declaration.JE_GB = branch.PK;
			declaration.JE_LocationQualifier = "22379";
			declaration.JE_LocationOfGoods = "인천 중구 공항동로296번길 98-114";
			declaration.JE_LocationOtherInformation = "04077012";
			declaration.JE_RL_NKOrigin = originUNLOCO.RL_Code;
			declaration.JE_ProcedureType = "H";
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			#endregion

			#region invoice
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Remarks = @"선적기간 : 2021-01-13 - 2021-02-12";
			invoice.JZ_DRWApplicantType = "2";
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.JZ_PaymentTerms = "TT";
			invoice.JZ_LetterOfCreditNumber = "";
			invoice.JZ_IncoTerm = "EXW";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceAmount = 24761.7m;
			invoice.JZ_InvoiceCurrExRate = 1075.85m;
			invoice.JZ_InvoiceNumber = "EC21-01024-MP";
			invoice.JZ_NoOfPacks = 98;

			invoice.JZ_Weight = 1468m;
			invoice.JZ_WeightUQ = "KG";
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 0m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			#endregion

			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "8523491020";
			invoiceLine.JI_Model = "CD";
			invoiceLine.JI_BrandName = "";
			invoiceLine.JI_CountryOfOrigin = "KR";
			invoiceLine.JI_COOLabelLocation = "";
			invoiceLine.JI_PreviousEntryNumber = "";
			invoiceLine.JI_NetWeight = 1400m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_Description = "CDCOMPACT DISC85234039";
			invoiceLine.JI_Ingredient = "";
			invoiceLine.JI_LotNumber = "";
			invoiceLine.JI_InvoiceUQ = "PC";
			invoiceLine.JI_InvoiceQuantity = 2044m;
			invoiceLine.JI_CustomsUnitQty = "CT";
			invoiceLine.JI_CustomsQuantity = 1235m;
			invoiceLine.UnitPrice = 11.967798m;
			invoiceLine.JI_LinePrice = 24462.18m;
			invoiceLine.JI_PackType = "CT";
			invoiceLine.JI_NoOfPacks = 97;

			invoiceLine.CertificateOfOriginIssueStatus = "Y";
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_RN_NKCountryCode = "KR";
			certificate.CSI_LineNo = 1;
			#endregion

			#region invoiceLine2
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_Tariff = "4910001000";
			invoiceLine2.JI_Model = "CALENDAR";
			invoiceLine2.JI_BrandName = "";
			invoiceLine2.JI_CountryOfOrigin = "KR";
			invoiceLine2.JI_COOLabelLocation = "";
			invoiceLine2.JI_PreviousEntryNumber = "";
			invoiceLine2.JI_NetWeight = 20m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_Description = "MDCALENDAR";
			invoiceLine2.JI_Ingredient = "";
			invoiceLine2.JI_LotNumber = "";
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "CT";
			invoiceLine2.JI_CustomsQuantity = 5678m;
			invoiceLine2.UnitPrice = 29.952m;
			invoiceLine2.JI_LinePrice = 299.52m;
			invoiceLine2.JI_PackType = "CT";
			invoiceLine2.JI_NoOfPacks = 1;
			#endregion

			#region entry
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "EXP";
			entry.EntryNumber = "1163921400096X";

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = ZDateTime.Empty;
			entryNum.CE_ExpiryDate = ZDateTime.Empty;
			entryNum.CE_EntryNum = "";
			entryNum.CE_EntryType = "UDM";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "11639");
			#endregion

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8523491020";
			entryLine.CL_CustomsValue = 26317636m;
			invoiceLine.JI_CL = entryLine.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "4910001000";
			entryLine2.CL_CustomsValue = 322238m;
			invoiceLine2.JI_CL = entryLine2.PK;
			#endregion

			return entry;
		}

		public void TestExportEntryHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			entry.InvoiceLines.ToList()[0].Charges[0].J7_Amount += 0.01m;
			entry.InvoiceLines.ToList()[0].Charges[1].J7_Amount += 0.01m;
			entry.InvoiceHeaders()[0].JZ_InvoiceAmount += 0.001m;
			entry.InvoiceHeaders()[0].JZ_Weight += 0.0001m;
			entry.InvoiceHeaders()[0].JZ_InvoiceCurrExRate += 0.00001m;
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);

			AssertEquals("6N00221000025X", entryHeader.ExportDeclarationNumber);
			AssertEquals("11", entryHeader.TransactionType);
			AssertEquals("B", entryHeader.ExportTypeCode);
			AssertEquals("130", entryHeader.DeclarationCustomsOffice);
			AssertEquals("10", entryHeader.DeclarationCustomsDivision);
			AssertEquals("HK", entryHeader.CountryOfDestination);
			AssertEquals("KRINC", entryHeader.PortOfLoading);
			AssertEquals("08589", entryHeader.GoodsLocationPostcode);
			AssertEquals("서울 금천구 가산디지털1로 119", entryHeader.GoodsLocationAddress);
			AssertEquals("서브", entryHeader.GoodsLocationAdditionalDetails);
			AssertEquals("02", entryHeader.SouthNorthTradeIdentification);
			AssertEquals("55555", entryHeader.FinalLoadingPlace);
			AssertEquals("99999999", entryHeader.GoodsLocationBondedAreaCode);
			AssertEquals(new ZDateTime(2021, 03, 08), entryHeader.PreferredInspectionDate);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.BondedTransportationFromDate);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.BondedTransportationToDate);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.DepartureDate);
			AssertEquals("1", entryHeader.DrawbackApplicantType);
			AssertEquals("H", entryHeader.DeclarationProcedureType);
			AssertEquals("TT", entryHeader.InvoicePaymentTerm);
			AssertEquals("C", entryHeader.ExporterType);
			AssertEquals("N", entryHeader.OutOfHoursDeclarationIndicator);
			AssertEquals("ZZ", entryHeader.ReturnReason);
			AssertEquals("A", entryHeader.ReturnType);
			AssertEquals("O", entryHeader.GoodsStatus);
			AssertEquals("NO", entryHeader.ApplicationForSimpleDrawback);
			AssertEquals(true, entryHeader.ContainerizedIndicator);
			AssertEquals("N", entryHeader.SouthNorthTradeYN);
			AssertEquals("AA", entryHeader.ContainerPackMode);
			AssertEquals("1234567", entryHeader.LCNo);
			AssertEquals("16HJSC0686I00080001", entryHeader.CargoManagement.ImportCargoManagementNumber);
			AssertEquals("10", entryHeader.TransportMode);
			AssertEquals("99999999999999999", entryHeader.UCR);
			AssertEquals("9999999999", entryHeader.LocationIDInBondedArea);
			AssertEquals("6N002", entryHeader.UnipassDeclarantID);
			AssertEquals("나대표", entryHeader.FreightForwarderContactName);
			AssertEquals("KE", entryHeader.CarrierID);
			AssertEquals("KOREAN AIR", entryHeader.ShippingLineOrAirlineName);
			AssertEquals("AA9999", entryHeader.VesselNameOrFlightNo);
			AssertEquals(34587292.99m, entryHeader.TotalCustomsValue);
			AssertEquals(899999999m, entryHeader.Freight);
			AssertEquals(799999999m, entryHeader.Insurance);
			AssertEquals("CFR", entryHeader.Incoterm);
			AssertEquals("USD", entryHeader.Currency);
			AssertEquals(27670.001m, entryHeader.TotalInvoiceAmount);
			AssertEquals(1300.75m, entryHeader.ExchangeRate);
			AssertEquals("신고인 기재란", entryHeader.DeclarantAdditionalDescription);
			AssertEquals(3m, entryHeader.TotalPackQty);
			AssertEquals("OU", entryHeader.PackType);
			AssertEquals(29600m, entryHeader.TotalGrossWeightInKG);

			#region Organisation
			var declarant = entryHeader.Declarant;
			AssertEquals("레디코리아", declarant.CompanyName);
			AssertEquals("김환태", declarant.RepresentativeName);

			var exporter = entryHeader.Exporter;
			AssertEquals("레디코리아", exporter.CompanyName);
			AssertEquals("레디코리1971018", exporter.UnipassIDForOrganization);
			AssertEquals("00000", exporter.OfficeID);

			var manufacturer = entryHeader.Manufacturer;
			AssertEquals("레디코리아", manufacturer.CompanyName);
			AssertEquals("04784", manufacturer.Postcode);
			AssertEquals("레디코리아999000", manufacturer.UnipassIDForOrganization);
			AssertEquals("00001", manufacturer.OfficeID);
			AssertEquals("888", entryHeader.IndustrialParkCode);

			var importer = entryHeader.Importer;
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", importer.CompanyName);
			AssertEquals("HKBOARAM0001A", importer.ForeignCompanyID);
			#endregion
		}

		public void TestTransportMode()
		{
			var declaration = CreateEmptyDeclaration();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entry = declaration.CustomsEntryHeaders[0];

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Air, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.FixedTransportInstallations, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.PassengerHandCarried;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Pedestrian;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Auto;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Road, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.BorderWaterBorne;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.WarehouseHandling;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.All;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Unknown;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Courier;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Mail, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Rail, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Road, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.SeaAir;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Complex, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.AirSea;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Complex, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Sea, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.InlandWaterwayTransport, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.RollOnRollOff;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);
		}
		public void TestFinalLoadingPlace()
		{
			var declaration = CreateEmptyDeclaration();
			declaration.JE_CustomsOffice = "130";
			declaration.JE_CustomsDivision = "10";
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			var entry = declaration.CustomsEntryHeaders[0];

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("13099999", entryHeader.FinalLoadingPlace);

			var terminalOperator1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "");
			TestOrgDataSetUpHelper.AddOrgAddress(terminalOperator1.MainAddress, "", "", "");
			var terminalOperatorCodes1 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = "TES", Number = "77777", CountryOfIssue = "KR" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(terminalOperator1.MainAddress, terminalOperatorCodes1);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator1.MainAddress.PK;

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("13099999", entryHeader.FinalLoadingPlace);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("", entryHeader.FinalLoadingPlace);

			var terminalOperator2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BU2", "READYKOREA2", "");
			TestOrgDataSetUpHelper.AddOrgAddress(terminalOperator2.MainAddress, "", "", "");
			var terminalOperatorCodes2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = OrgCusCode.CodeTypes.ControlledPremisesID, Number = "66666", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(terminalOperator2.MainAddress, terminalOperatorCodes2);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator2.MainAddress.PK;

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("66666", entryHeader.FinalLoadingPlace);
		}
		public void TestSupplier()
		{
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			supplier.OH_IsConsignor = true;
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "김택윤", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplier.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var supplierCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
				new IDNumberAndType() { Type = IdentificationType.PassportNo, Number = "YC00158522354" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, supplierCodes);
			var supplierAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier.MainAddress, supplierAddressCodes);

			var declaration = CreateEmptyDeclaration();
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			var entry = declaration.CustomsEntryHeaders[0];
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);

			AssertEquals("레디코리아", entryHeader.Supplier.CompanyName);
			AssertEquals("김택윤", entryHeader.Supplier.RepresentativeName);
			AssertEquals("서울특별시 서초구 동광로 41", entryHeader.Supplier.AddressLine1);
			AssertEquals("레디인빌딩", entryHeader.Supplier.AddressLine2);
			AssertEquals("06561", entryHeader.Supplier.Postcode);
			AssertEquals("101010", entryHeader.Supplier.RoadNameCode);
			AssertEquals("020120", entryHeader.Supplier.BuildingNumber);
			AssertEquals(false, entryHeader.Supplier.IsIndividual);

			AssertEquals("레디코리1971018", entryHeader.Supplier.UnipassIDForOrganization);
			AssertEquals("00001", entryHeader.Supplier.OfficeID);
			AssertEquals("020120", entryHeader.Supplier.BuildingNumber);
			AssertEquals("101010", entryHeader.Supplier.RoadNameCode);
			AssertEquals("1028142299", entryHeader.Supplier.BusinessRegNo);

			supplier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);

			AssertEquals(true, entryHeader.Supplier.IsIndividual);
			AssertEquals("레디코리1971018", entryHeader.Supplier.UnipassIDForOrganization);
			AssertEquals("00001", entryHeader.Supplier.OfficeID);
			AssertEquals("020120", entryHeader.Supplier.BuildingNumber);
			AssertEquals("101010", entryHeader.Supplier.RoadNameCode);
			AssertEquals("YC00158522354", entryHeader.Supplier.PassportNo);

			var supplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			supplier2.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			TestOrgDataSetUpHelper.AddOrgAddress(supplier2.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var supplierCodes2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리1971018" },
				new IDNumberAndType() { Type = IdentificationType.PassportNo, Number = "YC00158522354" },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "1234561243567" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2, supplierCodes2);
			var supplierAddressCodes2 = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "00001"  }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2.MainAddress, supplierAddressCodes2);
			declaration.JE_OA_SupplierAddress = supplier2.MainAddress.PK;

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(true, entryHeader.Supplier.IsIndividual);

			AssertEquals("레디코리1971018", entryHeader.Supplier.UnipassIDForOrganization);
			AssertEquals("00001", entryHeader.Supplier.OfficeID);
			AssertEquals("020120", entryHeader.Supplier.BuildingNumber);
			AssertEquals("101010", entryHeader.Supplier.RoadNameCode);
			AssertEquals("1234561243567", entryHeader.Supplier.KoreanRegNoForResident);
		}
		public void TestExportContainer()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			var containers = entryHeader.Containers;
			AssertNotNull(containers);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "999999999999999";

			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 2;
			containerLink.CCE_Status = "";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "899999999999999";

			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 1;
			containerLink2.CCE_Status = "";

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			containers = entryHeader.Containers;

			AssertEquals("Should be ordered by SequenceNo", "01", containers[0].SequenceNo);
			AssertEquals("899999999999999", containers[0].ContainerNo);
			AssertEquals("Should be ordered by SequenceNo", "02", containers[1].SequenceNo);
			AssertEquals("999999999999999", containers[1].ContainerNo);
		}

		public CusEntryHeader SetUpEntryLineData()
		{
			new TestDataSetupHelper(Factory).SetUpExportEntryTariffData(GlbCompany.CurrentCompany);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "OU";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "999999999";

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_AdValoremTariff = "8429521022";
			entryLine.CL_CustomsValue = 25987293m;

			#region invoiceLine11
			var invoiceLine11 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine11.JI_CL = entryLine.PK;
			invoiceLine11.JI_Tariff = "8429521022";
			invoiceLine11.JI_SequenceNumber = 1;
			invoiceLine11.JI_Model = "HYUNDAI ROBEX3000LC-7A";
			invoiceLine11.JI_BrandName = "상표명";
			invoiceLine11.JI_CountryOfOrigin = "KR";
			invoiceLine11.JI_COOLabelLocation = "N";
			invoiceLine11.JI_PreviousEntryNumber = "A";
			invoiceLine11.JI_PreviousEntryLineNumber = 1;
			invoiceLine11.JI_SkipManifestReport = "N";
			invoiceLine11.JI_CustomsUnitQty = "CT";
			invoiceLine11.JI_CustomsQuantity = 35m;
			invoiceLine11.JI_NetWeight = 360m;
			invoiceLine11.JI_NetWeightUQ = "KG";
			invoiceLine11.JI_NoOfPacks = 3;
			invoiceLine11.JI_PackType = "CT";
			invoiceLine11.JI_PrimaryPreference = "105";

			invoiceLine11.CertificateOfOriginIssueStatus = "Y";
			invoiceLine11.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate1 = invoiceLine11.CertificateOfOriginData;
			certificate1.CSI_RN_NKCountryCode = "KR";
			certificate1.CSI_LineNo = 1;
			invoiceLine11.PRA_ReferenceNumber = "KR00101010101";
			invoiceLine11.PRA_DateOfIssue = new ZDateTime(2018, 01, 01);
			invoiceLine11.PRA_DateOfExpiry = new ZDateTime(2018, 09, 01);
			#endregion

			#region invoiceLine12
			var invoiceLine12 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine12.JI_CL = entryLine.PK;
			invoiceLine12.JI_Tariff = "8429521022";
			invoiceLine12.JI_SequenceNumber = 1;
			invoiceLine12.JI_Model = "HYUNDAI ROBEX3000LC-7A";
			invoiceLine12.JI_BrandName = "상표명";
			invoiceLine12.JI_CountryOfOrigin = "KR";
			invoiceLine12.JI_COOLabelLocation = "N";
			invoiceLine12.JI_PreviousEntryNumber = "A";
			invoiceLine12.JI_PreviousEntryLineNumber = 1;
			invoiceLine12.JI_SkipManifestReport = "N";
			invoiceLine12.JI_CustomsUnitQty = "CT";
			invoiceLine12.JI_CustomsQuantity = 15m;
			invoiceLine12.JI_NetWeight = 140m;
			invoiceLine12.JI_NetWeightUQ = "KG";
			invoiceLine12.JI_PackType = "CT";
			invoiceLine12.JI_NoOfPacks = 4;
			invoiceLine12.JI_PrimaryPreference = "105";
			#endregion
			#endregion
			#region entryLine2
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			entryLine2.CL_AdValoremTariff = "4910001000";
			entryLine2.CL_CustomsValue = 12345.99m;

			#region invoiceLine21
			var invoiceLine21 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine21.JI_CL = entryLine2.PK;
			invoiceLine21.JI_Tariff = "4910001000";
			invoiceLine21.JI_SequenceNumber = 1;
			invoiceLine21.JI_Model = "거래품명1";
			invoiceLine21.JI_BrandName = "상표명1";
			invoiceLine21.JI_CountryOfOrigin = "KR";
			invoiceLine21.JI_CustomsUnitQty = "PC";
			invoiceLine21.JI_CustomsQuantity = 10m;
			invoiceLine21.JI_NetWeight = 40m;
			invoiceLine21.JI_NetWeightUQ = "KG";
			invoiceLine21.JI_PrimaryPreference = "106";
			#endregion
			#endregion

			#region entryLine3
			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_AdValoremTariff = "9404210010";
			entryLine3.CL_CustomsValue = 22345.99m;

			#region invoiceLine31
			var invoiceLine31 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine31.JI_CL = entryLine3.PK;
			invoiceLine31.JI_Tariff = "9404210010";
			invoiceLine31.JI_SequenceNumber = 1;
			invoiceLine31.JI_CustomsUnitQty = "KG";
			invoiceLine31.JI_CustomsQuantity = 30m;
			#endregion
			#endregion

			return entry;
		}

		public void TestExportEntryLine()
		{
			var entry = SetUpEntryLineData();
			entry.InvoiceLines.ToList()[2].JI_NetWeight = 40.0001m;
			entry.InvoiceLines.ToList()[2].JI_CustomsQuantity = 10.0001m;
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(3, entryHeader.EntryLines.Length);
			var exportEntryLine1 = entryHeader.EntryLines[0];

			AssertEquals("EntryLines Should be ordered by CL_LineNumber", "001", exportEntryLine1.EntryLineNo);
			AssertEquals("4910001000", exportEntryLine1.HSCode);
			AssertEquals("PAPER CALENDARS", exportEntryLine1.HSDescription);
			AssertEquals("거래품명1", exportEntryLine1.TradeName);
			AssertEquals("상표명1", exportEntryLine1.BrandName);
			AssertEquals("999999999", exportEntryLine1.InvoiceNo);
			AssertEquals("KR", exportEntryLine1.CountryOfOrigin);
			AssertEquals(null, exportEntryLine1.CountryOfOriginDeterminationRule);
			AssertEquals("", exportEntryLine1.CountryOfOriginLabelLocation);
			AssertEquals(null, exportEntryLine1.CertificateOfOriginIssued);
			AssertEquals("106", exportEntryLine1.FTAType);
			AssertEquals(40m, exportEntryLine1.NetWeightInKG);
			AssertEquals("PC", exportEntryLine1.QtyUnit);
			AssertEquals(10.0001m, exportEntryLine1.Qty);
			AssertEquals(0, exportEntryLine1.PackQty);
			AssertEquals("", exportEntryLine1.PackType);
			AssertEquals(12345.99m, exportEntryLine1.CustomsValue);
			AssertEquals("", exportEntryLine1.ImportDeclarationNumber);
			AssertEquals("", exportEntryLine1.ImportEntryLineNo);
			AssertEquals("", exportEntryLine1.SkipManifestReporting);
			AssertEquals(null, exportEntryLine1.PreApprovalType);
			AssertEquals("", exportEntryLine1.PreApprovalNo);
			AssertEquals(ZDateTime.Empty, exportEntryLine1.PreApprovalEffectiveFromDate);
			AssertEquals(ZDateTime.Empty, exportEntryLine1.PreApprovalEffectiveToDate);

			var exportEntryLine2 = entryHeader.EntryLines[1];
			AssertEquals("EntryLines Should be ordered by CL_LineNumber", "002", exportEntryLine2.EntryLineNo);
			AssertEquals("8429521022", exportEntryLine2.HSCode);
			AssertEquals("USED EXCAVATOR", exportEntryLine2.HSDescription);
			AssertEquals("HYUNDAI ROBEX3000LC-7A", exportEntryLine2.TradeName);
			AssertEquals("상표명", exportEntryLine2.BrandName);
			AssertEquals("999999999", exportEntryLine2.InvoiceNo);
			AssertEquals("KR", exportEntryLine2.CountryOfOrigin);
			AssertEquals("A", exportEntryLine2.CountryOfOriginDeterminationRule);
			AssertEquals("N", exportEntryLine2.CountryOfOriginLabelLocation);
			AssertEquals("Y", exportEntryLine2.CertificateOfOriginIssued);
			AssertEquals("105", exportEntryLine2.FTAType);
			AssertEquals(500m, exportEntryLine2.NetWeightInKG);
			AssertEquals("CT", exportEntryLine2.QtyUnit);
			AssertEquals(50m, exportEntryLine2.Qty);
			AssertEquals(7, exportEntryLine2.PackQty);
			AssertEquals("CT", exportEntryLine2.PackType);
			AssertEquals(25987293m, exportEntryLine2.CustomsValue);
			AssertEquals("A", exportEntryLine2.ImportDeclarationNumber);
			AssertEquals("001", exportEntryLine2.ImportEntryLineNo);
			AssertEquals("N", exportEntryLine2.SkipManifestReporting);
			AssertEquals("A", exportEntryLine2.PreApprovalType);
			AssertEquals("KR00101010101", exportEntryLine2.PreApprovalNo);
			AssertEquals(new ZDateTime(2018, 01, 01), exportEntryLine2.PreApprovalEffectiveFromDate);
			AssertEquals(new ZDateTime(2018, 09, 01), exportEntryLine2.PreApprovalEffectiveToDate);

			var exportEntryLine3 = entryHeader.EntryLines[2];
			AssertEquals("EntryLines Should be ordered by CL_LineNumber", "003", exportEntryLine3.EntryLineNo);
			AssertEquals("9404210010", exportEntryLine3.HSCode);
			AssertEquals(null, exportEntryLine3.QtyUnit);
			AssertEquals(0m, exportEntryLine3.Qty);
		}

		public void TestExportInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2402200000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region invoiceLine
			entryLine.CL_AdValoremTariff = "2402200000";
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Tariff = entryLine.CL_AdValoremTariff;
			invoiceLine1.JI_SequenceNumber = 2;
			invoiceLine1.JI_Description = "2007 N81011141";
			invoiceLine1.JI_Ingredient = "dfewe";
			invoiceLine1.JI_LotNumber = "AbcdZZZ";
			invoiceLine1.JI_InvoiceUQ = "EA";
			invoiceLine1.JI_InvoiceQuantity = 2m;
			invoiceLine1.JI_CustomsUnitQty = "U";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.UnitPrice = 10m;
			invoiceLine1.JI_LinePrice = 20m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Tariff = entryLine.CL_AdValoremTariff;
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_Description = "Detail Description";
			invoiceLine2.JI_Ingredient = "성분2";
			invoiceLine2.JI_LotNumber = "lotnum";
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = "U";
			invoiceLine2.JI_CustomsQuantity = 5.0000000001m;
			invoiceLine2.UnitPrice = 1.0000001m;
			invoiceLine2.JI_LinePrice = 30m;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "1234560000";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine3.JI_Tariff = entryLine2.CL_AdValoremTariff;
			invoiceLine3.JI_SequenceNumber = 1;
			invoiceLine3.JI_Description = "Detail Description3";
			invoiceLine3.JI_Ingredient = "성분3";
			invoiceLine3.JI_LotNumber = "lotnum3";
			invoiceLine3.JI_InvoiceUQ = "PC";
			invoiceLine3.JI_InvoiceQuantity = 30m;
			invoiceLine3.JI_CustomsUnitQty = "U";
			invoiceLine3.JI_CustomsQuantity = 5m;
			invoiceLine3.UnitPrice = 1m;
			invoiceLine3.JI_LinePrice = 30m;
			#endregion

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines.Length);
			AssertEquals(2, entryHeader.EntryLines[0].InvoiceLines.Length);
			var exportInvoiceLine1 = entryHeader.EntryLines[0].InvoiceLines[0];

			AssertEquals("InvoiceLine Should be ordered by JI_SequenceNumber", "01", exportInvoiceLine1.InvoiceLineNo);
			AssertEquals(5.0000m, exportInvoiceLine1.QtyOrWeight);
			AssertEquals("U", exportInvoiceLine1.QtyOrWeightUnit);
			AssertEquals(6.000000m, exportInvoiceLine1.UnitPrice);
			AssertEquals(30.0000m, exportInvoiceLine1.Amount);
			AssertEquals("성분2", exportInvoiceLine1.Ingredient);
			AssertEquals("Detail Description", exportInvoiceLine1.DetailDescription);
			AssertEquals("lotnum", exportInvoiceLine1.LotNumber);

			var exportInvoiceLine2 = entryHeader.EntryLines[0].InvoiceLines[1];
			AssertEquals("InvoiceLine Should be ordered by JI_SequenceNumber", "02", exportInvoiceLine2.InvoiceLineNo);
			AssertEquals(10m, exportInvoiceLine2.QtyOrWeight);
			AssertEquals("U", exportInvoiceLine2.QtyOrWeightUnit);
			AssertEquals(2m, exportInvoiceLine2.UnitPrice);
			AssertEquals(20m, exportInvoiceLine2.Amount);
			AssertEquals("dfewe", exportInvoiceLine2.Ingredient);
			AssertEquals("2007 N81011141", exportInvoiceLine2.DetailDescription);
			AssertEquals("AbcdZZZ", exportInvoiceLine2.LotNumber);
			AssertEquals(1, entryHeader.EntryLines[1].InvoiceLines.Length);
			var exportInvoiceLine3 = entryHeader.EntryLines[1].InvoiceLines[0];
			AssertEquals("InvoiceLine Should be ordered by JI_SequenceNumber", "01", exportInvoiceLine3.InvoiceLineNo);
			AssertEquals(30m, exportInvoiceLine3.QtyOrWeight);
			AssertEquals("PC", exportInvoiceLine3.QtyOrWeightUnit);
			AssertEquals(1m, exportInvoiceLine3.UnitPrice);
			AssertEquals(30m, exportInvoiceLine3.Amount);
		}

		[TestDate(2021, 02, 26)]
		public void TestExportGAApprovalDocument()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine = declaration.InvoiceLines[0];

			#region ApprovalDocument
			var approvalDocument11 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument11.CSI_LineNo = 2;
			approvalDocument11.CSI_RN_NKCountryCode = "KR";
			approvalDocument11.CSI_Procedure = "05";
			approvalDocument11.CSI_SubType = RequirementTypeCodeList.Codes._1;
			approvalDocument11.CSI_Code = "B";
			approvalDocument11.CSI_ReferenceNumber = "NO";
			approvalDocument11.CSI_Description = "식품등의 수입신고확인증";
			approvalDocument11.CSI_AdditionalDescription = "테스트";
			approvalDocument11.CSI_ReferenceNumber2 = "TEST";
			approvalDocument11.CSI_Status = "01";

			var approvalDocument12 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument12.CSI_LineNo = 1;
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
			#endregion

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments.Length);
			var approvalDocument1 = entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments[0];
			AssertEquals("approvalDocuments Should be ordered by CSI_LineNo", "01", approvalDocument1.SequenceNo);
			AssertEquals("899999999", approvalDocument1.RequirementApprovalNumber);
			AssertEquals("A", approvalDocument1.RequirementDocumentType);
			AssertEquals("발급서류명2", approvalDocument1.DocumentName);
			AssertEquals(new ZDateTime(2015, 01, 01), approvalDocument1.ApprovalDate);
			AssertEquals("11", approvalDocument1.RegulationCategoryCode);
			AssertEquals(RequirementTypeCodeList.Codes._3, approvalDocument1.RequirementType);
			AssertEquals("사유2", approvalDocument1.ReasonForMissingApprovalNumber);
			AssertEquals("586455632", approvalDocument1.UniqueItemID);
			AssertEquals("11302", approvalDocument1.NonGAReasonType);

			var approvalDocument2 = entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments[1];
			AssertEquals("approvalDocuments Should be ordered by CSI_LineNo", "02", approvalDocument2.SequenceNo);
			AssertEquals("NO", approvalDocument2.RequirementApprovalNumber);
			AssertEquals("B", approvalDocument2.RequirementDocumentType);
			AssertEquals("식품등의 수입신고확인증", approvalDocument2.DocumentName);
			AssertEquals(new ZDateTime(2021, 02, 26), approvalDocument2.ApprovalDate);
			AssertEquals("05", approvalDocument2.RegulationCategoryCode);
			AssertEquals("1", approvalDocument2.RequirementType);
			AssertEquals("테스트", approvalDocument2.ReasonForMissingApprovalNumber);
			AssertEquals("TEST", approvalDocument2.UniqueItemID);
			AssertEquals("05101", approvalDocument2.NonGAReasonType);
		}
		public void TestExportVehicleNo()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine = declaration.InvoiceLines[0];
			#region vehicleNumber
			var vehicleNumber1 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber1.CY_Order = 2;
			vehicleNumber1.CY_Data = "KN3HNP6N18K283119";

			var vehicleNumber2 = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber2.CY_Order = 1;
			vehicleNumber2.CY_Data = "CCCCZZZ";
			#endregion
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines[0].InvoiceLines[0].VehicleNumbers.Length);
			var exportVehicleNumber1 = entryHeader.EntryLines[0].InvoiceLines[0].VehicleNumbers[0];
			AssertEquals("vehicleNumbers Should be ordered by CY_Order", "001", exportVehicleNumber1.SequenceNo);
			AssertEquals("CCCCZZZ", exportVehicleNumber1.VIN);
			var exportVehicleNumber2 = entryHeader.EntryLines[0].InvoiceLines[0].VehicleNumbers[1];
			AssertEquals("vehicleNumbers Should be ordered by CY_Order", "002", exportVehicleNumber2.SequenceNo);
			AssertEquals("KN3HNP6N18K283119", exportVehicleNumber2.VIN);
		}
		public void TestPreferredInspectionDate()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(false, ((ZDateTime)entryHeader.PreferredInspectionDate).IsValid);

			var service = declaration.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(false, ((ZDateTime)entryHeader.PreferredInspectionDate).IsValid);

			declaration.InspectionDate = new ZDateTime(2021, 03, 08);
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("PreferredInspectionDate is set", new ZDateTime(2021, 03, 08), entryHeader.PreferredInspectionDate);

			declaration.JE_MessageSubType = ExportTypeCodeList.Codes.G;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("PreferredInspectionDate is set", ZDateTime.Today.AddYears(-30), entryHeader.PreferredInspectionDate);

			entry.EntryNumber = "1163921400096X";
			var entryNum1 = entry.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2021, 03, 08);

			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("PreferredInspectionDate is set", new DateTime(2021, 03, 08).AddYears(-30), entryHeader.PreferredInspectionDate);
		}

		[TestDate(2021, 03, 03)]
		public void TestSerialisationWithFullData()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			var exportEntryHeader = new ExportEntryHeaderCreator().Create(entry);
			var result = new GOVCBR830MessageBuilder(exportEntryHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR830DataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR830_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}
		[TestDate(2021, 01, 13)]
		public void TestSerialisationWithRealData()
		{
			var entry = SetUpRealData();
			var exportEntryHeader = new ExportEntryHeaderCreator().Create(entry);
			var result = new GOVCBR830MessageBuilder(exportEntryHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR830DataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR830_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestPortOfLoading()
		{
			RefUNLOCO originUNLOCO = Factory.New<RefUNLOCO>();
			originUNLOCO.RL_Code = "AAAAA";
			originUNLOCO.RL_IATA = "ICN";
			originUNLOCO.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			Factory.Save();

			var declaration = CreateEmptyDeclaration();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKOrigin = originUNLOCO.RL_Code;
			var entry = declaration.CustomsEntryHeaders[0];
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("ICN", entryHeader.PortOfLoading);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("AAAAA", entryHeader.PortOfLoading);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.JE_IATALoadPort = "";
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("AAAAA", entryHeader.PortOfLoading);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_IATALoadPort = "ICN";
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("ICN", entryHeader.PortOfLoading);
		}

		public void TestManufacturerUnipassIDWhenIDNumbersIsEmpty()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			var manufacturerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "레디코리아123456" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);

			var emptyManufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA4", "레디코리아4");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("레디코리아123456", entryHeader.Manufacturer.UnipassIDForOrganization);

			invoice.JZ_OA_ManufacturerAddress = emptyManufacturer.MainAddress.PK;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("제조미상9999000", entryHeader.Manufacturer.UnipassIDForOrganization);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("", entryHeader.Manufacturer.UnipassIDForOrganization);
		}

		public void TestTotalGrossWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_Weight = 100m;
			invoice1.JZ_WeightUQ = "KG";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_Weight = 200m;
			invoice2.JZ_WeightUQ = "KG";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 20m;
			invoiceLine2.JI_WeightUQ = "KG";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = new ExportEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("ExportEntryHeader.TotalGrossWeightInKG is the sum of Invoice.JZ_Weight, not the sum of InvoiceLine.JI_Weight.", 300m, entryHeader.TotalGrossWeightInKG);
		}

		public void TestCustomsMessageRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.JZ_Remarks = "Test";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.JZ_Remarks = "Value";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertContains("This item is the sum of the values of each InvoiceHeader.CustomsMessageRemarks.", "Test", entryHeader.DeclarantAdditionalDescription);
			AssertContains("This item is the sum of the values of each InvoiceHeader.CustomsMessageRemarks.", "Value", entryHeader.DeclarantAdditionalDescription);

			invoice1.JZ_Remarks = SetJZ_Remarks(1);
			invoice2.JZ_Remarks = SetJZ_Remarks(600);
			entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals("Currently, the sum of the values of the two fields is over 600.", 601, invoice1.JZ_Remarks.Length + invoice2.JZ_Remarks.Length);
			AssertEquals("This column is truncated because the maximum length is 600.", 600, entryHeader.DeclarantAdditionalDescription.Length);

			ZString SetJZ_Remarks(int index)
			{
				ZString jZ_Remarks = ZString.Empty;
				for (int i = 0; i < index; i++)
				{
					jZ_Remarks += "A";
				}
				return jZ_Remarks;
			}
		}

		public void TestExchangeRate()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = ZDateTime.Today;

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var jpyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);

			new TestDataSetupHelper(Factory).SetExchangeRate(declaration.Company, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1234.56m, usdCurrency);
			new TestDataSetupHelper(Factory).SetExchangeRate(declaration.Company, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 9876.54m, jpyCurrency);
			new TestDataSetupHelper(Factory).SetExchangeRate(declaration.Company, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 0m, audCurrency);

			var invoice = declaration.Invoices[0];
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var entryHeaderUSD = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, entryHeaderUSD.Currency);
			AssertEquals(1234.56m, entryHeaderUSD.ExchangeRate);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var entryHeaderJPY = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Core.Constants.CurrencyCodes.Japan, entryHeaderJPY.Currency);
			AssertEquals(1234.56m, entryHeaderJPY.ExchangeRate);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var entryHeaderAUD = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, entryHeaderAUD.Currency);
			AssertEquals(1234.56m, entryHeaderAUD.ExchangeRate);
		}

		public void TestCurrencyWithoutExchangeRate()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = ZDateTime.Today;

			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Zimbabwe);
			new TestDataSetupHelper(Factory).SetExchangeRate(declaration.Company, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1234.56m, currency);

			var invoice = declaration.Invoices[0];
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Zimbabwe;
			invoice.JZ_InvoiceAmount = 10000;
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.UnitPrice = 1000;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_LinePrice = 2000;
			invoice.JZ_InvoiceCurrExRate = 1234.56;

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, entryHeader.Currency);
			AssertEquals(12345600m, entryHeader.TotalInvoiceAmount);
			AssertEquals(2469120m, entryHeader.EntryLines[0].InvoiceLines[0].Amount);
			AssertEquals(1234560m, entryHeader.EntryLines[0].InvoiceLines[0].UnitPrice);
		}

		public void TestCurrencyWithExchangeRate()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = ZDateTime.Today;

			var invoice = declaration.Invoices[0];
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.JZ_InvoiceAmount = 10000;
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.UnitPrice = 1000;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_LinePrice = 2000;
			invoice.JZ_InvoiceCurrExRate = 1234.56;

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, entryHeader.Currency);
			AssertEquals(10000m, entryHeader.TotalInvoiceAmount);
			AssertEquals(2000m, entryHeader.EntryLines[0].InvoiceLines[0].Amount);
			AssertEquals(1000m, entryHeader.EntryLines[0].InvoiceLines[0].UnitPrice);
		}

		public void TestStringTypeTruncate830()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();

			entry.Declaration.JE_LocationOfGoods = "물품소재지 주소입니다.1물품소재지 주소입니다.2물품소재지 주소입니다.3물품소재지 주소입니다.4물품소재지 주소입니다.5물품소재지 주소입니다.6물품소재지 주소입니다.7";

			#region Organisation
			var branch = Factory.Load<GlbBranch>(entry.Declaration.JE_GB);
			var broker = Factory.Load<OrgHeader>(branch.GB_OH_OrgProxy);
			broker.OH_FullName = "레디코리아1레디코리아2레디코리아3레디코리아4레디코리아5";
			broker.Contacts[0].OC_ContactName = "홍길동이다 한국이름";

			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA12", "레디코리아셀러1레디코리아셀러2");
			entry.Declaration.JE_OA_SellerAddress = seller.MainAddress.PK;

			var supplierSet = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA13", "레디코리아서플라이어1레디코리아서플라이어2");
			TestOrgDataSetUpHelper.AddOrgContact(supplierSet, "홍길동판매 한국총판", true);

			var manufacturerSet = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA14", "레디코리아제조자1레디코리아제조자2");

			entry.Declaration.JE_OA_SupplierAddress = supplierSet.MainAddress.PK;
			entry.Declaration.Invoices[0].JZ_OA_ManufacturerAddress = manufacturerSet.MainAddress.PK;

			var forwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA6", "");
			TestOrgDataSetUpHelper.AddOrgContact(forwarder, "나대표입니다1나대표입니다2나대표입니다3", true);

			entry.Declaration.JE_OH_Forwarder = forwarder.PK;
			#endregion

			entry.Declaration.Invoices[0].JZ_Remarks = @"신고인기재란채우는데이터는한글로삼백글자입니다1
신고인기재란채우는데이터는한글로삼백글자입니다2
신고인기재란채우는데이터는한글로삼백글자입니다3
신고인기재란채우는데이터는한글로삼백글자입니다4
신고인기재란채우는데이터는한글로삼백글자입니다5
신고인기재란채우는데이터는한글로삼백글자입니다6
신고인기재란채우는데이터는한글로삼백글자입니다7
신고인기재란채우는데이터는한글로삼백글자입니다8
신고인기재란채우는데이터는한글로삼백글자입니다9
신고인기재란채우는데이터는한글로삼백글자입니다10
신고인기재란채우는데이터는한글로삼백글자입니다11
신고인기재란채우는데이터는한글로삼백글자입니다12
신고인기재란채우는데이터는한글로삼백글자입니다13";

			#region ApprovalDocument
			var approvalDocument12 = entry.Declaration.InvoiceLines[0].GAApprovalDataCollection[0];
			approvalDocument12.CSI_Description = "수출요건확인발급서류명1수출요건확인발급서류명2수출요건확인발급서류명3수출요건확인발급서류명4수출요건확인발급서류명5수출요건확인발급서류명6수출요건확인발급서류명7수출요건확인발급서류명8수출요건확인발급서류명9수출요건확인발급서류명10수출요건확인발급서류명11수출요건확인발급서류명12수출요건확인발급서류명13";
			approvalDocument12.CSI_AdditionalDescription = "요건승인번호 미기재 사유1요건승인번호 미기재 사유2요건승인번호 미기재 사유3요건승인번호 미기재 사유4요건승인번호 미기재 사유5요건승인번호 미기재 사유6요건승인번호 미기재 사유7요건승인번호 미기재 사유8요건승인번호 미기재 사유9";
			#endregion

			var entryHeader = new ExportEntryHeaderCreator().Create(entry);

			AssertEquals("물품소재지 주소입니다.1물품소재지 주소입니다.2물품소재지 주소입니다.3물품소재지 주소입니다.4물품소재지 주소입니다.5물품소재지 주소입니다.6물품소재지 ", entryHeader.GoodsLocationAddress);

			#region Organisation
			var declarant = entryHeader.Declarant;
			AssertEquals("레디코리아1레디코리아2레디코리아3레디코리아4레디코", declarant.CompanyName);
			AssertEquals("홍길동이다 ", declarant.RepresentativeName);

			var exporter = entryHeader.Exporter;
			AssertEquals("레디코리아셀러1레디코리아셀", exporter.CompanyName);

			var supplier = entryHeader.Supplier;
			AssertEquals("레디코리아서플라이어1레디코", supplier.CompanyName);
			AssertEquals("홍길동판매 ", supplier.RepresentativeName);

			var manufacturer = entryHeader.Manufacturer;
			AssertEquals("레디코리아제조자1레디코리아", manufacturer.CompanyName);
			#endregion

			AssertEquals("나대표입니다1나대표입니다2나대", entryHeader.FreightForwarderContactName);
			AssertEquals("신고인기재란채우는데이터는한글로삼백글자입니다1\r\n신고인기재란채우는데이터는한글로삼백글자입니다2\r\n신고인기재란채우는데이터는한글로삼백글자입니다3\r\n신고인기재란채우는데이터는한글로삼백글자입니다4\r\n신고인기재란채우는데이터는한글로삼백글자입니다5\r\n신고인기재란채우는데이터는한글로삼백글자입니다6\r\n신고인기재란채우는데이터는한글로삼백글자입니다7\r\n신고인기재란채우는데이터는한글로삼백글자입니다8\r\n신고인기재란채우는데이터는한글로삼백글자입니다9\r\n신고인기재란채우는데이터는한글로삼백글자입니다10\r\n신고인기재란채우는데이터는한글로삼백글자입니다11\r\n신고인기재란채우는데이터는한글로삼백글자입니다12\r\n신고인기", entryHeader.DeclarantAdditionalDescription);

			var approvalDocument1 = entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments[0];
			AssertEquals("수출요건확인발급서류명1수출요건확인발급서류명2수출요건확인발급서류명3수출요건확인발급서류명4수출요건확인발급서류명5수출요건확인발급서류명6수출요건확인발급서류명7수출요건확인발급서류명8수출요건확인발급서류명9수출요건확인발급서류명10수출요건확인발급서류명11수출요건확인발급서류명12수출요건확인발급서류", approvalDocument1.DocumentName);
			AssertEquals("요건승인번호 미기재 사유1요건승인번호 미기재 사유2요건승인번호 미기재 사유3요건승인번호 미기재 사유4요건승인번호 미기재 사유5요건승인번호 미기재 사유6요건승인번호 미기재 사유7요건승인번호 미기재 사유8", approvalDocument1.ReasonForMissingApprovalNumber);
		}

		public void TestRoundDecimalPlace()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			new TestDataSetupHelper(Factory).SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), 1234.56128m, usdCurrency);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 1002.8m;
			invoice.JZ_WeightUQ = "G";
			invoice.JZ_InvoiceAmount = 2000.7281m;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100m;
			invoiceLine.JI_LinePrice = 100.15788m;
			invoiceLine.JI_NetWeight = 100.12346m;
			invoiceLine.JI_NetWeightUQ = "G";
			invoiceLine.JI_InvoiceQuantity = 1.23456;
			invoiceLine.JI_FormattedTariff = "1111.11-1111";
			invoiceLine.JI_CustomsUnitQty = "CT";
			invoiceLine.JI_CustomsQuantity = 1.23456;
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100.123m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 455.5m, Core.Constants.CurrencyCodes.KoreaRepublicOf);

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100.333788m;
			invoiceLine.JI_LinePrice = 150.8542m;
			invoiceLine.JI_NetWeight = 100.4531m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 3.36479;
			invoiceLine.JI_FormattedTariff = "1111.11-1111";
			invoiceLine.JI_CustomsQuantity = 5.888123;
			invoiceLine.JI_CustomsUnitQty = "CT";

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UnitPrice = 100.333788m;
			invoiceLine.JI_LinePrice = 102.65411m;
			invoiceLine.JI_NetWeight = 100.45311m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_InvoiceQuantity = 2.66666;
			invoiceLine.JI_FormattedTariff = "2222.22-2222";
			invoiceLine.JI_CustomsQuantity = 1.91233;
			invoiceLine.JI_CustomsUnitQty = "G";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);

			entryHeader.RoundDecimalValueRoundedWithDecimalPlaces();

			var entryLine1 = entryHeader.EntryLines[0];
			var invoiceLine1 = entryLine1.InvoiceLines[0];

			AssertEquals(100m, entryHeader.Freight);
			AssertEquals(456m, entryHeader.Insurance);
			AssertEquals(354m, entryHeader.TotalCustomsValue);
			AssertEquals(2000.73m, entryHeader.TotalInvoiceAmount);
			AssertEquals(1234.5613m, entryHeader.ExchangeRate);
			AssertEquals(1.003m, entryHeader.TotalGrossWeightInKG);

			AssertEquals(100.553m, entryLine1.NetWeightInKG);
			AssertEquals(7m, entryLine1.Qty);
			AssertEquals(251m, entryLine1.CustomsValue);

			AssertEquals(1.2346m, invoiceLine1.QtyOrWeight);
			AssertEquals(100.1600m, invoiceLine1.Amount);
			AssertEquals(81.130119m, invoiceLine1.UnitPrice);

			AssertEquals(0m, entryHeader.EntryLines[1].Qty);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
	}
}
