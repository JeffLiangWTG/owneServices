using System;
using System.IO;
using System.Web;

namespace Enterprise.ZArchitecture.Web.Common.Test
{
	static class WebTestingUtilities
	{
		public static void DoWithMockHttpContext(
			Action action,
			Action<string> responseMessageAction = null,
			string requestFileName = "dummy",
			string requestUrl = "https://wtg.com",
			string queryString = "a=b")
		{
			using var stream = new MemoryStream();
			using var writer = new StreamWriter(stream);

			HttpContext.Current = new HttpContext(
				new HttpRequest(requestFileName, requestUrl, queryString),
				new HttpResponse(writer)
			);

			try
			{
				action();

				if (responseMessageAction != null)
				{
					stream.Position = 0;
					using var reader = new StreamReader(stream);
					var responseMessage = reader.ReadToEnd();
					responseMessageAction.Invoke(responseMessage);
				}
			}
			finally
			{
				HttpContext.Current = null;
			}
		}
	}
}
