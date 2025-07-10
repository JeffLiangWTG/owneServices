namespace Enterprise.Accounting.Business
{
	public enum ChargeComparison
	{
		Unknown = -1,
		Different = 0,
		SameChargeableButDifferentRate = 1,
		SameRateButDifferentChargeable = 2,
		Same = 3
	}
}