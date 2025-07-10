using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaScanForOutturnHost : ScanForOutturnHost
	{
		public SeaScanForOutturnHost(ZForm form, ScanCusSCAOceanBill hostBO)
			: base(form, hostBO)
		{ }

		protected override ScanForOutturnWizard GetScanForOutturnWizard(ScanMasterBill hostBO)
		{
			return new SeaScanForOutturnWizard((ScanCusSCAOceanBill)hostBO);
		}
	}
}
