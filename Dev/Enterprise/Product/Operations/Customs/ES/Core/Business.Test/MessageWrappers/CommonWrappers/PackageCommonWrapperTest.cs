using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class PackageCommonWrapperTest : WrapperHelperTest<PackageCommonWrapper>
	{
		public void TestPackageType()
		{
			AssertEquals("Expected filled PackageType", InternalPackage1.Type, wrapper.PackageType);
		}

		public void TestMarks()
		{
			AssertEquals("Expected filled Marks", InternalPackage1.Marks, wrapper.Marks);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new PackageCommonWrapper(InternalPackage1.Type, InternalPackage1.Marks);
		}
		PackageCommonWrapper wrapper;

		protected override PackageCommonWrapper GetProvider() => wrapper;
	}
}
