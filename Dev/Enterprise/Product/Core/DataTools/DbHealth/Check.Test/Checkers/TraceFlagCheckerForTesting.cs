using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.DbHealth.Check.Testing
{
	sealed class TraceFlagCheckerForTesting : TraceFlagChecker
	{
		readonly List<TraceFlag> enabledTFs = new List<TraceFlag>();

		internal void EnableTF(int tfNumber, bool isGlobal)
		{
			enabledTFs.Add(new TraceFlag(tfNumber, isGlobal));
		}

		internal void DisableTF(int tfNumber)
		{
			var ind = enabledTFs.FindIndex(x => x.TFNumber == tfNumber);
			if (ind >= 0)
			{
				enabledTFs.RemoveAt(ind);
			}
		}

		internal override List<TraceFlag> GetTraceFlags(DbConnection connection)
		{
			return enabledTFs;
		}
	}
}
