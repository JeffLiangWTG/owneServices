using NUnit.Framework;

namespace CargoWise.Data.Providers.Common.Test
{
	public class SqlFailoverSettingsTest
	{
		[TestCase("abcxyz.db.wisegrid.net", ExpectedResult = true)]
		[TestCase("abcxyzd.wisegrid.net\\Instance1", ExpectedResult = true)]
		[TestCase("abcxyzd.wisegrid.net", ExpectedResult = false)]
		[TestCase("abcxyzd.db.wtg.zone", ExpectedResult = false)]
		public bool ShouldSpecifyMultiSubnetFailover(string serverName)
		{
			// Arrange
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { "abcxyz.db.wisegrid.net", "abcxyzd.wisegrid.net$Instance1" }))
			{
				// Act
				var actual = SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(serverName);

				// Assert
				return actual;
			}
		}
	}
}
