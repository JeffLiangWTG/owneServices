using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceInstallInfo : AutoServiceInstallInfo
	{
		public ServiceInstallInfo(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Automatic = true;
		}
	}
}

