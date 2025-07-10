using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTINFMessageHeaderProvider : ExitMessageHeaderProvider
	{
		public EXTINFMessageHeaderProvider(CusExitReport cusExitReport) : base(cusExitReport)
		{
		}

		public override IExitHeader ExitHeader => exitHeader ?? (exitHeader = new EXTINFHeaderProvider(cusExitReport));
		IExitHeader exitHeader;
	}
}
