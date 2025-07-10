using System.Collections.Generic;
using Enterprise.StabilityChecker;

[assembly: StabilityChecker("License Stability Checker", "LIC", typeof(Enterprise.Licensing.LicenceStabilityChecker))]
namespace Enterprise.Licensing
{
	class LicenceStabilityChecker : IStabilityChecker
	{
		public StabilityResult[] Check()
		{
			List<StabilityResult> results = new List<StabilityResult>();
			var expiryCheck = LicenceExpiryCheck.Create();

			// Licences are renewed when they are 30 days from expiry.
			// A licence within 25 days of expiry is missing renewals.
			if (expiryCheck.DaysToExpiry <= 25)
			{
				results.Add(new StabilityResult(expiryCheck.DaysToExpiry <= 2 ? StabilityResultLevel.Critical : StabilityResultLevel.Warning,
					expiryCheck.StabilityErrorMessage));
			}
			return results.ToArray();
		}
	}
}
