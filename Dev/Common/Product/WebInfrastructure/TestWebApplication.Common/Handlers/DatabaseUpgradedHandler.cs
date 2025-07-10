using System.Web;
using CargoWise.Data;

namespace Enterprise.Web.TestWebApplication.Common.Handlers
{
	public class DatabaseUpgradedHandler : IHttpHandler
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContext context)
		{
			throw new DatabaseUpgradedException(false);
		}
	}
}
