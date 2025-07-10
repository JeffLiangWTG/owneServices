using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class StatusRequestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMovementReferenceNumber_Mandatory()
		{
			AssertYouHaveNotEnteredMessageError(statusRequest.MovementReferenceNumberInfo);
		}

		public void TestCheckMovementReferenceNumber_Format()
		{
			const string validationError = "Please enter a MRN in the following format with only numbers and upper case letters:";
			CombineAssertions(() =>
			{
				statusRequest.MovementReferenceNumber = "INVALID";
				AssertHasWarningContaining("Invalid MRN", statusRequest.MovementReferenceNumberInfo, validationError);
				statusRequest.MovementReferenceNumber = "11DE11111111111115";
				AssertNoWarningContaining("Valid MRN", statusRequest.MovementReferenceNumberInfo, validationError);
			});
		}

		public void TestCheckIdentification_Mandatory()
		{
			AssertYouHaveNotEnteredMessageError(statusRequest.IdentificationInfo);
		}

		public void TestCheckIdentification_List()
		{
			var org = Factory.New<OrgHeader>();
			statusRequest.Identification = org.PK;
			const string invalidMessage = "Enter a valid Identification.";
			CombineAssertions(() =>
			{
				statusRequest.Identification = ZGuid.Invalid;
				AssertHasError("Invalid", statusRequest.IdentificationInfo, invalidMessage);
				statusRequest.Identification = org.MainAddress.PK;
				AssertNoError("Valid", statusRequest.IdentificationInfo, invalidMessage);
			});
		}

		public void TestCheckIdentification_Eori()
		{
			const string aRegistrationNumberCodeOfTypeEorIsRequired = "A Registration Number / Code of type EOR is required.";
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				var org = Factory.NewWithValidTestData<OrgHeader>();
				statusRequest.Identification = org.PK;
				AssertHasMessageError("Without EOR", statusRequest.IdentificationInfo, aRegistrationNumberCodeOfTypeEorIsRequired);
				org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Greece);
				statusRequest.Validation.ValidateIdentification();
				AssertNoMessageError("With EOR", statusRequest.IdentificationInfo, aRegistrationNumberCodeOfTypeEorIsRequired);
			});
		}

		public void TestCheckRole()
		{
			statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
			AssertInvalidCodeOrEmptyMessageError(statusRequest.RoleInfo, "!", ExportStatusRequestNCTSRoleList.Codes.Principal);
		}

		public void TestValidateAll()
		{
			statusRequest.Module = ExportStatusRequestModuleCodeList.Codes.NCTS;
			statusRequest.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasMessageErrors("MovementReferenceNumber", statusRequest.MovementReferenceNumberInfo);
				AssertHasMessageErrors("Identification", statusRequest.IdentificationInfo);
				AssertHasMessageErrors("Role", statusRequest.RoleInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
		}
		StatusRequest statusRequest;
	}
}
