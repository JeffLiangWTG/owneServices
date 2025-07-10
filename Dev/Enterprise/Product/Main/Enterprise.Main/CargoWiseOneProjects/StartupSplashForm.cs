using CargoWise.BrandManager;

namespace Enterprise.Startup
{
	public class StartupSplashForm : CargoWise.Loader.Common.CWNextProgressForm
	{
		public static new StartupSplashForm New()
		{
			return new StartupSplashForm();
		}

		public StartupSplashForm()
		{
			this.Text = BrandingFactory.Instance.ProductBrandingName;
		}
	}
}
