using System.Web;
using CargoWise.Data;

namespace Enterprise.Web.TestWebApplication.Common.Handlers
{
	public class DatabaseUpgradeIsInProgressHandler : IHttpHandler
	{
		public bool IsReusable => true;

		public void ProcessRequest(HttpContext context)
		{
			throw new DatabaseUpgradeInProgressException();
		}
	}
}
