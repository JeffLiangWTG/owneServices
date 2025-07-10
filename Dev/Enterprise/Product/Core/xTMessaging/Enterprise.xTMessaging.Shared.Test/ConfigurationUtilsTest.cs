using NUnit.Framework;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Shared.Test
{
	class ConfigurationUtilsTest : TestCase
	{
		public void TestIsValid_Empty()
		{
			var emptyConfiguration = new Configuration();
			AssertEquals(false, emptyConfiguration.IsValid());
		}

		public void TestIsValid_ConnectEmpty()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			testConfiguration.Connect = string.Empty;
			AssertEquals(false, testConfiguration.IsValid());
		}

		public void TestIsValid_CAEmpty()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			testConfiguration.CA = string.Empty;
			AssertEquals(false, testConfiguration.IsValid());
		}

		public void TestIsValid_ApplicationNull()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			testConfiguration.Application = null;
			AssertEquals(false, testConfiguration.IsValid());
		}

		public void TestIsValid_ApplicationURIEmpty()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			testConfiguration.Application.URI = string.Empty;
			AssertEquals(false, testConfiguration.IsValid());
		}

		public void TestIsValid_ApplicationPasswordEmpty()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			testConfiguration.Application.Password = string.Empty;
			AssertEquals(false, testConfiguration.IsValid());
		}

		public void TestIsValid_WithLoadedData()
		{
			var testConfiguration = TestUtils.GetTestConfiguration();
			AssertEquals("127.0.0.1:61001", testConfiguration.Connect);
			AssertEquals(true, testConfiguration.IsValid());
		}
	}
}
