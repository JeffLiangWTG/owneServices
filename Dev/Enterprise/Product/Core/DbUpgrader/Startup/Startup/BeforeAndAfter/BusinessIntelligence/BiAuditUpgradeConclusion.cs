using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Startup
{
	class BiAuditUpgradeConclusion : BusinessIntelligenceUpgradeConclusion
	{
		readonly bool isProductRegistrationValid;

		public BiAuditUpgradeConclusion(AdminConnection mainDbConnection, AdminConnection biConnection, bool isProductRegistrationValid)
			: base(mainDbConnection, biConnection)
		{
			this.isProductRegistrationValid = isProductRegistrationValid;
		}

		protected override string BiDatabaseType => "Audit";
		protected override string BiDatabaseName => Db.AuditDatabaseName;

		public override void RunAfterUpgradeSteps(IUpgradeContext upgradeContext, IUpgradeTaskWorkflowLogger logger)
		{
			RunAuditTablePartitioning(logger);
			base.RunAfterUpgradeSteps(upgradeContext, logger);
		}

		internal virtual void RunAuditTablePartitioning(IUpgradeTaskWorkflowLogger logger)
		{
			if (ShouldRunAuditTablePartitioning())
			{
				logger.ShowInfoMessage("Running Audit table partitioning script.");

				using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture,
						@"[{0}].[{1}].[usp_RecreatePartitionsAndPurgeOldData]",
						Db.AuditDatabaseName,
						BiConstants.BiAdminSchemaName);

					using (var cmd = biConnection.Command(sqlText))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, -1);
						cmd.AddOutputParameter("@InfoMessage", SqlDbType.VarChar, -1, 0, 0, "");
						cmd.AddOutputParameter("@ErrorMessage", SqlDbType.VarChar, -1, 0, 0, "");

						cmd.ExecuteNonQuery();

						var errorCode = Convert.ToInt32(cmd.GetParameterValue("@ErrorCode"));
						var infoMessage = cmd.GetParameterValue("@InfoMessage").ToString();
						var errorMessage = cmd.GetParameterValue("@ErrorMessage").ToString();

						if (errorCode != 0)
						{
							logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Audit database partitioning failed.\r\n{0}\r\n{1}", infoMessage, errorMessage));
						}
					}
				}
			}
		}

		protected virtual bool ShouldRunAuditTablePartitioning()
		{
			return isProductRegistrationValid &&
				UnpartitionedAuditTablesExist();
		}

		bool UnpartitionedAuditTablesExist()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"IF EXISTS (SELECT NULL FROM [{0}].[{1}].TableState WHERE PartitioningStatus <> 'Partitioned') SELECT 1 ELSE SELECT 0",
					Db.AuditDatabaseName,
					BiConstants.BiAdminSchemaName);

				return Convert.ToBoolean(biConnection.ExecuteScalar(sqlText));
			}
		}
	}
}
