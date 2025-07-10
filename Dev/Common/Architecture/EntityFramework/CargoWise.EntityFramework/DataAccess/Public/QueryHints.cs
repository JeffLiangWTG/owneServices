using System;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum QueryHints
	{
		None = 0,
		RECOMPILE = 1,
		LOOPJOIN = 2,
		MAXDOP1 = 4,
		CARDINALITY = 8,
	}

	static class QueryHintNames
	{
		public static string Name(QueryHints queryHint)
		{
			switch (queryHint)
			{
				case QueryHints.RECOMPILE:
					return "RECOMPILE";
				case QueryHints.LOOPJOIN:
					return (NoResString)"LOOP JOIN";
				case QueryHints.MAXDOP1:
					return (NoResString)"MAXDOP 1";
				case QueryHints.CARDINALITY:
					return (NoResString)"USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION')";
			}

			throw new InvalidOperationException("Invalid QueryHint");
		}
	}
}
