using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryItemExtentionsTest : TestCase
	{
		public void TestGetFactoryCachedValue()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var innerMock = new Mock<IRegistryItemInternals>(MockBehavior.Strict);
			innerMock.Setup(m => m.Name).Returns("SNTH");

			var mock = new Mock<StronglyTypedRegistryItem<string, string>>(MockBehavior.Strict, innerMock.Object);

			mock.Protected().Setup<object>("ValueCore").Returns("Blaticus");
			string value1 = mock.Object.GetFactoryCachedValue(factory1);
			mock.Verify();
			AssertEquals("should return the value from the registry item", "Blaticus", value1);

			string value2 = mock.Object.GetFactoryCachedValue(factory1);
			AssertEquals("same factory, should return the value from the cache", "Blaticus", value2);

			mock.Protected().Setup<object>("ValueCore").Returns("Awsomeness");
			string value3 = mock.Object.GetFactoryCachedValue(factory2);
			mock.VerifyAll();
			innerMock.VerifyAll();
			AssertEquals("different factory, should hit the registry item again", "Awsomeness", value3);
		}
	}
}
