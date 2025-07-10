using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaManifestHeaderValidation))]
sealed class CGMAsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAMA_Nature()
	{
		CombineAssertions(() =>
		{
			Header.AMA_Nature = ZString.Empty;
			AssertHasMessageErrorContaining("Message error when Manifest Nature is empty", Header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_Nature = INManifestNatures.Codes.IMP;
			AssertNoMessageErrorContaining("No error when Manifest Nature is set", Header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckImportGeneralManifestNumber()
	{
		const string expectedMessage = "You have entered an invalid IGM Number. The expected value is a 7-digit number.";

		var invalidManifestNumbers = new[] { ".", ",", ":", "-", "!", "A", "/", "<", "{" };
		CombineAssertions(() =>
		{
			foreach (var invalidManifestNumber in invalidManifestNumbers)
			{
				Header.ImportGeneralManifestNumber = invalidManifestNumber;
				AssertHasMessageError($"When {invalidManifestNumber} entered", Header.ImportGeneralManifestNumberInfo, expectedMessage);
			}

			Header.ImportGeneralManifestNumber = "1234567";
			AssertNoMessageError("When all numbers entered", Header.ImportGeneralManifestNumberInfo, expectedMessage);
		});
	}

	public void TestCheckRegistrationStatus()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(Header.RegistrationStatusInfo, "XYZ", RegistrationStatusList.Codes.ManifestRegistered);
	}

	public void TestCheckAMA_RL_NKPortOfFirstArrival()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertInvalidCodeMessageError(Header.AMA_RL_NKPortOfFirstArrivalInfo, "XXXXX", "AUSYD");

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.AMA_RL_NKPortOfFirstArrival = "XXXXX";
		AssertNoNotifications(Header.AMA_RL_NKPortOfFirstArrivalInfo);
	}

	public void TestCheckAMA_VesselName()
	{
		ValidationTestHelper.AssertFieldIsNotMandatory(Header.AMA_VesselNameInfo);
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Header.AMA_VesselNameInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.AMA_VesselNameInfo);
	}

	public void TestCheckAMA_VoyageMandatory()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertNoWarningIfNotEntered(Header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage("Voyage"));

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertWarningIfNotEntered(Header.AMA_VoyageInfo, MandatoryValidation.YouHaveNotEnteredMessage("Flight"));
	}

	public void TestCheckAMA_CustomsOfficeMandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.AMA_CustomsOfficeInfo);
	}

	public void TestCheckAMA_MasterBill()
	{
		var expectedMessageError = "You have entered invalid MBL/BOL.";
		var invalidInputs = new[] { "a b", "a.b", "a#b", "a@b" };

		foreach (var invalidInput in invalidInputs)
		{
			CombineAssertions($"for input - {invalidInput}", () =>
			{
				Header.AMA_TransportMode = TransportTypeList.Codes.Air;
				Header.AMA_MasterBill = invalidInput;

				Header.Validation.ValidateAMA_MasterBill();
				AssertNoMessageError("No Message Error for AirCGM", Header.AMA_MasterBillInfo, expectedMessageError);

				Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				Header.Validation.ValidateAMA_MasterBill();
				AssertHasMessageError("Message Error for SeaCGM, invalid input", Header.AMA_MasterBillInfo, expectedMessageError);
			});
		}
		Header.AMA_MasterBill = "121ADSF3";
		AssertNoMessageError("No Message Error for SeaCGM, valid input", Header.AMA_MasterBillInfo, expectedMessageError);
	}

	public void TestCheckAMA_LloydsNumber()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(Header.AMA_LloydsNumberInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Header.AMA_LloydsNumberInfo);
		});
	}

	public void TestCheckAMA_MessageStatus()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(Header.AMA_MessageStatusInfo, "XYZ", "SNT");
	}

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
