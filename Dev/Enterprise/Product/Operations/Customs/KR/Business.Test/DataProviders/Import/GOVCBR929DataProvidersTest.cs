using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR929DataProvidersTest : TestCaseWithFactory
	{
		[TestDate(2013, 01, 01)]
		public void TestEntryHeader()
		{
			SetUpTariffData();
			var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(ZString.Empty, ZBool.False);
			GetOrCreateChargesData(ChargeTypeList.Codes.Duty, 10000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.SpecialConsumptionTax, 11000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.LiquorTax, 12000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.TransportationTax, 13000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.EducationTax, 14000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.AgricultureTax, 15000m);
			GetOrCreateChargesData(ChargeTypeList.Codes.VAT, 16000m);

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("4062001070010U", import929.ImportDeclarationNumber);
			AssertEquals("010", import929.DeclarationCustomsOffice);
			AssertEquals("20", import929.DeclarationCustomsDivision);
			AssertEquals("HJSC98100123AB01", import929.HouseBillNumber);
			AssertEquals(true, import929.HouseBillSplitDeclarationIndicator);
			AssertEquals("A", import929.HouseBillSplitDeclarationReasonCode);
			AssertEquals("a", import929.HouseBillSplitDeclarationReasonDescription);
			AssertEquals("01KE0766SS200100003", import929.CargoManagementNo);
			AssertEquals(new ZDateTime(2013, 01, 01), import929.UnderbondMovementArrivalDate);
			AssertEquals(new ZDateTime(2012, 01, 01), import929.ArrivalDateAtDischargePort);
			AssertEquals("13", import929.PaymentType);
			AssertEquals(ImportPersonTypeCodeList.Codes.B, import929.ImporterType);
			AssertEquals("세관무역(주)", import929.FreightForwarderCompanyName);
			AssertEquals("ABCD", import929.FreightForwarderID);
			AssertEquals("D", import929.DeclarationPlanCode);
			AssertEquals("A", import929.ImportTypeCode);
			AssertEquals("11", import929.TradeType);
			AssertEquals("L", import929.DeclarationProcedureType);
			AssertEquals("Y", import929.ValueDeclarationAttached);
			AssertEquals(125.6m, import929.TotalGrossWeightInKG);
			AssertEquals(2, import929.TotalPackQty);
			AssertEquals("CT", import929.PackType);
			AssertEquals("KRPUS", import929.ArrivalPort);
			AssertEquals("40", import929.TransportMode);
			AssertEquals("ETC", import929.ContainerPackMode);
			AssertEquals("PR", import929.DepartureCountryCode);
			AssertEquals("KE1098", import929.VesselOrFlightNo);
			AssertEquals("DBSC96100123AB01", import929.MasterBillNumber);
			AssertEquals("HJSC", import929.CarrierID);
			AssertEquals("13011013", import929.BondedAreaCode);
			AssertEquals("1A123", import929.LocationIDInBondedArea);
			AssertEquals("CFR", import929.Incoterm);
			AssertEquals(0m, import929.TotalInvoiceAmount);
			AssertEquals("USD", import929.InvoiceAmountCurrency);
			AssertEquals("DA", import929.InvoicePaymentTerm);
			AssertEquals(719424460431m, import929.TotalCustomsValueUSD);
			AssertEquals(999999999999m, import929.TotalCustomsValueKRW);
			AssertEquals(1210.12m, import929.ExchangeRate);
			AssertEquals(0m, import929.Freight);
			AssertEquals(0m, import929.Insurance);
			AssertEquals(0m, import929.AdditionalAmount);
			AssertEquals(0m, import929.DeductedAmount);
			AssertEquals("HJSC", import929.CourierCompanyID);
			AssertEquals("1234567890", import929.AuthorizedImporterRegNo);
			AssertEquals("123456789", import929.OwnerReferenceNumber);
			AssertEquals("Y", import929.SouthNorthTradeYN);
			AssertEquals("Y", import929.GoldTradeTransactionYN);
			AssertEquals("B", import929.BondedFactoryUseCode);
			AssertEquals(new ZDateTime(2012, 02, 10, 10, 12, 00), import929.BondedFactoryUseDate);
			AssertEquals("A", import929.CustomsBrokerCommentCode1);
			AssertEquals("A", import929.CustomsBrokerCommentCode2);
			AssertEquals("A", import929.CustomsBrokerCommentCode3);
			AssertEquals("기재사항1", import929.CustomsBrokerComment1);
			AssertEquals(ZBool.True, import929.ApplicationForAgreedRate);
			AssertEquals("AAAAAAAAAAAA", import929.BlanketValuationDeclarationNumber);
			AssertEquals("Y", import929.CertificateOfOriginIssued);

			AssertEquals(10000m, import929.TotalDutyAmount);
			AssertEquals(11000m, import929.TotalSpecialConsumptionTax);
			AssertEquals(12000m, import929.TotalLiquorTax);
			AssertEquals(13000m, import929.TotalTransportationTax);
			AssertEquals(14000m, import929.TotalEducationTax);
			AssertEquals(15000m, import929.TotalAgricultureTax);
			AssertEquals(16000m, import929.TotalVAT);

			#region Organisation
			var importer = import929.Importer;
			AssertEquals("조인성", importer.CompanyName);
			AssertEquals("관세상사1234561", importer.UnipassIDForOrganization);

			var seller = import929.Supplier;
			AssertEquals("OMR ENGR", seller.CompanyName);
			AssertEquals("JP", seller.CountryCode);
			AssertEquals("CNTOSHIN12347", seller.ForeignCompanyID);
			#endregion

			void GetOrCreateChargesData(string chargeType, decimal chargeAmount)
			{
				var charge = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == chargeType);
				if (charge == null)
				{
					charge = entry.Charges.AddNew();
					charge.C1_ChargeType = chargeType;
				}
				charge.C1_ChargeAmount = chargeAmount;
			}
		}

		public void TestBroker()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";
			Factory.Save();

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "상호";
			broker.OH_IsBroker = true;
			var orgContact = TestOrgDataSetUpHelper.AddOrgContact(broker, "신고인", true);
			orgContact.OC_PhoneExtension = "0000";
			var brokeraddress = broker.MainAddress;

			TestOrgDataSetUpHelper.AddOrgAddress(brokeraddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			brokeraddress.OA_Email = "id@nnnn";
			brokeraddress.OA_Phone = "0000000000";
			brokeraddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			branch.GB_OH_OrgProxy = broker.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var import929 = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("상호", import929.Declarant.CompanyName);
			AssertEquals("id@nnnn", import929.Declarant.Email);
			AssertEquals("0000000000", import929.Declarant.PhoneNumber);
			AssertNullOrEmpty(import929.Declarant.AddressLine1);
			AssertNullOrEmpty(import929.Declarant.AddressLine2);
			AssertNullOrEmpty(import929.Declarant.Postcode);
			AssertNullOrEmpty(import929.Declarant.RoadNameCode);
			AssertNullOrEmpty(import929.Declarant.BuildingNumber);
		}

		public void TestImporter()
		{
			var importer1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(importer1, "조인성", true);
			TestOrgDataSetUpHelper.AddOrgAddress(importer1.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var importerrCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "관세상사1234561" },
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001A" },
				new IDNumberAndType() { Type = IdentificationType.AuthorizedImporterRegNo,  Number = "1234567890" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer1, importerrCodes);

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
			declaration.JE_OA_ImporterAddress = importer1.MainAddress.PK;

			var import929 = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("레디코리아", import929.Importer.CompanyName);
			AssertEquals("관세상사1234561", import929.Importer.UnipassIDForOrganization);
			AssertNullOrEmpty(import929.Importer.Postcode);
			AssertNullOrEmpty(import929.Importer.RoadNameCode);
			AssertNullOrEmpty(import929.Importer.BuildingNumber);
			AssertEquals("Although 929 does not have this as data item, we need this for document printing", "조인성", import929.Importer.RepresentativeName);
		}

		public void TestPayerRegistrationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "모나리자(주)");
			payer.OH_IsBroker = false;
			var payerCustomsAddress = payer.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			TestOrgDataSetUpHelper.AddOrgContact(payer, "홍나리", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payerCustomsAddress, "서울시 강남구 논현동 235", "7층 101호", "11087", "012345678912", "1234567891234567891234567");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "모나리자1771025" },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "6510071645915" },
				new IDNumberAndType() { Type = IdentificationType.PassportNo, Number = "YC00158522354" },
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo,  Number = "1028142299" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			var payerAddressCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.OfficeID, Number = "0001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer.CustomsAddress, payerAddressCodes);

			payerCustomsAddress.OA_Email = "id@domain.com";
			payerCustomsAddress.OA_Phone = "0000000000";

			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer.PK;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertNotNull(import929.Payer);

			declaration.JE_OH_DutyPayer = payer.PK;

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("모나리자(주)", import929.Payer.CompanyName);
			AssertEquals("서울시 강남구 논현동 235", import929.Payer.AddressLine1);
			AssertEquals("7층 101호", import929.Payer.AddressLine2);
			AssertEquals("id@domain.com", import929.Payer.Email);
			AssertEquals("0000000000", import929.Payer.PhoneNumber);
			AssertEquals("11087", import929.Payer.Postcode);
			AssertEquals("012345678912", import929.Payer.RoadNameCode);
			AssertEquals("1234567891234567891234567", import929.Payer.BuildingNumber);
			AssertEquals(false, import929.Payer.IsIndividual);
			AssertEquals("모나리자1771025", import929.Payer.UnipassIDForOrganization);
			AssertEquals("0001", import929.Payer.OfficeID);
			AssertEquals("1028142299", import929.Payer.BusinessRegNo);

			payer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(true, import929.Payer.IsIndividual);
			AssertEquals("모나리자1771025", import929.Payer.UnipassIDForOrganization);
			AssertEquals("0001", import929.Payer.OfficeID);
			AssertEquals("6510071645915", import929.Payer.KoreanRegNoForResident);
		}

		public void TestSeller()
		{
			var seller = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "OMR ENGR");
			TestOrgDataSetUpHelper.AddOrgAddress(seller.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "", "", "", "CN");
			var sellerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "CNTOSHIN12347" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller, sellerCodes);

			var customsAddressOfRecord = seller.Addresses.AddNew();
			customsAddressOfRecord.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			customsAddressOfRecord.OA_CompanyNameOverride = "OMR ENGR2";
			TestOrgDataSetUpHelper.AddOrgAddress(customsAddressOfRecord, "전주시 완산구 평화동", "하늘채아파트", "", "", "", "JP");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_OH_Supplier = seller.PK;
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var import929 = new ImportEntryHeaderCreator().Create(entry);
			var seller1 = import929.Supplier;

			AssertEquals("OMR ENGR2", seller1.CompanyName);
			AssertEquals("JP", seller1.CountryCode);
			AssertEquals("CNTOSHIN12347", seller1.ForeignCompanyID);
			AssertNullOrEmpty(seller1.AddressLine1);
			AssertNullOrEmpty(seller1.Postcode);
			AssertNullOrEmpty(seller1.RoadNameCode);
			AssertNullOrEmpty(seller1.BuildingNumber);
			AssertNullOrEmpty(seller1.PhoneNumber);
			AssertNullOrEmpty(seller1.Email);
			AssertNullOrEmpty(seller1.RepresentativeName);

			customsAddressOfRecord.Delete();
			import929 = new ImportEntryHeaderCreator().Create(entry);
			seller1 = import929.Supplier;
			AssertEquals("OMR ENGR", seller1.CompanyName);
			AssertEquals("CN", seller1.CountryCode);
			AssertEquals("CNTOSHIN12347", seller1.ForeignCompanyID);
			AssertNullOrEmpty(seller1.AddressLine1);
			AssertNullOrEmpty(seller1.Postcode);
			AssertNullOrEmpty(seller1.RoadNameCode);
			AssertNullOrEmpty(seller1.BuildingNumber);
			AssertNullOrEmpty(seller1.PhoneNumber);
			AssertNullOrEmpty(seller1.Email);
			AssertNullOrEmpty(seller1.RepresentativeName);
		}

		public void TestEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062001070010U";

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(new ZDateTime(), import929.UnderbondMovementArrivalDate);

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_ExpiryDate = new ZDateTime(2012, 01, 01);
			entryNum.CE_EntryType = "";

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(new ZDateTime(), import929.UnderbondMovementArrivalDate);

			declaration.UnderbondMovementArrivalDate = new ZDateTime(2012, 02, 01);

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(new ZDateTime(2012, 02, 01), import929.UnderbondMovementArrivalDate);
		}

		public void TestAirlineCountryCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_VoyageFlightNo = "KI1020";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, "KE");
			airline.RM_TwoCharacterCode = "KE";
			airline.RM_RN_NKAirlineCountry = "KR";

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(ZString.Empty, import929.VesselCountryCode);

			declaration.JE_VoyageFlightNo = "KE1098";
			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("KR", import929.VesselCountryCode);
		}

		public void TestVesselCountryCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_VesselName = "KE1099";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "KE1098";
			vessel.RV_RN_NKCountryOfReg = "KR";

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(ZString.Empty, import929.VesselCountryCode);

			declaration.JE_VesselName = "KE1098";
			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("KR", import929.VesselCountryCode);
		}

		public void TestCustomsBrokerCommentCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("", import929.CustomsBrokerCommentCode1);
			AssertEquals("", import929.CustomsBrokerCommentCode2);
			AssertEquals("", import929.CustomsBrokerCommentCode3);

			var referenceNumber1 = declaration.DeclarationRefs.AddNew();
			referenceNumber1.J3_ReferenceType = "257";
			referenceNumber1.J3_ReferenceNumber = "A";

			var referenceNumber2 = declaration.DeclarationRefs.AddNew();
			referenceNumber2.J3_ReferenceType = "258";
			referenceNumber2.J3_ReferenceNumber = "B";

			var referenceNumber3 = declaration.DeclarationRefs.AddNew();
			referenceNumber3.J3_ReferenceType = "259";
			referenceNumber3.J3_ReferenceNumber = "C";

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("A", import929.CustomsBrokerCommentCode1);
			AssertEquals("B", import929.CustomsBrokerCommentCode2);
			AssertEquals("C", import929.CustomsBrokerCommentCode3);
		}

		public void TestCustomsBrokerComment()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Remarks = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
								 "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
						 "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", import929.CustomsBrokerComment1);
			AssertNullOrEmpty(import929.CustomsBrokerComment2);

			invoice.JZ_Remarks = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
								 "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" + "가";

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
						 "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", import929.CustomsBrokerComment1);
			AssertEquals("가", import929.CustomsBrokerComment2);
		}

		public void TestFormattWithToDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(ZDateTime.Empty, import929.ArrivalDateAtDischargePort);

			declaration.JE_DateOfArrival = new ZDateTime(2021, 01, 01);

			import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(new ZDateTime(2021, 01, 01), import929.ArrivalDateAtDischargePort);
		}

		public void TestIsSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "KE1098";

			var import929 = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("KI1098", import929.VesselOrFlightNo);
		}

		public void TestIsAir()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "KE1098";

			var import929 = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("KE1098", import929.VesselOrFlightNo);
		}

		public void TestFreightForwarder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var freightForwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "세관무역(주)");
			declaration.JE_OH_Forwarder = freightForwarder.PK;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("세관무역(주)", import929.FreightForwarderCompanyName);
			AssertNullOrEmpty(import929.FreightForwarderID);
			AssertNullOrEmpty(import929.CourierCompanyID);
		}

		public void TestFreightForwarder2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var freightForwarder = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "세관무역(주)");

			var freightForwarderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode,  Number = "ABCD" },
				new IDNumberAndType() { Type = IdentificationType.CourierCompanyID,  Number = "HJSC" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(freightForwarder, freightForwarderCodes);
			declaration.JE_OH_Forwarder = freightForwarder.PK;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("세관무역(주)", import929.FreightForwarderCompanyName);

			AssertEquals("ABCD", import929.FreightForwarderID);
			AssertEquals("HJSC", import929.CourierCompanyID);
		}

		public void TestBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertNullOrEmpty(import929.MasterBillNumber);
			AssertNullOrEmpty(import929.HouseBillNumber);
			AssertNullOrEmpty(import929.HouseBillSplitDeclarationReasonCode);
			AssertNullOrEmpty(import929.HouseBillSplitDeclarationReasonDescription);

			declaration.CustomsEntryHeaders.RemoveAll();

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillNum = "DBSC96100123AB01";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			houseBill.CU_HBSplitDecReasonCode = "A";
			houseBill.HBSplitDecReasonRemark = "a";
			houseBill.CU_BillNum = "HJSC98100123AB01";

			entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_ImportCargoManagementNumber = "01KE0766SS200100003";

			JobComInvoiceLine invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			#endregion

			import929 = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("01KE0766SS200100003", import929.CargoManagementNo);
			AssertEquals("DBSC96100123AB01", import929.MasterBillNumber);
			AssertEquals("HJSC98100123AB01", import929.HouseBillNumber);
			AssertEquals(true, import929.HouseBillSplitDeclarationIndicator);
			AssertEquals("A", import929.HouseBillSplitDeclarationReasonCode);
			AssertEquals("a", import929.HouseBillSplitDeclarationReasonDescription);
		}

		public void TestBillWhenNoMasterBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			houseBill.CU_HBSplitDecReasonCode = "A";
			houseBill.HBSplitDecReasonRemark = "a";
			houseBill.CU_BillNum = "HJSC98100123AB01";

			#region invoice
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_ImportCargoManagementNumber = "01KE0766SS200000003";

			JobComInvoiceLine invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			#endregion

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals("01KE0766SS200000003", import929.CargoManagementNo);
			AssertNull(import929.MasterBillNumber);
			AssertEquals("HJSC98100123AB01", import929.HouseBillNumber);
			AssertEquals(true, import929.HouseBillSplitDeclarationIndicator);
			AssertEquals("A", import929.HouseBillSplitDeclarationReasonCode);
			AssertEquals("a", import929.HouseBillSplitDeclarationReasonDescription);
		}

		public void TestTransportMode()
		{
			var declaration = CreateEmptyDeclaration();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var entry = declaration.CustomsEntryHeaders[0];

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Air, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.FixedTransportInstallations, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.PassengerHandCarried;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Pedestrian;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Auto;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Road, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.BorderWaterBorne;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.WarehouseHandling;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.All;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Unknown;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Courier;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Mail, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Rail, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Road, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.SeaAir;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Complex, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.AirSea;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Complex, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Sea, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.InlandWaterwayTransport, entryHeader.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.RollOnRollOff;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(Constants.TransportModes.Other, entryHeader.TransportMode);
		}

		public void TestImportEntryLine()
		{
			SetUpTariffData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0208100000";
			entryLine.CL_CustomsValue = 999999999990m;
			entryLine.CL_ValueForVAT = 11m;

			var entryLine1cud = entryLine.Fees.AddNew();
			entryLine1cud.CF_ChargeType = "DTY";
			entryLine1cud.CF_ChargeAmount = 999999999999m;

			var entryLine1vat = entryLine.Fees.AddNew();
			entryLine1vat.CF_ChargeType = "VAT";
			entryLine1vat.CF_ChargeAmount = 888888888888m;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
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
			hsExtensionCode2.CY_Code = "1N";

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
			invoiceLine.JI_PrimaryPreference = "F";
			invoiceLine.JI_AdditionalDutyRate = 99.99M;
			invoiceLine.JI_AdditionalDutyType = "I";
			invoiceLine.JI_DomesticTaxExemptionCode = "0000001";
			invoiceLine.JI_DomesticTaxCode = "AA-";
			invoiceLine.JI_CustomsUnitQty = "DZ";
			invoiceLine.JI_CustomsQuantity = 30m;
			invoiceLine.JI_ZZF_NKTaxType = "VTA";
			invoiceLine.JI_VATReductionCode = "11";
			#endregion

			#region invoiceLine2
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_SequenceNumber = 2;

			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_Weight = 30.2m;
			invoiceLine2.JI_NetWeight = 10.3;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_CustomsUnitQty = "DZ";
			invoiceLine2.JI_CustomsQuantity = 20m;
			invoiceLine2.JI_PrimaryPreference = "F";
			invoiceLine2.CriteriaForDeterminingCountryOfOrigin = "A";
			var certificate2 = invoiceLine2.CertificateOfOriginData;
			certificate2.CSI_ReferenceNumber = "12345678";
			certificate2.CSI_Procedure = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			certificate2.CSI_DateOfIssue = new ZDateTime(2013, 01, 01);
			certificate2.CSI_RN_NKCountryCode = "KR";
			certificate2.CSI_Description = "발행기관명";
			certificate2.CSI_AdditionalDescription = "발급지역명";
			certificate2.CSI_ReferenceNumber2 = "발급담당자명";
			certificate2.CSI_Status = Constants.YesNo.Yes;
			certificate2.CSI_Quantity = 10000m;
			certificate2.CSI_Quantity2 = 10000m;
			certificate2.CSI_Quantity3 = 10000m;
			#endregion

			var invoice2 = declaration.Invoices.AddNew();
			#region entryLine2
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0208122222";
			entryLine2.CL_CustomsValue = 9m;
			entryLine2.CL_ValueForVAT = 11m;

			var entryLine2cud = entryLine2.Fees.AddNew();
			entryLine2cud.CF_ChargeType = "DTY";
			entryLine2cud.CF_ChargeAmount = 777777777777m;

			var entryLine2vat = entryLine2.Fees.AddNew();
			entryLine2vat.CF_ChargeType = "VAT";
			entryLine2vat.CF_ChargeAmount = 666666666666m;
			#endregion

			#region invoiceLine3
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine3.JI_SequenceNumber = 1;

			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_Weight = 65.2m;

			invoiceLine3.JI_BrandCode = "XXXX";
			invoiceLine3.JI_BrandName = "상표명2";
			invoiceLine3.JI_Tariff = "0208122222";
			invoiceLine3.JI_SpecificUseCodeDutyRatePermitNo = "987654321";
			invoiceLine3.JI_Model = "RABBIT MEAT2";

			var hsExtensionCode3 = invoiceLine3.HSExtensionCodeCollection.AddNew();
			hsExtensionCode3.CY_Order = 0;
			hsExtensionCode3.CY_Code = "01";

			var hsExtensionCode4 = invoiceLine3.HSExtensionCodeCollection.AddNew();
			hsExtensionCode4.CY_Order = 1;
			hsExtensionCode4.CY_Code = "1R";

			var hsExtensionCode5 = invoiceLine3.HSExtensionCodeCollection.AddNew();
			hsExtensionCode5.CY_Order = 2;
			hsExtensionCode5.CY_Code = "2L";

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
			invoiceLine3.JI_CustomsUnitQty = "G";
			invoiceLine3.JI_CustomsQuantity = 30m;
			invoiceLine3.JI_ZZF_NKTaxType = "VTB";
			invoiceLine3.JI_VATReductionCode = "11";

			#endregion
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines.Length);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("EntryLines Should be ordered by CL_LineNumber", 1, importEntryLine1.EntryLineNo);
			AssertEquals("TACKS", importEntryLine1.HSDescription);
			AssertEquals("RABBIT MEAT", importEntryLine1.ModelName);
			AssertEquals("ZZZZ", importEntryLine1.BrandCode);
			AssertEquals("상표명", importEntryLine1.BrandName);
			AssertEquals("0208100000-01-1N", importEntryLine1.AdditionalTariffCode);
			AssertEquals("0208100000", importEntryLine1.HSCode);
			AssertEquals("123456789", importEntryLine1.SpecificUseCodeDutyRatePermitNo);
			AssertEquals("12345678", importEntryLine1.CertificateOfOriginNo);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.A, importEntryLine1.CertificateOfOriginCriteriaCode);
			AssertEquals(new ZDateTime(2013, 01, 01), importEntryLine1.CertificateOfOriginIssueDate);
			AssertEquals("KR", importEntryLine1.CertificateOfOriginIssuingCountry);
			AssertEquals("발행기관명", importEntryLine1.CertificateOfOriginAgencyName);
			AssertEquals("발급지역명", importEntryLine1.CertificateOfOriginAreaName);
			AssertEquals("발급담당자명", importEntryLine1.CertificateOfOriginPersonName);
			AssertEquals("Y", importEntryLine1.CertificateOfOriginSplitIndicator);
			AssertEquals("CN", importEntryLine1.CountryOfOrigin);
			AssertEquals("A", importEntryLine1.CountryOfOriginDeterminationRule);
			AssertEquals("Y", importEntryLine1.CountryOfOriginLabelLocation);
			AssertEquals("A", importEntryLine1.CountryOfOriginLabelType);
			AssertEquals("14", importEntryLine1.CertificateOfOriginExemptionReason);
			AssertEquals("7", importEntryLine1.ProductOrMaterialCode);
			AssertEquals(123, importEntryLine1.MaterialLineNo);
			AssertEquals("Y", importEntryLine1.MightRequireInspectionIndicator);
			AssertEquals("023", importEntryLine1.PostClearanceProcedureAgency1);
			AssertEquals("019", importEntryLine1.PostClearanceProcedureAgency2);
			AssertEquals("020", importEntryLine1.PostClearanceProcedureAgency3);
			AssertEquals(100010.2m, importEntryLine1.NetWeightInKG);
			AssertEquals("DZ", importEntryLine1.QuantityUnit);
			AssertEquals(50m, importEntryLine1.Quantity);
			AssertEquals(999999999990m, importEntryLine1.CustomsValueKRW);
			AssertEquals("Y", importEntryLine1.CourierCargoSelectivityIndicator);
			AssertEquals("F", importEntryLine1.DutyRateCode);
			AssertEquals(99.99M, importEntryLine1.AdditionalDutyRate);
			AssertEquals("I", importEntryLine1.AdditionalDutyCode);
			AssertEquals(999999999999m, importEntryLine1.DutyAmount);
			AssertEquals("AA", importEntryLine1.DomesticTaxCode);
			AssertEquals("A", importEntryLine1.VATRateCode);
			AssertEquals(null, importEntryLine1.VATReductionCode);
			AssertEquals(11m, importEntryLine1.ValueForVAT);
			AssertEquals(888888888888m, importEntryLine1.VATAmount);
			AssertEquals("", importEntryLine1.EducationTaxExemptIndicator);

			var importEntryLine2 = entryHeader.EntryLines[1];
			AssertEquals("EntryLines Should be ordered by CL_LineNumber", 2, importEntryLine2.EntryLineNo);
			AssertEquals("TACKS2", importEntryLine2.HSDescription);
			AssertEquals("RABBIT MEAT2", importEntryLine2.ModelName);
			AssertEquals("XXXX", importEntryLine2.BrandCode);
			AssertEquals("상표명2", importEntryLine2.BrandName);
			AssertEquals("0208122222-01-1R-2L", importEntryLine2.AdditionalTariffCode);
			AssertEquals("0208122222", importEntryLine2.HSCode);
			AssertEquals("987654321", importEntryLine2.SpecificUseCodeDutyRatePermitNo);
			AssertEquals("87654321", importEntryLine2.CertificateOfOriginNo);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.B, importEntryLine2.CertificateOfOriginCriteriaCode);
			AssertEquals(new ZDateTime(2013, 02, 02), importEntryLine2.CertificateOfOriginIssueDate);
			AssertEquals("KR", importEntryLine2.CertificateOfOriginIssuingCountry);
			AssertEquals("발행기관명2", importEntryLine2.CertificateOfOriginAgencyName);
			AssertEquals("발급지역명2", importEntryLine2.CertificateOfOriginAreaName);
			AssertEquals("발급담당자명2", importEntryLine2.CertificateOfOriginPersonName);
			AssertEquals("Y", importEntryLine2.CertificateOfOriginSplitIndicator);
			AssertEquals("CN", importEntryLine2.CountryOfOrigin);
			AssertEquals("1", importEntryLine2.CountryOfOriginDeterminationRule);
			AssertEquals("Y", importEntryLine2.CountryOfOriginLabelLocation);
			AssertEquals("D", importEntryLine2.CountryOfOriginLabelType);
			AssertEquals("12", importEntryLine2.CertificateOfOriginExemptionReason);
			AssertEquals("8", importEntryLine2.ProductOrMaterialCode);
			AssertEquals(123, importEntryLine2.MaterialLineNo);
			AssertEquals("Y", importEntryLine2.MightRequireInspectionIndicator);
			AssertEquals("024", importEntryLine2.PostClearanceProcedureAgency1);
			AssertEquals("020", importEntryLine2.PostClearanceProcedureAgency2);
			AssertEquals("021", importEntryLine2.PostClearanceProcedureAgency3);
			AssertEquals(99999.9m, importEntryLine2.NetWeightInKG);
			AssertEquals(null, importEntryLine2.QuantityUnit);
			AssertEquals(0m, importEntryLine2.Quantity);
			AssertEquals(9m, importEntryLine2.CustomsValueKRW);
			AssertEquals("Y", importEntryLine2.CourierCargoSelectivityIndicator);
			AssertEquals("x", importEntryLine2.DutyRateCode);
			AssertEquals(99.99M, importEntryLine2.AdditionalDutyRate);
			AssertEquals("I", importEntryLine2.AdditionalDutyCode);
			AssertEquals(777777777777m, importEntryLine2.DutyAmount);
			AssertEquals("AA", importEntryLine2.DomesticTaxCode);
			AssertEquals("B", importEntryLine2.VATRateCode);
			AssertEquals("11", importEntryLine2.VATReductionCode);
			AssertEquals(0m, importEntryLine2.ValueForVAT);
			AssertEquals(0m, importEntryLine2.VATAmount);
			AssertEquals("", importEntryLine2.EducationTaxExemptIndicator);
		}

		public void TestImportEntryLine2()
		{
			SetUpTariffData();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_ValueExemptForVAT = 10m;
			entryLine.CL_DutyReductionAmount = 3000m;

			var entryLine1dty = entryLine.Fees.AddNew();
			entryLine1dty.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			entryLine1dty.CF_Rate = 8m;

			var entryLine1edt = entryLine.Fees.AddNew();
			entryLine1edt.CF_ChargeType = ChargeTypeList.Codes.EducationTax;
			entryLine1edt.CF_ChargeAmount = 9999m;

			var entryLine1agt = entryLine.Fees.AddNew();
			entryLine1agt.CF_ChargeType = ChargeTypeList.Codes.AgricultureTax;
			entryLine1agt.CF_ChargeAmount = 888m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_DrawbackUQ = "PC";
			invoiceLine.JI_DrawbackQuantity = 10m;
			#endregion

			#region invoiceLine2
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_DrawbackUQ = "PC";
			invoiceLine2.JI_DrawbackQuantity = 20m;
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(1, entryHeader.EntryLines.Length);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("PC", importEntryLine1.QuantityUQToClaimRefund);
			AssertEquals(30m, importEntryLine1.QuantityToClaimRefund);
			AssertEquals(8m, importEntryLine1.SpecificDutyRate);
			AssertEquals(10m, importEntryLine1.ValueExemptForVAT);
			AssertEquals(3000m, importEntryLine1.DutyReductionAmount);
			AssertEquals(9999m, importEntryLine1.EducationTaxAmount);
			AssertEquals(888m, importEntryLine1.AgricultureTax);
		}

		public void TestDATandDTS()
		{
			#region ZZ Data

			var zzDataSetUpper = new UniversalReferenceTestDataHelper(Factory);
			zzDataSetUpper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = zzDataSetUpper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var dutyRateType = zzDataSetUpper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var adValoremRateCode = zzDataSetUpper.LoadOrCreateNewCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, dutyRateType.PK);
			var specificRateCode = zzDataSetUpper.LoadOrCreateNewCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, dutyRateType.PK);

			var cnTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.China);
			zzDataSetUpper.AddCountry(cnTradeGroup, Core.Constants.CountryCodes.China);
			var fcn1Preference = zzDataSetUpper.CreatePreferenceForCountry("FCN1", "한ㆍ중국 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);

			var allTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.All);
			zzDataSetUpper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.China);
			var aPreference = zzDataSetUpper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff = zzDataSetUpper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299000", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var allRate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.08", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.005", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff2 = zzDataSetUpper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299010", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var allRate2 = zzDataSetUpper.CreateRate(hsTariff2, specificRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "[KG] * 8000", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate2 = zzDataSetUpper.CreateRate(hsTariff2, specificRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "[KG] * 3000", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			zzDataSetUpper.CreateCusApplicability(allRate, allTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(fcn1Rate, cnTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(allRate2, allTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(fcn1Rate2, cnTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;

			invoiceLine.JI_Tariff = "0101299000";
			invoiceLine.JI_PrimaryPreference = "FCN1";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_SequenceNumber = 1;
			invoiceLine2.JI_Tariff = "0101299010";
			invoiceLine2.JI_PrimaryPreference = "FCN1";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines.Length);
			AssertEquals("1", entryHeader.EntryLines[0].DutyRateTypeCode);
			AssertEquals("3", entryHeader.EntryLines[1].DutyRateTypeCode);
		}

		public void TestDutyReductionClassificationCodeA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A088000101", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제88조제1항제1호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);

			var cusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var cusRateCode = helper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyReductionRate, cusRateType.PK);
			helper.CreateRefCusRate(tariff1.PK, cusRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 1", null, "100", Core.Constants.CountryCodes.KoreaSouth);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 3000m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "A088000101";
			invoiceLine.JI_InstallmentCode = "";
			#endregion
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];

			AssertEquals("A", importEntryLine1.DutyReductionClassification);
			AssertEquals("A088000101", importEntryLine1.DutyReductionOrInstallmentCode);
			AssertEquals(100m, importEntryLine1.DutyReductionRate);
			AssertEquals(3000m, importEntryLine1.DutyReductionAmount);
		}

		public void TestDutyReductionClassificationCodeB()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A090010201", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제88조제1항제1호 해당물품");
			var cusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var cusRateCode = helper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyReductionRate, cusRateType.PK);
			helper.CreateRefCusRate(tariff1.PK, cusRateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "VFD * 0.8", null, "80", "");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 8258m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "A090010201";
			invoiceLine.JI_InstallmentCode = "";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("B", importEntryLine1.DutyReductionClassification);
			AssertEquals("A090010201", importEntryLine1.DutyReductionOrInstallmentCode);
			AssertEquals(80m, importEntryLine1.DutyReductionRate);
			AssertEquals(8258m, importEntryLine1.DutyReductionAmount);
		}

		public void TestDutyReductionClassificationCodeC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 10m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_InstallmentCode = "A10700020111";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];

			AssertEquals("C", importEntryLine1.DutyReductionClassification);
			AssertEquals("A10700020111", importEntryLine1.DutyReductionOrInstallmentCode);
			AssertEquals(10m, importEntryLine1.DutyReductionAmount);
		}

		public void TestDutyReductionClassificationCodeD()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 20m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_InstallmentCode = "";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("D", importEntryLine1.DutyReductionClassification);
			AssertEquals(20m, importEntryLine1.DutyReductionAmount);
		}

		public void TestDutyReductionClassificationCodeT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A090010201", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "과학장비의 일시수입에 관한 관세협약");
			var cusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var cusRateCode = helper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyReductionRate, cusRateType.PK);
			helper.CreateRefCusRate(tariff1.PK, cusRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0", null, "0%", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_DutyReductionAmount = 200m;

			#endregion
			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_IsSpecificUseCode = true;
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_InstallmentCode = "A090010201";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("T", importEntryLine1.DutyReductionClassification);
			AssertEquals("A090010201", importEntryLine1.DutyReductionOrInstallmentCode);
			AssertEquals(0m, importEntryLine1.DutyReductionRate);
			AssertEquals(200m, importEntryLine1.DutyReductionAmount);
		}

		public void TestDomesticTaxClassificationLQT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "941210-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "약주");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.LiquorTax, tariff1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var entryLine2sct = entryLine.Fees.AddNew();
			entryLine2sct.CF_ChargeType = "LQT";
			entryLine2sct.CF_Rate = 20m;
			entryLine2sct.CF_ChargeAmount = 300m;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_UseCode = "";
			invoiceLine.JI_DomesticTaxCode = "941210-A";
			invoiceLine.JI_DomesticTaxExemptionCode = "E103003";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("10", importEntryLine1.DomesticTaxClassification);
			AssertEquals("E103003", importEntryLine1.ExemptionCodeOfLiquorTax);
			AssertEquals(20m, importEntryLine1.DomesticTaxRate);
			AssertEquals(300m, importEntryLine1.LiquorTax);
		}

		public void TestDomesticTaxClassificationTRT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "821100-C", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "휘발유와 이와 유사한 대체유류");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.TransportationTax, tariff1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var entryLine3trt = entryLine.Fees.AddNew();
			entryLine3trt.CF_ChargeType = "TRT";
			entryLine3trt.CF_Rate = 10m;
			entryLine3trt.CF_ChargeAmount = 100m;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_UseCode = "";
			invoiceLine.JI_DomesticTaxCode = "821100-C";
			invoiceLine.JI_DomesticTaxExemptionCode = "";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("6C", importEntryLine1.DomesticTaxClassification);
			AssertEquals(10m, importEntryLine1.DomesticTaxRate);
			AssertEquals(100m, importEntryLine1.TransportationTax);

			invoiceLine.JI_DomesticTaxExemptionCode = "0000001";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("60", importEntryLine1.DomesticTaxClassification);
			AssertEquals("0000001", importEntryLine1.ExemptionCodeOfTransportationTax);
		}

		public void TestDomesticTaxClassificationSCT()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "803008-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "냄새 맡는 담배");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.SpecialConsumptionTax, tariff1);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var entryLine1sct = entryLine.Fees.AddNew();
			entryLine1sct.CF_ChargeType = "SCT";
			entryLine1sct.CF_Rate = 43m;
			entryLine1sct.CF_ChargeAmount = 130m;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_UseCode = "2";
			invoiceLine.JI_DomesticTaxCode = "803008-A";
			invoiceLine.JI_DomesticTaxExemptionCode = "L140101";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("20", importEntryLine1.DomesticTaxClassification);
			AssertEquals("L140101", importEntryLine1.ExemptionCodeOfSpecialConsumptionTax);
			AssertEquals(43m, importEntryLine1.DomesticTaxRate);
			AssertEquals(130m, importEntryLine1.SpecialConsumptionTax);

			invoiceLine.JI_DomesticTaxExemptionCode = "";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("2A", importEntryLine1.DomesticTaxClassification);
		}

		public void TestAgricultureTaxClassificationCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "E118000103", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "조세특례제한법 제118조(관세의 경감) 제1항제3호 해당물품 (신재생에너지 관련 기자재)");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, YesNo.Yes, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "E118000104", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "조세특례제한법 제118조(관세의 경감) 제1항제3호 해당물품 (신재생에너지 관련 기자재)");

			var tariffType2 = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType2.PK, "602001-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "수렵용 총포류");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.AgricultureTaxBApplies, YesNo.Yes, tariff3);
			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType2.PK, "602002-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "수렵용 총포류");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_SecondaryPreference = "E118000103";
			invoiceLine.JI_DomesticTaxCode = "";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("A", importEntryLine1.AgricultureTaxClassification);

			invoiceLine.JI_DomesticTaxCode = "602001-A";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("C", importEntryLine1.AgricultureTaxClassification);

			invoiceLine.JI_SecondaryPreference = "E118000104";
			invoiceLine.JI_DomesticTaxCode = "";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(ZString.Empty, importEntryLine1.AgricultureTaxClassification);

			invoiceLine.JI_DomesticTaxCode = "602001-A";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals("B", importEntryLine1.AgricultureTaxClassification);

			invoiceLine.JI_DomesticTaxCode = "602002-A";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(ZString.Empty, importEntryLine1.AgricultureTaxClassification);
		}

		public void TestDomesticTaxBaseQtyOrPrice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "941210-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "탁주");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.LiquorTax, tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "424000-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "탁주");
			helper.CreateTariffUOM(tariff2, "CU1", DomesticTaxBaseQtyOrPriceCode.UQs.Unit);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_DomesticTaxCode = "941210-A";

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "AC";
			invoiceLine.JI_CustomsSecondQuantity = 30m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "U";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "MIN";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(10m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "PC";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "AC";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "PC";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(20m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "AC";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "AC";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "PC";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(10m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_DomesticTaxCode = "524000-A";
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsSecondQuantity = 30m;
			invoiceLine.JI_CustomsSecondUnitQty = "GR";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "L";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(0m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_DomesticTaxCode = "424000-A";
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsSecondQuantity = 30m;
			invoiceLine.JI_CustomsSecondUnitQty = "GR";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "L";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(20m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsSecondQuantity = 30m;
			invoiceLine.JI_CustomsSecondUnitQty = "U";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "L";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(30m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.JI_CustomsSecondUnitQty = "U";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "U";
			invoiceLine.JI_CustomsFourthQuantity = 50m;
			invoiceLine.JI_CustomsFourthUnitQty = "L";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(10m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_Tariff = "8523292231";
			invoiceLine.JI_DomesticTaxCode = "";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "MIN";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			invoiceLine.JI_CustomsThirdUnitQty = "AC";
			invoiceLine.JI_CustomsFourthQuantity = 40m;
			invoiceLine.JI_CustomsFourthUnitQty = "U";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(10m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "AC";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			invoiceLine.JI_CustomsThirdUnitQty = "MIN";
			invoiceLine.JI_CustomsFourthQuantity = 40m;
			invoiceLine.JI_CustomsFourthUnitQty = "U";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(30m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 40m;
			invoiceLine.JI_CustomsThirdUnitQty = "MIN";
			invoiceLine.JI_CustomsFourthQuantity = 30m;
			invoiceLine.JI_CustomsFourthUnitQty = "MIN";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(40m, importEntryLine1.DomesticTaxBaseQtyOrPrice);

			invoiceLine.JI_Tariff = "9123292231";
			invoiceLine.JI_DomesticTaxCode = "";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "MIN";
			invoiceLine.JI_CustomsSecondQuantity = 20m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 30m;
			invoiceLine.JI_CustomsThirdUnitQty = "AC";
			invoiceLine.JI_CustomsFourthQuantity = 40m;
			invoiceLine.JI_CustomsFourthUnitQty = "U";

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(0m, importEntryLine1.DomesticTaxBaseQtyOrPrice);
		}

		public void TestSumDomesticTaxBaseQtyOrPrice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDMT = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DomesticTaxRate);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "424000-A", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffUOM(tariff, "CU1", DomesticTaxBaseQtyOrPriceCode.UQs.Unit);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();

			#region invoiceLine
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_DomesticTaxCode = "424000-A";
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine.JI_CustomsSecondQuantity = 2000m;
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdQuantity = 3000m;
			invoiceLine.JI_CustomsThirdUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.Unit;
			invoiceLine.JI_CustomsFourthQuantity = 4000m;
			invoiceLine.JI_CustomsFourthUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_DomesticTaxCode = "424000-A";
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.Unit;
			invoiceLine2.JI_CustomsSecondQuantity = 20m;
			invoiceLine2.JI_CustomsSecondUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
			invoiceLine2.JI_CustomsThirdQuantity = 30m;
			invoiceLine2.JI_CustomsThirdUnitQty = "KG";
			invoiceLine2.JI_CustomsFourthQuantity = 40m;
			invoiceLine2.JI_CustomsFourthUnitQty = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
			#endregion

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders.FirstOrDefault();
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(3010m, importEntryLine1.DomesticTaxBaseQtyOrPrice);
		}

		public void TestNonGADetail()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines.AddNew();

			#region NonGADetail
			var nonGADetail11 = entryLine.NonGADetailCollection.AddNew();
			nonGADetail11.CSI_LineNo = 2;
			nonGADetail11.CSI_Code = "B";
			nonGADetail11.CSI_Procedure = "06";
			nonGADetail11.CSI_Description = "기타사유2";

			var nonGADetail12 = entryLine.NonGADetailCollection.AddNew();
			nonGADetail12.CSI_LineNo = 1;
			nonGADetail12.CSI_Code = "A";
			nonGADetail12.CSI_Procedure = "05";
			nonGADetail12.CSI_Description = "기타사유1";
			nonGADetail12.CSI_Status = "02";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines[0].NonGADetails.Length);
			var nonGADetail1 = entryHeader.EntryLines[0].NonGADetails[0];
			AssertEquals("nonGADetails Should be ordered by CSI_LineNo", 1, nonGADetail1.SequenceNo);
			AssertEquals("A", nonGADetail1.ReasonType);
			AssertEquals("05", nonGADetail1.RegulationCategoryCode);
			AssertEquals("기타사유1", nonGADetail1.Reason);
			AssertEquals("05A02", nonGADetail1.NonGAReasonType);

			var nonGADetail2 = entryHeader.EntryLines[0].NonGADetails[1];
			AssertEquals("nonGADetails Should be ordered by CSI_LineNo", 2, nonGADetail2.SequenceNo);
			AssertEquals("B", nonGADetail2.ReasonType);
			AssertEquals("06", nonGADetail2.RegulationCategoryCode);
			AssertEquals("기타사유2", nonGADetail2.Reason);
			AssertEquals(string.Empty, nonGADetail2.NonGAReasonType);
		}

		public void TestImportPreviousExpDecLine()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines.AddNew();

			#region ImportPreviousExpDecLine
			var importPreviousExpDecLine11 = entryLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine11.CSI_LineNo = 1;
			importPreviousExpDecLine11.CSI_ReferenceNumber = "7654321";
			importPreviousExpDecLine11.CSI_ReferenceNumber2 = "321";
			importPreviousExpDecLine11.CSI_ItemNumber = 21;
			importPreviousExpDecLine11.CSI_UnitOfQuantity = PackageKindCodeList.Codes.CT;
			importPreviousExpDecLine11.CSI_Quantity = 321;

			var importPreviousExpDecLine12 = entryLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine12.CSI_LineNo = 2;
			importPreviousExpDecLine12.CSI_ReferenceNumber = "1234567";
			importPreviousExpDecLine12.CSI_ReferenceNumber2 = "123";
			importPreviousExpDecLine12.CSI_ItemNumber = 12;
			importPreviousExpDecLine12.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
			importPreviousExpDecLine12.CSI_Quantity = 123;

			var importPreviousExpDecLine14 = entryLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine14.CSI_LineNo = 4;
			importPreviousExpDecLine14.CSI_ReferenceNumber = "7654321";
			importPreviousExpDecLine14.CSI_ReferenceNumber2 = "321";
			importPreviousExpDecLine14.CSI_ItemNumber = 22;
			importPreviousExpDecLine14.CSI_UnitOfQuantity = PackageKindCodeList.Codes.CT;
			importPreviousExpDecLine14.CSI_Quantity = 321;

			var importPreviousExpDecLine13 = entryLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine13.CSI_LineNo = 3;
			importPreviousExpDecLine13.CSI_ReferenceNumber = "1234567";
			importPreviousExpDecLine13.CSI_ReferenceNumber2 = "122";
			importPreviousExpDecLine13.CSI_ItemNumber = 12;
			importPreviousExpDecLine13.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
			importPreviousExpDecLine13.CSI_Quantity = 123;
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(4, entryHeader.EntryLines[0].PreviousExpDecLines.Length);
			var importPreviousExpDecLine1 = entryHeader.EntryLines[0].PreviousExpDecLines[0];
			AssertEquals((short)1, importPreviousExpDecLine1.SequenceNumber);
			AssertEquals("7654321", importPreviousExpDecLine1.DeclarationNumber);
			AssertEquals(321, importPreviousExpDecLine1.EntryLineNo);
			AssertEquals(21, importPreviousExpDecLine1.InvoiceLineNo);
			AssertEquals("CT", importPreviousExpDecLine1.UQ);
			AssertEquals(321m, importPreviousExpDecLine1.UsedQty);

			var importPreviousExpDecLine2 = entryHeader.EntryLines[0].PreviousExpDecLines[1];
			AssertEquals((short)2, importPreviousExpDecLine2.SequenceNumber);
			AssertEquals("1234567", importPreviousExpDecLine2.DeclarationNumber);
			AssertEquals(123, importPreviousExpDecLine2.EntryLineNo);
			AssertEquals(12, importPreviousExpDecLine2.InvoiceLineNo);
			AssertEquals("KG", importPreviousExpDecLine2.UQ);
			AssertEquals(123m, importPreviousExpDecLine2.UsedQty);

			var importPreviousExpDecLine3 = entryHeader.EntryLines[0].PreviousExpDecLines[2];
			AssertEquals((short)3, importPreviousExpDecLine3.SequenceNumber);
			AssertEquals("1234567", importPreviousExpDecLine3.DeclarationNumber);
			AssertEquals(122, importPreviousExpDecLine3.EntryLineNo);
			AssertEquals(12, importPreviousExpDecLine3.InvoiceLineNo);
			AssertEquals("KG", importPreviousExpDecLine3.UQ);
			AssertEquals(123m, importPreviousExpDecLine3.UsedQty);

			var importPreviousExpDecLine4 = entryHeader.EntryLines[0].PreviousExpDecLines[3];
			AssertEquals((short)4, importPreviousExpDecLine4.SequenceNumber);
			AssertEquals("7654321", importPreviousExpDecLine4.DeclarationNumber);
			AssertEquals(321, importPreviousExpDecLine4.EntryLineNo);
			AssertEquals(22, importPreviousExpDecLine4.InvoiceLineNo);
			AssertEquals("CT", importPreviousExpDecLine4.UQ);
			AssertEquals(321m, importPreviousExpDecLine4.UsedQty);
		}

		public void TestImportInvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);
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
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_Description = "모델 및 규격1";
			invoiceLine1.JI_Ingredient = "성분1";
			invoiceLine1.JI_InvoiceUQ = "PC";
			invoiceLine1.JI_InvoiceQuantity = 3m;
			invoiceLine1.JI_LinePrice = 30.66m;
			invoiceLine1.UnitPrice = 10.22m;
			invoiceLine1.JI_CustomsUnitQty = "U";
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_LotNumber = "K12123";

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "1234560000";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = entryLine2.CL_AdValoremTariff;
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.UnitPrice = 100m;
			invoiceLine2.JI_CustomsUnitQty = "U";
			invoiceLine2.JI_CustomsQuantity = 10m;
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines.Length);
			AssertEquals(1, entryHeader.EntryLines[0].InvoiceLines.Length);
			var importInvoiceLine1 = entryHeader.EntryLines[0].InvoiceLines[0];
			AssertEquals(1, importInvoiceLine1.InvoiceLineNo);
			AssertEquals("모델 및 규격1", importInvoiceLine1.ItemDescription);
			AssertEquals("성분1", importInvoiceLine1.Ingredient);
			AssertEquals("U", importInvoiceLine1.InvoiceUnitOfQuantiy);
			AssertEquals(1m, importInvoiceLine1.InvoiceQuantity);
			AssertEquals(30.66m, importInvoiceLine1.Amount);
			AssertEquals(30.66m, importInvoiceLine1.UnitPrice);
			AssertEquals("K12123", importInvoiceLine1.PartNumber);

			var importInvoiceLine2 = entryHeader.EntryLines[1].InvoiceLines[0];
			AssertEquals(2, importInvoiceLine2.InvoiceLineNo);
			AssertEquals("PC", importInvoiceLine2.InvoiceUnitOfQuantiy);
			AssertEquals(5m, importInvoiceLine2.InvoiceQuantity);
			AssertEquals(500m, importInvoiceLine2.Amount);
			AssertEquals(100m, importInvoiceLine2.UnitPrice);
		}

		public void TestImportGAApprovalDocument()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			var invoiceLine = declaration.InvoiceLines[0];

			#region ApprovalDocument
			var approvalDocument11 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument11.CSI_LineNo = 2;
			approvalDocument11.CSI_SubType = CommodityUsageCodeList.Codes._12;
			approvalDocument11.CSI_ReferenceNumber = "02";
			approvalDocument11.CSI_Procedure = "98";
			approvalDocument11.CSI_Description = "서류명2";
			approvalDocument11.CSI_DateOfIssue = new ZDateTime("2012-01-02");
			approvalDocument11.CSI_Code = ImportRequirementTypeCodeList.Codes._1;
			approvalDocument11.CSI_ReferenceNumber2 = "23655421";

			var approvalDocument12 = invoiceLine.GAApprovalDataCollection.AddNew();
			approvalDocument12.CSI_LineNo = 1;
			approvalDocument12.CSI_SubType = CommodityUsageCodeList.Codes._11;
			approvalDocument12.CSI_ReferenceNumber = "01";
			approvalDocument12.CSI_Procedure = "12";
			approvalDocument12.CSI_Description = "서류명1";
			approvalDocument12.CSI_DateOfIssue = new ZDateTime("2012-01-01");
			approvalDocument12.CSI_Code = ImportRequirementTypeCodeList.Codes._2;
			approvalDocument12.CSI_ReferenceNumber2 = "12455632";
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments.Length);
			var approvalDocument1 = entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments[0];
			AssertEquals("approvalDocuments Should be ordered by CSI_LineNo", 1, approvalDocument1.SequenceNo);
			AssertEquals("2", approvalDocument1.RequirementDocumentType);
			AssertEquals("01", approvalDocument1.RequirementApprovalNumber);
			AssertEquals("12", approvalDocument1.RegulationCategoryCode);
			AssertEquals("서류명1", approvalDocument1.DocumentName);
			AssertEquals(new ZDateTime("2012-01-01"), approvalDocument1.ApprovalDate);
			AssertEquals("11", approvalDocument1.UseCode);
			AssertEquals("12455632", approvalDocument1.UniqueItemID);

			var approvalDocument2 = entryHeader.EntryLines[0].InvoiceLines[0].GAApprovalDocuments[1];
			AssertEquals("approvalDocuments Should be ordered by CSI_LineNo", 2, approvalDocument2.SequenceNo);
			AssertEquals("1", approvalDocument2.RequirementDocumentType);
			AssertEquals("02", approvalDocument2.RequirementApprovalNumber);
			AssertEquals("98", approvalDocument2.RegulationCategoryCode);
			AssertEquals("서류명2", approvalDocument2.DocumentName);
			AssertEquals(new ZDateTime("2012-01-02"), approvalDocument2.ApprovalDate);
			AssertEquals("12", approvalDocument2.UseCode);
			AssertEquals("23655421", approvalDocument2.UniqueItemID);
		}

		public void TestImportInvoiceLineOrdered()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 3;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_SequenceNumber = 2;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine.PK;
			invoiceLine3.JI_SequenceNumber = 1;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(3, entryHeader.EntryLines[0].InvoiceLines.Length);
			var importInvoiceLine1 = entryHeader.EntryLines[0].InvoiceLines[0];
			AssertEquals(1, importInvoiceLine1.InvoiceLineNo);

			var importInvoiceLine2 = entryHeader.EntryLines[0].InvoiceLines[1];
			AssertEquals(2, importInvoiceLine2.InvoiceLineNo);

			var importInvoiceLine3 = entryHeader.EntryLines[0].InvoiceLines[2];
			AssertEquals(3, importInvoiceLine3.InvoiceLineNo);
		}

		public void TestInvoiceLineDataCheckConditional()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importInvoiceLine1 = entryHeader.EntryLines[0].InvoiceLines;
			AssertEquals(1, importInvoiceLine1.Length);
			AssertEquals(0, importInvoiceLine1[0].InvoiceLineNo);
			AssertEquals("", importInvoiceLine1[0].ItemDescription);
			AssertEquals("", importInvoiceLine1[0].Ingredient);
			AssertEquals("", importInvoiceLine1[0].InvoiceUnitOfQuantiy);
			AssertEquals(0m, importInvoiceLine1[0].InvoiceQuantity);
			AssertEquals(0m, importInvoiceLine1[0].UnitPrice);
			AssertEquals(0m, importInvoiceLine1[0].Amount);
			AssertEquals("", importInvoiceLine1[0].PartNumber);
		}

		public void TestEntryLineDataCheckConditional()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(null, importEntryLine.AdditionalTariffCode);
			AssertEquals("", importEntryLine.SpecificUseCodeDutyRatePermitNo);
			AssertEquals(null, importEntryLine.CertificateOfOriginNo);
			AssertEquals(null, importEntryLine.CertificateOfOriginCriteriaCode);
			AssertEquals(ZDateTime.Empty, importEntryLine.CertificateOfOriginIssueDate);
			AssertEquals(null, importEntryLine.CertificateOfOriginIssuingCountry);
			AssertEquals(null, importEntryLine.CertificateOfOriginAgencyName);
			AssertEquals(null, importEntryLine.CertificateOfOriginAreaName);
			AssertEquals(null, importEntryLine.CertificateOfOriginPersonName);
			AssertEquals(null, importEntryLine.CertificateOfOriginSplitIndicator);
			AssertEquals("", importEntryLine.CountryOfOriginLabelType);
			AssertEquals(ZString.Empty, importEntryLine.CertificateOfOriginExemptionReason);
			AssertEquals("", importEntryLine.ProductOrMaterialCode);
			AssertEquals(0, importEntryLine.MaterialLineNo);
			AssertEquals("", importEntryLine.MightRequireInspectionIndicator);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency1);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency2);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency3);
			AssertEquals(0m, importEntryLine.NetWeightInKG);
			AssertEquals("", importEntryLine.QuantityUnit);
			AssertEquals(0m, importEntryLine.CustomsValueKRW);
			AssertEquals(0m, importEntryLine.CustomsValueUSD);
			AssertEquals("", importEntryLine.CourierCargoSelectivityIndicator);
			AssertEquals(0m, importEntryLine.AdditionalDutyRate);
			AssertEquals("", importEntryLine.AdditionalDutyCode);
			AssertEquals(null, importEntryLine.VATRateCode);
			AssertEquals(null, importEntryLine.VATReductionCode);
			AssertEquals(0m, importEntryLine.ValueForVAT);
			AssertEquals(0m, importEntryLine.VATAmount);
			AssertEquals("", importEntryLine.EducationTaxExemptIndicator);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine2.PK;
			entryHeader = new ImportEntryHeaderCreator().Create(entry2);
			importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(null, importEntryLine.AdditionalTariffCode);
			AssertEquals("", importEntryLine.SpecificUseCodeDutyRatePermitNo);
			AssertEquals(null, importEntryLine.CertificateOfOriginNo);
			AssertEquals(null, importEntryLine.CertificateOfOriginCriteriaCode);
			AssertEquals(ZDateTime.Empty, importEntryLine.CertificateOfOriginIssueDate);
			AssertEquals(null, importEntryLine.CertificateOfOriginIssuingCountry);
			AssertEquals(null, importEntryLine.CertificateOfOriginAgencyName);
			AssertEquals(null, importEntryLine.CertificateOfOriginAreaName);
			AssertEquals(null, importEntryLine.CertificateOfOriginPersonName);
			AssertEquals(null, importEntryLine.CertificateOfOriginSplitIndicator);
			AssertEquals("", importEntryLine.CountryOfOriginLabelType);
			AssertEquals("", importEntryLine.CertificateOfOriginExemptionReason);
			AssertEquals("", importEntryLine.ProductOrMaterialCode);
			AssertEquals(0, importEntryLine.MaterialLineNo);
			AssertEquals("", importEntryLine.MightRequireInspectionIndicator);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency1);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency2);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency3);
			AssertEquals(0m, importEntryLine.NetWeightInKG);
			AssertEquals("", importEntryLine.QuantityUnit);
			AssertEquals(0m, importEntryLine.CustomsValueKRW);
			AssertEquals(0m, importEntryLine.CustomsValueUSD);
			AssertEquals("", importEntryLine.CourierCargoSelectivityIndicator);
			AssertEquals(0m, importEntryLine.AdditionalDutyRate);
			AssertEquals("", importEntryLine.AdditionalDutyCode);
			AssertEquals(null, importEntryLine.VATRateCode);
			AssertEquals(null, importEntryLine.VATReductionCode);
			AssertEquals(0m, importEntryLine.ValueForVAT);
			AssertEquals(0m, importEntryLine.VATAmount);
			AssertEquals("", importEntryLine.EducationTaxExemptIndicator);

			invoiceLine.CertificateOfOriginIssueStatus = "";

			entryHeader = new ImportEntryHeaderCreator().Create(entry2);
			importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(null, importEntryLine.AdditionalTariffCode);
			AssertEquals("", importEntryLine.SpecificUseCodeDutyRatePermitNo);
			AssertEquals("", importEntryLine.CertificateOfOriginNo);
			AssertEquals("", importEntryLine.CertificateOfOriginCriteriaCode);
			AssertEquals(ZDateTime.Empty, importEntryLine.CertificateOfOriginIssueDate);
			AssertEquals("", importEntryLine.CertificateOfOriginIssuingCountry);
			AssertEquals("", importEntryLine.CertificateOfOriginAgencyName);
			AssertEquals("", importEntryLine.CertificateOfOriginAreaName);
			AssertEquals("", importEntryLine.CertificateOfOriginPersonName);
			AssertEquals("", importEntryLine.CertificateOfOriginSplitIndicator);
			AssertEquals("", importEntryLine.CountryOfOriginLabelType);
			AssertEquals("", importEntryLine.CertificateOfOriginExemptionReason);
			AssertEquals("", importEntryLine.ProductOrMaterialCode);
			AssertEquals(0, importEntryLine.MaterialLineNo);
			AssertEquals("", importEntryLine.MightRequireInspectionIndicator);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency1);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency2);
			AssertEquals("", importEntryLine.PostClearanceProcedureAgency3);
			AssertEquals(0m, importEntryLine.NetWeightInKG);
			AssertEquals("", importEntryLine.QuantityUnit);
			AssertEquals(0m, importEntryLine.CustomsValueKRW);
			AssertEquals(0m, importEntryLine.CustomsValueUSD);
			AssertEquals("", importEntryLine.CourierCargoSelectivityIndicator);
			AssertEquals(0m, importEntryLine.AdditionalDutyRate);
			AssertEquals("", importEntryLine.AdditionalDutyCode);
			AssertEquals(null, importEntryLine.VATRateCode);
			AssertEquals(null, importEntryLine.VATReductionCode);
			AssertEquals(0m, importEntryLine.ValueForVAT);
			AssertEquals(0m, importEntryLine.VATAmount);
			AssertEquals("", importEntryLine.EducationTaxExemptIndicator);
		}

		public void TestAdditionalTariffCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;

			entryLine.CL_AdValoremTariff = "0208100000";
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(null, importEntryLine.AdditionalTariffCode);

			var hsExtensionCode1 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode1.CY_Order = 0;
			hsExtensionCode1.CY_Code = "01";

			var hsExtensionCode2 = invoiceLine.HSExtensionCodeCollection.AddNew();
			hsExtensionCode2.CY_Order = 1;
			hsExtensionCode2.CY_Code = "1N";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine = entryHeader.EntryLines[0];
			AssertEquals("0208100000-01-1N", importEntryLine.AdditionalTariffCode);
		}

		public void TestEntryLineOrdered()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 2;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			AssertEquals(2, entry.MergedLines.Count);
			var entryLine = entry.MergedLines;
			AssertEquals(new ZShort(2), entryLine[0].CL_LineNumber);
			AssertEquals(new ZShort(001), entryLine[1].CL_LineNumber);

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			AssertNotNull(import929.EntryLines);
			AssertEquals(2, import929.EntryLines.Length);

			var lines = import929.EntryLines;
			AssertEquals(1, lines[0].EntryLineNo);
			AssertEquals(2, lines[1].EntryLineNo);
		}

		public void TestEntryLineDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.CertificateOfOriginIssueStatus = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = "F";
			var certificate = invoiceLine.CertificateOfOriginData;
			certificate.CSI_DateOfIssue = ZDateTime.Empty;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(ZDateTime.Empty, importEntryLine1.CertificateOfOriginIssueDate);

			certificate.CSI_DateOfIssue = new ZDateTime(2021, 08, 20);
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine1 = entryHeader.EntryLines[0];
			AssertEquals(new ZDateTime(2021, 08, 20), importEntryLine1.CertificateOfOriginIssueDate);
		}

		public void TestExemptVat()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_ValueForVAT = 3772393m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ZZF_NKTaxType = VATRateTypeCodeList.Codes.VATLevy;
			invoiceLine.JI_VATReductionCode = "E106205";

			var entryLine1vat = entryLine.Fees.AddNew();
			entryLine1vat.CF_ChargeType = "VAT";
			entryLine1vat.CF_ChargeAmount = 377239m;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(null, importEntryLine.VATReductionCode);
			AssertEquals(3772393m, importEntryLine.ValueForVAT);
			AssertEquals(377239m, importEntryLine.VATAmount);

			invoiceLine.JI_ZZF_NKTaxType = VATRateTypeCodeList.Codes.VATExemption;

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine = entryHeader.EntryLines[0];
			AssertEquals("E106205", importEntryLine.VATReductionCode);
			AssertEquals(0m, importEntryLine.ValueForVAT);
			AssertEquals(0m, importEntryLine.VATAmount);

			invoiceLine.JI_ZZF_NKTaxType = VATRateTypeCodeList.Codes.VATReduction;

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			importEntryLine = entryHeader.EntryLines[0];
			AssertEquals("E106205", importEntryLine.VATReductionCode);
			AssertEquals(3772393m, importEntryLine.ValueForVAT);
			AssertEquals(377239m, importEntryLine.VATAmount);
		}
		public void TestCustomsValueUSD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_LineNumber = 1;
			entryLine.CL_CustomsValue = 10000m;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var krwCurrency = entry.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 1100.5m, usdCurrency);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today, ZDateTime.Today, 0.750m, usdCurrency);

			var usdRate = entry.CurrencyConverter.GetExchangeRate(usdCurrency);
			AssertEquals(1100.5m, usdRate);

			var usdDollarCalculation = new Money(entryLine.CL_CustomsValue / usdRate, usdCurrency).Amount.Truncate();
			AssertEquals(9m, usdDollarCalculation);

			var usdDollarConvert = entry.CurrencyConverter.ConvertExact(new Money(entryLine.CL_CustomsValue, krwCurrency), usdCurrency).Amount.Truncate();
			AssertEquals(9m, usdDollarConvert);

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var importEntryLine = entryHeader.EntryLines[0];
			AssertEquals(9m, importEntryLine.CustomsValueUSD);
		}

		public void TestOnlineTradeDistributor()
		{
			var agent = SetupEntryHeader("JZ_OA_DistributorAddress", new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ECommerceCompanyID, Number = "E123123", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth } }).OnlineTradeDistributor;

			AssertEquals("레디코리아", agent.CompanyName);
			AssertEquals("E123123", agent.ECommerceCompanyID);
			AssertNullOrEmpty(agent.Postcode);
			AssertNullOrEmpty(agent.RoadNameCode);
			AssertNullOrEmpty(agent.BuildingNumber);
			AssertNullOrEmpty(agent.RepresentativeName);
		}

		public void TestShipper()
		{
			var agent = SetupEntryHeader("JZ_OA_ShipperAddress", new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "E123123", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth } }).Shipper;

			AssertEquals("레디코리아", agent.CompanyName);
			AssertEquals("E123123", agent.ForeignCompanyID);
			AssertNullOrEmpty(agent.Postcode);
			AssertNullOrEmpty(agent.RoadNameCode);
			AssertNullOrEmpty(agent.BuildingNumber);
			AssertNullOrEmpty(agent.RepresentativeName);
		}

		public void TestOnlineTradeType()
		{
			var declaration = CreateEmptyDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.RandomHeader.JZ_OnlineTradeType = ImportOnlineTypeCodeList.Codes.A;
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);

			AssertEquals("A", entryHeader.OnlineTradeType);
		}

		public void TestOnlineTradeSeller()
		{
			var agent = SetupEntryHeader("JZ_OA_SellerAddress", new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ECommerceCompanyID, Number = "E123123", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth } }).OnlineTradeSeller;

			AssertEquals("레디코리아", agent.CompanyName);
			AssertEquals("E123123", agent.ECommerceCompanyID);
			AssertNullOrEmpty(agent.Postcode);
			AssertNullOrEmpty(agent.RoadNameCode);
			AssertNullOrEmpty(agent.BuildingNumber);
			AssertNullOrEmpty(agent.RepresentativeName);
		}

		public void TestOnlineTradeSellingAgent()
		{
			var agent = SetupEntryHeader("JZ_OH_SellingAgent", new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ECommerceCompanyID, Number = "E123123", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth } }, true).OnlineTradeSellingAgent;

			AssertEquals("레디코리아", agent.CompanyName);
			AssertEquals("E123123", agent.ECommerceCompanyID);
			AssertNullOrEmpty(agent.CountryCode);
			AssertNullOrEmpty(agent.AddressLine1);
			AssertNullOrEmpty(agent.Postcode);
			AssertNullOrEmpty(agent.RoadNameCode);
			AssertNullOrEmpty(agent.BuildingNumber);
			AssertNullOrEmpty(agent.PhoneNumber);
			AssertNullOrEmpty(agent.Email);
			AssertNullOrEmpty(agent.RepresentativeName);
		}

		ImportEntryHeader SetupEntryHeader(string mapping, IDNumberAndType[] codes = default, bool isOrganization = false)
		{
			var declaration = CreateEmptyDeclaration();
			var invoice = declaration.Invoices[0];
			var entry = declaration.CustomsEntryHeaders[0];
			var org = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			if (codes?.Length > 0)
			{
				TestOrgDataSetUpHelper.AddCustomsCode(org, codes);
			}
			TestOrgDataSetUpHelper.AddOrgAddress(org.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120", "KR");
			invoice.SetPropertyValue(mapping, isOrganization ? org.PK : org.MainAddress.PK);
			Factory.Save();

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			return entryHeader;
		}

		public void TestImportContainers()
		{
			SetUpTariffData();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			#region container
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "999999999999999";

			var containerLink = entry.PivotsToContainers.AddNew();
			containerLink.CCE_CO_Container = container.PK;
			containerLink.CCE_SequenceNumber = 1;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "899999999999999";

			var containerLink2 = entry.PivotsToContainers.AddNew();
			containerLink2.CCE_CO_Container = container2.PK;
			containerLink2.CCE_SequenceNumber = 2;
			#endregion

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.Containers.Length);

			AssertEquals(1, entryHeader.Containers[0].SequenceNo);
			AssertEquals("999999999999999", entryHeader.Containers[0].ContainerNo);

			AssertEquals(2, entryHeader.Containers[1].SequenceNo);
			AssertEquals("899999999999999", entryHeader.Containers[1].ContainerNo);
		}

		public void TestImportOnlineOrders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.Invoices.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var onlineOrder1 = entryInstruction1.OnlineOrders.AddNew();
			onlineOrder1.CY_Data = "123456789";
			onlineOrder1.CY_Order = 2;

			var onlineOrder2 = entryInstruction1.OnlineOrders.AddNew();
			onlineOrder2.CY_Data = "987654321";
			onlineOrder2.CY_Order = 1;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var onlineOrder3 = entryInstruction2.OnlineOrders.AddNew();
			onlineOrder3.CY_Data = "8527419630";
			onlineOrder3.CY_Order = 1;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction1.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];

			AssertEquals(entryInstruction1, entry.EntryInstruction);

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(2, entryHeader.OnlineOrders.Length);
			AssertEquals(1, entryHeader.OnlineOrders[0].SequenceNo);
			AssertEquals("987654321", entryHeader.OnlineOrders[0].OnlineOrderNo);
			AssertEquals(2, entryHeader.OnlineOrders[1].SequenceNo);
			AssertEquals("123456789", entryHeader.OnlineOrders[1].OnlineOrderNo);
		}

		public void TestImmediateDeliveries()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			var entryLine3 = entry.MergedLines.AddNew();

			var immediateDelivery = entryLine1.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 2;
			immediateDelivery.CY_Data = "22222";
			immediateDelivery = entryLine1.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.CY_Data = "11111";

			immediateDelivery = entryLine2.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.CY_Data = "33333";
			immediateDelivery = entryLine2.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 2;
			immediateDelivery.CY_Data = "33333";
			immediateDelivery = entryLine2.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 3;
			immediateDelivery.CY_Data = "44444";

			immediateDelivery = entryLine3.ImmediateDeliveries.AddNew();
			immediateDelivery.CY_Order = 1;
			immediateDelivery.CY_Data = "44444";

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var immadiateDeliveries = entryHeader.EntryLines[0].ImmediateDeliveries;
			AssertEquals(2, immadiateDeliveries.Length);

			AssertEquals(1, immadiateDeliveries[0].SequenceNo);
			AssertEquals("11111", immadiateDeliveries[0].ImmediateDeliveryNo);

			AssertEquals(2, immadiateDeliveries[1].SequenceNo);
			AssertEquals("22222", immadiateDeliveries[1].ImmediateDeliveryNo);

			immadiateDeliveries = entryHeader.EntryLines[1].ImmediateDeliveries;
			AssertEquals(3, immadiateDeliveries.Length);

			AssertEquals(1, immadiateDeliveries[0].SequenceNo);
			AssertEquals("33333", immadiateDeliveries[0].ImmediateDeliveryNo);

			AssertEquals(2, immadiateDeliveries[1].SequenceNo);
			AssertEquals("33333", immadiateDeliveries[1].ImmediateDeliveryNo);

			AssertEquals(3, immadiateDeliveries[2].SequenceNo);
			AssertEquals("44444", immadiateDeliveries[2].ImmediateDeliveryNo);

			immadiateDeliveries = entryHeader.EntryLines[2].ImmediateDeliveries;
			AssertEquals(1, immadiateDeliveries.Length);

			AssertEquals(1, immadiateDeliveries[0].SequenceNo);
			AssertEquals("44444", immadiateDeliveries[0].ImmediateDeliveryNo);
		}

		public void TestAdditionalAndDeductionAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			entryLine.CL_LineNumber = 1;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			var charge104 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 1000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge104.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge104.J7_IsDutiable = true;
			var apportionedCharge104 = ((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A104, 10.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			apportionedCharge104.J7_Calc_IsIncludedInInvoiceAmount = false;
			apportionedCharge104.J7_IsDutiable = true;
			var charge118 = invoiceLine.Charges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 2000.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			charge118.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge118.J7_IsDutiable = false;
			var apportionedCharge118 = ((IChargeApportionee)invoiceLine).ApportionedCharges.AddNew(ImportChargeMethodOneCodeList.Codes.A118, 20.00m, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			apportionedCharge118.J7_Calc_IsIncludedInInvoiceAmount = true;
			apportionedCharge118.J7_IsDutiable = false;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(1010m, entryHeader.AdditionalAmount);
			AssertEquals(2020m, entryHeader.DeductedAmount);
		}

		public void TestEducationTaxExemptIndicator()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DomesticTaxRate);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "941211-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "EDT Rate not exist");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "941210-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "EDT Rate exist");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.KoreaSouth, ChargeTypeList.Codes.EducationTax);
			var rateCode = helper.CreateCusRateCode(Factory, ChargeTypeList.Codes.EducationTax, rateType.PK);
			helper.CreateRate(tariff2, rateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var line = entryHeader.EntryLines[0];
			AssertEquals(ZString.Empty, invoiceLine.DomesticTaxCode);
			AssertEquals(ZString.Empty, line.EducationTaxExemptIndicator);

			invoiceLine.JI_DomesticTaxCode = "XXXXXX-X";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			line = entryHeader.EntryLines[0];
			AssertEquals("XXXXXX", invoiceLine.DomesticTaxCode);
			AssertEquals(ZString.Empty, line.EducationTaxExemptIndicator);

			invoiceLine.JI_DomesticTaxCode = "941211-A";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			line = entryHeader.EntryLines[0];
			AssertEquals("941211", invoiceLine.DomesticTaxCode);
			AssertEquals(ZString.Empty, line.EducationTaxExemptIndicator);

			invoiceLine.JI_DomesticTaxCode = "941210-A";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			line = entryHeader.EntryLines[0];
			AssertEquals("941210", invoiceLine.DomesticTaxCode);
			Assert(invoiceLine.JI_DomesticTaxExemptionCode.IsEmpty);
			AssertEquals(EducationTaxTypeCodeList.Codes.A, line.EducationTaxExemptIndicator);

			invoiceLine.JI_DomesticTaxExemptionCode = "0000001";
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			line = entryHeader.EntryLines[0];
			AssertEquals("941210", invoiceLine.DomesticTaxCode);
			Assert(!invoiceLine.JI_DomesticTaxExemptionCode.IsEmpty);
			AssertEquals(EducationTaxTypeCodeList.Codes.B, line.EducationTaxExemptIndicator);
		}

		public void TestCertificateOfCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			SetData(1, Constants.ZZ.Preferences.GeneralInternationalPreference);
			SetData(2, Constants.ZZ.Preferences.IsraelFTA);
			SetData(3, Constants.ZZ.Preferences.GeneralInternationalPreference1);

			void SetData(ZShort lineNumber, string preference)
			{
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = lineNumber;

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_PrimaryPreference = preference;
				invoiceLine.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.F;
				invoiceLine.CertificateOfOriginCriteriaCode = CountryOfOriginDeterminationRuleCodeList.Codes._8;
				invoiceLine.CertificateOfOriginNo = "ReferenceNumber";
				invoiceLine.CertificateOfOriginStatus = CertificateOfOriginSplitCodeList.Codes.N;
				invoiceLine.CertificateOfOriginIssuingCountry = Core.Constants.CountryCodes.Australia;
				invoiceLine.CertificateOfOriginAgencyName = "Description";
				invoiceLine.CertificateOfOriginIssueDate = new ZDateTime("2024-01-01");
				invoiceLine.CertificateOfOriginPersonName = "ReferenceNumber2";
				invoiceLine.CertificateOfOriginAreaName = "AdditionalDescription";
			}

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			AssertEquals(3, entryHeader.EntryLines.Length);

			var entryLine1 = entryHeader.EntryLines[0];
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.F, entryLine1.CountryOfOriginDeterminationRule);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes._8, entryLine1.CertificateOfOriginCriteriaCode);
			AssertEquals("ReferenceNumber", entryLine1.CertificateOfOriginNo);
			AssertEquals(CertificateOfOriginSplitCodeList.Codes.N, entryLine1.CertificateOfOriginSplitIndicator);
			AssertEquals(Core.Constants.CountryCodes.Australia, entryLine1.CertificateOfOriginIssuingCountry);
			AssertEquals("Description", entryLine1.CertificateOfOriginAgencyName);
			AssertEquals(new ZDateTime("2024-01-01"), entryLine1.CertificateOfOriginIssueDate);
			AssertEquals("ReferenceNumber2", entryLine1.CertificateOfOriginPersonName);
			AssertEquals("AdditionalDescription", entryLine1.CertificateOfOriginAreaName);

			var entryLine2 = entryHeader.EntryLines[1];
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.F, entryLine2.CountryOfOriginDeterminationRule);
			AssertNull(entryLine2.CertificateOfOriginCriteriaCode);
			AssertNull(entryLine2.CertificateOfOriginNo);
			AssertNull(entryLine2.CertificateOfOriginSplitIndicator);
			AssertNull(entryLine2.CertificateOfOriginIssuingCountry);
			AssertNull(entryLine2.CertificateOfOriginAgencyName);
			AssertEquals(ZDateTime.Empty, entryLine2.CertificateOfOriginIssueDate);
			AssertNull(entryLine2.CertificateOfOriginPersonName);
			AssertNull(entryLine2.CertificateOfOriginAreaName);

			var entryLine3 = entryHeader.EntryLines[2];
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.F, entryLine3.CountryOfOriginDeterminationRule);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes._8, entryLine3.CertificateOfOriginCriteriaCode);
			AssertEquals("ReferenceNumber", entryLine3.CertificateOfOriginNo);
			AssertEquals(CertificateOfOriginSplitCodeList.Codes.N, entryLine3.CertificateOfOriginSplitIndicator);
			AssertEquals(Core.Constants.CountryCodes.Australia, entryLine3.CertificateOfOriginIssuingCountry);
			AssertEquals("Description", entryLine3.CertificateOfOriginAgencyName);
			AssertEquals(new ZDateTime("2024-01-01"), entryLine3.CertificateOfOriginIssueDate);
			AssertEquals("ReferenceNumber2", entryLine3.CertificateOfOriginPersonName);
			AssertEquals("AdditionalDescription", entryLine3.CertificateOfOriginAreaName);
		}

		RefExchangeRate SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();

			return result;
		}

		JobDeclaration CreateEmptyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		void SetUpTariffData()
		{
			new TestDataSetupHelper(Factory).SetEntry929Tariff();
		}
	}
}
