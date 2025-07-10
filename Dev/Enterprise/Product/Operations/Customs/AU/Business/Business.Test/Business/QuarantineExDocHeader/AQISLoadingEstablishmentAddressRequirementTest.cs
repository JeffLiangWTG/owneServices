using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISLoadingEstablishmentAddressRequirement))]
	sealed class AQISLoadingEstablishmentAddressRequirementTest : TestCaseWithFactory
	{
		public void TestDefaultValues()
		{
			AssertEquals("DefaultDocAddressType", DocAddressType.AQISLoadingEstablishment, Requirement.DefaultDocAddressType);
		}

		public void TestNoValidationProperties()
		{
			var jobDocAddress = Factory.NewWithValidTestData<AUJobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.ClearAllNotifications();

			Requirement.ValidateCity(jobDocAddress.Validation);
			Requirement.ValidateAddress1(jobDocAddress.Validation);
			Requirement.ValidateCompanyName(jobDocAddress.Validation);

			AssertNoNotifications(jobDocAddress);
		}

		public void TestRegistrationNumber()
		{
			var jobDocAddress = Factory.NewWithValidTestData<AUJobDocAddress>();

			var aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = "TAQS";
			aqisEstablishment.OH_FullName = "AQIS SYDNEY";
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";

			jobDocAddress.E2_OA_Address = mainAddress.PK;

			var registrationNumberResult = Requirement.GetRegistrationNumberResult(jobDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals("RegistrationNumber", "77", registrationNumberResult.RegistrationNumber);
				AssertEquals("RegistrationNumberType", OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, registrationNumberResult.RegistrationNumberType);
			});
		}

		AQISLoadingEstablishmentAddressRequirement requirement;
		AQISLoadingEstablishmentAddressRequirement Requirement => requirement ??= new AQISLoadingEstablishmentAddressRequirement();
	}
}
