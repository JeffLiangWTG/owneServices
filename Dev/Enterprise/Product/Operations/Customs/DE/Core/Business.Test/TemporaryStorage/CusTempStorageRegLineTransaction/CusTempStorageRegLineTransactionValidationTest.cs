using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageRegLineTransactionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSRT_GrossWeight_TransactionTypeOBL()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction = line.CusTempStorageRegLineTransactions.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("TransactionType", TransactionTypes.Codes.OpeningBalance, transaction.SRT_TransactionType);

				transaction.Validation.ValidateSRT_GrossWeight();
				AssertHasErrorContaining("1st transaction, SRT_GrossWeight = 0", transaction.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);

				transaction.SRT_PackageQty = 5;
				transaction.Validation.ValidateSRT_GrossWeight();
				AssertHasErrorContaining("1st transaction, SRT_GrossWeight = 0 but SRT_PackageQty > 0", transaction.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);

				transaction.SRT_GrossWeight = 3.68m;
				AssertNoErrorContaining("1st transaction, SRT_GrossWeight > 0", transaction.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckSRT_GrossWeight_TransactionTypeADJ()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			line.CusTempStorageRegLineTransactions.AddNew();
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("TransactionType", TransactionTypes.Codes.Adjustment, transaction2.SRT_TransactionType);

				transaction2.SRT_PackageQty = 1;
				transaction2.Validation.ValidateSRT_GrossWeight();
				AssertNoErrorContaining("SRT_GrossWeight = 0 but SRT_PackageQty > 0", transaction2.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);

				transaction2.SRT_PackageQty = 0;
				transaction2.Validation.ValidateSRT_GrossWeight();
				AssertHasErrorContaining("SRT_GrossWeight = 0 and SRT_PackageQty = 0", transaction2.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);

				transaction2.SRT_GrossWeight = 1.36m;
				AssertNoErrorContaining("SRT_GrossWeight > 0", transaction2.SRT_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckSRT_PackageQty()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			transaction1.Validation.ValidateSRT_PackageQty();

			AssertHasErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_GrossWeight = 5.36m;
			AssertHasErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_PackageQty = 2;
			AssertNoErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_TransactionType = TransactionTypes.Codes.Adjustment;
			transaction1.SRT_PackageQty = 0;
			transaction1.SRT_GrossWeight = 0;

			AssertHasErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_PackageQty = 2;
			AssertNoErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_GrossWeight = 2.25;
			transaction1.SRT_PackageQty = 0;
			AssertNoErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeZero);

			transaction1.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			transaction1.SRT_PackageQty = 10;

			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_TransactionType = TransactionTypes.Codes.Adjustment;
			transaction2.SRT_PackageQty = -20;

			var notNegativeError = "The sum of Package Qty. across all transaction lines should not be negative.";
			AssertHasErrorContaining(transaction2.SRT_PackageQtyInfo, notNegativeError);

			transaction2.SRT_PackageQty = -5;
			AssertNoErrorContaining(transaction2.SRT_PackageQtyInfo, notNegativeError);

			transaction1.SRT_PackageQty = -1;
			AssertHasErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeNegative);

			transaction1.SRT_PackageQty = 10;
			AssertNoErrorContaining(transaction1.SRT_PackageQtyInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckSRT_ReferenceType()
		{
			var transaction = Factory.New<CusTempStorageRegLineTransaction>();
			ValidationTestHelper.AssertInvalidCodeMessageError(transaction.SRT_ReferenceTypeInfo, "~", TransactionReferenceTypes.Codes.AUFT);
		}

		public void TestCheckSRT_Reference()
		{
			var transaction = Factory.New<CusTempStorageRegLineTransaction>();

			transaction.Validation.ValidateSRT_Reference();
			AssertNoMessageErrorContaining(transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			transaction.SRT_ReferenceType = "AUFT";
			transaction.Validation.ValidateSRT_Reference();
			AssertHasMessageErrorContaining(transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			transaction.SRT_Reference = "RE0005";
			AssertNoMessageErrorContaining(transaction.SRT_ReferenceInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}

