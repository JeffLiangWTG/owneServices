using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Core;

[assembly: StabilityChecker("Accounting Balances BatchProcessor Stability Checker", "ACC", typeof(Enterprise.Accounting.Business.StabilityCheck.AccOrgBalanceStabilityChecker))]
namespace Enterprise.Accounting.Business.StabilityCheck
{
	class AccOrgBalanceStabilityChecker : IStabilityChecker
	{
		#region IStabilityChecker Members

		public StabilityResult[] Check()
		{
			List<StabilityResult> finalResult = new List<StabilityResult>();

			try
			{
				var firstAttempt = ExecuteCheckAndRepairAccOrgBalanceProcedureInTransaction();

				if (firstAttempt.Rows.Count != 0)
				{
					var firstAttemptText = BuildStringFromTable(firstAttempt);
					var secondAttempt = ExecuteCheckAndRepairAccOrgBalanceProcedureInTransaction();

					if (secondAttempt.Rows.Count == 0)
					{
						finalResult.Add(new StabilityResult(StabilityResultLevel.Warning, Res.GetString("f08b8780-1449-41df-9e5b-f5722734c5c2", "Stored accounting balances did not match balances calculated from detailed records. This has been repaired successfully and now stored balances match the balances calculated from detailed records.")));
					}
					else
					{
						var secondAttemptText = BuildStringFromTable(secondAttempt);
						finalResult.Add(new StabilityResult(StabilityResultLevel.Critical, Res.GetString("27316980-bd9a-49b2-9810-6e7b2458a904", "Stored accounting balances did not match balances calculated from detailed records and were not repaired successfully. Please contact support.")));
						ErrorReporter.ReportOnce("AccOrgBalancesChecker repaired balances", string.Format("Details of adjustments.\r\n\r\n First attempt:\r\n{0}\r\n\r\nSecond attempt:\r\n{1}", firstAttemptText.ToString(), secondAttemptText.ToString()));
					}
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				finalResult.Add(new StabilityResult(StabilityResultLevel.Exception, Res.GetString("d549ebcd-c884-4dbf-802f-10cdfa54f3d9", "There was a problem accessing the Database. {0}", ex.Message)));
			}
			return finalResult.ToArray();
		}

		DataTable ExecuteCheckAndRepairAccOrgBalanceProcedureInTransaction()
		{
			DataTable result;

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				result = DataUtils.GetDataTableFromQuery(Db.Connection, (NoResString)"EXEC CheckAndRepairAccOrgBalance");
				transactionManager.CommitTransaction();
			}

			return result;
		}

		static StringBuilder BuildStringFromTable(DataTable dataTable)
		{
			var sb = new StringBuilder((NoResString)"OrgCode, CompanyCode, Ledger, BalanceTotalDiff, RecognizedTotalDiff, UnrecognizedTotalDiff, ClaimTotalDiff, OrganizationPK, CompanyPK\r\n");

			foreach (DataRow row in dataTable.Rows)
			{
				sb.Append(row["OrgCode"] + ", ");
				sb.Append(row["CompanyCode"] + ", ");
				sb.Append(row["Ledger"] + ", ");
				sb.Append(row["BalanceTotalSum"] + ", ");
				sb.Append(row["RecognizedTotalSum"] + ", ");
				sb.Append(row["UnrecognizedTotalSum"] + ", ");
				sb.Append(row["ClaimTotalSum"] + ", ");
				sb.Append(row["OrganizationPK"] + ", ");
				sb.Append(row["CompanyPK"]);
				sb.AppendLine();
			}
			return sb;
		}

		#endregion
	}
}
