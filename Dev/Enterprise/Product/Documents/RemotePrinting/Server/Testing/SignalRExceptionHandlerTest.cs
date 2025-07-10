using System;
using System.Web;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class SignalRExceptionHandlerTest : TestCase
	{
		public void TestHandleException()
		{
			var handler = new SignalRExceptionHandler();

			Assert("Should not handle random exception", !handler.HandleException(new ApplicationException("abc")));
			Assert("Should not handle random http exception", !handler.HandleException(new HttpException("abc")));
			Assert("Should handle http exception from SignalR", handler.HandleException(Microsoft.AspNet.SignalR.TaskAsyncHelper.Testing.SignalRExceptionHandlerTestHelper.GetHttpException()));
		}
	}
}

namespace Microsoft.AspNet.SignalR.TaskAsyncHelper.Testing
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in test - false positive")]
	static class SignalRExceptionHandlerTestHelper
	{
		public static HttpException GetHttpException()
		{
			try
			{
				throw new HttpException("abc");
			}
			catch (HttpException ex)
			{
				return ex;
			}
		}
	}
}
