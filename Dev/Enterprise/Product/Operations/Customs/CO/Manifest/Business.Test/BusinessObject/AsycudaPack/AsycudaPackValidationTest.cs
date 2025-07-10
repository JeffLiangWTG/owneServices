using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsHazardous()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";

			var pack = bill1.Packs.AddNew();
			pack.Validation.ValidateIsHazardous();
			AssertNoMessageErrorContaining("IsHazardousInfo", pack.IsHazardousInfo, "You have not specified that the item is hazardous.");

			var undg = pack.UNDGs.AddNew();
			undg.DI_IMOClass = "1";
			pack.Validation.ValidateIsHazardous();
			AssertHasMessageErrorContaining("IsHazardousInfo", pack.IsHazardousInfo, "You have not specified that the item is hazardous.");

			undg.DI_IMOClass = ZString.Empty;
			undg.DI_DG_NKSubs = "1";
			pack.Validation.ValidateIsHazardous();
			AssertHasMessageErrorContaining("IsHazardousInfo", pack.IsHazardousInfo, "You have not specified that the item is hazardous.");

			undg.DI_IMOClass = "1";
			pack.IsHazardous = true;
			pack.Validation.ValidateIsHazardous();
			AssertNoMessageErrorContaining("IsHazardousInfo", pack.IsHazardousInfo, "You have not specified that the item is hazardous.");
		}
	}
}
