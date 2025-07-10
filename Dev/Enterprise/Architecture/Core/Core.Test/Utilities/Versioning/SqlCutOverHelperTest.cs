using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SqlCutOverHelperTest : TestCase
	{
		public void TestReleaseNoLongerSupportsOldVersionsOfSqlDisplayText()
		{
			AssertEquals("Versions released after October 2022 no longer support SQL versions older than Microsoft SQL Server 2019.", SqlCutOverHelper.VersionNoLongerSupportsOlderSqlVersionsDisplayText);
		}

		public void TestFirstDateSupportingOnlySql2019OrLaterNameContainsMinimumGeneration()
		{
			// Arrange
			var nameOfFirstDateSupportingOnlySql2019OrLater = nameof(SqlCutOverHelper.DateWhenMinimumGenerationBumpToSql2019);
			var minimumGeneration = SqlServerVersionNumber.SupportedVersions.Min().Generation;

			// Act
			// Assert
			AssertContains(minimumGeneration.Name, nameOfFirstDateSupportingOnlySql2019OrLater, true);
		}
	}
}
