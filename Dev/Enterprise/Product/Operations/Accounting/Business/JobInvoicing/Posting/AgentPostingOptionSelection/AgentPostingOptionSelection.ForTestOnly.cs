#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection
{
	partial class AgentPostingOptionSelector
	{
		public IReceivablesPostingChargeCollection AgentChargesBeingPosted_ForTestOnly => AgentChargesBeingPosted;

		public void ValidateExchangeRateCalculationMethod_ForTestOnly() => ValidateExchangeRateCalculationMethod();
	}
}

#endif
