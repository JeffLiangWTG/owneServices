using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using static Enterprise.ZArchitecture.Web.Utilities.Environment.WebDbEnvironment;

namespace Enterprise.RemotePrinting.Server
{
	public class WebPrintDbEnvironment : WebDbEnvironment
	{
		public override IConnectionPooling ConnectionPooling => new WebPrintConnectionPooling();
	}

	class WebPrintConnectionPooling : WebConnectionPooling
	{
		public override int MinPoolSize => 1;
	}
}
