// the below module is no longer needed when removing the security right, remove the class
using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APInvoiceProcessingPortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.APInvoiceProcessingPortal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override Uri Url => null;

		public override void Show()
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			if (string.IsNullOrEmpty(baseURL))
			{
				var result = ResString.GetMultilingualString
					(
						"637ffa3a-a228-4b17-a46a-5d507ca80fef",
						"The invoice processing portal cannot be opened in a browser as GLOW has not been configured for this client."
					);
				Globals.Message.ShowError(result);
				return;
			}

			var url = UrlBuilder.GenerateURL(new Uri(baseURL), "goto/payables");
			WebUrlLauncher.Launch(url.ToString());
		}
	}
}
