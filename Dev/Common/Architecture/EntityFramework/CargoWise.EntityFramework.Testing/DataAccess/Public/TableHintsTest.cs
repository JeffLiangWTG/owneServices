using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class TableHintsTest : TestCase
	{
		public void TestTableHintNames()
		{
			foreach (TableHints hint in Enum.GetValues(typeof(TableHints)))
			{
				if (hint != TableHints.None)
				{
					AssertEquals(hint.ToString(), TableHintNames.Name(hint));
				}
			}
		}
	}
}
