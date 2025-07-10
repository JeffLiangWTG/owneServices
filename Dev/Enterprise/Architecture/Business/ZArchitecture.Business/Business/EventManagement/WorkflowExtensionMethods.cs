using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	static class WorkflowExtensionMethods
	{
		public static PartialEventLog ToPartialEventLog(this IStmALog log)
		{
			return log != null ? new PartialEventLog(log) : null;
		}

		public static bool IsPartialLog(this IStmALog log)
		{
			return log != null
				&& log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial)
				&& numberRegex.Value.IsMatch(log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial])
				&& log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total)
				&& numberRegex.Value.IsMatch(log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total]);
		}

		static readonly Lazy<Regex> numberRegex = new Lazy<Regex>(() => new Regex("^[0-9]+$", RegexOptions.Compiled));

		public static IEnumerable<PartialEventLog> ToPartialEventLogs(this IEnumerable<StmALog> source, ZString eventCode)
		{
			foreach (var log in source)
			{
				if (!log.SL_IsCancelled
					&& log.SL_SE_NKEvent == eventCode
					&& IsPartialLog(log))
				{
					yield return log.ToPartialEventLog();
				}
			}
		}

		public static bool HasTriggerFired(this IWorkflowItem workflowItem)
		{
			return (workflowItem as IWorkflowTrigger)?.LastFiredTime.IsValid ?? false;
		}
	}
}
