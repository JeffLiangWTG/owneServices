using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.KR.Messaging.Constants.ZZ;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public override void TestBizObjectFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			TestBizObjectFieldsCore(declaration);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			TestSettingValueCallsRefreshBindingCore(declaration);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<JobDeclaration>(), "KRJobDeclaration");
		}

		public override void TestMessageTypeDescription()
		{
			using (KRCustomsRegistry.Instance.EnableToSaveImportDeclaration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				base.TestMessageTypeDescription();
			}
		}

		public override void TestMutexForDoMergeCore()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				base.TestMutexForDoMergeCore();
			}
		}

		public override void TestFactorySavingDoesNotCauseAMergeToBeRequired()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				base.TestFactorySavingDoesNotCauseAMergeToBeRequired();
			}
		}

		public override void TestMergedSuccessfullyEvent()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				base.TestMergedSuccessfullyEvent();
			}
		}

		public void TestContainerPackingNotRequiredForKR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			Assert(!declaration.IsContainerPackingRequired);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX34987432";
			Assert(!container.CO_ContainerNumberInfo.HasMessageError(CusContainerValidation.ContainersRequirePackages));
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			AssertEquals(true, dec.AreMultipleEntryInstructionsAllowed);
			dec.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			AssertEquals(true, dec.AreMultipleEntryInstructionsAllowed);
			dec.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export type", JobMessageTypeList.Codes.Export, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = "LEX";
			AssertEquals("Local Export type", "LEX", declaration.MessageTypeForDocumentFilter);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			Assert("KR Declaration should support EntryInstructions in Export.", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			Assert("EntryInstructions should not be required in Export.", declaration.IsEntryInstructionRequired);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			Assert("KR Declaration should support EntryInstructions in Import.", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			Assert("EntryInstructions should be required in Import.", declaration.IsEntryInstructionRequired);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			Assert("KR Declaration should support EntryInstructions in LocalExport.", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			Assert("EntryInstructions should not be required in LocalExport.", !declaration.IsEntryInstructionRequired);
		}

		public void TestStevedoreDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var stevedore = declaration.StevedoreCompany;
			AssertEquals(DocAddressType.Stevedore, stevedore.DocAddressType);
			AssertEquals(1, declaration.DocAddresses.Count);
			AssertCollectionContains(stevedore, declaration.DocAddresses);
		}

		public void TestPayerAddress()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var payer_HasNoCustomsAddress = Factory.NewWithValidTestData<OrgHeader>();
			var payer_HasCustomsAddress = Factory.NewWithValidTestData<OrgHeader>();
			payer_HasCustomsAddress.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_DutyPayer = payer_HasNoCustomsAddress.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_PaidBy = ZString.Empty;
			AssertNotNull(declaration.PayerAddress);

			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
			AssertEquals(importer.MainAddress.PK, declaration.PayerAddress.PK);

			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer_HasNoCustomsAddress.PK;
			AssertNull(payer_HasNoCustomsAddress.CustomsAddress);
			AssertEquals(payer_HasNoCustomsAddress.MainAddress.PK, declaration.PayerAddress.PK);

			declaration.JE_OH_DutyPayer = payer_HasCustomsAddress.PK;
			AssertNotNull(payer_HasCustomsAddress.CustomsAddress);
			AssertEquals(payer_HasCustomsAddress.CustomsAddress.PK, declaration.PayerAddress.PK);

			declaration.JE_PaidBy = ZString.Empty;
			AssertEquals(payer_HasCustomsAddress.CustomsAddress.PK, declaration.PayerAddress.PK);
		}

		public void TestBrokerAddress()
		{
			var broker1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = broker1.Addresses.AddNew();
			address1.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			address1.OA_Address1 = "Address1";

			var broker2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = broker2.MainAddress;
			address2.AddAddressType(OrgAddressType.Office);
			address2.OA_Address1 = "Address2";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var declaration = Factory.New<JobDeclaration>();

			var branch1 = company.Branches.AddNew();
			branch1.GB_OH_OrgProxy = broker1.PK;
			company.GC_OH_OrgProxy = broker2.PK;
			declaration.JE_GB = branch1.PK;
			AssertEquals(address1.PK, declaration.BrokerAddress.PK);

			var branch2 = company.Branches.AddNew();
			branch2.GB_OH_OrgProxy = broker2.PK;
			company.GC_OH_OrgProxy = broker1.PK;
			declaration.JE_GB = branch2.PK;
			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(address1.PK, declaration.BrokerAddress.PK);

			var branch3 = company.Branches.AddNew();
			branch3.GB_OH_OrgProxy = ZGuid.Empty;
			company.GC_OH_OrgProxy = broker1.PK;
			declaration.JE_GB = branch3.PK;
			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(address1.PK, declaration.BrokerAddress.PK);

			var branch4 = company.Branches.AddNew();
			branch4.GB_OH_OrgProxy = ZGuid.Empty;
			company.GC_OH_OrgProxy = broker2.PK;
			declaration.JE_GB = branch4.PK;
			declaration = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(address2.PK, declaration.BrokerAddress.PK);
		}

		public void TestTransportMean()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testTransportMean = declaration.TransportMeans.AddNew();

			testTransportMean.CY_Data = "CY_Data Test";
			testTransportMean.CY_Code = "Test";

			Factory.Save();

			var transportMeanLoaded = new BusinessObjectFactory().Load<CusCodeData>(testTransportMean.PK);
			AssertEquals(CusCodeDataTypeList.Codes.TransportMean, transportMeanLoaded.CY_Type);
			AssertEquals("CY_Data Test", transportMeanLoaded.CY_Data);
			AssertEquals("Test", transportMeanLoaded.CY_Code);
		}

		public void TestVesselCountryCodeForAirDigit()
		{
			SetUpTariffData();

			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, "8B");
			airline.RM_RN_NKAirlineCountry = "KE";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "8B";

			AssertEquals("KE", declaration.VesselCountryCode);
			AssertEquals(declaration.VesselCountryCode, declaration.JE_RN_NKTransportNationality);
			AssertEquals("Kenya", declaration.VesselCountryKRCCode);
		}

		public void TestVesselCountryCodeForAir()
		{
			SetUpTariffData();

			var airline = RefAirline.LoadFromAirline2LetterCode(Factory, "KE");
			airline.RM_RN_NKAirlineCountry = "KE";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "KE1098";

			AssertEquals("KE", declaration.VesselCountryCode);
			AssertEquals(declaration.VesselCountryCode, declaration.JE_RN_NKTransportNationality);
			AssertEquals("Kenya", declaration.VesselCountryKRCCode);
		}

		public void TestVesselCountryCodeForSea()
		{
			SetUpTariffData();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "KI1098";
			vessel.RV_RN_NKCountryOfReg = "KR";
			vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_VesselName = "KI1098";
			declaration.JE_VoyageFlightNo = "KE1098";

			AssertEquals("KR", declaration.VesselCountryCode);
			AssertEquals(declaration.VesselCountryCode, declaration.JE_RN_NKTransportNationality);
			AssertEquals("Kor", declaration.VesselCountryKRCCode);
		}

		[TestDate(2021, 10, 10)]
		public void TestICurrencyConverterDataProvider()
		{
			var declaration0 = Factory.New<JobDeclaration>();
			declaration0.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry0 = declaration0.CustomsEntryHeaders.AddNew();
			entry0.CH_MessageType = declaration0.JE_MessageType;
			entry0.EntryNumber = "1234567890I";

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2021, 10, 10), ((ICurrencyConverterDataProvider)declaration0).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)declaration0).RateType);
			AssertNotNull("CurrencyConverter", ((ICurrencyConverterProvider)declaration0).CurrencyConverter);
			AssertEquals("ICurrencyConverterDataProvider.MaximumDaysToFallback", 0, ((ICurrencyConverterDataProvider)declaration0).MaximumDaysToFallback);
			AssertEquals("ICurrencyConverterDataProvider.Company", GlbCompany.CurrentCompany.PK, ((ICurrencyConverterDataProvider)declaration0).Company.PK);
			AssertEquals("ICurrencyConverterDataProvider.LocalCurrencyCodeOverride", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, ((ICurrencyConverterDataProvider)declaration0).LocalCurrencyCodeOverride);
			AssertEquals("ICurrencyConverterDataProvider.IsReciprocalOverride", true, ((ICurrencyConverterDataProvider)declaration0).IsReciprocalOverride);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration1.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 1, 1);

			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = declaration1.JE_MessageType;
			entry2.EntryNumber = "1234567891I";
			var entryNum2 = entry2.CusEntryNumber;
			entryNum2.CE_IssueDate = new ZDateTime(2022, 1, 4);

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2021, 10, 10), ((ICurrencyConverterDataProvider)declaration1).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)declaration1).RateType);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = declaration2.JE_MessageType;
			entry3.EntryNumber = "1234567892I";
			var entryNum3 = entry3.CusEntryNumber;
			entryNum3.CE_IssueDate = new ZDateTime(2022, 1, 1);

			var entry4 = declaration2.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = declaration2.JE_MessageType;
			entry4.EntryNumber = "1234567893I";
			var entryNum4 = entry4.CusEntryNumber;
			entryNum4.CE_IssueDate = new ZDateTime(2022, 1, 1);

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2022, 1, 1), ((ICurrencyConverterDataProvider)declaration2).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, ((ICurrencyConverterDataProvider)declaration2).RateType);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration3.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;

			var entry5 = declaration3.CustomsEntryHeaders.AddNew();
			entry5.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry5.EntryNumber = "1234567892I";
			var entryNum5 = entry5.CusEntryNumber;
			entryNum5.CE_IssueDate = new ZDateTime(2022, 1, 1);

			AssertEquals("ICurrencyConverterDataProvider.DateOfValuation", new ZDateTime(2022, 1, 1), ((ICurrencyConverterDataProvider)declaration3).DateOfValuation);
			AssertEquals("ICurrencyConverterDataProvider.RateType", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, ((ICurrencyConverterDataProvider)declaration3).RateType);
		}

		void SetUpTariffData()
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "AD", "ANDORA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("CNTRY", "AE", "U.A.E", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("CNTRY", "KE", "Kenya", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("CNTRY", "KR", "Kor", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			#endregion
		}

		public void TestDepartureCountryDescription()
		{
			SetUpTariffData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsLoadPort = "AD";

			AssertEquals("ANDORA", declaration.DepartureCountryKRCCode);
		}

		void SetUpTariffDataCustomsOffice()
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var office1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(office1.PK, Constants.ZZ.CodeListAttributeNames.RefundDepartment, "75");
			var office2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "011", "성남세관 의정부세관비즈니스센터", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(office2.PK, Constants.ZZ.CodeListAttributeNames.RefundDepartment, "10");
			var office3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "013", "인천공항국제우편세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(office3.PK, Constants.ZZ.CodeListAttributeNames.RefundDepartment, "07");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
			#endregion
		}

		public void TestCustomsOfficeRefundDepartment()
		{
			SetUpTariffDataCustomsOffice();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "20";
			AssertEquals("75", declaration.CustomsOfficeRefundDepartment);

			declaration.JE_CustomsOffice = "011";
			AssertEquals("10", declaration.CustomsOfficeRefundDepartment);

			declaration.JE_CustomsOffice = "012";
			AssertEquals("20", declaration.CustomsOfficeRefundDepartment);
		}
		public void TestUnderbondMovement()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(0, declaration.AdditionalReferenceNumbers.Count);
			AssertEquals(ZString.Empty, declaration.JE_LocationIDInBondedArea);
			AssertEquals(ZDateTime.Empty, declaration.UnderbondMovementArrivalDate);
			AssertEquals(ZDateTime.Empty, declaration.UnderbondMovementDepartureDate);

			declaration.JE_LocationIDInBondedArea = "9999999999";
			declaration.UnderbondMovementArrivalDate = new ZDateTime(2014, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2015, 01, 01);
			AssertEquals(1, declaration.AdditionalReferenceNumbers.Count);
			AssertEquals("9999999999", declaration.JE_LocationIDInBondedArea);
			AssertEquals(new ZDateTime(2014, 01, 01), declaration.UnderbondMovementArrivalDate);
			AssertEquals(new ZDateTime(2015, 01, 01), declaration.UnderbondMovementDepartureDate);

			declaration.JE_LocationIDInBondedArea = "8888888";
			declaration.UnderbondMovementArrivalDate = new ZDateTime(2017, 01, 01);
			declaration.UnderbondMovementDepartureDate = new ZDateTime(2018, 01, 01);
			AssertEquals(1, declaration.AdditionalReferenceNumbers.Count);
			AssertEquals("8888888", declaration.JE_LocationIDInBondedArea);
			AssertEquals(new ZDateTime(2017, 01, 01), declaration.UnderbondMovementArrivalDate);
			AssertEquals(new ZDateTime(2018, 01, 01), declaration.UnderbondMovementDepartureDate);
		}

		public void TestBondedAreaCodeAndName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01011041", "삼원산업 보세창고", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOtherInformation = "01001001";

			AssertEquals("경의선철도 입출경검사장(지상)", declaration.BondedAreaName);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		void WarehouseDocAddress_RefSetUp(ZString type)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeType(type, "RefType" + type);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, type, "98765", "Test1_" + type, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, type, "45678", "Test2_" + type, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();
		}

		OrgHeader WarehouseDocAddress_SetUp(string headerCode, string address = "TestAddress", string postcode = "12345")
		{
			var warehouseHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", headerCode, "TestCompanyName");
			TestOrgDataSetUpHelper.AddOrgAddress(warehouseHeader.MainAddress, address);
			warehouseHeader.MainAddress.Postcode = postcode;
			//var warehouseCodes = new IDNumberAndType[]
			//{
			//	new IDNumberAndType { Type = codeType, Number = "98765", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			//};
			//TestOrgDataSetUpHelper.AddCustomsCode(warehouseHeader.MainAddress, warehouseCodes);

			return warehouseHeader;
		}

		public void TestWarehouseDocAddress_Export()
		{
			WarehouseDocAddress_RefSetUp(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode);
			var warehouseHeader = WarehouseDocAddress_SetUp("KRTEST1");
			var warehouseHeaderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = Constants.IdentificationType.ControlledPremisesID, Number = "98765", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(warehouseHeader.MainAddress, warehouseHeaderCodes);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseHeader.MainAddress.PK;

			AssertEquals("12345", declaration.JE_LocationQualifier);
			AssertEquals("Test1_BNDAR", declaration.JE_SubLocationOfGoods);
			AssertEquals("TestAddress", declaration.JE_LocationOfGoods);
			AssertEquals("98765", declaration.JE_LocationOtherInformation);

			var warehouseHeaderCodeTypeNotCCP = WarehouseDocAddress_SetUp("KRTEST2", "TestAddress2", "85202");
			var warehouseHeaderCodeTypeNotCCPCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = Constants.IdentificationType.BusinessRegNo, Number = "98765", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(warehouseHeader, warehouseHeaderCodeTypeNotCCPCodes);

			declaration.WarehouseDocAddress.E2_OA_Address = warehouseHeaderCodeTypeNotCCP.MainAddress.PK;
			declaration.WarehouseDocAddress.E2_AddressOverride = true;
			declaration.WarehouseDocAddress.E2_GovRegNum = "45678";
			declaration.JE_LocationQualifier = "";
			declaration.JE_SubLocationOfGoods = "";
			declaration.JE_LocationOfGoods = "";
			declaration.JE_LocationOtherInformation = "";
			declaration.WarehouseDocAddress.E2_GovRegNumType = Constants.IdentificationType.ControlledPremisesID;

			AssertEquals("85202", declaration.JE_LocationQualifier);
			AssertEquals("Test2_BNDAR", declaration.JE_SubLocationOfGoods);
			AssertEquals("TestAddress2", declaration.JE_LocationOfGoods);
			AssertEquals("45678", declaration.JE_LocationOtherInformation);

			declaration.JE_SubLocationOfGoods = "";
			declaration.JE_LocationOtherInformation = "";
			declaration.WarehouseDocAddress.E2_GovRegNumType = Constants.IdentificationType.BusinessRegNo;

			AssertEquals("TestCompanyName", declaration.JE_SubLocationOfGoods);
			AssertNullOrEmpty(declaration.JE_LocationOtherInformation);
		}

		public void TestWarehouseDocAddress_Import()
		{
			WarehouseDocAddress_RefSetUp(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode);
			var warehouseHeader = WarehouseDocAddress_SetUp("KRTEST1");
			var warehouseHeaderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = Constants.IdentificationType.ControlledPremisesID, Number = "98765", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(warehouseHeader.MainAddress, warehouseHeaderCodes);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseHeader.MainAddress.PK;

			AssertNullOrEmpty(declaration.JE_SubLocationOfGoods);
			AssertNullOrEmpty(declaration.JE_LocationOfGoods);
			AssertEquals("98765", declaration.JE_LocationOtherInformation);
		}

		public void TestMessageSubTypeMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals("If JE_MessageType is 'EXP', This length is 1.", 1, declaration.MessageSubTypeMaxLength);
			AssertEquals("If JE_MessageType is not 'IMP', This length is 1.", 1, declaration.JE_ProcedureType_MaxLength);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals("If JE_MessageType is 'LEX', This length is 2.", 2, declaration.MessageSubTypeMaxLength);
			AssertEquals("If JE_MessageType is not 'IMP', This length is 1.", 1, declaration.JE_ProcedureType_MaxLength);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals("If JE_MessageType is 'IMP', This length is 2.", 2, declaration.MessageSubTypeMaxLength);
			AssertEquals("If JE_MessageType is 'IMP', This length is 2.", 2, declaration.JE_ProcedureType_MaxLength);
		}

		public void TestIsUnamendableFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			AssertEquals(false, declaration.JE_UCRInfo.ReadOnly);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2023, 1, 6);

			AssertEquals(true, declaration.JE_UCRInfo.ReadOnly);
		}

		public void TestMaxLengthChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(3, declaration.JE_CustomsOfficeInfo.MaxLength);
			AssertEquals(5, declaration.JE_LocationQualifierInfo.MaxLength);
			AssertEquals(8, declaration.JE_LocationOtherInformationInfo.MaxLength);
			AssertEquals(3, declaration.JE_TaxOfficeInfo.MaxLength);
		}

		public void TestIndustrialParkCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.IndustrialParkCode, "Industrial Park Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, "999", "No Value", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, "001", "Value", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var org1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES1", "CompanyName1");
			var cusCode = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = Constants.IdentificationType.IndustrialParkCode, Number = "001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1.MainAddress, cusCode);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTES2", "CompanyName2");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertEquals("999", declaration.IndustrialParkCode);

			declaration.JE_OA_ManufacturerAddress = org1.MainAddress.PK;
			AssertEquals("001", declaration.IndustrialParkCode);

			declaration.JE_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertEquals("999", declaration.IndustrialParkCode);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertEquals("", declaration.IndustrialParkCode);
		}

		public void TestFinalBondedWarehouse()
		{
			WarehouseDocAddress_RefSetUp(Constants.IdentificationType.ControlledPremisesID);
			var orgHeader = WarehouseDocAddress_SetUp("KRTEST1");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.E;
			AssertEquals("", declaration.FinalBondedWarehouse);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertEquals("01099999", declaration.FinalBondedWarehouse);

			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("01099999", declaration.FinalBondedWarehouse);

			var orgHeaderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = Constants.IdentificationType.ControlledPremisesID, Number = "98765", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(orgHeader.MainAddress, orgHeaderCodes);
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("98765", declaration.FinalBondedWarehouse);
		}

		public void TestExtraInspectionData()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZDateTime.Empty, declaration.InspectionDate);

			declaration.InspectionDate = new ZDateTime(2022, 08, 16);
			AssertEquals(new ZDateTime(2022, 08, 16), declaration.InspectionDate);

			var extraInspectionData = declaration.DocsAndCartage.Services.Cast<JobService>().FirstOrDefault(x => x.ES_ServiceCode == Core.Constants.FreightServiceType.Codes.ExtraInspection);
			extraInspectionData.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertEquals(ZDateTime.Empty, declaration.InspectionDate);

			extraInspectionData.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;
			declaration.InspectionDate = new ZDateTime(2022, 08, 17);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declaration1 = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(new ZDateTime(2022, 08, 17), declaration1.InspectionDate);

			var newExtraInspectionData = declaration1.DocsAndCartage.Services.Cast<JobService>().FirstOrDefault(x => x.ES_ServiceCode == Core.Constants.FreightServiceType.Codes.ExtraInspection);
			declaration1.DocsAndCartage.Services.RemoveAndDelete(newExtraInspectionData);

			AssertEquals(ZDateTime.Empty, declaration1.InspectionDate);
		}

		public void TestGetEntryIssueDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry1 = invoiceLine1.CusEntryLine.Header;
			var entryNum1 = entry1.EntryNumbers.GetOrCreateCusEntryNum(JobMessageTypeList.Codes.Export);

			AssertEquals(ZDateTime.Today, declaration.GetEntryIssueDate(entry1.PK));

			entryNum1.CE_IssueDate = new ZDateTime(2022, 08, 08);
			AssertEquals(new ZDateTime(2022, 08, 08), declaration.GetEntryIssueDate(entry1.PK));

			var entry2 = Factory.New<CusEntryHeader>();
			var entryNum2 = entry2.EntryNumbers.GetOrCreateCusEntryNum(JobMessageTypeList.Codes.Export);
			entryNum2.CE_IssueDate = new ZDateTime(2022, 08, 09);
			AssertEquals(ZDateTime.Today, declaration.GetEntryIssueDate(entry2.PK));
		}
		public void TestJE_ExportGoodsTypeAndJE_ProcedureType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("", declaration.JE_ExportGoodsType);
			AssertEquals("", declaration.JE_ProcedureType);
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
			AssertEquals(TransactionTypeCodeList.Codes._11, declaration.JE_ExportGoodsType);
			AssertEquals("", declaration.JE_ProcedureType);
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._78;
			AssertEquals(TransactionTypeCodeList.Codes._78, declaration.JE_ExportGoodsType);
			AssertEquals(DeclarationProcedureTypeList.Codes.M, declaration.JE_ProcedureType);
			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertEquals(TransactionTypeCodeList.Codes._11, declaration.JE_ExportGoodsType);
			AssertEquals(DeclarationProcedureTypeList.Codes.B, declaration.JE_ProcedureType);
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._79;
			AssertEquals(TransactionTypeCodeList.Codes._79, declaration.JE_ExportGoodsType);
			AssertEquals(DeclarationProcedureTypeList.Codes.M, declaration.JE_ProcedureType);
			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			AssertEquals(TransactionTypeCodeList.Codes._11, declaration.JE_ExportGoodsType);
			AssertEquals(DeclarationProcedureTypeList.Codes.B, declaration.JE_ProcedureType);
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._71;
			AssertEquals(TransactionTypeCodeList.Codes._71, declaration.JE_ExportGoodsType);
			AssertEquals(DeclarationProcedureTypeList.Codes.M, declaration.JE_ProcedureType);
			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);
		}

		public void TestJE_ProcedureTypeAndJE_TradeTypeReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._71;
			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);
			AssertEquals(false, declaration.JE_TradeTypeInfo.ReadOnly);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._71;
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);
			AssertEquals(false, declaration.JE_TradeTypeInfo.ReadOnly);

			Factory.Save();
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);
			AssertEquals(false, declaration.JE_TradeTypeInfo.ReadOnly);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);
			AssertEquals(false, declaration.JE_TradeTypeInfo.ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);
			AssertEquals(false, declaration.JE_TradeTypeInfo.ReadOnly);
		}

		public void TestIsReadonlyFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";

			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;

			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			AssertEquals(false, declaration.JE_ProcedureTypeInfo.ReadOnly);

			var entryNum1 = entry.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 1, 1);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(true, declaration.JE_ProcedureTypeInfo.ReadOnly);
		}

		public void TestSetDefaultValueOfJE_ContainerPackMode()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerPackMode = ZString.Empty;
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("BU", declaration1.JE_ContainerPackMode);

			declaration1.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration1.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.RO;
			AssertEquals("BU", declaration1.JE_ContainerPackMode);

			declaration1.JE_ContainerPackMode = ZString.Empty;
			declaration1.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.RO;
			AssertEquals("RO", declaration1.JE_ContainerPackMode);
		}

		public void TestLoadPortNameInKorean()
		{
			RefUNLOCO seaPort = Factory.New<RefUNLOCO>();
			seaPort.RL_Code = "XXPUS";
			seaPort.RL_IATA = "PUS";
			seaPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			RefLanguageText seaPortText = Factory.New<RefLanguageText>();
			seaPortText.RLT_ParentId = seaPort.PK;
			seaPortText.RLT_Text = "부산항";
			seaPortText.RLT_ColumnName = RefUNLOCO.Schema.RL_PortName;
			seaPortText.RLT_ParentTableCode = "RL";
			seaPortText.RLT_Language = "KO-KR";
			seaPortText.RLT_IsSystem = true;

			RefUNLOCO airPort = Factory.New<RefUNLOCO>();
			airPort.RL_Code = "XXSEL";
			airPort.RL_IATA = "SEL";
			airPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			RefUNLOCO mailPort = Factory.New<RefUNLOCO>();
			mailPort.RL_Code = "XXHIN";
			mailPort.RL_IATA = "HIN";
			mailPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			RefLanguageText mailPortText = Factory.New<RefLanguageText>();
			mailPortText.RLT_ParentId = mailPort.PK;
			mailPortText.RLT_Text = "진주항";
			mailPortText.RLT_ColumnName = RefUNLOCO.Schema.RL_PortName;
			mailPortText.RLT_ParentTableCode = "RL";
			mailPortText.RLT_Language = "KO-KR";
			mailPortText.RLT_IsSystem = true;
			Factory.Save();

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_RL_NKPortOfLoading = seaPort.RL_Code;
			AssertEquals("When JE_TransportMode is SEA, find the explanation in RefLanguageText.", seaPortText.RLT_Text, declaration1.LoadPortNameInKorean);
			AssertEquals("XXPUS", declaration1.KRPortOfLoading);
			AssertNotEquals(IATALoadPortKRList.Descriptions.PUS, declaration1.LoadPortNameInKorean);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_RL_NKPortOfLoading = airPort.RL_Code;
			declaration2.JE_IATALoadPort = "SEL";
			AssertEquals("SEL", declaration2.KRPortOfLoading);
			AssertEquals("When JE_TransportMode is AIR, find the explanation in IATALoadPortKRList.", IATALoadPortKRList.Descriptions.SEL, declaration2.LoadPortNameInKorean);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration3.JE_RL_NKPortOfLoading = mailPort.RL_Code;
			declaration3.JE_IATALoadPort = "HIN";
			AssertEquals("HIN", declaration3.KRPortOfLoading);
			AssertEquals("When JE_TransportMode is MAI and JE_IATALoadPort is not empty, find the explanation in IATALoadPortKRList.", IATALoadPortKRList.Descriptions.HIN, declaration3.LoadPortNameInKorean);

			declaration3.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration3.JE_RL_NKPortOfLoading = mailPort.RL_Code;
			declaration3.JE_IATALoadPort = "";
			AssertEquals("XXHIN", declaration3.KRPortOfLoading);
			AssertEquals("When JE_TransportMode is MAI and JE_IATALoadPort is empty, find the explanation in RefLanguageText.", mailPortText.RLT_Text, declaration3.LoadPortNameInKorean);
		}

		public void TestLastestCustomsEntryIssueDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			AssertEquals(ZDateTime.Empty, declaration.LastestCustomsEntryIssueDate);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 1, 1);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = declaration.JE_MessageType;
			entry2.EntryNumber = "1234567891I";
			var entryNum2 = entry2.CusEntryNumber;
			entryNum2.CE_IssueDate = new ZDateTime(2022, 1, 4);

			AssertEquals(new ZDateTime(2022, 1, 4), declaration.LastestCustomsEntryIssueDate);

			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = declaration.JE_MessageType;
			entry3.EntryNumber = "1234567892I";

			AssertEquals(new ZDateTime(2022, 1, 4), declaration.LastestCustomsEntryIssueDate);
		}

		public void TestCopytoInvoiceOrganization()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_OH_Manufacturer = manufacturer1.PK;
			declaration.JE_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			declaration.JE_OH_Importer = importer1.PK;

			AssertNotEquals(declaration.Supplier, invoiceHeader.Supplier);
			AssertEquals(declaration.Manufacturer, invoiceHeader.Manufacturer);
			AssertEquals(declaration.ManufacturerAddress, invoiceHeader.ManufacturerAddress);
			AssertEquals(declaration.Importer, invoiceHeader.Buyer);

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_Supplier = supplier2.PK;
			declaration.JE_OH_Manufacturer = manufacturer2.PK;
			declaration.JE_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			declaration.JE_OH_Importer = importer2.PK;

			AssertNotEquals(declaration.Supplier, invoiceHeader.Supplier);
			AssertNotEquals(declaration.Manufacturer, invoiceHeader.Manufacturer);
			AssertNotEquals(declaration.ManufacturerAddress, invoiceHeader.ManufacturerAddress);
			AssertNotEquals(declaration.Importer, invoiceHeader.Buyer);

			AssertNotEquals(supplier1, invoiceHeader.Supplier);
			AssertEquals(manufacturer1, invoiceHeader.Manufacturer);
			AssertEquals(manufacturer1.MainAddress, invoiceHeader.ManufacturerAddress);
			AssertEquals(importer1, invoiceHeader.Buyer);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var importer3 = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_Importer = importer3.PK;
			AssertNotEquals(declaration.Importer, invoiceHeader.Buyer);
		}

		public void TestJE_ExportDate()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var info = declaration.JE_ExportDateInfo;
				var dataBoundBusinessObject = new DataBoundBusinessObject(declaration);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(
					info,
					multipleResourceKey: null,
					caption: "Departure",
					shortCaption: "Dep.",
					dataBoundBusinessObject: dataBoundBusinessObject);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(
					info,
					multipleResourceKey: KRJobMessageTypeList.Codes.PersonalItems,
					caption: "Start Date",
					dataBoundBusinessObject: dataBoundBusinessObject);
			});
		}

		[TestDate(2023, 2, 28)]
		public void TestMRNTypeAndMRNNumber()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST SANTA";
			vessel.RV_MalaysiaVesselId = "1";
			vessel.RV_RadioCallSign = "RAD123";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.JE_VesselName = "TEST SANTA";

			declaration.JE_MRNType = MRNTypeList.Codes.Normal;
			AssertEquals("", declaration.MRNJ3_ReferenceNumber);

			declaration.MRNJ3_ReferenceNumber = "AAAAA";
			AssertEquals("AAAAA", declaration.MRNJ3_ReferenceNumber);

			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;
			AssertEquals("23ZZZZZZZZZ", declaration.MRNJ3_ReferenceNumber);

			declaration.JE_MRNType = MRNTypeList.Codes.ChangeOfQualification;
			AssertEquals("3RAD123", declaration.MRNJ3_ReferenceNumber);

			declaration.JE_MRNType = MRNTypeList.Codes.ScheduledToArrive;
			AssertEquals("4RAD123", declaration.MRNJ3_ReferenceNumber);

			declaration.MRNJ3_ReferenceNumber = "BBBBB";
			AssertEquals("BBBBB", declaration.MRNJ3_ReferenceNumber);
		}

		[TestDate(2023, 2, 28)]
		public void TestMRNNumberByIssueDate()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TEST SANTA";
			vessel.RV_LloydsNumber = "1";
			vessel.RV_RadioCallSign = "RAD123";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.JE_VesselName = "TEST SANTA";
			declaration.MRNJ3_ReferenceNumber = "AAAAA";

			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;
			AssertEquals("23ZZZZZZZZZ", declaration.MRNJ3_ReferenceNumber);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = declaration.JE_MessageType;
			entry1.EntryNumber = "1234567890I";
			var entryNum1 = entry1.CusEntryNumber;
			entryNum1.CE_IssueDate = new ZDateTime(2022, 12, 26);
			entryNum1.CE_EntryLineReference = "1";

			declaration.JE_MRNType = MRNTypeList.Codes.Normal;
			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;
			AssertEquals("22ZZZZZZZZZ", declaration.MRNJ3_ReferenceNumber);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = declaration.JE_MessageType;
			entry2.EntryNumber = "1234567891I";
			var entryNum2 = entry2.CusEntryNumber;
			entryNum2.CE_IssueDate = new ZDateTime(2023, 1, 4);
			entryNum2.CE_EntryLineReference = "2";

			declaration.JE_MRNType = MRNTypeList.Codes.Normal;
			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;
			AssertEquals("22ZZZZZZZZZ", declaration.MRNJ3_ReferenceNumber);
		}

		public void TestGetLocalExportMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(ZString.Empty, declaration.GetLocalExportMessageType());

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._03;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._04;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, declaration.GetLocalExportMessageType());

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, declaration.GetLocalExportMessageType());
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, declaration.GetLocalExportMessageType());
		}

		public void TestChangeMessageTypeIsNonTransportDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertEquals(expected: true, declaration.IsNonTransportDeclarationType);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(expected: false, declaration.IsNonTransportDeclarationType);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertEquals(expected: true, declaration.IsNonTransportDeclarationType);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertEquals(expected: true, declaration.IsNonTransportDeclarationType);

			var orgSupplier = Factory.NewWithValidTestData<OrgHeader>();
			orgSupplier.OH_RL_NKClosestPort = "KRPUS";
			declaration.JE_OH_Supplier = orgSupplier.PK;
			AssertEquals("", declaration.JE_RL_NKPortOfLoading);

			var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
			orgImporter.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_OH_Importer = orgImporter.PK;
			AssertEquals("", declaration.JE_RL_NKPortOfArrival);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(expected: false, declaration.IsNonTransportDeclarationType);
			AssertEquals("KRPUS", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfArrival);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(expected: true, declaration.IsNonTransportDeclarationType);
			AssertEquals("", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("", declaration.JE_RL_NKPortOfArrival);
		}

		public void TestJE_MessageType_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType_ReadOnly = true;

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType_ReadOnly = false;

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(false, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(false, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertEquals(true, declaration.JE_MessageType_ReadOnly);
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("", declaration.JE_TotalNoOfPacksPackType);
			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.RO;
			AssertEquals("RO", declaration.JE_TotalNoOfPacksPackType);
		}
		public void TestCopytoInvoiceSupplierAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var customsAddress = supplier1.Addresses.AddNew();
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			customsAddress.OA_Address1 = "test1";

			declaration.JE_OH_Supplier = supplier1.PK;
			AssertEquals("defaulted to customs address", customsAddress, declaration.SupplierAddress);
			AssertNull(invoiceHeader.Supplier);
			AssertNull(invoiceHeader.SupplierAddress);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var invoiceHeader1 = declaration1.Invoices.AddNew();

			declaration1.JE_OH_Supplier = supplier1.PK;
			AssertNull(invoiceHeader1.Supplier);
			AssertNull(invoiceHeader1.SupplierAddress);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoiceHeader2 = declaration2.Invoices.AddNew();

			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			declaration2.JE_OH_Supplier = supplier2.PK;
			AssertEquals(declaration2.Supplier, invoiceHeader2.Supplier);
			AssertNotEquals(declaration2.Supplier.MainAddress, invoiceHeader2.SupplierAddress);
		}

		public void TestImporterAddressSettingWhenImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importerAddress = importer1.Addresses.AddNew();
			importerAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(declaration.ImporterAddress, importerAddress);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(declaration.Importer, invoice.Buyer);
		}

		public void TestDefaultMessageTypeFromSupplierOrImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var code = "KRMIK";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = code;
			unloco.RL_RN_NKCountryCode = "KR";

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_RL_NKClosestPort = code;

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_RL_NKClosestPort = code;

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(declaration.JE_MessageType, KRJobMessageTypeList.Codes.Import);

			declaration.JE_OH_Supplier = supplier1.PK;
			AssertEquals(declaration.JE_MessageType, KRJobMessageTypeList.Codes.Export);

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier1.PK;
			AssertEquals(declaration.JE_MessageType, ElectronicDocumentTypeList.Codes._D87);

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(declaration.JE_MessageType, ElectronicDocumentTypeList.Codes._008);

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(declaration.JE_MessageType, ElectronicDocumentTypeList.Codes._5SM);
		}

		public void TestShouldKeepDeletedLinesOnAmendmentCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "USED EXCAVATOR");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "899999999", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "품명2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8523491020", new ZDateTime(2014, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "DISCS FOR LASER READING SYSTEMS FOR REPRODUCING SO");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			invoiceLine.JI_Tariff = "8429521022";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_Tariff = "899999999";

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 3;
			invoiceLine3.JI_Tariff = "8523491020";

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];

			var export830 = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(3, entry.AllEntryLines.Count);
			AssertEquals((short)3, entry.CH_HighestLineNumber);

			var entryLine2 = invoiceLine2.CusEntryLine;

			invoiceLine2.Delete();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, entry.AllEntryLines.Count);
			AssertEquals(true, entryLine2.IsDeleted);
		}

		public void TestGetTemplateCopyStrategy()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			var stratety = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
			AssertType<JobDeclarationDeepCloneStrategy>(stratety);
		}

		public void TestCopyBondedWarehouseInLEX()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;

			var organization = Factory.New<OrgHeader>();
			organization.MainAddress.Address1 = "Address 111";
			var cusCode = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = Constants.IdentificationType.ControlledPremisesID, Number = "01001001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(organization.MainAddress, cusCode);

			declaration.WarehouseDocAddress.E2_OA_Address = organization.MainAddress.PK;

			AssertEquals("01001001", declaration.JE_LocationOtherInformation);
			AssertEquals("경의선철도 입출경검사장(지상)", declaration.JE_SubLocationOfGoods);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._18;
			AssertEquals("", declaration.JE_LocationOtherInformation);
			AssertEquals("", declaration.JE_SubLocationOfGoods);

			var newOrg = Factory.New<OrgHeader>();
			declaration.WarehouseDocAddress.E2_OA_Address = newOrg.MainAddress.PK;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			AssertEquals("", declaration.JE_LocationOtherInformation);
			AssertEquals("", declaration.JE_SubLocationOfGoods);

			declaration.WarehouseDocAddress.E2_OA_Address = organization.MainAddress.PK;
			AssertEquals("01001001", declaration.JE_LocationOtherInformation);
			AssertEquals("경의선철도 입출경검사장(지상)", declaration.JE_SubLocationOfGoods);
		}

		public void TestRemoveData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.JE_EntryDate = ZDate.Today;
			declaration.JE_VoyageDuration = 1;
			declaration.JE_NoOfCrew = 2;
			declaration.Persons.AddNew();
			declaration.TransportMeans.AddNew();

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			AssertEquals(ZDate.Today, declaration.JE_EntryDate);
			AssertEquals(1, declaration.JE_VoyageDuration);
			AssertEquals(2, declaration.JE_NoOfCrew);
			AssertEquals(1, declaration.Persons.Count);
			AssertEquals(1, declaration.TransportMeans.Count);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			AssertEquals(ZDate.Empty, declaration.JE_EntryDate);
			AssertEquals(ZInt.Zero, declaration.JE_VoyageDuration);
			AssertEquals(ZInt.Zero, declaration.JE_NoOfCrew);
			AssertEquals(0, declaration.Persons.Count);
			AssertEquals(0, declaration.TransportMeans.Count);

			declaration.JE_EntryDate = ZDate.Today;
			declaration.JE_VoyageDuration = 1;
			declaration.JE_NoOfCrew = 2;
			declaration.Persons.AddNew();
			declaration.TransportMeans.AddNew();
			AssertEquals(ZDate.Today, declaration.JE_EntryDate);
			AssertEquals(1, declaration.JE_VoyageDuration);
			AssertEquals(2, declaration.JE_NoOfCrew);
			AssertEquals(1, declaration.Persons.Count);
			AssertEquals(1, declaration.TransportMeans.Count);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._06;
			AssertEquals(ZDate.Empty, declaration.JE_EntryDate);
			AssertEquals(ZInt.Zero, declaration.JE_VoyageDuration);
			AssertEquals(ZInt.Zero, declaration.JE_NoOfCrew);
			AssertEquals(0, declaration.Persons.Count);
			AssertEquals(0, declaration.TransportMeans.Count);

			declaration.JE_EntryDate = ZDate.Today;
			declaration.JE_VoyageDuration = 1;
			declaration.JE_NoOfCrew = 2;
			declaration.Persons.AddNew();
			declaration.TransportMeans.AddNew();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals(ZDate.Empty, declaration.JE_EntryDate);
			AssertEquals(ZInt.Zero, declaration.JE_VoyageDuration);
			AssertEquals(ZInt.Zero, declaration.JE_NoOfCrew);
			AssertEquals(0, declaration.Persons.Count);
			AssertEquals(0, declaration.TransportMeans.Count);
		}

		public void TestHasInvoiceLinesEligibleForSimpleDrawback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202201000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0202301000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var condtionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", RefCusConditionType.SimpleDrawback);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff1.PK, "Valid test value", false, true, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.KoreaSouth, condtionType.PK, tariff2.PK, "Invalid test value", false, true, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals(false, declaration.HasInvoiceLinesEligibleForSimpleDrawback);

			invoiceLine1.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals(true, declaration.HasInvoiceLinesEligibleForSimpleDrawback);
			invoiceLine2.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals(true, declaration.HasInvoiceLinesEligibleForSimpleDrawback);

			invoiceLine1.Delete();
			AssertEquals(false, declaration.HasInvoiceLinesEligibleForSimpleDrawback);
		}

		public void TestSetValidationModesBasedOnData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertEquals("Export", declaration.ValidationMode.ToString());

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals("LocalExport", declaration.ValidationMode.ToString());

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals("Import", declaration.ValidationMode.ToString());

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			AssertEquals("PersonalItemDec", declaration.ValidationMode.ToString());

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			AssertEquals("CarnetCertificate", declaration.ValidationMode.ToString());
		}

		public void TestSetAndRemoveValidationModeOnElectronicMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals("Import", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("Import, FTA", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals("Import, FTA, DetailedFTA", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			AssertEquals("Import, FTA, DetailedFTA, ValuationDeclaration", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._D72);
			AssertEquals("Import, FTA, DetailedFTA, ValuationDeclaration, ExtendReExport", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UA);
			AssertEquals("Import, FTA, DetailedFTA, ValuationDeclaration, ExtendReExport, PenaltyExemption", declaration.ValidationMode.ToString());

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals("Import, FTA, DetailedFTA, ValuationDeclaration, ExtendReExport, PenaltyExemption, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("Import, DetailedFTA, ValuationDeclaration, ExtendReExport, PenaltyExemption, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals("Import, ValuationDeclaration, ExtendReExport, PenaltyExemption, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			AssertEquals("Import, ExtendReExport, PenaltyExemption, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._D72);
			AssertEquals("Import, PenaltyExemption, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UA);
			AssertEquals("Import, RefundRequest", declaration.ValidationMode.ToString());

			declaration.RemoveValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals("Import", declaration.ValidationMode.ToString());
		}

		public void TestMessageType008()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(0, declaration.Invoices.Count);
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertEquals("BLT", declaration.JE_ApplicationCode);
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.Charges.Count);
			var charge = invoice.Charges[0];
			AssertEquals(CustomsChargeTypeList.Codes.OverseasFreight, charge.J7_ChargeType);
		}

		public void TestOFTProxyFields()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
				var invoiceHeader = declaration.Invoices[0];
				var chargeOFT = invoiceHeader.Charges[0];
				chargeOFT.J7_Amount = 10.0m;

				AssertEquals(chargeOFT.J7_Amount, declaration.PIDFreightAmount);

				declaration.PIDFreightAmount = 20m;
				AssertEquals(20m, chargeOFT.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, chargeOFT.J7_RX_NKCurrency);
			}
		}

		public void TestWhenJE_MessageTypeSetValueIsD87()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(0, declaration.Invoices.Count);
			AssertEquals(0, declaration.InvoiceLines.Count);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertEquals("BLT", declaration.JE_ApplicationCode);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(1, declaration.InvoiceLines.Count);
		}

		public void TestD87InvoiceAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			AssertEquals(0m, declaration.D87InvoiceAmount);
			AssertEquals(0m, declaration.InvoiceLines[0].JI_LinePrice);
			AssertEquals(ZString.Empty, declaration.D87InvoiceCurrency);

			declaration.D87InvoiceAmount = 100m;
			declaration.D87InvoiceCurrency = "KRW";
			AssertEquals(100m, declaration.D87InvoiceAmount);
			AssertEquals(100m, declaration.InvoiceLines[0].JI_LinePrice);
			AssertEquals("KRW", declaration.D87InvoiceCurrency);
		}

		public void TestD87HBSplitDecInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			AssertEquals(ZString.Empty, declaration.JE_HouseBill);
			AssertEquals(0, declaration.Bills.Count);
			Assert(declaration.D87HBSplitDecIndInfo.ReadOnly);

			declaration.JE_HouseBill = "HB1";
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("HB1", declaration.Bills[0].CU_BillNum);
			Assert(!declaration.D87HBSplitDecIndInfo.ReadOnly);
		}

		public void TestGetValidation()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			AssertEquals(typeof(EXPJobDeclarationValidation), declaration.Validation.GetType());

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			AssertEquals(typeof(IMPJobDeclarationValidation), declaration.Validation.GetType());

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(typeof(LEXJobDeclarationValidation), declaration.Validation.GetType());

			declaration.JE_MessageType = ZString.Empty;
			AssertEquals(typeof(JobDeclarationValidation), declaration.Validation.GetType());
		}

		public void TestJE_CustomsLoadPort()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._13;
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, declaration.JE_CustomsLoadPort);
		}
		public void TestIsImporterInformationRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			Assert(!declaration.IsImporterInformationRequired);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			Assert(declaration.IsImporterInformationRequired);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.B;
			Assert(declaration.IsImporterInformationRequired);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.F;
			Assert(declaration.IsImporterInformationRequired);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.G;
			Assert(declaration.IsImporterInformationRequired);

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			Assert(declaration.IsImporterInformationRequired);
		}

		public void TestJE_OH_DutyPayer_Import()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var importer1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "READYKOREA1");
			var importer2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA1");
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "READYKOREA2");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer2, payerCodes);
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			declaration.JE_OH_Importer = importer1.PK;
			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;
			AssertEquals(declaration.JE_OH_DutyPayer, importer1.PK);
			AssertEquals(true, declaration.JE_OH_DutyPayerInfo.ReadOnly);

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(declaration.JE_OH_DutyPayer, importer2.PK);
			AssertEquals(true, declaration.JE_OH_DutyPayerInfo.ReadOnly);

			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			AssertEquals(declaration.JE_OH_DutyPayer, ZGuid.Empty);
			AssertEquals(false, declaration.JE_OH_DutyPayerInfo.ReadOnly);

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(declaration.JE_OH_DutyPayer, ZGuid.Empty);
			AssertEquals(false, declaration.JE_OH_DutyPayerInfo.ReadOnly);

			declaration.JE_OH_DutyPayer = payer.PK;
			AssertEquals(declaration.JE_OH_DutyPayer, payer.PK);
		}

		public override void TestDeclarantCode()
		{
			Assert("KR does not make use of the column.", true);
		}

		public override void TestDeclarantName()
		{
			Assert("KR does not make use of the column.", true);
		}

		public override void TestIsOverrideRecipientEmailEnabled()
		{
			Assert("KR does not make use of the column.", true);
		}

		public void TestAreImporterDutyPayerTheSame()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;

			var importer1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "READYKOREA1");
			var importer2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA1");
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "READYKOREA2");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer2, payerCodes);
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_DutyPayer = ZGuid.Empty;
			AssertEquals(false, declaration.AreImporterDutyPayerTheSame);

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(false, declaration.AreImporterDutyPayerTheSame);

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(false, declaration.AreImporterDutyPayerTheSame);

			declaration.JE_OH_DutyPayer = importer2.PK;
			AssertEquals(true, declaration.AreImporterDutyPayerTheSame);

			declaration.JE_OH_DutyPayer = importer1.PK;
			AssertEquals(false, declaration.AreImporterDutyPayerTheSame);

			declaration.JE_OH_DutyPayer = payer.PK;
			AssertEquals(true, declaration.AreImporterDutyPayerTheSame);
		}

		public void TestMessageType5SM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertEquals("BLT", declaration.JE_ApplicationCode);
		}

		public void TestIsValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.IsValidationModeSetForDetailedFTA);
			AssertEquals(false, declaration.IsValidationModeSetFor934);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals(true, declaration.IsValidationModeSetForDetailedFTA);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			AssertEquals(true, declaration.IsValidationModeSetFor934);
		}

		public void TestTransshipmentData()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_TransshipmentPort = "KRABC";
			AssertEquals(YesNo.Yes, declaration.TransshipmentYN);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, declaration.TransshipmentCountryCode);

			declaration.JE_TransshipmentPort = ZString.Empty;
			AssertEquals(YesNo.No, declaration.TransshipmentYN);
			AssertNullOrEmpty(declaration.TransshipmentCountryCode);
		}

		public void TestPIDHasItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			declaration.OnHasItemsChanging += new System.ComponentModel.CancelEventHandler(OnHasItemsChanging);
			declaration.InvoiceLines.AddNew().JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			declaration.InvoiceLines.AddNew().JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			declaration.InvoiceLines.AddNew().JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(YesNoList.Codes.Yes, declaration.PIDHasItems);
			AssertEquals(3, declaration.InvoiceLines.Count);

			AssertEquals(true, declaration.HasPIDItemLinesAboutToLose(YesNoList.Codes.No));
			declaration.PIDHasItems = YesNoList.Codes.No;
			AssertEquals(1, declaration.InvoiceLines.Count);
			AssertEquals(ZString.Empty, declaration.InvoiceLines[0].JI_CountryOfOrigin);

			AssertEquals(false, declaration.HasPIDItemLinesAboutToLose(YesNoList.Codes.Yes));
			declaration.PIDHasItems = YesNoList.Codes.Yes;
			AssertEquals(0, declaration.InvoiceLines.Count);

			AssertEquals(false, declaration.HasPIDItemLinesAboutToLose(YesNoList.Codes.No));

			void OnHasItemsChanging(object sender, System.ComponentModel.CancelEventArgs e)
			{
			}
		}

		public void TestPIDQuestions()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.PIDWeapon);
			AssertEquals(ZString.Empty, declaration.PIDDrug);
			AssertEquals(ZString.Empty, declaration.PIDAnimal);
			AssertEquals(ZString.Empty, declaration.PIDEndangeredItems);
			AssertEquals(ZString.Empty, declaration.PIDCounterfeit);
			AssertEquals(ZString.Empty, declaration.PIDCommercialUseItems);
			AssertEquals(ZString.Empty, declaration.PIDExcessTimeLimitItems);
			AssertEquals(ZString.Empty, declaration.PIDPornography);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;

			AssertEquals(YesNoList.Codes.No, declaration.PIDWeapon);
			AssertEquals(YesNoList.Codes.No, declaration.PIDDrug);
			AssertEquals(YesNoList.Codes.No, declaration.PIDAnimal);
			AssertEquals(YesNoList.Codes.No, declaration.PIDEndangeredItems);
			AssertEquals(YesNoList.Codes.No, declaration.PIDCounterfeit);
			AssertEquals(YesNoList.Codes.No, declaration.PIDCommercialUseItems);
			AssertEquals(YesNoList.Codes.No, declaration.PIDExcessTimeLimitItems);
			AssertEquals(YesNoList.Codes.No, declaration.PIDPornography);
		}

		public void TestVDAuthor()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "Peter";
			contact.OC_Phone = "02-123-4562";
			contact.OC_Title = "Manager";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			declaration.JE_OH_Importer = importer.PK;
			declaration.VDAuthor = contact.PK;
			AssertEquals("Peter", declaration.JE_AuthorName);
			AssertEquals("02-123-4562", declaration.JE_AuthorPhone);
			AssertEquals("Manager", declaration.JE_AuthorJobTitle);

			declaration.VDAuthor = ZGuid.Empty;
			AssertEquals("", declaration.JE_AuthorName);
			AssertEquals("", declaration.JE_AuthorPhone);
			AssertEquals("", declaration.JE_AuthorJobTitle);

			declaration.VDAuthor = contact.PK;
			AssertEquals("Peter", declaration.JE_AuthorName);
			AssertEquals("02-123-4562", declaration.JE_AuthorPhone);
			AssertEquals("Manager", declaration.JE_AuthorJobTitle);

			importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "(주)레디코리아2");
			contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "Peter";
			contact.OC_Phone = "02-123-4562";
			contact.OC_Title = "Manager";
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(ZGuid.Empty, declaration.VDAuthor);
			AssertEquals("", declaration.JE_AuthorName);
			AssertEquals("", declaration.JE_AuthorPhone);
			AssertEquals("", declaration.JE_AuthorJobTitle);
		}

		public void TestVDAuditor()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "Paul";
			contact.OC_Phone = "02-123-1234";
			contact.OC_Title = "Accounts";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			declaration.JE_OH_Importer = importer.PK;
			declaration.VDAuditor = contact.PK;
			AssertEquals("Paul", declaration.JE_AuditorName);
			AssertEquals("02-123-1234", declaration.JE_AuditorPhone);
			AssertEquals("Accounts", declaration.JE_AuditorJobTitle);

			declaration.VDAuditor = ZGuid.Empty;
			AssertEquals("", declaration.JE_AuditorName);
			AssertEquals("", declaration.JE_AuditorPhone);
			AssertEquals("", declaration.JE_AuditorJobTitle);

			declaration.VDAuditor = contact.PK;
			AssertEquals("Paul", declaration.JE_AuditorName);
			AssertEquals("02-123-1234", declaration.JE_AuditorPhone);
			AssertEquals("Accounts", declaration.JE_AuditorJobTitle);

			importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "(주)레디코리아2");
			contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "Paul";
			contact.OC_Phone = "02-123-1234";
			contact.OC_Title = "Accounts";
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(ZGuid.Empty, declaration.VDAuditor);
			AssertEquals("", declaration.JE_AuditorName);
			AssertEquals("", declaration.JE_AuditorPhone);
			AssertEquals("", declaration.JE_AuditorJobTitle);
		}

		public void TestKR_SimpleDRWAppAndKR_DRWApplicantType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.NO;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Supplier;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Supplier;
			AssertEquals(ApplicationForSimpleDrawbackCodeList.Codes.NO, declaration.JE_SimpleDRWApp);
			AssertEquals(DrawbackApplicantTypeList.Codes.Supplier, invoice1.JZ_DRWApplicantType);
			AssertEquals(DrawbackApplicantTypeList.Codes.Supplier, invoice2.JZ_DRWApplicantType);

			declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.AD;
			AssertEquals(ApplicationForSimpleDrawbackCodeList.Codes.AD, declaration.JE_SimpleDRWApp);
			AssertEquals(DrawbackApplicantTypeList.Codes.Manufacturer, invoice1.JZ_DRWApplicantType);
			AssertEquals(DrawbackApplicantTypeList.Codes.Manufacturer, invoice2.JZ_DRWApplicantType);

			var invoice3 = declaration.Invoices.AddNew();
			AssertEquals(DrawbackApplicantTypeList.Codes.Manufacturer, invoice3.JZ_DRWApplicantType);
		}

		public override void TestJE_DateOfArrivalCaption()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var dataBoundBusinessObject = new DataBoundBusinessObject(declaration);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(
					declaration.JE_DateOfArrivalInfo,
					multipleResourceKey: KRJobMessageTypeList.Codes.Import,
					caption: "Arrival Date",
					mediumCaption: "Arrival",
					shortCaption: "Arr.",
					dataBoundBusinessObject: dataBoundBusinessObject);
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(
					declaration.JE_DateOfArrivalInfo,
					multipleResourceKey: KRJobMessageTypeList.Codes.PersonalItems,
					caption: "Arrival Date",
					dataBoundBusinessObject: dataBoundBusinessObject);
			});
		}

		#region Implementation
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		protected override bool ExpectedSupportInvoiceLineRefs => true;

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}
		#endregion
	}

	#region JobDeclarationForTesting
	public class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);
	}
	#endregion
}
