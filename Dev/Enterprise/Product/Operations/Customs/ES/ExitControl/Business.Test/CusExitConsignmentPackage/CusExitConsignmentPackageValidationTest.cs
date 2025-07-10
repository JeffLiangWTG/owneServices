using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentPackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXP_MarksAndNumbersStatus_Length()
		{
			CombineAssertions(() =>
			{
				var expectedError = "The length must be 0 or 3";
				var exitHeader = Factory.New<CusExitHeader>();
				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				var itemPackagePivot = exitConsignment.CusExitConsignmentItems.AddNew().CusExitConsignmentPackagePivots.AddNew();
				var package = itemPackagePivot.Package;

				package.CXP_MarksAndNumbersStatus = "AH";
				AssertHasErrorContaining(package.CXP_MarksAndNumbersStatusInfo, expectedError);

				package.CXP_MarksAndNumbersStatus = "AH3";
				AssertNoErrorContaining(package.CXP_MarksAndNumbersStatusInfo, expectedError);
			});
		}

		public void TestCheckCXP_MarksAndNumbersStatus_ListValidation()
		{
			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitHeader>();
				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				var itemPackagePivot = exitConsignment.CusExitConsignmentItems.AddNew().CusExitConsignmentPackagePivots.AddNew();
				var package = itemPackagePivot.Package;

				package.CXP_MarksAndNumbersStatus = "AH3";
				AssertListValidationInvalidCodeMessageError(package.CXP_MarksAndNumbersStatusInfo, true);

				package.CXP_MarksAndNumbersStatus = "DIF";
				AssertListValidationInvalidCodeMessageError(package.CXP_MarksAndNumbersStatusInfo, false);
			});
		}

		public void TestCheckCXP_Sequence_NoDuplicateValues_PackagesBelongingToDifferentConsignmentItems()
		{
			var exitHeader = Factory.New<CusExitHeader>();

			var exitConsignment = exitHeader.CusExitConsignments.AddNew();

			var consignmentItem1 = exitConsignment.CusExitConsignmentItems.AddNew();
			var itemPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();

			var consignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			var itemPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();

			itemPackagePivot1.Package.CXP_Sequence = 1;
			itemPackagePivot2.Package.CXP_Sequence = 1;
			itemPackagePivot1.Package.Validation.ValidateCXP_Sequence();
			itemPackagePivot2.Package.Validation.ValidateCXP_Sequence();

			CombineAssertions("When 2 packages have the same Sequence Number but belong to different consignment items", () =>
			{
				AssertNoErrors(itemPackagePivot1.Package.CXP_SequenceInfo);
				AssertNoErrors(itemPackagePivot2.Package.CXP_SequenceInfo);
			});
		}

		public void TestCheckCXP_Sequence_NoDuplicateValues_PackagesBelongingToDifferentConsignmentHeaders()
		{
			var exitHeader = Factory.New<CusExitHeader>();

			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem1 = exitConsignment1.CusExitConsignmentItems.AddNew();
			var itemPackagePivot1 = consignmentItem1.CusExitConsignmentPackagePivots.AddNew();

			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem2 = exitConsignment2.CusExitConsignmentItems.AddNew();
			var itemPackagePivot2 = consignmentItem2.CusExitConsignmentPackagePivots.AddNew();

			itemPackagePivot1.Package.CXP_Sequence = 1;
			itemPackagePivot2.Package.CXP_Sequence = 1;
			itemPackagePivot1.Package.Validation.ValidateCXP_Sequence();
			itemPackagePivot2.Package.Validation.ValidateCXP_Sequence();

			CombineAssertions("When 2 packages have the same Sequence Number but belong to different consignment headers", () =>
			{
				AssertNoErrors(itemPackagePivot1.Package.CXP_SequenceInfo);
				AssertNoErrors(itemPackagePivot2.Package.CXP_SequenceInfo);
			});
		}
	}
}
