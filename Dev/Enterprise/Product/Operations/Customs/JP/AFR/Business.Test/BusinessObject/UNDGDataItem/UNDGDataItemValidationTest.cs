using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(UNDGDataItemValidation))]
	sealed class UNDGDataItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDI_DG()
		{
			var undg1 = DGSubstanceTestHelper.Create("1234", "a", "IMO");
			var undg2 = DGSubstanceTestHelper.Create("5678", "b", "IMO");
			var undg3 = DGSubstanceTestHelper.Create("9012", "c", "IMO");

			var expectedError = ValidationConstants.Bill.DuplicatedUNDG(undg1);

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_DG = undg1.PK;

			var item1 = bill.UNDGs.AddNew();
			AssertNoMessageError(item1.DI_DGInfo, expectedError);

			item1.DI_DG = undg1.PK;
			AssertHasMessageError(item1.DI_DGInfo, expectedError);

			item1.DI_DG = undg2.PK;
			AssertNoMessageError(item1.DI_DGInfo, expectedError);

			var item2 = bill.UNDGs.AddNew();
			item2.DI_DG = undg2.PK;
			item1.Validation.ValidateDI_DG();

			expectedError = ValidationConstants.Bill.DuplicatedUNDG(undg2);

			AssertHasMessageError(item1.DI_DGInfo, expectedError);
			AssertHasMessageError(item2.DI_DGInfo, expectedError);

			item2.DI_DG = undg3.PK;
			item1.Validation.ValidateDI_DG();

			AssertNoMessageError(item1.DI_DGInfo, expectedError);
			AssertNoMessageError(item2.DI_DGInfo, expectedError);
		}
	}
}
