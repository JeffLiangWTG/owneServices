using System.Web;
using CargoWise.Data;

namespace Enterprise.Web.TestWebApplication.Common.Handlers
{
	public class DbConnectionHandler : IHttpHandler
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContext context)
		{
			Db.Connection.EnsureIsOpen();
			ResponseWriter.WriteResponse(context, $"ServerName={Db.Connection.ServerName};DatabaseName={Db.Connection.CurrentDatabase}");
		}
	}
}
