using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class ISTCusTempStorageLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckTSL_LocationOfGoods_WhenListIsEmpty()
		{
			AssertEquals(0, line.Lookups.GoodsLocations.Count);
			line.TSL_LocationOfGoods = ZString.Empty;
			AssertNoMessageErrors(line.TSL_LocationOfGoodsInfo);
			line.TSL_LocationOfGoods = "^^^^";
			AssertNoMessageErrors(line.TSL_LocationOfGoodsInfo);
		}

		public void TestCheckTSL_LocationOfGoods_WhenListIsNotEmpty()
		{
			var presenter = Factory.New<OrgHeader>();
			var addressA = presenter.Addresses.AddNew();
			var addressB = presenter.Addresses.AddNew();
			var authLocation1 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Paris");
			var authLocation2 = addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("Toulouse");
			header.SJH_OA_Presenter = addressA.PK;
			AssertEquals(2, line.Lookups.GoodsLocations.Count);

			line.TSL_LocationOfGoods = ZString.Empty;
			AssertNoMessageErrors(line.TSL_LocationOfGoodsInfo);
			line.TSL_LocationOfGoods = "Paris";
			AssertNoMessageErrorContaining(line.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			line.TSL_LocationOfGoods = "Lyon";
			AssertHasMessageErrorContaining(line.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			header.CreateRelatedCusTempStorageDec();
			line = (ISTCusTempStorageLine)header.CusTempStorageDec.CusTempStorageLines.AddNew();
		}

		CusTempStorageJobHeader header;
		ISTCusTempStorageLine line;
	}
}
