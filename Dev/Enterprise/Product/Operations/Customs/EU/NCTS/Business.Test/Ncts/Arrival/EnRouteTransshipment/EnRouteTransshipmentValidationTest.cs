using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class EnRouteTransshipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBN_EventCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Event Country/Region", transshipment.BN_EventCountryCodeInfo.HumanReadableName);

				transshipment.BN_EventCountryCode = Core.Constants.CountryCodes.Algeria;
				AssertHasMessageError("Invalid Code", transshipment.BN_EventCountryCodeInfo, ListValidation.InvalidCodeMessageError);

				transshipment.BN_EventCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError("Valid Code", transshipment.BN_EventCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBN_TransportCountryCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "New Transport Nationality", transshipment.BN_TransportCountryCodeInfo.HumanReadableName);
				transshipment.BN_TransportCountryCode = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory", transshipment.BN_TransportCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				transshipment.BN_TransportCountryCode = "C1";
				AssertNoMessageErrorContaining("Entered", transshipment.BN_TransportCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError("Invalid Code", transshipment.BN_TransportCountryCodeInfo, ListValidation.InvalidCodeMessageError);

				transshipment.BN_TransportCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError("Valid Code", transshipment.BN_TransportCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBN_EndorsementDate()
		{
			AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Transhipment Date", transshipment.BN_EndorsementDateInfo.HumanReadableName);
			transshipment.BN_EndorsementDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(transshipment.BN_EndorsementDateInfo, MandatoryValidation.YouHaveNotEntered);

			transshipment.BN_EndorsementDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(transshipment.BN_EndorsementDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBN_EndorsementPlace()
		{
			AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Place Reported", transshipment.BN_EndorsementPlaceInfo.HumanReadableName);
			transshipment.BN_EndorsementPlace = ZString.Empty;
			AssertHasMessageErrorContaining(transshipment.BN_EndorsementPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			transshipment.BN_EndorsementPlace = "ABC";
			AssertNoMessageErrorContaining(transshipment.BN_EndorsementPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBN_EndorsementCountryCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption is set in the business layer so that validation and UI can use it without setting it twice.", "Country/Region Reported", transshipment.BN_EndorsementCountryCodeInfo.HumanReadableName);
				transshipment.BN_EndorsementCountryCode = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory", transshipment.BN_EndorsementCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

				transshipment.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Aruba;
				AssertNoMessageErrorContaining("Value entered", transshipment.BN_EndorsementCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError("Invalid code", transshipment.BN_EndorsementCountryCodeInfo, ListValidation.InvalidCodeMessageError);

				transshipment.BN_EndorsementCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError("Valid Code", transshipment.BN_EndorsementCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBN_TransportAtDepartureID_MaxLength()
		{
			transshipment.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(transshipment.BN_TransportAtDepartureIDInfo, 27);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0009", "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "C0009", "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			transshipment = header.EnRouteTransshipments.AddNew();
		}
		EnRouteTransshipment transshipment;
	}
}
