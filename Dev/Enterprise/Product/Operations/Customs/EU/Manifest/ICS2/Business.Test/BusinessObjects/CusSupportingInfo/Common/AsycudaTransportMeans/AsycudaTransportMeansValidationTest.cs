using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaTransportMeansValidation))]
	sealed class AsycudaTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPM_IdentificationNumber()
		{
			var transportMeans = Factory.New<AsycudaTransportMeans>();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_IdentificationNumberInfo);
		}

		public void TestCheckTPM_TypeOfIdentification()
		{
			var transportMeans = Factory.New<AsycudaTransportMeans>();
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_TypeOfIdentificationInfo);

				transportMeans.TPM_TypeOfIdentification = "AB";
				AssertHasMessageError(transportMeans.TPM_TypeOfIdentificationInfo, "The code you have selected is not in the list.");

				transportMeans.TPM_TypeOfIdentification = EUICS2ModeOfTransportIdentifierTypeList.Codes.CL750_10;
				AssertNoMessageError(transportMeans.TPM_TypeOfIdentificationInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestCheckTPM_TypeOfTransportMeans()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);

			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MT;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "150", "General cargo vessel Vessel designed to carry general cargo", startDate, endDate);

			Factory.Save();

			var transportMeans = Factory.New<AsycudaTransportMeans>();
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_TypeOfTransportMeansInfo);

				transportMeans.TPM_TypeOfTransportMeans = "1234";
				AssertHasMessageError(transportMeans.TPM_TypeOfTransportMeansInfo, "The code you have selected is not in the list.");

				transportMeans.TPM_TypeOfTransportMeans = "150";
				AssertNoMessageError(transportMeans.TPM_TypeOfTransportMeansInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestCheckTPM_RN_NKTransportNationality()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var dayAfterTomorrow = today.AddDays(2);
			var dayBeforeYesterday = today.AddDays(-2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("NC008", "NC008");

			helper.CreateCusCodeList("EUN", "NC008", "DE", "Italy", yesterday, tomorrow);
			Factory.Save();

			var countryList = Factory.NewWithValidTestData<AsycudaTransportMeans>();
			var list = countryList.Lookups.CountryList;
			list.Load();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var transportMeans = bill.AsycudaTransportMeans.AddNew();
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_RN_NKTransportNationalityInfo);

				transportMeans.TPM_RN_NKTransportNationality = "XY";
				AssertHasMessageError(transportMeans.TPM_RN_NKTransportNationalityInfo, "The code you have selected is not in the list.");

				transportMeans.TPM_RN_NKTransportNationality = "DE";
				AssertNoMessageError(transportMeans.TPM_RN_NKTransportNationalityInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestCheckTPM_RN_NKTransportNationalityFromPackNotMandatoryForF51()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var transportMeans = pack.AsycudaTransportMeans.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_RN_NKTransportNationalityInfo);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;

			ValidationTestHelper.AssertFieldIsNotMandatory(transportMeans.TPM_RN_NKTransportNationalityInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationalityFromBillNotMandatoryForF51()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			_ = bill.Packs.AddNew();
			var transportMeans = bill.AsycudaTransportMeans.AddNew();

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportMeans.TPM_RN_NKTransportNationalityInfo);

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;

			ValidationTestHelper.AssertFieldIsNotMandatory(transportMeans.TPM_RN_NKTransportNationalityInfo);
		}
	}
}
