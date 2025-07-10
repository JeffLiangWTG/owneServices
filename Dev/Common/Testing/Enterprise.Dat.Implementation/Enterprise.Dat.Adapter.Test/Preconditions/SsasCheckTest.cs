using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Preconditions.Testing
{
	[RequiresSoftware(RequiredSoftware.SsasTabular2016OrLater)]
	abstract class SsasCheckTest : TestCase
	{
		public void TestEnabled()
		{
			Assert(true);
		}

		sealed class CurrentSqlServerTest : SsasCheckTest
		{
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		sealed class LatestAvailableSqlServerTest : SsasCheckTest
		{
		}
	}
}
