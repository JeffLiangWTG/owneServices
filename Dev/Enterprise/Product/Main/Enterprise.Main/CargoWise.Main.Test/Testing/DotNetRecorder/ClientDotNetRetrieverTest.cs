using Enterprise.Upgrades;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class ClientDotNetRetrieverTest : TestCase
	{
		public void TestGetDotNetVersionStringAbove45()
		{
			var mock = new Mock<ClientDotNetRetriever>();
			mock.Protected().Setup<DotNetVersion>("GetVersionFromRegistryMoreThan45").Returns(new DotNetVersion("4.6.2", 394802));
			AssertEquals("DotNetVersion:4.6.2|ReleaseNumber:394802", mock.Object?.GetDotNetVersionString());
			mock.VerifyAll();
		}
		public void TestGetDotNetVersionStringBelow45()
		{
			var mock = new Mock<ClientDotNetRetriever>();
			mock.Protected().Setup<DotNetVersion>("GetVersionFromRegistryMoreThan45").Returns((DotNetVersion)null);
			mock.Protected().Setup<DotNetVersion>("GetVersionFromRegistryLessThan45").Returns(new DotNetVersion("3.5.2", 0));
			AssertEquals("DotNetVersion:3.5.2|ReleaseNumber:0", mock.Object?.GetDotNetVersionString());
			mock.VerifyAll();
		}
		public void TestGetDotNetVersionFromRegister()
		{
			var mock = new ClientDotNetRetriever();
			AssertNotNullOrEmpty("The method was not able to get a valid .Net version", mock.GetDotNetVersionString());
			AssertNotContains("The method was not able to get a valid .Net version", "DotNetVersion:0.0", mock.GetDotNetVersionString());
		}
	}
}
