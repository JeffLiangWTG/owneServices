using System;
using CargoWise.Data;
using Enterprise.DbHealth.Check.Checkers;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;

[assembly: StabilityChecker("Database Version Stability Checker", "DBC", typeof(DbVersionStabilityChecker))]

namespace Enterprise.DbHealth.Check.Checkers
{
	class DbVersionStabilityChecker : IStabilityChecker
	{
		public StabilityResult[] Check()
		{
			if (!EnvProxy.IsHostedWithCargowise)
			{
				_ = Db.Connection.ServerVersionNumber.IsSupported(out var failureMessage);

				if (!string.IsNullOrEmpty(failureMessage))
				{
					return new[] { new StabilityResult(StabilityResultLevel.Warning, failureMessage) };
				}
			}

			return Array.Empty<StabilityResult>();
		}
	}
}
