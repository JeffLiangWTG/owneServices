using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTANTMessageHeaderProvider : ExitMessageHeaderProvider
	{
		public EXTANTMessageHeaderProvider(CusExitReport cusExitReport) : base(cusExitReport)
		{
		}

		public override IExitHeader ExitHeader => exitHeader ?? (exitHeader = new EXTANTHeaderProvider(cusExitReport));
		IExitHeader exitHeader;
	}
}
