using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitNotificationPackageValidationTest : TestCaseWithFactory
	{
		public void TestPackQTY()
		{
			const string message = "must not be less than 0 or greater than 99.999.999";
			var package = Factory.New<CusExitItem>().Packages.AddNew();
			var sut = new ExitNotificationPackage(package);
			CombineAssertions(() =>
			{
				sut.PackQTY = -1;
				AssertHasErrorContaining(sut.PackQTYInfo, message);

				sut.PackQTY = 100_000_000u;
				AssertHasErrorContaining(sut.PackQTYInfo, message);

				sut.PackQTY = 10_000_000u;
				AssertNoErrorContaining(sut.PackQTYInfo, message);
			});
		}
	}
}
