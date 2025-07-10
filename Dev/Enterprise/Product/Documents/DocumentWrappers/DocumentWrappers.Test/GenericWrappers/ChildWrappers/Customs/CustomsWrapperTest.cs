using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Moq;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Customs.Tests
{
	sealed class CustomsWrapperTest : TestCaseWithFactory
	{
		public void TestCA()
		{
			var declaration = Factory.New<Integration.Customs.CA.IJobDeclaration>();
			var customsInfoMock = new Mock<ICustomsInfo>();

			customsInfoMock.Setup(m => m.Declaration).Returns(declaration as BaseJobDeclaration);

			var wrapper = CustomsWrapper.New(customsInfoMock.Object, Factory);
			AssertNotNull(wrapper.CA);
			AssertEquals(declaration, wrapper.CA.DeclarationExposed);

			customsInfoMock.VerifyAll();
		}
	}
}
