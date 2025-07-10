namespace Enterprise.Customs.Common
{
	public sealed class ApportionStrategy : IApportionStrategy
	{
		public decimal Round(decimal amount)
		{
			return ZArchitecture.Core.Utilities.Round(amount, JobComInvCharge.NumberOfDecimals);
		}

		public bool ShouldBackApportion(ApportionChargeKey chargeKey) => true;

		public decimal UnitOfAmountToBackApportion => JobComInvCharge.UnitOfAmountToBackApportion;
	}
}
