using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.DbHealth.Check
{
	sealed class PhysicalFileCheckerForTesting : PhysicalFileSizeChecker
	{
		public PhysicalFileCheckerForTesting()
			: base()
		{
		}

		public void CheckDesktopExpressDataSize_Exposed(DbHealthWarningList warningList)
		{
			warningList.Clear();
			DoCheckDesktopExpressDataSize(Db.Connection, warningList);
		}

		public void CheckDiscSpace_Exposed(DbConnection connection, IEnumerable<string> dbList, DbHealthWarningList warningList)
		{
			warningList.Clear();
			DoCheckDiskSpace(dbList, connection, warningList);
		}

		protected override int FreeSpaceUsedSpaceMinRatio
		{
			get
			{
				return (FreeSpaceUsedSpaceMinRatioOverride == null) ? base.FreeSpaceUsedSpaceMinRatio : FreeSpaceUsedSpaceMinRatioOverride.Value;
			}
		}
		public int? FreeSpaceUsedSpaceMinRatioOverride;

		protected override int SqlExpressDataPagesThreshold
		{
			get
			{
				return (SqlExpressDataPagesThresholdOverride == null) ? base.SqlExpressDataPagesThreshold : SqlExpressDataPagesThresholdOverride.Value;
			}
		}

		public int? SqlExpressDataPagesThresholdOverride;

		protected override bool IsExpressSqlServerEdition(DbConnection.SqlServerEdition serverEdition)
		{
			return (IsDesktopOrExpressSqlServerEditionOverride == null) ? base.IsExpressSqlServerEdition(serverEdition) : IsDesktopOrExpressSqlServerEditionOverride.Value;
		}
		public bool? IsDesktopOrExpressSqlServerEditionOverride;

		public long GetDiskFreeSpaceInMb_Exposed(string driveLetter, DbConnection conn)
		{
			return GetDiskFreeSpaceInMb(driveLetter, conn);
		}
	}
}
