using System;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	abstract class CusTempStorageJobHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : SumACusTempStorageJobHeaderProvider
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), new object[] { null });
		}

		[TestDate(2019, 2, 25, 15, 53, 15)]
		public void TestPreparationDateAndTimeCET()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Date", new DateTime(2019, 2, 25), TempStorageHeaderWrapped.PreparationDateAndTimeCET.Date);
				AssertEquals("Time no seconds", "16:53:00", TempStorageHeaderWrapped.PreparationDateAndTimeCET.Time);
			});
		}

		public void TestInterchangeSenderEoriNumber()
		{
			CombineAssertions(() =>
			{
				tempStorageHeader.SJH_OA_Representative = ZGuid.Empty;
				tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
				AssertEquals(string.Empty, TempStorageHeaderWrapped.InterchangeSenderEoriNumber);
			});
		}

		public void TestInterchangeSenderEoriNumber_Representative()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var representative = Factory.NewWithValidTestData<OrgHeader>();
				tempStorageHeader.SJH_OA_Representative = representative.MainAddress.PK;
				AddGreekEoriNumber(representative, "1234567890");
				AssertEquals("Representative eori", "GR1234567890", TempStorageHeaderWrapped.InterchangeSenderEoriNumber);
			});
		}

		public void TestInterchangeSenderEoriNumber_Presenter()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var presenter = Factory.NewWithValidTestData<OrgHeader>();
				tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
				AddGreekEoriNumber(presenter, "9876543210");
				AssertEquals("Presenter eori", "GR9876543210", TempStorageHeaderWrapped.InterchangeSenderEoriNumber);
			});
		}

		public void TestInterchangeSenderEoriBranch()
		{
			tempStorageHeader.SJH_OA_Representative = ZGuid.Empty;
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertEquals(string.Empty, TempStorageHeaderWrapped.InterchangeSenderEoriBranch);
		}

		public void TestInterchangeSenderEoriBranch_Representative()
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			tempStorageHeader.SJH_OA_Representative = representative.MainAddress.PK;
			AddCusCodeWithPremisesAddress(representative, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "1111");
			AssertEquals("1111", TempStorageHeaderWrapped.InterchangeSenderEoriBranch);
		}

		public void TestInterchangeSenderEoriBranch_Presenter()
		{
			var presenter = Factory.NewWithValidTestData<OrgHeader>();
			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AddCusCodeWithPremisesAddress(presenter, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "2222");
			AssertEquals("2222", TempStorageHeaderWrapped.InterchangeSenderEoriBranch);
		}

		public void TestInterchangeRecipientReferenceNumber()
		{
			tempStorageHeader.SJH_CustomsOffice = "DE002102";
			AssertEquals("DE002102", TempStorageHeaderWrapped.InterchangeRecipientReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			tempStorageHeader.SJH_ReferenceNumber = "TS00000001";
			AssertEquals("TS00000001", TempStorageHeaderWrapped.LocalReferenceNumber);
		}

		public void TestLocalReferenceNumber_SJH_ReferenceNumberIsEmpty()
		{
			tempStorageHeader.SJH_ReferenceNumber = "";
			tempStorageHeader.SJH_JobReference = "TS00000001";
			AssertEquals("TS00000001", TempStorageHeaderWrapped.LocalReferenceNumber);
		}

		public void TestArrivalDate()
		{
			var arrivalDate = new ZDate(2018, 11, 15);
			tempStorageHeader.SJH_ArrivalDate = arrivalDate;
			AssertEquals(arrivalDate, TempStorageHeaderWrapped.ArrivalDate);
		}

		public void TestArrivalDate_Empty()
		{
			tempStorageHeader.SJH_ArrivalDate = ZDate.Empty;
			AssertEquals(null, TempStorageHeaderWrapped.ArrivalDate);
		}

		public void TestArrivalDate_Invalid()
		{
			tempStorageHeader.SJH_ArrivalDate = ZDate.Invalid;
			AssertEquals(null, TempStorageHeaderWrapped.ArrivalDate);
		}

		public void TestPresentationDate()
		{
			var presentationDate = new ZDate(2018, 11, 15);
			tempStorageHeader.SJH_PresentationDate = presentationDate;
			AssertEquals(presentationDate, TempStorageHeaderWrapped.PresentationDate);
		}

		public void TestPresentationDate_Empty()
		{
			tempStorageHeader.SJH_PresentationDate = ZDate.Empty;
			AssertEquals(null, TempStorageHeaderWrapped.PresentationDate);
		}

		public void TestPresentationDate_Invalid()
		{
			tempStorageHeader.SJH_PresentationDate = ZDate.Invalid;
			AssertEquals(null, TempStorageHeaderWrapped.PresentationDate);
		}

		public void TestNCTSFlag()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "SUMDATA");
			CombineAssertions(() =>
			{
				tempStorageHeader.SJH_NCTSFlag = true;
				AssertEquals("Not Supported", false, TempStorageHeaderWrapped.NCTSFlag);
				tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
				AssertEquals("Supported", true, TempStorageHeaderWrapped.NCTSFlag);
			});
		}

		public void TestMaritimeTransportFlag()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var customsOffice1 = helper.CreateNewOrGetExistingCusCodeList("DE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE019004", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute1 = helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice1.PK, "ROLE", "DES");
			helper.CreateTransportModeForCusCodeAttribute(attribute1.PK, "AIR");
			var customsOffice2 = helper.CreateNewOrGetExistingCusCodeList("DE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002102", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attribute2 = helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice2.PK, "ROLE", "DES");
			helper.CreateTransportModeForCusCodeAttribute(attribute2.PK, "SEA");
			Factory.Save();

			CombineAssertions(() =>
			{
				tempStorageHeader.SJH_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("No Customs Office", false, TempStorageHeaderWrapped.MaritimeTransportFlag);
				tempStorageHeader.SJH_CustomsOffice = "DE019004";
				AssertEquals("Air Customs Office", false, TempStorageHeaderWrapped.MaritimeTransportFlag);
				tempStorageHeader.SJH_CustomsOffice = "DE002102";
				AssertEquals("Sea Customs Office", true, TempStorageHeaderWrapped.MaritimeTransportFlag);
			});
		}

		public void TestLoadingPlace()
		{
			tempStorageHeader.SJH_RL_NKLoading = "AREZE";
			AssertEquals("EZE", TempStorageHeaderWrapped.LoadingPlace);
		}

		public void TestLoadingPlace_Empty()
		{
			tempStorageHeader.SJH_RL_NKLoading = ZString.Empty;
			AssertNull(TempStorageHeaderWrapped.LoadingPlace);
		}

		public void TestUnloadingPlace()
		{
			tempStorageHeader.SJH_RL_NKLoading = "AREZE";
			AssertEquals("Ministro Pistarini Apt/Buenos Aires", TempStorageHeaderWrapped.UnloadingPlace);
		}

		public void TestUnloadingPlace_Empty()
		{
			tempStorageHeader.SJH_RL_NKLoading = ZString.Empty;
			AssertNull(TempStorageHeaderWrapped.UnloadingPlace);
		}

		public void TestAdditionalInformation()
		{
			tempStorageHeader.SJH_AdditionalInformation = "TEST ADDITIONAL INFORMATION";
			AssertEquals("TEST ADDITIONAL INFORMATION", TempStorageHeaderWrapped.AdditionalInformation);
		}

		public void TestAuthorisationNumber()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			tempStorageHeader.SJH_OA_Representative = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_Presenter()
		{
			CombineAssertions(() =>
			{
				var presenter = Factory.New<OrgHeader>();
				tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
				tempStorageHeader.SJH_OA_Representative = ZGuid.Empty;
				AssertEquals("No Authorisation No.", ZString.Empty, TempStorageHeaderWrapped.AuthorisationNumber);
				AddCusCodeWithPremisesAddress(presenter, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "PRESENTERATLAS");
				AssertEquals("Authorisation No.", "PRESENTERATLAS", TempStorageHeaderWrapped.AuthorisationNumber);
			});
		}

		public void TestAuthorisationNumber_Representative()
		{
			CombineAssertions(() =>
			{
				var representative = Factory.New<OrgHeader>();
				tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
				tempStorageHeader.SJH_OA_Representative = representative.MainAddress.PK;
				AssertNull("No Authorisation No.", TempStorageHeaderWrapped.AuthorisationNumber);
				AddCusCodeWithPremisesAddress(representative, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "REPRESENTATIVEATLAS");
				AssertEquals("Authorisation No.", "REPRESENTATIVEATLAS", TempStorageHeaderWrapped.AuthorisationNumber);
			});
		}

		public void TestPresenterEoriNumber()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertCustomsCodeCodeValue(() => TempStorageHeaderWrapped.PresenterEoriNumber, tempStorageHeader.SJH_OA_PresenterInfo, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Greece, "SUMVALUE", "GRSUMVALUE");
		}

		public void TestPresenterEoriBranch()
		{
			AssertCustomsCodeCodeValue(() => TempStorageHeaderWrapped.PresenterEoriBranch, tempStorageHeader.SJH_OA_PresenterInfo, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, "1111", "1111");
		}

		public void TestPresenterName()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.OH_FullName = "TEST PRESENTER";

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals("TEST PRESENTER", TempStorageHeaderWrapped.PresenterName);
		}

		public void TestPresenterName_Empty()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterName);
		}

		public void TestPresenterAddress()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.MainAddress.OA_Address1 = "MAIN ADDRESS 1";

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals("MAIN ADDRESS 1", TempStorageHeaderWrapped.PresenterAddress);
		}

		public void TestPresenterAddress_Null()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterAddress);
		}

		public void TestPresenterCountry()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Kosovo;

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals(Core.Constants.CountryCodes.Kosovo, TempStorageHeaderWrapped.PresenterCountry);
		}

		public void TestPresenterCountry_Null()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterCountry);
		}

		public void TestPresenterPostcode()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.MainAddress.OA_PostCode = "123456";

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals("123456", TempStorageHeaderWrapped.PresenterPostcode);
		}

		public void TestPresenterPostcode_Null()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterPostcode);
		}

		public void TestPresenterCity()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.MainAddress.OA_City = "NEW YORK";

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals("NEW YORK", TempStorageHeaderWrapped.PresenterCity);
		}

		public void TestPresenterCity_Null()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterCity);
		}

		public void TestPresenterDistrict()
		{
			var presenter = Factory.New<OrgHeader>();
			presenter.MainAddress.OA_Address2 = "PRESENTER ADDRESS 2";

			tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
			AssertEquals("PRESENTER ADDRESS 2", TempStorageHeaderWrapped.PresenterDistrict);
		}

		public void TestPresenterDistrict_Null()
		{
			tempStorageHeader.SJH_OA_Presenter = ZGuid.Empty;
			AssertNull(TempStorageHeaderWrapped.PresenterDistrict);
		}

		public void TestRepresentativeEoriNumber()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertCustomsCodeCodeValue(() => TempStorageHeaderWrapped.RepresentativeEoriNumber, tempStorageHeader.SJH_OA_RepresentativeInfo, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.Greece, "SUMVALUE", "GRSUMVALUE");
		}

		public void TestRepresentativeEoriBranch()
		{
			AssertCustomsCodeCodeValue(() => TempStorageHeaderWrapped.RepresentativeEoriBranch, tempStorageHeader.SJH_OA_RepresentativeInfo, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany, "2222", "2222");
		}

		public void TestContactName()
		{
			AssertEquals(Env.CurrentUser.FullName, TempStorageHeaderWrapped.ContactName);
		}

		public void TestContactPosition()
		{
			AssertEquals(Env.CurrentUser.Title, TempStorageHeaderWrapped.ContactPosition);
		}

		public void TestContactPhoneNumber()
		{
			AssertEquals(Env.CurrentUser.WorkPhone, TempStorageHeaderWrapped.ContactPhoneNumber);
		}

		public void TestContactEmailAddress()
		{
			AssertEquals(Env.CurrentUser.EmailAddress, TempStorageHeaderWrapped.ContactEmailAddress);
		}

		public void TestFirstEntryCustomsOfficeReferenceNumber()
		{
			tempStorageHeader.SJH_CustomsOfficeOfEntryIntoEU = "DE019437";
			AssertEquals("DE019437", TempStorageHeaderWrapped.FirstEntryCustomsOfficeReferenceNumber);
		}

		public void TestTransportMeansCode()
		{
			tempStorageHeader.SJH_TransportMeansCode = TemporaryStorageTransportMeansList.Codes.Truck;
			AssertEquals(TemporaryStorageTransportMeansList.Codes.Truck, TempStorageHeaderWrapped.TransportMeansCode);
		}

		public void TestBorderTransportDescription()
		{
			tempStorageHeader.SJH_TransportMeansDescription = "INFORMATION";
			AssertEquals("INFORMATION", TempStorageHeaderWrapped.BorderTransportDescription);
		}

		public void TestTransportRegistrationNumber()
		{
			tempStorageHeader.SJH_TransportRegNo = "QF98";
			AssertEquals("QF98", TempStorageHeaderWrapped.TransportRegistrationNumber);
		}

		public void TestPreviousReferenceType()
		{
			tempStorageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._T1IF;
			AssertEquals(PreviousReferenceType.Codes._T1IF, TempStorageHeaderWrapped.PreviousReferenceType);
		}

		public void TestPreviousReferenceNumber()
		{
			tempStorageHeader.SJH_PreviousReferenceNumber = "PREVIOUS REF NUMBER";
			AssertEquals("PREVIOUS REF NUMBER", TempStorageHeaderWrapped.PreviousReferenceNumber);
		}

		public void TestContainerQuantity()
		{
			tempStorageHeader.SJH_ContainerCount = 89;
			AssertEquals(89, TempStorageHeaderWrapped.ContainerQuantity);
		}

		protected ITempStorageHeader TempStorageHeaderWrapped => tempStorageHeaderWrapped ?? (tempStorageHeaderWrapped = GetTempStorageHeaderWrapped(tempStorageHeader));
		ITempStorageHeader tempStorageHeaderWrapped;

		protected abstract ITempStorageHeader GetTempStorageHeaderWrapped(CusTempStorageJobHeader tempStorageHeader);

		protected override T GetProvider() => (T)TempStorageHeaderWrapped;

		protected override void SetUp()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "C1";
			tempStorageHeader = Factory.New<CusTempStorageJobHeader>();
			tempStorageHeader.SJH_OH_Customer = customer.PK;
		}
		protected CusTempStorageJobHeader tempStorageHeader;

		void AssertCustomsCodeCodeValue(Func<ZString> wrappedProperty, ZPropertyInfo propertyInfo, ZString customsCodeType, ZString countryCode, ZString customsRegNo, ZString expectedValue)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TSTCODE";

			Assert(wrappedProperty.Invoke().IsEmpty);
			propertyInfo.Value = org.MainAddress.PK;
			Assert(wrappedProperty.Invoke().IsEmpty);

			var customscode = org.MainAddress.CustomsCodes.AddNew();
			customscode.OK_CodeType = customsCodeType;
			customscode.OK_RN_NKCodeCountry = countryCode;
			customscode.OK_CustomsRegNo = customsRegNo;
			AssertEquals(expectedValue, wrappedProperty.Invoke());
		}

		void AddGreekEoriNumber(OrgHeader orgHeader, ZString eoriNumber)
		{
			var cusCodeEori = orgHeader.CustomsCodes.AddNew();
			cusCodeEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCodeEori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
			cusCodeEori.OK_CustomsRegNo = eoriNumber;
		}

		void AddCusCodeWithPremisesAddress(OrgHeader orgHeader, ZString codeType, ZString cusRegNumber)
		{
			var cusCodeEbs = orgHeader.CustomsCodes.AddNew();
			cusCodeEbs.OK_CodeType = codeType;
			cusCodeEbs.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCodeEbs.OK_CustomsRegNo = cusRegNumber;
			cusCodeEbs.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
		}
	}
}
