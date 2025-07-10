using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStoragePackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_PackUQ()
		{
			SetUpPackageTypes();
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(package.APA_PackUQInfo, invalidCode: "KG", validCode: "PP", ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckAPA_MarksAndNumbersMustBeEnteredForNonBulkBreakBulkPackages()
		{
			SetUpPackageTypes();
			Factory.Save();

			CombineAssertions(() =>
			{
				package.APA_PackUQ = "PP";
				package.Validation.ValidateAPA_MarksAndNumbers();
				AssertHasMessageErrorContaining("When APA_PackUQ is not Bulk nor BreakBulk and APA_MarksAndNumbers is not entered", package.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				package.APA_MarksAndNumbers = "1";
				AssertNoMessageErrorContaining("When APA_PackUQ is not Bulk nor BreakBulk and APA_MarksAndNumbers is entered", package.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				package.APA_PackUQ = "VG";
				package.APA_MarksAndNumbers = "";
				AssertNoMessageErrorContaining("When APA_PackUQ is not Bulk and APA_MarksAndNumbers is not entered", package.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				package.APA_PackUQ = "NE";
				package.Validation.ValidateAPA_MarksAndNumbers();
				AssertNoMessageErrorContaining("When APA_PackUQ is BreakBulk and APA_MarksAndNumbers is not entered", package.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

				package.Bill.Header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				package.APA_PackUQ = "PP";
				package.Validation.ValidateAPA_MarksAndNumbers();
				AssertNoMessageErrorContaining("When APA_PackUQ is not Bulk nor BreakBulk and APA_MarksAndNumbers is not entered and AMA_MessageType is transfer", package.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckAPA_PackQty_MustBeZeroForBulkOrBreakBulkPackages()
		{
			const string expectedErrorMessage = "Quantity (on Pack) must be zero for the selected Pack Unit.";

			SetUpPackageTypes();
			Factory.Save();

			CombineAssertions(() =>
			{
				package.APA_PackQty = 1;

				package.APA_PackUQ = "VG";
				package.Validation.ValidateAPA_PackQty();
				AssertHasMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is Bulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackUQ = "NE";
				package.Validation.ValidateAPA_PackQty();
				AssertHasMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackQty = 0;

				package.APA_PackUQ = "VG";
				package.Validation.ValidateAPA_PackQty();
				AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is Bulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackUQ = "NE";
				package.Validation.ValidateAPA_PackQty();
				AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackUQ = "PP";
				package.Validation.ValidateAPA_PackQty();
				AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is not Bulk nor BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);
			});
		}

		public void TestCheckAPA_PackQty_MustBeEnteredForNonBulkBreakBulkPackages()
		{
			const string expectedErrorMessage = "Quantity (on Pack) cannot be zero for the selected Pack Unit.";

			SetUpPackageTypes();
			Factory.Save();

			CombineAssertions(() =>
			{
				package.APA_PackQty = 0;

				package.APA_PackUQ = "VG";
				package.Validation.ValidateAPA_PackQty();
				AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is Bulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackUQ = "NE";
				package.Validation.ValidateAPA_PackQty();
				AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackUQ = "PP";
				package.Validation.ValidateAPA_PackQty();
				AssertHasMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is not Bulk nor BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);

				package.APA_PackQty = 1;
				AssertNoMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is not Bulk nor BreakBulk", package.APA_PackQtyInfo, expectedErrorMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			package = bill.Packs.AddNew();
		}

		TemporaryStoragePack package;

		void SetUpPackageTypes()
		{
			const string dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations;
			const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "UN Package code List");
			helper.CreateCusCodeList(dataGrouping, codeType, "PP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		}
	}
}
