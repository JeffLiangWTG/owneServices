using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	internal class AsycudaContainerBillOrPackageLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIfBillAndPackHasValue()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var pack = Factory.NewWithValidTestData<AsycudaPack>();
			var pivot = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			var container = Factory.NewWithValidTestData<AsycudaContainer>();
			pivot.APC_APA_Pack = pack.PK;
			pivot.APC_ABL_Bill = bill.PK;
			pivot.APC_ACN_Container = container.PK;
			AssertHasError(pivot.APC_ACN_ContainerInfo, "You can't provide a bill and a pack for same container.");
		}
	}
}
