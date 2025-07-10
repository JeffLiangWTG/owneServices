using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	/// <summary>
	/// Checks whether or not Instant File Initialization is enabled
	/// </summary>
	class InstantFileInitializationChecker : IChecker
	{
		readonly IEnumerable<IInstantFileInitializationStrategy> strategies;

		public InstantFileInitializationChecker(params IInstantFileInitializationStrategy[] strategies)
		{
			if (strategies.Length == 0)
			{
				strategies = new IInstantFileInitializationStrategy[]
				{
					new InstantFileInitializationHostLocationStrategy(),
					new InstantFileInitializationFastQueryStrategy(),
					new InstantFileInitializationSlowQueryStrategy()
				};
			}

			this.strategies = strategies.AsEnumerable();
		}

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			foreach (var strategy in strategies)
			{
				switch (strategy.IsInstantFileInitializationEnabled(logger))
				{
					case true:
						return;

					case false:
						warningList.Add(
							CreateWarning("Instant file initialization should be enabled on your SQL Server instance."));
						return;
				}
			}

			warningList.Add(CreateWarning("Unable to determine if instant file initialization is enabled."));
		}

		string IChecker.Description
		{
			get { return "Check Instant File Initialization is enabled"; }
		}

		internal IEnumerable<Type> StrategyTypes => strategies.Select(s => s.GetType());

		DatabaseWarning CreateWarning(string action)
		{
			return new DatabaseWarning(
				Db.DatabaseName,
				ServerWarning.InstantFileInitializationWarning,
				"Instant File Initialization",
				action);
		}
	}
}
