using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_MessageType()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = "";

			AssertHasMessageErrorContaining("Message Type is required", jobDeclaration.JE_MessageTypeInfo, "You have not entered");

			jobDeclaration.JE_MessageType = "IMP";
			AssertNoMessageErrorContaining("Message Type is required", jobDeclaration.JE_MessageTypeInfo, "You have not entered");
		}

		public void TestCheckJE_MessageSubType()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageSubType = "";

			AssertHasMessageErrorContaining("Message Sub Type is required", jobDeclaration.JE_MessageSubTypeInfo, "You have not entered");

			jobDeclaration.JE_MessageSubType = "IM";
			AssertNoMessageErrorContaining("Message Sub Type is required", jobDeclaration.JE_MessageSubTypeInfo, "You have not entered");
		}

		public void TestCheckJE_TransportMode()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_TransportMode = "";

			AssertHasMessageErrorContaining("Transport Mode is required", jobDeclaration.JE_TransportModeInfo, "You have not entered");

			jobDeclaration.JE_TransportMode = "AIR";
			AssertNoMessageErrorContaining("Transport Mode is required", jobDeclaration.JE_TransportModeInfo, "You have not entered");
		}

		public void TestCheckJE_TransportMeans()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_TransportMeans = "";

			AssertHasMessageErrorContaining("Transport Means is required", jobDeclaration.JE_TransportMeansInfo, "You have not entered");

			jobDeclaration.JE_TransportMeans = "1";
			AssertNoMessageErrorContaining("Transport Means is required", jobDeclaration.JE_TransportMeansInfo, "You have not entered");
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;

			AssertHasMessageErrorContaining("Declarant Address is required", jobDeclaration.JE_OA_DeclarantAddressInfo, "You have not entered");

			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining("Declarant Address is required", jobDeclaration.JE_OA_DeclarantAddressInfo, "You have not entered");
		}

		public void TestCheckJE_ManifestNumber()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Sea, ZString.Empty, false, false, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Sea, "A123", true, false, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Sea, "1234567", false, true, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Sea, "A123456", true, true, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Sea, "123456", false, false, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Road, ZString.Empty, false, false, false);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Road, "A123456789123456", false, false, true);
			AssertManifestNumberValidation(jobDeclaration, Core.Constants.TransportModes.Road, "A12345678912345", false, false, false);
		}

		public void TestCheckJE_LocationOfGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC00001", "Customs Enclosure Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOfGoodsInfo, "XXX99999", "FAC00001");
		}

		static void AssertManifestNumberValidation(JobDeclaration jobDeclaration, ZString transportMode, ZString manifestNumber, bool isRestrictedToDigitsOnly, bool isMaxOf6, bool isMaxOf15)
		{
			jobDeclaration.JE_TransportMode = transportMode;
			jobDeclaration.JE_ManifestNumber = manifestNumber;

			if (isRestrictedToDigitsOnly)
			{
				AssertHasMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should contain only digits");
			}
			else
			{
				AssertNoMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should contain only digits");
			}

			if (isMaxOf6)
			{
				AssertHasMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should not exceed 6 digits");
			}
			else
			{
				AssertNoMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should not exceed 6 digits");
			}

			if (isMaxOf15)
			{
				AssertHasMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should not exceed 15 characters");
			}
			else
			{
				AssertNoMessageErrorContaining("Manifest Number not restricted", jobDeclaration.JE_ManifestNumberInfo, "Manifest Number should not exceed 15 characters");
			}
		}
	}
}
