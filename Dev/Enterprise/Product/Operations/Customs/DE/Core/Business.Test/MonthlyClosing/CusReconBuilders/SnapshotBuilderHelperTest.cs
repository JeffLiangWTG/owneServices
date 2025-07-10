using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class SnapshotBuilderHelperTest : TestCase
	{
		public void TestNullIfNotSpecified()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Specified", new DateTime(2022, 11, 16), SnapshotBuilderHelper.NullIfNotSpecified(new DateTime(2022, 11, 16), true));
				AssertEquals("Not specified", null, SnapshotBuilderHelper.NullIfNotSpecified((DateTime)default, false));
				AssertEquals("Specified default", new DateTime(1, 1, 1), SnapshotBuilderHelper.NullIfNotSpecified((DateTime)default, true));
				AssertEquals("Not specified with value", null, SnapshotBuilderHelper.NullIfNotSpecified(new DateTime(2022, 11, 16), false));
			});
		}
	}
}
