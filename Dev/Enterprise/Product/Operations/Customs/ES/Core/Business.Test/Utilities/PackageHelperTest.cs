using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class PackageHelperTest : TestCaseWithFactory
	{
		public void TestPackTypeIsBulk()
		{
			Factory.SetBulkTypeHelper("BK");

			CombineAssertions(() =>
			{
				AssertEquals("Is not bulk type", false, PackageHelper.PackTypeIsBulk("AH", Factory));

				AssertEquals("Is bulk type", true, PackageHelper.PackTypeIsBulk("BK", Factory));
			});
		}
	}
}
