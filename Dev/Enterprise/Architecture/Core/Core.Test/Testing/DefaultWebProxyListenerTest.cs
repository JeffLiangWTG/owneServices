using System.Net;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DefaultWebProxyListenerTest : TestCase
	{
		public void TestThrowErrorIfDefaultWebProxyIsChangedAfterTestIsRun()
		{
			DefaultWebProxyListener listener = DefaultWebProxyListener.Instance;
			var original = WebRequest.DefaultWebProxy;

			listener.StartAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
			try
			{
				listener.BeforeEachTest(EnvProxy.Instance.Time.CurrentLocalDateTime);
				WebRequest.DefaultWebProxy = null;
				listener.AfterEachTest(EnvProxy.Instance.Time.CurrentLocalDateTime);
			}
			catch (AssertionFailedError ex)
			{
				Assert("Error message should be rised.", ex.Message.Contains("WebRequest.DefaultWebProxy"));
			}
			finally
			{
				listener.EndAllTests(EnvProxy.Instance.Time.CurrentLocalDateTime);
				WebRequest.DefaultWebProxy = original;
			}
		}
	}
}
