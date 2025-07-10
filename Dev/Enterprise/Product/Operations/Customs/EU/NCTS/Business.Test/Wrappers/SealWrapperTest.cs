using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class SealWrapperTest : Customs.Business.Testing.DataProviderTestCase<SealWrapper>
	{
		public void TestSealIdentity()
		{
			AssertEquals("SEAL123", wrapper.SealIdentity);
		}

		public void TestSealIdentityLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.SealIdentityLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new SealWrapper("SEAL123");
		}
		SealWrapper wrapper;

		protected override SealWrapper GetProvider() => wrapper;
	}
}
