using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class NACCSUtilsTest : TestCase
	{
		public void TestBuildxTConfiguration()
		{
			var mockSetting = Mock.Of<IJPNACCSClientApplicationSetting>(_ =>
				_.xTApplicationNode == "JPNACCS"
				&& _.xTServerAddress == "127.0.0.1"
				&& _.xTPassword == "pwd"
				&& _.xTServerCertificate == "CA");
			var xTConfiguration = NACCSUtils.BuildxTConfiguration(mockSetting);
			CombineAssertions(() =>
			{
				AssertEquals("JPNACCS", xTConfiguration.Application.URI);
				AssertEquals("pwd", xTConfiguration.Application.Password);
				AssertEquals("127.0.0.1", xTConfiguration.Connect);
				AssertEquals("CA", xTConfiguration.CA);
			});
		}
	}
}
