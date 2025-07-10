using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Tests
{
	sealed class CACustomsWrapperTest : TestCaseWithFactory
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = CACustomsWrapper.New(declaration, Factory);

			AssertEquals(declaration, wrapper.Declaration);
			AssertNotNull(wrapper.DocDeclaration);
		}
	}
}
