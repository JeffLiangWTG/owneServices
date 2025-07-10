using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	class WebPrintForceToUseHTTPSForWebPrintRequestsRegistryItem : BooleanDbRegistryItem
	{
		public override string ItemName
		{
			get { return "WebPrintForceToUseHTTPSForWebPrintRequests"; }
		}

		protected override bool DefaultValue => EnvProxy.IsHostedWithCargowise;
	}
}
