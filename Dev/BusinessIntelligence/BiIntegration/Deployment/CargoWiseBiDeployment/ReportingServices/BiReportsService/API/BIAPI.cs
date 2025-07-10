using System;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public abstract class BIAPI
	{
		[ThreadSafe]
		public abstract bool IsAPIEnabled { get; }

		public abstract void VerifyAPIIsEnabled();

		[ThreadSafe]
		public string AuditServer
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
				}
			}
		}

		public int APIBatchSize
		{
			get
			{
				using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(AuditServer, Db.AuditDatabaseName))
				{
					var result = BiMasterState.GetParameter(auditConnection, BiConstants.AspBatchSize);
					if (string.IsNullOrEmpty(result))
					{
						return 5000;
					}
					else
					{
						return Convert.ToInt32(result, CultureInfo.InvariantCulture);
					}
				}
			}
		}
	}
}
