using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSWatermarkUpdater
	{
		void Update();
		bool CanMoveForwardWatermark { get; }
	}

	public class JCSWatermarkUpdater : IJCSWatermarkUpdater
	{
		public void Update()
		{
			#region SuppressResourceStringsCheckRegion

			var newLatestJobCreationDate = Db.Connection.ExecuteScalar<DateTime>("SELECT ISNULL(MAX(JHC_SystemCreateTimeUtc), @mindate) FROM dbo.JobToCloseQueue"
				, (cmd) => cmd.AddParameter("@mindate", System.Data.SqlDbType.SmallDateTime, MinSqlSmallDateTime));

			#endregion

			var currentWatermark = new ZDateTime(AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.Value);

			if (newLatestJobCreationDate != currentWatermark)
			{
				AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newLatestJobCreationDate);
			}

			if (newLatestJobCreationDate == MinSqlSmallDateTime)
			{
				AccountingConfigurationRegistry.Instance.LastUTCDateTimeOfReachingTheHighestJCSWatermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}
		}

		public bool CanMoveForwardWatermark
		{
			get
			{
				var lastTimeReachedTheHighestWatermark = AccountingConfigurationRegistry.Instance.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Value;
				return ZDateTime.UtcNow.ToDateTime().Subtract(lastTimeReachedTheHighestWatermark).TotalDays > 1D;
			}
		}

		internal static readonly DateTime MinSqlSmallDateTime = new DateTime(1900, 1, 1);
	}
}
