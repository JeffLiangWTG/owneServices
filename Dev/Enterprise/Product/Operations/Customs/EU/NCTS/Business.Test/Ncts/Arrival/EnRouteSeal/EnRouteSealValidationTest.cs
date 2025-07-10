using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class EnRouteSealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG9_EventCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0009", "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "C0009", "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				enRouteSeal.BN_EventCountryCode = Core.Constants.CountryCodes.Algeria;
				AssertHasMessageError("Invalid Code", enRouteSeal.BN_EventCountryCodeInfo, ListValidation.InvalidCodeMessageError);

				enRouteSeal.BN_EventCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError("Valid Code", enRouteSeal.BN_EventCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBN_TransportAtDepartureID_MaxLength()
		{
			enRouteSeal.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(enRouteSeal.BN_TransportAtDepartureIDInfo, 27);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			enRouteSeal = header.EnRouteSeals.AddNew();
		}

		public void TestCheckBN_NoOfSeals()
		{
			enRouteSeal.BN_NoOfSeals = 2;

			var messageError = "The maximum number of seals entered in the grid should be 2.";

			CombineAssertions(() =>
			{
				var container = enRouteSeal.SealContainers.AddNew();
				container.BC_Seal1 = "SEALNUM1";
				enRouteSeal.Validation.ValidateBN_NoOfSeals();
				AssertNoMessageErrorContaining("When number of seals is lower than the max number there should be no error message",
					enRouteSeal.BN_NoOfSealsInfo, messageError);

				var container2 = enRouteSeal.SealContainers.AddNew();
				container2.BC_Seal1 = "SEALNUM2";
				enRouteSeal.Validation.ValidateBN_NoOfSeals();
				AssertNoMessageErrorContaining("When number of seals is equal to the max number there should be no error message",
					enRouteSeal.BN_NoOfSealsInfo, messageError);

				var container3 = enRouteSeal.SealContainers.AddNew();
				container3.BC_Seal1 = "SEALNUM3";
				enRouteSeal.Validation.ValidateBN_NoOfSeals();
				AssertHasMessageError("When number of seals is greater than the max number there should be an error message",
					enRouteSeal.BN_NoOfSealsInfo, messageError);
			});
		}

		EnRouteSeal enRouteSeal;
	}
}
