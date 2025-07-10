using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Core;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.Core
{
	public static class SqlCutOverHelper
	{
		internal static readonly DateTime DateWhenMinimumGenerationBumpToSql2019 = new DateTime(2022, 10, 1);

		public static string VersionNoLongerSupportsOlderSqlVersionsDisplayText
		{
			get
			{
				var minimumRequireGeneration = SqlServerVersionNumber.SupportedVersions.Min().Generation;
				const string MonthYearFormat = "Y";
				return SourceGenerated.Res.GetString(
					"fe5a315f-4f88-4282-85e4-d2f2529d61e3",
					"Versions released after {0} no longer support SQL versions older than {1}.",
					DateWhenMinimumGenerationBumpToSql2019.ToString(MonthYearFormat),
					minimumRequireGeneration.FormalName
				);
			}
		}

		#region Upgrade Notification Constants

		public static string UpgradeToSupportedSqlVersionAction { get { return Invariant($@" 
{Constants.ProductName} requires MS {SqlServerVersionNumber.SqlMinimumSupportedGenerationEdition} or later. Please upgrade in order to proceed.

As an alternative, we urge you to consider moving to WiseCloud and receiving the many cost and reliability benefits that it entails. WiseCloud is included virtually free (except for low cost premium services) with your license and any monthly or annual fees you pay and provides very high performance, resilience, redundancy, failover and disaster recovery in ways that almost no customer can afford or implement. It is a state of the art Cloud and has an N+1 Datacenter design with compute locations and 24x7 teams in the US, AU and UK.

Please contact your CustomerCare representative if you have any questions or need information or assistance.

"); } } // Non-Operational only message should not be translated

		public static string UpgradeToSqlEnterpriseEditionAction { get { return UpgradeToSupportedSqlVersionAction; } }

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.ZArchitecture.Core
{
	public static class SqlCutOverHelperTestHelper
	{
		public static DateTime DateWhenMinimumGenerationBump => SqlCutOverHelper.DateWhenMinimumGenerationBumpToSql2019;
	}
}
#endif
#endregion
