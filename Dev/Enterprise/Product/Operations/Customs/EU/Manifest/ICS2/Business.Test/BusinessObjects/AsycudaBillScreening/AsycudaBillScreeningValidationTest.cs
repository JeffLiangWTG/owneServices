using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaBillScreeningValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckResult()
		{
			billScreening.ASR_Result = string.Empty;
			Assert(billScreening.ASR_ResultInfo.HasMessageError("You have not entered a Result."));

			billScreening.ASR_Result = "XXX";
			Assert(billScreening.ASR_ResultInfo.HasMessageError("The code you have selected is not in the list."));
		}

		public void TestCheckAuthorizedPersonFieldsRequirement()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			billScreening.ASR_PER_AuthorizedPerson = person.PK;
			Assert(billScreening.ASR_AuthorizedPersonTypeInfo.HasMessageError("You have not entered an Authorized Person Type."));

			billScreening.ASR_AuthorizedPersonType = "X";
			Assert(billScreening.ASR_AuthorizedPersonTypeInfo.HasMessageError("The code you have selected is not in the list."));

			billScreening.ASR_AuthorizedPersonType = "1";
			billScreening.ASR_PER_AuthorizedPerson = ZGuid.Empty;
			CombineAssertions("Authorized Person, Name and Identifier should not be empty", () =>
			{
				Assert(billScreening.ASR_PER_AuthorizedPersonInfo.HasMessageError("You have not entered an Authorized Person."));
				Assert(billScreening.ASR_AuthorizedPersonNameInfo.HasMessageError("You have not entered a Name."));
				Assert(billScreening.ASR_AuthorizedPersonIdentifierInfo.HasMessageError("You have not entered an Identifier."));
			});

			billScreening.ASR_AuthorizedPersonName = "Tony Hawks";
			Assert(!billScreening.ASR_AuthorizedPersonNameInfo.HasMessageErrors());

			billScreening.ASR_AuthorizedPersonIdentifier = "001122";
			Assert(!billScreening.ASR_AuthorizedPersonIdentifierInfo.HasMessageErrors());

			billScreening.ASR_AuthorizedPersonType = "3";
			CombineAssertions("Only Name and Identifier should not be empty when person type is 3", () =>
			{
				Assert(!billScreening.ASR_PER_AuthorizedPersonInfo.HasMessageErrors());
				Assert(billScreening.ASR_AuthorizedPersonNameInfo.HasMessageError("You have not entered a Name."));
				Assert(billScreening.ASR_AuthorizedPersonIdentifierInfo.HasMessageError("You have not entered an Identifier."));
			});

			billScreening.ASR_AuthorizedPersonType = string.Empty;
			CombineAssertions("No errors when all are empty", () =>
			{
				Assert(!billScreening.ASR_PER_AuthorizedPersonInfo.HasMessageErrors());
				Assert(!billScreening.ASR_AuthorizedPersonNameInfo.HasMessageErrors());
				Assert(!billScreening.ASR_AuthorizedPersonIdentifierInfo.HasMessageErrors());
			});
		}

		public void TestCheckASR_TransportNumberType_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(billScreening.ASR_TransportNumberTypeInfo, "????", ASYCUDA.Business.TransportDocumentTypes.Codes.CL754_C624);
		}

		public void TestCheckASR_TransportNumberType()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(billScreening.ASR_TransportNumberInfo, billScreening.ASR_TransportNumberTypeInfo);
		}

		public void TestCheckASR_TransportNumber()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(billScreening.ASR_TransportNumberTypeInfo, billScreening.ASR_TransportNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			billScreening = Factory.New<AsycudaBillScreening>();
		}
		AsycudaBillScreening billScreening;
	}
}
