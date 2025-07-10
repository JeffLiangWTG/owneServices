namespace Enterprise.StabilityChecker.Testing
{
	sealed class StabilityCheckerHealthy : IStabilityChecker
	{
		#region IStabilityChecker Members

		StabilityResult[] IStabilityChecker.Check()
		{
			return new StabilityResult[1] { new StabilityResult(StabilityResultLevel.Healthy, "All OK") };
		}

		#endregion
	}
}
