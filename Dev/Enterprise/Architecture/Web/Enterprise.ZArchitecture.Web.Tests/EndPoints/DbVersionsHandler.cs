using System.IO;
using System.Net;
using System.Text;
using System.Web;
using CargoWise.Data;
using Enterprise.Environment;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Web.EndPoints
{
	public sealed class DbVersionsHandler : IHttpHandler
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContextBase context)
		{
			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.CacheControl = "no-cache";
			context.Response.ContentType = "text/plain;charset=utf-8";

			var output = context.Response.OutputStream;
			using (var writer = new StreamWriter(output, Encoding.UTF8))
			{
				writer.Write(Invariant($"{Db.ServerName}, {Db.DatabaseName}, SchemaVersion: {Env.Registry.DatabaseMajorSchemaVersion}.{Env.Registry.DatabaseMinorSchemaVersion}"));
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			ProcessRequest(new HttpContextWrapper(context));
		}
	}
}
