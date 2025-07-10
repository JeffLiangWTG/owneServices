using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSQueuePopulator
	{
		bool Populate(DateTime jobCreationDateUtc, IEnumerable<ZGuid> companyWithConfigurations);
	}

	public class JCSQueuePopulator : IJCSQueuePopulator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public bool Populate(DateTime jobCreationDateUtc, IEnumerable<ZGuid> companyWithConfigurations)
		{
			var affectedRows = -1;

			var queueSize = AccountingConfigurationRegistry.Instance.AutoJobClosureQueueMaximumLength.Value;

			var whereClause = @"JH_GC IN (SELECT [Value] from @companyPKs)
								AND JH_Status<> 'CLS'
								AND JH_ParentTableCode<> 'TH'";

			var sqlText = $@"INSERT INTO dbo.JobToCloseQueue (JHC_JH, JHC_GC, JHC_SystemCreateTimeUtc)
							SELECT JH_PK, JH_GC, JH_SystemCreateTimeUtc
							FROM dbo.JobHeader
							WHERE {whereClause}
								AND JH_SystemCreateTimeUtc > @jobCreationDateUtc
								AND JH_SystemCreateTimeUtc <= (SELECT MAX(JH_SystemCreateTimeUtc)
																FROM
																	(
																		SELECT TOP {queueSize} JH_SystemCreateTimeUtc
																		FROM dbo.JobHeader
																		WHERE {whereClause}
																			AND JH_SystemCreateTimeUtc > @jobCreationDateUtc
																		ORDER BY JH_SystemCreateTimeUtc
																	) TopNJobs
																)
							ORDER BY JH_SystemCreateTimeUtc";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@jobCreationDateUtc", System.Data.SqlDbType.SmallDateTime, jobCreationDateUtc == DateTime.MinValue ? JCSWatermarkUpdater.MinSqlSmallDateTime : jobCreationDateUtc );
				cmd.AddTableValuedParameter("@companyPKs", JobHeaderSchema.JH_GC, companyWithConfigurations);
				affectedRows = cmd.ExecuteNonQuery();
			}

			return affectedRows > 0;
		}
	}
}
