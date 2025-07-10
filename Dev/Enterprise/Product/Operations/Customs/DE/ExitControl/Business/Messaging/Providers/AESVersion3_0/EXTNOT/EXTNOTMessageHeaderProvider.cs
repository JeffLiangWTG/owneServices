using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public class EXTNOTMessageHeaderProvider : ExitMessageHeaderProvider
	{
		public EXTNOTMessageHeaderProvider(CusExitReport cusExitReport) : base(cusExitReport)
		{
		}

		public override IExitHeader ExitHeader => exitHeader ?? (exitHeader = new EXTNOTHeaderProvider(cusExitReport));
		IExitHeader exitHeader;
	}
}
