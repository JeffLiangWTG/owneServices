using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.RemotePrinting.Server
{
	public class WebPrintEnvironmentProvider : WebEnvironmentProvider
	{
		protected override IDbEnvironment GetDbEnvironmentInstance()
		{
			return new WebPrintDbEnvironment();
		}
	}
}
