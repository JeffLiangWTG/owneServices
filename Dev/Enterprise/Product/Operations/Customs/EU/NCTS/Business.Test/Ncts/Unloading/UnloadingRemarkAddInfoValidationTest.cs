using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class UnloadingRemarkAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG9_NoOfSeals_Negative()
		{
			CombineAssertions(() =>
			{
				unloadingRemarkAddInfo.G9_NoOfSeals = -1;
				AssertHasMessageErrorContaining("Negative", unloadingRemarkAddInfo.G9_NoOfSealsInfo, MandatoryValidation.ValueCannotBeNegative);
				unloadingRemarkAddInfo.G9_NoOfSeals = 0;
				AssertNoMessageErrorContaining("Not Negative", unloadingRemarkAddInfo.G9_NoOfSealsInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckG9_NoOfSeals_MaximumNumberOfSeals()
		{
			const string maximumNumberOfSeals = "The maximum number of seal numbers entered in the grid should be 2.";

			CombineAssertions(() =>
			{
				unloadingRemarkAddInfo.G9_NoOfSeals = 2;
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertNoMessageError("No Seal", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);

				header.Seals.AddNew();
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertNoMessageError("One Seal", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);

				header.Seals.AddNew();
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertNoMessageError("Two Seals", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);

				header.Seals.AddNew();
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertHasMessageError("Three Seals", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);
			});
		}

		public void TestCheckG9_NoOfSeals_MaximumNumberOfSealsIsEmpty()
		{
			const string maximumNumberOfSeals = "The maximum number of seal numbers entered in the grid should be 0.";

			CombineAssertions(() =>
			{
				unloadingRemarkAddInfo.G9_NoOfSeals = 0;
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertNoMessageError("No Seal", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);

				header.Seals.AddNew();
				unloadingRemarkAddInfo.Validation.ValidateG9_NoOfSeals();
				AssertHasMessageError("Seal exists", unloadingRemarkAddInfo.G9_NoOfSealsInfo, maximumNumberOfSeals);
			});
		}

		public void TestCheckG9_StateOfSealsOk()
		{
			unloadingRemarkAddInfo.G9_StateOfSealsOk = ZString.Empty;
			unloadingRemarkAddInfo.Validation.ValidateG9_StateOfSealsOk();
			AssertNoMessageError("empty is ok", unloadingRemarkAddInfo.G9_StateOfSealsOkInfo, ListValidation.InvalidCodeMessageError);

			unloadingRemarkAddInfo.G9_StateOfSealsOk = YesNoEmpty.Codes.NotApplicable;
			unloadingRemarkAddInfo.Validation.ValidateG9_StateOfSealsOk();
			AssertNoMessageError("empty is ok", unloadingRemarkAddInfo.G9_StateOfSealsOkInfo, ListValidation.InvalidCodeMessageError);

			unloadingRemarkAddInfo.G9_StateOfSealsOk = YesNoEmpty.Codes.Yes;
			unloadingRemarkAddInfo.Validation.ValidateG9_StateOfSealsOk();
			AssertNoMessageError("Y is ok", unloadingRemarkAddInfo.G9_StateOfSealsOkInfo, ListValidation.InvalidCodeMessageError);

			unloadingRemarkAddInfo.G9_StateOfSealsOk = YesNoEmpty.Codes.No;
			unloadingRemarkAddInfo.Validation.ValidateG9_StateOfSealsOk();
			AssertNoMessageError("N is ok", unloadingRemarkAddInfo.G9_StateOfSealsOkInfo, ListValidation.InvalidCodeMessageError);

			unloadingRemarkAddInfo.G9_StateOfSealsOk = "O";
			unloadingRemarkAddInfo.Validation.ValidateG9_StateOfSealsOk();
			AssertHasMessageError("O is not ok", unloadingRemarkAddInfo.G9_StateOfSealsOkInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			unloadingRemarkAddInfo = header.UnloadingRemark;
		}

		NctsHeader header;
		UnloadingRemarkAddInfo unloadingRemarkAddInfo;
	}
}
