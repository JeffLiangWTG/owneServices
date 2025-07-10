using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobProfitLossCalculation
	{
		public JobProfitLossCalculation(JobHeader job)
		{
			SetProfitLossValues(job);
		}

		public ZDecimal TotalAccrual { get; private set; }

		public ZDecimal TotalCost { get; private set; }

		public ZDecimal TotalLineAmount { get; private set; }

		public ZDecimal TotalRevenue { get; private set; }

		public ZDecimal TotalWIP { get; private set; }

		public ZDecimal TotalMargin { get; private set; }

		public ZDecimal RevRecognized { get; private set; }

		public ZDecimal RevNotRecognized { get; private set; }

		public ZDecimal CstRecognized { get; private set; }

		public ZDecimal CstNotRecognized { get; private set; }

		public ZDecimal WipRecognized { get; private set; }

		public ZDecimal WipNotRecognized { get; private set; }

		public ZDecimal AcrRecognized { get; private set; }

		public ZDecimal AcrNotRecognized { get; private set; }

		public ZDecimal ProfitLossRecognized { get; private set; }

		public ZDecimal ProfitLossNotRecognized { get; private set; }

		#region Implementation

		void SetProfitLossValues(JobHeader job)
		{
			var line = GetProfitLossLine(job);
			if (line == null)
			{
				return;
			}

#pragma warning disable CA1507 // Use nameof to express symbol names

			TotalRevenue = (ZDecimal)line["TotalRevenue"];
			TotalWIP = (ZDecimal)line["TotalWIP"];
			TotalCost = (ZDecimal)line["TotalCost"];
			TotalAccrual = (ZDecimal)line["TotalAccrual"];
			TotalLineAmount = (ZDecimal)line["TotalLineAmount"];
			RevRecognized = (ZDecimal)line["RevRecognized"];
			RevNotRecognized = (ZDecimal)line["RevNotRecognized"];
			CstRecognized = (ZDecimal)line["CstRecognized"];
			CstNotRecognized = (ZDecimal)line["CstNotRecognized"];
			WipRecognized = (ZDecimal)line["WipRecognized"];
			WipNotRecognized = (ZDecimal)line["WipNotRecognized"];
			AcrRecognized = (ZDecimal)line["AcrRecognized"];
			AcrNotRecognized = (ZDecimal)line["AcrNotRecognized"];
			ProfitLossRecognized = (ZDecimal)line["ProfitLossRecognized"];
			ProfitLossNotRecognized = (ZDecimal)line["ProfitLossNotRecognized"];

#pragma warning restore CA1507 // Use nameof to express symbol names

			var totalRevenue_Effective = (ZDecimal)line["TotalRevenue_Effective"];
			var totalWIP_Effective = (ZDecimal)line["TotalWIP_Effective"];
			var totalLineAmount_Effective = (ZDecimal)line["TotalLineAmount_Effective"];
			var totalCost_Effective = (ZDecimal)line["totalCost_Effective"];
			var totalAccrual_Effective = (ZDecimal)line["totalAccrual_Effective"];

			if (totalRevenue_Effective == 0m && totalWIP_Effective == 0m && totalCost_Effective == 0m && totalAccrual_Effective == 0m)
			{
				TotalMargin = 0m;
			}
			else if ((totalRevenue_Effective + totalWIP_Effective) != 0m)
			{
				TotalMargin = new ZDecimal((totalLineAmount_Effective / (totalRevenue_Effective + totalWIP_Effective) * 100m)).Round(2);
			}
			else
			{
				TotalMargin = totalLineAmount_Effective > 0 ? 100m : -100m;
			}
		}

		DynamicBusinessObject GetProfitLossLine(JobHeader job)
		{
			var (baseSqlQuery, parameters) = GetSqlQueryParameters(job);

			var lines = new DynamicBusinessObjectCollection(job.Factory);
			lines.Load(baseSqlQuery, parameters);
			return lines.Any() ? lines[0] : null;
		}

		(string baseSqlQuery, ZSqlParameter[] parameters) GetSqlQueryParameters(JobHeader job)
		{
			var baseSqlQuery = GetBaseSqlQuery(job);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@JobPK", job.PK, AccTransactionLinesSchema.AL_JH);
			parameters.Add("@REV", ZArchitecture.Core.TransactionLineTypes.Revenue, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@WIP", ZArchitecture.Core.TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@CST", ZArchitecture.Core.TransactionLineTypes.Cost, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@ACR", ZArchitecture.Core.TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);

			if (job.Parent is IJobInvoicingPlugInAdditionalJobs)
			{
				var parentAsIJobInvoiceingPlugInAdditionalJobs = (IJobInvoicingPlugInAdditionalJobs)job.Parent;
				var foreignKeysStringBuilder = new StringBuilder();
				if (parentAsIJobInvoiceingPlugInAdditionalJobs != null && parentAsIJobInvoiceingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor.Length > 0)
				{
					baseSqlQuery += " OR (JH_ParentID IN (%RelatedForeignKeys%) AND JH_GC = @CurrentCompany)";
					parentAsIJobInvoiceingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor.Select(x => x.PK).ToList().ForEach(x => foreignKeysStringBuilder.Append("'" + x + "',"));
					baseSqlQuery = baseSqlQuery.Replace("%RelatedForeignKeys%", foreignKeysStringBuilder.ToString().TrimEnd(','));
					parameters.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				}
			}

			return (baseSqlQuery,  parameters.ToArray());
		}

		string GetBaseSqlQuery(JobHeader job)
		{
			var disbursementChargeCode = GetDisbursementChargeCode(job);
			var excludeDisbursementTransactionLines = !string.IsNullOrEmpty(disbursementChargeCode)
				? " AND AL_AC NOT IN (" + disbursementChargeCode + ")" 
				: string.Empty;
			var excludeDisbursementJobCharges = !string.IsNullOrEmpty(disbursementChargeCode)
				? " AND JR_AC NOT IN (" + disbursementChargeCode + ")"
				: string.Empty;

			return @$"
				SELECT 
					SUM(RevRecognized) + SUM(RevNotRecognized) AS TotalRevenue,
					SUM(RevRecognized_Effective) + SUM(RevNotRecognized_Effective) AS TotalRevenue_Effective, 

					SUM(WipRecognized) + SUM(WipNotRecognized) AS TotalWIP,
					SUM(WipRecognized_Effective) + SUM(WipNotRecognized_Effective) AS TotalWIP_Effective,
					
					SUM(CstRecognized) + SUM(CstNotRecognized) AS TotalCost,  
					SUM(CstRecognized_Effective) + SUM(CstNotRecognized_Effective) AS TotalCost_Effective, 
 
					SUM(AcrRecognized) + SUM(AcrNotRecognized) AS TotalAccrual, 
					SUM(AcrRecognized_Effective) + SUM(AcrNotRecognized_Effective) AS TotalAccrual_Effective, 

					SUM(RevRecognized) + SUM(WipRecognized) + SUM(CstRecognized) + SUM(AcrRecognized) + SUM(RevNotRecognized) + SUM(WipNotRecognized) + SUM(CstNotRecognized) + SUM(AcrNotRecognized) AS TotalLineAmount,
					SUM(RevRecognized_Effective) + SUM(WipRecognized_Effective) + SUM(CstRecognized_Effective) + SUM(AcrRecognized_Effective) + SUM(RevNotRecognized_Effective) + SUM(WipNotRecognized_Effective) + SUM(CstNotRecognized_Effective) + SUM(AcrNotRecognized_Effective) AS TotalLineAmount_Effective,

					SUM(RevRecognized) AS RevRecognized, 
					SUM(RevRecognized_Effective) AS RevRecognized_Effective, 

					SUM(RevNotRecognized) AS RevNotRecognized, 
					SUM(RevNotRecognized_Effective) AS RevNotRecognized_Effective, 

					SUM(CstRecognized) AS CstRecognized,
					SUM(CstRecognized_Effective) AS CstRecognized_Effective,

					SUM(CstNotRecognized) AS CstNotRecognized, 
					SUM(CstNotRecognized_Effective) AS CstNotRecognized_Effective, 

					SUM(WipRecognized) AS WipRecognized, 
					SUM(WipRecognized_Effective) AS WipRecognized_Effective, 

					SUM(WipNotRecognized) AS WipNotRecognized, 
					SUM(WipNotRecognized_Effective) AS WipNotRecognized_Effective, 

					SUM(AcrRecognized) AS AcrRecognized,
					SUM(AcrRecognized_Effective) AS AcrRecognized_Effective,

					SUM(AcrNotRecognized) AS AcrNotRecognized,
					SUM(AcrNotRecognized_Effective) AS AcrNotRecognized_Effective,

					SUM(RevRecognized) + SUM(WipRecognized) + SUM(CstRecognized) + SUM(AcrRecognized) AS ProfitLossRecognized, 
					SUM(RevRecognized_Effective) + SUM(WipRecognized_Effective) + SUM(CstRecognized_Effective) + SUM(AcrRecognized_Effective) AS ProfitLossRecognized_Effective, 

					SUM(RevNotRecognized) + SUM(WipNotRecognized) + SUM(CstNotRecognized) + SUM(AcrNotRecognized)  AS ProfitLossNotRecognized,
					SUM(RevNotRecognized_Effective) + SUM(WipNotRecognized_Effective) + SUM(CstNotRecognized_Effective) + SUM(AcrNotRecognized_Effective)  AS ProfitLossNotRecognized_Effective

					FROM 
					(
					SELECT
						AL_AC as ChargCode,
						AL_JH AS JH_PK,  

						CASE WHEN AL_LineType = @REV THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS TotalRevenue,    
						CASE WHEN AL_LineType = @REV {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS TotalRevenue_Effective,    

						CASE WHEN AL_LineType = @CST THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS TotalCost,
						CASE WHEN AL_LineType = @CST {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS TotalCost_Effective,       

						CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NOT NULL THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS RevRecognized,
						CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NOT NULL {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS RevRecognized_Effective,   

						CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NULL THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS RevNotRecognized,
						CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NULL {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS RevNotRecognized_Effective, 

						CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NOT NULL THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS CstRecognized,
						CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NOT NULL {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS CstRecognized_Effective,

						CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NULL THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS CstNotRecognized,
						CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NULL {excludeDisbursementTransactionLines} THEN CAST(AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS CstNotRecognized_Effective, 

						CASE WHEN AL_LineType = @WIP AND AL_ReverseDate IS NULL THEN CAST(-AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS WipRecognized,
						CASE WHEN AL_LineType = @WIP AND AL_ReverseDate IS NULL {excludeDisbursementTransactionLines} THEN CAST(-AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS WipRecognized_Effective, 

						0 AS WipNotRecognized, 
						0 AS WipNotRecognized_Effective, 
						
						CASE WHEN AL_LineType = @ACR AND AL_ReverseDate IS NULL THEN CAST(-AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS AcrRecognized,
						CASE WHEN AL_LineType = @ACR AND AL_ReverseDate IS NULL {excludeDisbursementTransactionLines} THEN CAST(-AL_LineAmount AS DECIMAL(24,9)) ELSE 0 END AS AcrRecognized_Effective,

						0 AS AcrNotRecognized,
						0 AS AcrNotRecognized_Effective

						FROM dbo.AccTransactionLines	
					
					UNION ALL

					SELECT 
						JR_AC as ChargCode,
						JR_JH AS JH_PK, 

						0 AS TotalRevenue,
						0 AS TotalRevenue_Effective,
						 
						0 AS TotalCost,
						0 AS TotalCost_Effective,
						  
						0 AS RevRecognized,
						0 AS RevRecognized_Effective,
						 
						0 AS RevNotRecognized,
						0 AS RevNotRecognized_Effective,
						 
						0 AS CstRecognized,
						0 AS CstRecognized_Effective,

						0 AS CstNotRecognized,
						0 AS CstNotRecognized_Effective,
						 
						0 AS WipRecognized, 
						0 AS WipRecognized_Effective,

						CASE WHEN JR_AL_ARLine IS NULL THEN JR_LocalSellAmt ELSE 0 END AS WipNotRecognized, 
						CASE WHEN JR_AL_ARLine IS NULL {excludeDisbursementJobCharges} THEN JR_LocalSellAmt ELSE 0 END AS WipNotRecognized_Effective, 

						0 AS AcrRecognized,
						0 AS AcrRecognized_Effective,

						CASE WHEN JR_AL_APLine IS NULL THEN -JR_LocalCostAmt ELSE 0 END  AS AcrNotRecognized,
						CASE WHEN JR_AL_APLine IS NULL {excludeDisbursementJobCharges} THEN -JR_LocalCostAmt ELSE 0 END  AS AcrNotRecognized_Effective

						FROM dbo.JobCharge
					) ResultSet
					INNER JOIN dbo.JobHeader ON (ResultSet.JH_PK = JobHeader.JH_PK)
					WHERE JobHeader.JH_PK = @JobPK  
				";	
		}

		ZString GetDisbursementChargeCode(JobHeader job)
		{
			var disbursementChargeCodesCommaSeparated = ZString.Empty;
			if (!AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
			{
				var readonlyfactory = job.Factory.GetCachedReadOnlyFactory();
				var reloadedJob = readonlyfactory.Load<Job>(job.PK);
				var disbursmentChargeCode = new HashSet<ZString>();

				if (reloadedJob != null)
				{
					reloadedJob.Charges.IsManagedForDataRefresh = false; //fix performance issue - see WI00155924

					if (reloadedJob.Charges.Count > 0)
					{
						foreach (JobCharge charge in reloadedJob.Charges)
						{
							if (Job.GetChargeTypeInformation(charge.ChargeCode, reloadedJob).IsDisbursement)
							{
								disbursmentChargeCode.Add(charge.JR_AC.ToString());
							}
						}

						if (disbursmentChargeCode.Count > 0)
						{
							var chargeCodes = new ZString[disbursmentChargeCode.Count];
							disbursmentChargeCode.CopyTo(chargeCodes);
							disbursementChargeCodesCommaSeparated = "'" + string.Join("', '", chargeCodes) + "'";
						}
					}
				}
			}
			return disbursementChargeCodesCommaSeparated;
		}

		#endregion
	}
}
