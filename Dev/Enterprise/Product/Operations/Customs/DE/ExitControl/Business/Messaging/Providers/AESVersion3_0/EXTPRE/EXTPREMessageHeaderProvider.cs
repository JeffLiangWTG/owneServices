using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0
{
	public sealed class EXTPREMessageHeaderProvider : ExitMessageHeaderProvider
	{
		public EXTPREMessageHeaderProvider(CusExitReport cusExitReport) : base(cusExitReport)
		{
		}

		public override IExitHeader ExitHeader => exitHeader ?? (exitHeader = new EXTPREHeaderProvider(cusExitReport));
		IExitHeader exitHeader;
	}
}
