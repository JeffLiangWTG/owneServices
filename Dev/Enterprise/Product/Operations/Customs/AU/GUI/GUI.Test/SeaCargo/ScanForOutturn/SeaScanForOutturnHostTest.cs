using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.AU.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaScanForOutturnHostTest : ScanForOutturnHostTest
	{
		protected override ScanForOutturnHost GetNewScanForOutturnHost(ZForm form) => new SeaScanForOutturnHost(form, new ScanCusSCAOceanBill(Factory.New<CusSCAOceanBill>()));
	}
}
