using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsArrivalMovementHeaderPhase4ValidationTest : TestCaseWithFactory
	{
		public void TestCheckBM_PlaceOfUnloading()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			arrivalMovement.BM_PlaceOfUnloading = "";
			AssertNoMessageErrorContaining(arrivalMovement.BM_PlaceOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalMovement.BM_PlaceOfUnloading = "";
			AssertHasMessageErrorContaining(arrivalMovement.BM_PlaceOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);

			arrivalMovement.BM_PlaceOfUnloading = "ABC";
			AssertNoMessageErrorContaining(arrivalMovement.BM_PlaceOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival;
			arrivalMovement.BM_PlaceOfUnloading = "";
			AssertHasMessageErrorContaining(arrivalMovement.BM_PlaceOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);

			arrivalMovement.BM_PlaceOfUnloading = "ABC";
			AssertNoMessageErrorContaining(arrivalMovement.BM_PlaceOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
