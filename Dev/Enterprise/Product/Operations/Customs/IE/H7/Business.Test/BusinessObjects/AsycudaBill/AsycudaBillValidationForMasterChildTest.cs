using Enterprise.Customs.ASYCUDA.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AsycudaBillValidationForMasterChildTest : AsycudaBillValidationAbstractTest
	{
		public void TestABL_ShipmentType()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipmentType = "ZZZ";
			AssertListValidationInvalidCodeError(bill.ABL_ShipmentTypeInfo, isExpectingError: true);
		}
	}
}
