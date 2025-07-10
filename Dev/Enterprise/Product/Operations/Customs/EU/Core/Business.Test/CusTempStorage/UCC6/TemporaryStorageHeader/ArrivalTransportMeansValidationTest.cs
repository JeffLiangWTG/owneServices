using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class ArrivalTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckArrivalTransportMeans()
		{
			var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			tempHeader.TransportType = "10";
			tempHeader.ArrivalTransportMeansCode = "KKK12345";
			AssertHasMessageError("Arrival Transport Means Code doesn't have enough numeric digits.", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 10, the Arrival Transport Means should be started with IMO and followed with 6 numeric digits and a check digit.");

			tempHeader.ArrivalTransportMeansCode = "KKK1234567";
			AssertHasMessageError("Arrival Transport Means Code doesn't strat with IMO", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 10, the Arrival Transport Means should be started with IMO and followed with 6 numeric digits and a check digit.");

			tempHeader.ArrivalTransportMeansCode = "IMO9619900";
			AssertNoMessageError("Arrival Transport Means Code has correct overall format.", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 10, the Arrival Transport Means should be started with IMO and followed with 6 numeric digits and a check digit.");
			AssertHasWarning("Arrival Transport Means Code check digit is wrong.", tempHeader.ArrivalTransportMeansCodeInfo, "Arrival Transport Means does not have a valid check digit. The check digit should be 7.");

			tempHeader.ArrivalTransportMeansCode = "IMO9619907";
			AssertNoMessageError("Arrival Transport Means Code has correct overall format.", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 10, the Arrival Transport Means should be started with IMO and followed with 6 numeric digits and a check digit.");
			AssertNoWarning("Arrival Transport Means Code has correct check digit.", tempHeader.ArrivalTransportMeansCodeInfo, "Arrival Transport Means does not have a valid check digit. The check digit should be 7.");

			tempHeader.TransportType = "80";
			tempHeader.ArrivalTransportMeansCode = "A1234567";
			AssertHasMessageError("Message Error on Arrival Transport Means Code", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 80, the Arrival Transport Means should be 8 digit number.");

			tempHeader.ArrivalTransportMeansCode = "123456789";
			AssertHasMessageError("Message Error on Arrival Transport Means Code", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 80, the Arrival Transport Means should be 8 digit number.");

			tempHeader.ArrivalTransportMeansCode = "12345678";
			AssertNoMessageError("Message Error on Arrival Transport Means Code", tempHeader.ArrivalTransportMeansCodeInfo, "When transport type is 80, the Arrival Transport Means should be 8 digit number.");

			tempHeader.ArrivalTransportMeansCode = "";
			AssertHasMessageError(tempHeader.ArrivalTransportMeansCodeInfo, "You have not entered an Arrival Transport Means.");
		}

		public void TestCheckTPM_IdentificationNumber_IsTransfer()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();

			CombineAssertions(() =>
			{
				tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				AssertHasMessageErrorContaining("TPM_IdentificationNumber is empty", tempHeader.ArrivalTransportMeans.TPM_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);

				tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				AssertNoMessageErrorContaining("TPM_IdentificationNumber is empty, but messageType is TF - Transfer", tempHeader.ArrivalTransportMeans.TPM_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTPM_IdentificationNumber_IsDeconsolidation()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();

			CombineAssertions(() =>
			{
				tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				AssertHasMessageErrorContaining("TPM_IdentificationNumber is empty", tempHeader.ArrivalTransportMeans.TPM_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);

				tempHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				tempHeader.ArrivalTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				AssertNoMessageErrorContaining("TPM_IdentificationNumber is empty, but messageType is DC - Deconsolidation", tempHeader.ArrivalTransportMeans.TPM_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}
}
