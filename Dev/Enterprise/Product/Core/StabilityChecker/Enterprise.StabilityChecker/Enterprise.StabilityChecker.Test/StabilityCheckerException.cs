using System;

namespace Enterprise.StabilityChecker.Testing
{
	sealed class StabilityCheckerException : IStabilityChecker
	{
		#region IStabilityChecker Members

		StabilityResult[] IStabilityChecker.Check()
		{
			throw new ArgumentException("Something went wrong");
		}

		#endregion
	}
}
