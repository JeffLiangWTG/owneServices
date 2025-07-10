using System;
using System.Data;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ConcurrencyPolicyTest : NUnit.Framework.TestCase
	{
		public void TestHasChanges()
		{
			DataTable table = new DataTable("DummyBizo");
			var c1 = table.Columns.Add("C1", typeof(Decimal));
			var c2 = table.Columns.Add("C2", typeof(DateTime));
			var row = table.NewRow();
			table.Rows.Add(row);

			row[c1] = 1;
			row[c2] = new DateTime(2001, 2, 3);

			table.AcceptChanges();

			Assert(!ConcurrencyPolicy.HasChanged(row, c1));
			Assert(!ConcurrencyPolicy.HasChanged(row, c2));

			row[c1] = 2;

			Assert(ConcurrencyPolicy.HasChanged(row, c1));
			Assert(!ConcurrencyPolicy.HasChanged(row, c2));

			row[c2] = new DateTime(2004, 5, 6);

			Assert(ConcurrencyPolicy.HasChanged(row, c1));
			Assert(ConcurrencyPolicy.HasChanged(row, c2));
		}

		public void TestStrategies()
		{
			ConcurrencyPolicy defaultCP = ConcurrencyPolicy.Default;
			AssertEquals("Default - NotifyAndMerge", defaultCP.Strategy());
			AssertEquals("Default - NotifyAndMerge", defaultCP.ToString());
			ConcurrencyPolicy strictCP = ConcurrencyPolicy.Strict;
			AssertEquals("Strict - Notify", strictCP.Strategy());
			AssertEquals("Strict - Notify", strictCP.ToString());
			ConcurrencyPolicy ignoreCP = ConcurrencyPolicy.Ignore;
			AssertEquals("Ignore - OverwriteOtherUser", ignoreCP.Strategy());
			AssertEquals("Ignore - OverwriteOtherUser", ignoreCP.ToString());
			ConcurrencyPolicy observeCP = ConcurrencyPolicy.Observe;
			AssertEquals("Observe - NotifyAndMergeOnlyIfChangedInThisBusinessObject", observeCP.Strategy());
			AssertEquals("Observe - NotifyAndMergeOnlyIfChangedInThisBusinessObject", observeCP.ToString());
			ConcurrencyPolicy protectCP = ConcurrencyPolicy.Protect;
			AssertEquals("Protect - NotifyOnlyIfChangedInThisBusinessObject", protectCP.Strategy());
			AssertEquals("Protect - NotifyOnlyIfChangedInThisBusinessObject", protectCP.ToString());
		}
	}
}
