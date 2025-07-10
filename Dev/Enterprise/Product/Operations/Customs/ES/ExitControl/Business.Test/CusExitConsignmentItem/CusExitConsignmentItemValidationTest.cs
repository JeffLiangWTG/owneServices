using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class CusExitConsignmentItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCCI_UniqueConsignmentReferenceStatus_Length()
		{
			CombineAssertions(() =>
			{
				var expectedError = "The length must be 0 or 3";
				var exitHeader = Factory.New<CusExitHeader>();
				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				var item = exitConsignment.CusExitConsignmentItems.AddNew();

				item.CCI_UniqueConsignmentReferenceStatus = "AH";
				AssertHasErrorContaining(item.CCI_UniqueConsignmentReferenceStatusInfo, expectedError);

				item.CCI_UniqueConsignmentReferenceStatus = "AH3";
				AssertNoErrorContaining(item.CCI_UniqueConsignmentReferenceStatusInfo, expectedError);
			});
		}

		public void TestCheckCCI_UniqueConsignmentReferenceStatus_ListValidation()
		{
			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitHeader>();
				var exitConsignment = exitHeader.CusExitConsignments.AddNew();
				var item = exitConsignment.CusExitConsignmentItems.AddNew();

				item.CCI_UniqueConsignmentReferenceStatus = "AH3";
				AssertListValidationInvalidCodeMessageError(item.CCI_UniqueConsignmentReferenceStatusInfo, true);

				item.CCI_UniqueConsignmentReferenceStatus = "DIF";
				AssertListValidationInvalidCodeMessageError(item.CCI_UniqueConsignmentReferenceStatusInfo, false);
			});
		}

		public void TestCheckCCI_DiscrepancyStatus()
		{
			var header = Factory.New<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			CombineAssertions(() =>
			{
				var item = consignment.CusExitConsignmentItems.AddNew();
				item.CCI_DiscrepancyStatus = ZString.Empty;
				AssertListValidationInvalidCodeError(item.CCI_DiscrepancyStatusInfo, false);

				item.CCI_DiscrepancyStatus = "asd";
				AssertListValidationInvalidCodeError(item.CCI_DiscrepancyStatusInfo, true);

				item.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertListValidationInvalidCodeError(item.CCI_DiscrepancyStatusInfo, false);
			});
		}
	}
}
