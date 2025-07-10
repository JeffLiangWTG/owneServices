using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using static CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PartialEventHandlerForTest : PartialEventHandler
	{
		protected override IEnumerable<KeyValuePair<string, string>> GetParametersForCompletionLog(IEnumerable<IStmALog> logs)
		{
			var log = logs.Last();

			if (log?.Parameters.ContainsKey(Location) ?? false)
			{
				yield return new KeyValuePair<string, string>(Location, log.Parameters[Location]);
			}
		}

		protected override bool Equals(IStmALog log1, IStmALog log2) => log1 == log2;
	}
}
