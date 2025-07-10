using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.BufferManagement.Business
{
	public static class AcceptabilityBandMatchingWorkflowsCalculator
	{
		public static AcceptabilityBandMatchingWorkflowResultCollection GetMatchingWorkflows(BMComponentAcceptabilityBand band, AcceptabilityBandDataProvider provider, AcceptabilityBandSqlBuilderParameters parameters)
		{
			var results = new AcceptabilityBandMatchingWorkflowResultCollection(band.PK.ToGuid());

			if (!band.UseFilterStripsExclusively)
			{
				throw new ArgumentException($"Acceptability Band {band.HumanReadableName} is of type {band.BAB_Type}. Only Acceptability Bands of type Count, Number As Percentage, Planned Duration Percentage, and Total Planned Duration can return matching workflows");
			}

			var command = GetDbCommand(band, provider, parameters) ?? throw new ArgumentException($"Could not formulate an SQL query from Acceptability Band {band.HumanReadableName}");

			using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (reader.Read())
				{
					var workflowPK = reader.GetGuid(0);
					results.MatchingWorkflows.Add(workflowPK);
				}
			}

			return results;
		}

		public static DbCommand GetDbCommand(BMComponentAcceptabilityBand band, AcceptabilityBandDataProvider provider, AcceptabilityBandSqlBuilderParameters parameters)
		{
			var (matchingWorkflowSql, matchingWorkflowParameters) = band.GetSqlStrategy(parameters).GetMatchingWorkflowsSql();
			var connection = provider.ConnectionWrapper.Connection;
			var command = new ZSqlConnectionInfo(connection, null).GetNewDbCommandForSelect(matchingWorkflowSql, matchingWorkflowParameters);

			var commandTimeoutValue = Env.Instance.IsWebService
				? BMSRegistry.Instance.AcceptabilityBandCalculationServiceExecutionTimeout.Value
				: BMSRegistry.Instance.AcceptabilityBandLocalCalculationExecutionTimeout.Value;

			if (commandTimeoutValue > 0)
			{
				command.CommandTimeout = commandTimeoutValue;
			}

			return command;
		}
	}
}
