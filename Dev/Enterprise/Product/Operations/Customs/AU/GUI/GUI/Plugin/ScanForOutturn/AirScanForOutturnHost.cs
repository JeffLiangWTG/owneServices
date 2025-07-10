using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirScanForOutturnHost : ScanForOutturnHost
	{
		public AirScanForOutturnHost(ZForm form, CusMAWB cusMAWB)
			: base(form, new ScanCusMAWB(cusMAWB))
		{
		}

		protected override ScanForOutturnWizard GetScanForOutturnWizard(ScanMasterBill hostBO)
		{
			return new AirScanForOutturnWizard((ScanCusMAWB)hostBO);
		}
	}
}
