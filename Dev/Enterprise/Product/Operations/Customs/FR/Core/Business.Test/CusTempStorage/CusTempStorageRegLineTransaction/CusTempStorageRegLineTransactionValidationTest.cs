using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageRegLineTransactionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSRT_GrossWeight()
		{
			var transaction = line.CusTempStorageRegLineTransactions.AddNew();

			transaction.SRT_GrossWeight = 0;
			AssertHasWarning("There should be a warning message as SRT_GrossWeight = 0.", transaction.SRT_GrossWeightInfo, "Gross Weight in KGs should be greater than zero.");

			transaction.SRT_GrossWeight = 5;
			AssertNoWarning("There should be no warning message as SRT_GrossWeight > 0.", transaction.SRT_GrossWeightInfo, "Gross Weight in KGs should be greater than zero.");
		}

		public void TestCheckSRT_PackageQty_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transaction.SRT_PackageQtyInfo);
		}

		public void TestCheckSRT_PackageQty_CanBeNegative()
		{
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = 1;
			transaction.SRT_PackageQty = -1;
			AssertNoNotifications("SRT_PackageQty can be negative", transaction.SRT_PackageQtyInfo);
		}

		public void TestCheckSRT_PackageQty_SumCannotBeNegative()
		{
			const string message = "The sum of Package Qty. across all transaction lines should not be negative.";
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			CombineAssertions(() =>
			{
				transaction.SRT_PackageQty = -1;
				AssertHasError("Total SRT_PackageQty < 0", transaction.SRT_PackageQtyInfo, message);

				transaction2.SRT_PackageQty = 1;
				transaction.Validation.ValidateSRT_PackageQty();
				AssertNoError("Total SRT_PackageQty = 0", transaction.SRT_PackageQtyInfo, message);

				transaction2.SRT_PackageQty = 2;
				transaction.Validation.ValidateSRT_PackageQty();
				AssertNoError("Total SRT_PackageQty > 0", transaction.SRT_PackageQtyInfo, message);
			});
		}

		public void TestCheckSRT_ReferenceType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(transaction.SRT_ReferenceTypeInfo, "XYZ", CusTempStorageRegLineTransactionReferenceTypeList.Codes.CustomsDeclaration);
		}

		public void TestCheckSRT_Reference()
		{
			CombineAssertions(() =>
			{
				transaction.Validation.ValidateSRT_Reference();
				AssertNoMessageErrorContaining("SRT_ReferenceType is empty", transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

				transaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.CustomsDeclaration;
				transaction.Validation.ValidateSRT_Reference();
				AssertHasMessageErrorContaining("SRT_ReferenceType not empty", transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

				transaction.SRT_Reference = "REF123";
				AssertNoMessageErrorContaining("SRT_Reference set", transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			line = Factory.New<CusTempStorageRegLine>();
			transaction = line.CusTempStorageRegLineTransactions.AddNew();
		}
		CusTempStorageRegLine line;
		CusTempStorageRegLineTransaction transaction;
	}
}
