using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTANTHeaderProvider : ExitPresentationHeaderProvider, IEXTANTHeader
	{
		public EXTANTHeaderProvider(CusExitReport report) : base(report)
		{
		}
	}
}
