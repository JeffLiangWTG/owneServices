using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Enterprise.Web.TestWebApplication.Common.Handlers
{
	static class ResponseWriter
	{
		public static void WriteResponse(HttpContext context, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			context.Response.StatusCode = (int)statusCode;
			context.Response.CacheControl = "no-cache";
			context.Response.ContentType = "text/plain;charset=utf-8";

			var output = context.Response.OutputStream;
			using var writer = new StreamWriter(output, Encoding.UTF8);
			writer.Write(message);
		}
	}
}
