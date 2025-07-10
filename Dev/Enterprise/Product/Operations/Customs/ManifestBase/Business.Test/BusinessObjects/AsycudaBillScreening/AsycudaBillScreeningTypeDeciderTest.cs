using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaBillScreeningTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header.AMA_JobReference = "456";
			header.AMA_ApplicationCode = "NVC";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;

			var headerBill = header.Bills.AddNew();
			var headerBillScreening = Factory.New<AsycudaBillScreening>();
			headerBillScreening.ASR_ABL = headerBill.PK;

			Factory.Save();

			Assert(new BusinessObjectFactory().Load<AsycudaBillScreening>(headerBillScreening.PK) is Integration.Customs.ManifestBase.IAsycudaBillScreening);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaBillScreening), new AsycudaBillScreeningTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaBillScreening), new AsycudaBillScreeningTypeDecider().GetTypeForBinding());
		}
	}
}
