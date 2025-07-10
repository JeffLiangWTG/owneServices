using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.MessageWrappers;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class ExternalPackagesInfoCommonWrapperTest : TestCaseWithFactory
	{
		public abstract void TestPackageType();
	}

	sealed class ExternalPackagesInfoCommonWrapperTest_DoNotInherit : ExternalPackagesInfoCommonWrapperTest
	{
		public void TestNumberOfPackages()
		{
			AssertEquals(10, wrapper.NumberOfPackages);
		}

		public override void TestPackageType()
		{
			AssertEquals("External PackageType will always be CN", BusinessQuantityUnit.Container, wrapper.PackageType);
		}

		public void TestTags()
		{
			AssertContainsExactElementsInAnyOrder(WrapperHelperTest<ExternalPackagesInfoCommonWrapper>.ContainerTags, wrapper.Tags);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new ExternalPackagesInfoCommonWrapper(WrapperHelperTest<ExternalPackagesInfoCommonWrapper>.ContainerTags);
		}
		ExternalPackagesInfoCommonWrapper wrapper;
	}
}
