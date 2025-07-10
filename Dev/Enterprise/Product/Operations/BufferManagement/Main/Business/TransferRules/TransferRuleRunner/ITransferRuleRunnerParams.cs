namespace Enterprise.BufferManagement.Business
{
	public interface ITransferRuleRunnerParams
	{
		bool IsCdcEnabled { get; }

		bool IsResponsive { get; }
	}
}
