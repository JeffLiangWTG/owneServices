using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ProfitLossSummaryCollection : ProfitLossSummaryCollectionBase
	{
		public ProfitLossSummaryCollection(IJobProfitLoss profitLossParent, IJobCostingPlugIn plugin)
			: base(profitLossParent, plugin)
		{
		}

		public ProfitLossSummaryCollection(IJobProfitLoss profitLossParent, params ZGuid[] jobPKs)
			: base(profitLossParent, jobPKs)
		{
		}

		#region Load

		public override void Load()
		{
			RemoveAll();

			if (JobPKs != null && JobPKs.Length > 0)
			{
				var @params = new ZSqlParameterCollection();
				string whereClause = " AND";
				if (ProfitLossParent.Filter.DepartmentFilter.IsValid)
				{
					whereClause += " ZZ_Calc_GE = @DepartmentPK AND";
					@params.Add("@DepartmentPK", ProfitLossParent.Filter.DepartmentFilter.ToGuid(), GlbDepartmentSchema.PK);
				}
				if (ProfitLossParent.Filter.BranchFilter.IsValid)
				{
					whereClause += " ZZ_Calc_GB = @BranchPK AND";
					@params.Add("@BranchPK", ProfitLossParent.Filter.BranchFilter.ToGuid(), GlbBranchSchema.PK);
				}
				if (!ProfitLossParent.Filter.JobNumberFilter.IsEmpty)
				{
					whereClause += " ZZ_Calc_JH = @JobPK AND ";
					@params.Add("@JobPK", ProfitLossParent.Filter.JobNumberFilter, JobHeaderSchema.PK);
				}
				if (!ProfitLossParent.Filter.ChargeCodeFilter.IsEmpty)
				{
					whereClause += " ZZ_Calc_ChargeCode = @ChargeCode AND ";
					@params.Add("@ChargeCode", ProfitLossParent.Filter.ChargeCodeFilter, AccChargeCodeSchema.AC_Code);
				}
				whereClause += " 1 = 1 ";

				string sQL = @"SELECT 
	ZZ_Calc_AC,
	ZZ_Calc_JH,
	ZZ_Calc_GB,
	ZZ_Calc_GE,
	ZZ_Calc_ChargeCodeDescription,
	ZZ_Calc_JobLocalReferenceNum,
	SUM(ZZ_Calc_Revenue_Recognized) + SUM(CASE WHEN ZZ_CALC_AL_TYPE = @REV THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) AS ZZ_Calc_Revenue_Recognized,      
	SUM(ZZ_Calc_WIP_Recognized) AS ZZ_Calc_WIP_Recognized,      
	SUM(ZZ_Calc_Cost_Recognized) + SUM(CASE WHEN ZZ_CALC_AL_TYPE = @CST THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) AS ZZ_Calc_Cost_Recognized,      
	SUM(ZZ_Calc_Accrual_Recognized) AS ZZ_Calc_Accrual_Recognized,     
	SUM(ZZ_Calc_Revenue_Recognized) + SUM(ZZ_Calc_WIP_Recognized) + SUM(ZZ_Calc_Cost_Recognized) + SUM(ZZ_Calc_Accrual_Recognized) + SUM(CASE WHEN ZZ_CALC_AL_TYPE = @REV THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) + SUM(CASE WHEN ZZ_CALC_AL_TYPE = @CST THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) AS ZZ_Calc_LineAmount_Recognized,
	SUM(ZZ_Calc_Revenue_NotRecognized) AS ZZ_Calc_Revenue_NotRecognized,      
	SUM(ZZ_Calc_WIP_NotRecognized) AS ZZ_Calc_WIP_NotRecognized,      
	SUM(ZZ_Calc_Cost_NotRecognized) AS ZZ_Calc_Cost_NotRecognized,      
	SUM(ZZ_Calc_Accrual_NotRecognized) AS ZZ_Calc_Accrual_NotRecognized,     
	SUM(ZZ_Calc_Revenue_NotRecognized) + SUM(ZZ_Calc_WIP_NotRecognized) + SUM(ZZ_Calc_Cost_NotRecognized) + SUM(ZZ_Calc_Accrual_NotRecognized) AS ZZ_Calc_LineAmount_NotRecognized,
	SUM(CASE WHEN ZZ_CALC_AL_TYPE = @REV THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) AS ZZ_Calc_TaxExpenseRevenue,
	SUM(CASE WHEN ZZ_CALC_AL_TYPE = @CST THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END) AS ZZ_Calc_TaxExpenseCost
FROM
	(SELECT
		AL_PK AS ZZ_CALC_AL,
		AL_LineType AS ZZ_CALC_AL_TYPE,
		AL_AC AS ZZ_Calc_AC,
		AL_JH AS ZZ_Calc_JH,
		AL_GB AS ZZ_Calc_GB,
		AL_GE AS ZZ_Calc_GE,
		JH_JobLocalReference AS ZZ_Calc_JobLocalReferenceNum,
		AC_Code AS ZZ_Calc_ChargeCode,
		AC_Desc AS ZZ_Calc_ChargeCodeDescription,
		CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NOT NULL THEN AL_LineAmount ELSE 0 END AS ZZ_Calc_Revenue_Recognized,      
		CASE WHEN AL_LineType = @WIP AND AL_ReverseDate IS NULL THEN -AL_LineAmount ELSE 0 END AS ZZ_Calc_WIP_Recognized,      
		CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NOT NULL THEN AL_LineAmount ELSE 0 END AS ZZ_Calc_Cost_Recognized,      
		CASE WHEN AL_LineType = @ACR AND AL_ReverseDate IS NULL THEN -AL_LineAmount ELSE 0 END AS ZZ_Calc_Accrual_Recognized,
		0 AS ZZ_Calc_Revenue_NotRecognized,      
		0 AS ZZ_Calc_WIP_NotRecognized,      
		0 AS ZZ_Calc_Cost_NotRecognized,      
		0 AS ZZ_Calc_Accrual_NotRecognized

	FROM 
		dbo.AccTransactionLines 
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK AND AC_GC = @CompanyPK
		LEFT JOIN dbo.JobHeader ON AL_JH = JH_PK
	WHERE
		@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'REC')
		AND (AL_ReverseDate IS NOT NULL OR AL_LineType in (@WIP, @ACR))
		AND AL_GC = @CompanyPK

	UNION ALL

	SELECT
		AL_PK AS ZZ_CALC_AL,
		AL_LineType AS ZZ_CALC_AL_TYPE,
		AL_AC AS ZZ_Calc_AC,
		AL_JH AS ZZ_Calc_JH,
		AL_GB AS ZZ_Calc_GB,
		AL_GE AS ZZ_Calc_GE,
		JH_JobLocalReference AS ZZ_Calc_JobLocalReferenceNum,
		AC_Code AS ZZ_Calc_ChargeCode,
		AC_Desc AS ZZ_Calc_ChargeCodeDescription,
		0 AS ZZ_Calc_Revenue_Recognized,      
		0 AS ZZ_Calc_WIP_Recognized,      
		0 AS ZZ_Calc_Cost_Recognized,      
		0 AS ZZ_Calc_Accrual_Recognized,
		CASE WHEN AL_LineType = @REV AND AL_ReverseDate IS NULL THEN AL_LineAmount ELSE 0 END AS ZZ_Calc_Revenue_NotRecognized,      
		0 AS ZZ_Calc_WIP_NotRecognized,      
		CASE WHEN AL_LineType = @CST AND AL_ReverseDate IS NULL THEN AL_LineAmount ELSE 0 END AS ZZ_Calc_Cost_NotRecognized,      
		0 AS ZZ_Calc_Accrual_NotRecognized

	FROM 
		dbo.AccTransactionLines 
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK AND AC_GC = @CompanyPK
		LEFT JOIN dbo.JobHeader ON AL_JH = JH_PK
	WHERE
		@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
		AND AL_ReverseDate IS NULL
		AND AL_LineType in (@REV, @CST)
		AND AL_GC = @CompanyPK

	UNION ALL

	SELECT
		NULL AS ZZ_CALC_AL,
		NULL AS ZZ_CALC_AL_TYPE,
		JR_AC AS ZZ_Calc_AC,
		JR_JH AS ZZ_Calc_JH, 
		JR_GB AS ZZ_Calc_GB, 
		JR_GE AS ZZ_Calc_GE,
		JH_JobLocalReference AS ZZ_Calc_JobLocalReferenceNum,
		AC_Code AS ZZ_Calc_ChargeCode,
		AC_Desc AS ZZ_Calc_ChargeCodeDescription, 
		0 AS ZZ_Calc_Revenue_Recognized,
		0 AS ZZ_Calc_WIP_Recognized,
		0 AS ZZ_Calc_Cost_Recognized,
		0 AS ZZ_Calc_Accrual_Recognized,
		0 AS ZZ_Calc_Revenue_NotRecognized,
		CASE WHEN JR_AL_ARLine IS NULL THEN JR_LocalSellAmt ELSE 0 END AS ZZ_Calc_WIP_NotRecognized,
		0 AS ZZ_Calc_Cost_NotRecognized,
		CASE WHEN JR_AL_APLine IS NULL THEN -JR_LocalCostAmt ELSE 0 END AS ZZ_Calc_Accrual_NotRecognized
	FROM 
		dbo.JobCharge 
		INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC AND AC_GC = @CompanyPK
		LEFT JOIN dbo.JobHeader ON JR_JH = JH_PK
	WHERE
		@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
		AND 
		(
			(JR_LocalSellAmt != 0 AND JR_AL_ARLine IS NULL) 
			OR (JR_LocalCostAmt != 0 AND JR_AL_APLine IS NULL)
		)
		AND JR_GC = @CompanyPK
	) AS RawData
LEFT JOIN
(
	SELECT
		ATP_AL_TransactionLine,
		SUM(ATP_LocalTaxAmount) AS ATP_LocalTaxAmount
	FROM
		dbo.AccTaxRecordTransactionLinePivot
	WHERE
		ATP_IsTaxExpense = 1
	GROUP BY
		ATP_AL_TransactionLine
) TaxExpense ON RawData.ZZ_CALC_AL = TaxExpense.ATP_AL_TransactionLine
WHERE
	ZZ_Calc_JH IN (@JobPKs) 
	[FILTERWHERECLAUSE]
GROUP BY
	ZZ_Calc_AC,
	ZZ_Calc_JH, 
	ZZ_Calc_GB, 
	ZZ_Calc_GE,
	ZZ_Calc_ChargeCodeDescription,
	ZZ_Calc_JobLocalReferenceNum";

				sQL = sQL.Replace("[FILTERWHERECLAUSE]", whereClause);

				if (JobPKs.Length == 1)
				{
					@params.Add(ZSqlParameter.New("@JobPKs", JobPKs.First().ToGuid(), AccTransactionLinesSchema.AL_JH));
				}
				else
				{
					@params.Add(ZSqlParameter.New("@JobPKs", JobPKs.Select(x => x.ToGuid()).ToArray(), AccTransactionLinesSchema.AL_JH, true));
				}
				@params.Add("@CompanyPK", GlbCompany.CurrentCompany.PK.ToGuid(), GlbCompanySchema.PK);
				@params.Add("@WIP", "WIP", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@ACR", "ACR", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@REV", "REV", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@CST", "CST", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@RecognizedCharges", ProfitLossParent.Filter.RecognizedChargesFilter.IsEmpty ? new ZString("ALL") : ProfitLossParent.Filter.RecognizedChargesFilter, AccTransactionLinesSchema.AL_LineType);

				Load(sQL, @params);

				foreach (ProfitLossSummaryDetail detail in this)
				{
					detail.GetCFXAmount = delegate(ZGuid jobPK, ZGuid chargeCodePK, ZGuid branchPK, ZGuid departmentPK)
					{
						ZDecimal result = ZDecimal.Zero;
						Job job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.PK, jobPK));
						if (job != null)
						{
							foreach (Charge charge in job.Charges)
							{
								if (charge.JR_AC == chargeCodePK && charge.JR_GB == branchPK && charge.JR_GE == departmentPK && !charge.JR_AL_APLine.IsValid && !charge.JR_AL_ARLine.IsValid)
								{
									result += charge.JR_CFXAmtReverseSign;
								}
							}
						}
						return result;
					};
				}
			}

			if (Plugin == null && JobPKs.Length > 0)
			{
				if (Job.HasOrphanWIPsorAccrualsForJobs(Factory, JobPKs))
				{
					ProfitLossParent.TotalLineAmountInfo.AddWarning(Res.GetString("4cca37bb-396b-4e2c-be4e-974004abf8d6", "There are WIPs or Accruals linked to this Job which should be reversed but are not. As a result the Profit and Loss figure might not be accurate.\r\nPlease review the costs and revenues entered for this job."));
				}
			}

			ProfitLossParent.RefreshBinding();
		}

		public ZGuid[] JobPKs
		{
			get
			{
				ZGuid[] result = null;

				if (ManualJobPKs != null && ManualJobPKs.Length > 0)
				{
					return ManualJobPKs;
				}
				else if (Plugin != null)
				{
					result = new JobInvoicingDataAccessor(Factory).GetJobsFromShipment(Plugin.CostSupporter.ShipmentsList);
				}

				return result;
			}
		}

		#endregion
	}
}
