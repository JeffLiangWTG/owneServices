using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Bi.Product.Module.Controller
{
	class PowerBiAnalyticsReportsModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AnalyticsReports; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BusinessIntelligence; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZPopupController)ZControllerFactory.Create(ControllerIDs.PowerBiAnalyticsReports);
		}

		public override void Show()
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var baseUri = new Uri(baseURL);
			var portal = "BIA/" + FormFactor.Desktop; // url path
			var powerBiReportsPage = "/analytics/logistics";
			var url = UrlBuilder.GenerateURL(baseUri, portal, powerBiReportsPage);
			WebUrlLauncher.Launch(url.ToString());
		}
	}
}
