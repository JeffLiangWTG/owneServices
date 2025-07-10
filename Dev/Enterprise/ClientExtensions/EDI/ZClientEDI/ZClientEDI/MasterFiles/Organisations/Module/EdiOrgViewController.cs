using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Licencing.Module;
using Enterprise.Client.EDI.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Organisations.Module
{
	public class EdiOrgViewController : IEdiOrgViewController
	{
		public ILicenceViewController LicViewController => lic ?? (lic = new LicenceViewController());
		ILicenceViewController lic;
	}
}
