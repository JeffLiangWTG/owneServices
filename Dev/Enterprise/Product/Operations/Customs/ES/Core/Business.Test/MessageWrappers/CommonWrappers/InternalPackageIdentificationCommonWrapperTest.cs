using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class InternalPackageIdentificationCommonWrapperTest : WrapperHelperTest<InternalPackageIdentificationCommonWrapper>
	{
		public void TestTag()
		{
			AssertEquals(InternalPackage1.Marks, wrapper.Tag);
		}

		public void TestElementsType()
		{
			AssertEquals(InternalPackage1.Type, wrapper.ElementsType);
		}
		public void TestNumberOfElements()
		{
			AssertEquals(InternalPackage1.NumberOfElements, wrapper.NumberOfElements);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new InternalPackageIdentificationCommonWrapper(InternalPackage1.Marks, InternalPackage1.Type, InternalPackage1.NumberOfElements);
		}
		InternalPackageIdentificationCommonWrapper wrapper;

		protected override InternalPackageIdentificationCommonWrapper GetProvider() => wrapper;
	}
}
