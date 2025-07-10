using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceAuditSnapshotTest : TestCase
	{
		public void TestInitializeProperties()
		{
			var snapshot = new ComplianceAuditSnapshot();
			CombineAssertions(() =>
			{
				AssertNotNull(snapshot.Parties);
				AssertNotNull(snapshot.Countries);
				AssertNotNull(snapshot.Commodities);
				AssertNotNull(snapshot.ComplianceJobDirection);
				AssertNotNull(snapshot.OverrideDecision);
			});
		}
	}
}
