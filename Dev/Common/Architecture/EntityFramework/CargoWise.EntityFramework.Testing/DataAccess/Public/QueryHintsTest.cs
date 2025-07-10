using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class QueryHintsTest : TestCase
	{
		public void TestQueryHintNames()
		{
			foreach (QueryHints hint in Enum.GetValues(typeof(QueryHints)))
			{
				switch (hint)
				{
					case QueryHints.None:
						continue;
					case QueryHints.LOOPJOIN:
						AssertEquals("LOOP JOIN", QueryHintNames.Name(hint));
						continue;
					case QueryHints.MAXDOP1:
						AssertEquals("MAXDOP 1", QueryHintNames.Name(hint));
						continue;
					case QueryHints.RECOMPILE:
						AssertEquals("RECOMPILE", QueryHintNames.Name(hint));
						continue;
					case QueryHints.CARDINALITY:
						AssertEquals("USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION')", QueryHintNames.Name(hint));
						continue;
					default:
						AssertEquals(hint.ToString(), QueryHintNames.Name(hint));
						break;
				}
			}
		}
	}
}
