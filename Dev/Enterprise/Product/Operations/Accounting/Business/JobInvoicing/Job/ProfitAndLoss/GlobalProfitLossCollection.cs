using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class GlobalProfitLossCollection : ProfitLossCollectionBase
	{
		public GlobalProfitLossCollection(IJobProfitLoss profitLossParent, IJobHeaderParent jobHeaderParent)
			: base(profitLossParent, jobHeaderParent as IJobCostingPlugIn)
		{
			this.JobHeaderParent = jobHeaderParent;
		}

		public GlobalProfitLossCollection(IJobProfitLoss profitLossParent, params ZGuid[] jobPKs)
			: base(profitLossParent, jobPKs)
		{
		}

		#region Load

		protected virtual bool HasRecognizedChargesFilter
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be SQL expression, Hard-coded constant")]
		public override void Load()
		{
			RemoveAll();

			if (JobPKs != null && JobPKs.Length > 0)
			{
				var @params = new ZSqlParameterCollection();
				string whereClause = " WHERE";
				if (ProfitLossParent.Filter.DepartmentFilter.IsValid)
				{
					whereClause += " ZY_Calc_GE = @DepartmentPK AND";
					@params.Add("@DepartmentPK", ProfitLossParent.Filter.DepartmentFilter.ToGuid(), GlbDepartmentSchema.PK);
				}
				if (ProfitLossParent.Filter.BranchFilter.IsValid)
				{
					whereClause += " ZY_Calc_GB = @BranchPK AND";
					@params.Add("@BranchPK", ProfitLossParent.Filter.BranchFilter.ToGuid(), GlbBranchSchema.PK);
				}
				if (!ProfitLossParent.Filter.ChargeCodeFilter.IsEmpty)
				{
					whereClause += " ZY_Calc_AC_Code = @ChargeCode AND ";
					@params.Add("@ChargeCode", ProfitLossParent.Filter.ChargeCodeFilter, AccChargeCodeSchema.AC_Code);
				}
				if (!ProfitLossParent.Filter.JobNumberFilter.IsEmpty)
				{
					whereClause += " ZY_Calc_JH = @JobPK AND ";
					@params.Add("@JobPK", ProfitLossParent.Filter.JobNumberFilter, JobHeaderSchema.PK);
				}

				whereClause += " 1 = 1 ";

				string reversedTransSQL = @"SELECT
	ZY_Calc_JR,
	ZY_Calc_AH,
	ZY_Calc_AL,
	ZY_Calc_Ledger,
	ZY_Calc_TransactionType,
	ZY_Calc_AC,
	ZY_Calc_JH,
	ZY_Calc_JobLocalReferenceNum,
	ZY_Calc_GB,
	ZY_Calc_GE,
	ZY_Calc_GC,
	ZY_Calc_ChargeCodeDescription,
	ZY_Calc_LineAmount,
	ZY_Calc_LineType,
	ZY_Calc_PostDate,
	ZY_Calc_InvoiceDate,
	ZY_Calc_TransactionNum,
	ZY_Calc_FullyPaidDate,
	ZY_Calc_OH,
	CHARINDEX(ZY_Calc_LineType, @ALLTYPES) AS ZY_Calc_ChargeTypeIndex,
	1 AS ZY_Calc_IsReversedWIPAccrual,
	ZY_Calc_RecognizedDate,
	ZY_Calc_LocalCurrency,
	ZY_Calc_RecognitionType,
	ZY_Calc_ConsolNum,
	ZY_Calc_ReversalDate,
	ZY_Calc_SystemCreateTime,
	[COMPANYTOTALSFIELDLIST],
	ZY_Calc_AuditedBy,
	ZY_Calc_APLine,
	ZY_Calc_ARLine
FROM
	(SELECT 
		NULL AS ZY_Calc_JR,
		AL_AH AS 		ZY_Calc_AH,
		AL_PK AS 		ZY_Calc_AL,
		/* StringLiteral */ 'JC' AS ZY_Calc_Ledger,
		AL_LineType AS 		ZY_Calc_TransactionType,
		AL_AC AS 		ZY_Calc_AC,
		AL_JH AS 		ZY_Calc_JH,
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		AL_GB AS 		ZY_Calc_GB,
		AL_GE AS 		ZY_Calc_GE,
		AL_GC AS		ZY_Calc_GC,
		AC_Desc AS 		ZY_Calc_ChargeCodeDescription,
        AC_Code AS      ZY_Calc_AC_Code,
		-AL_LineAmount AS 	ZY_Calc_LineAmount,
		AL_LineType AS 		ZY_Calc_LineType,
		AL_PostDate AS 		ZY_Calc_PostDate,
		AL_PostDate AS 		ZY_Calc_InvoiceDate,
		'' AS 			ZY_Calc_TransactionNum,
		AH_FullyPaidDate AS	ZY_Calc_FullyPaidDate,
		AL_OH AS 		ZY_Calc_OH,
		AL_PostDate AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		AL_RevRecognitionType AS ZY_Calc_RecognitionType,
		CASE WHEN AL_LineType = @ACR THEN JobConsol.JK_UniqueConsignRef 
             ELSE NULL
		END AS ZY_Calc_ConsolNum,
		SL_EventTime AS ZY_Calc_ReversalDate,
		AL_SystemCreateTimeUtc AS ZY_Calc_SystemCreateTime,
		AH_GS_NKAuditedBy AS ZY_Calc_AuditedBy,
		APJobCharge.JR_AL_APLine AS ZY_Calc_APLine,
		ARJobCharge.JR_AL_ARLine AS ZY_Calc_ARLine
	FROM
		dbo.AccTransactionLines
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		INNER JOIN dbo.GLBCompany ON GC_PK = AL_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		LEFT OUTER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
		LEFT JOIN dbo.JobHeader ON AL_JH = JH_PK
		LEFT JOIN FilteredJobCharges APJobCharge ON APJobCharge.JR_AL_APLine = AL_PK
		LEFT JOIN FilteredJobCharges ARJobCharge ON ARJobCharge.JR_AL_ARLine = AL_PK
		LEFT JOIN dbo.JobConsolCost ON APJobCharge.JR_E6 = JobConsolCost.E6_PK
		LEFT JOIN dbo.JobConsol ON JobConsol.JK_PK = JobConsolCost.E6_ParentID
		LEFT JOIN dbo.StmALog ON AL_PK = SL_Parent AND SL_SE_NKEvent = 'REV'
	WHERE 
		AL_LineType IN (@WIP, @ACR)
		AND AL_ReverseDate is not null
		AND AL_JH IN (@JobPKs)
		AND @RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'REC')
		[LINECOMPANYWHERECLAUSE]

	UNION

	SELECT
		NULL AS ZY_Calc_JR,
		AL_AH AS ZY_Calc_AH,
		AL_PK AS ZY_Calc_AL,
		/* StringLiteral */ 'JC' AS ZY_Calc_Ledger,
		AL_LineType AS ZY_Calc_TransactionType,
		AL_AC AS ZY_Calc_AC,
		AL_JH AS ZY_Calc_JH,
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		AL_GB AS ZY_Calc_GB,
		AL_GE AS ZY_Calc_GE,
		AL_GC AS ZY_Calc_GC,
		AC_Desc AS ZY_Calc_ChargeCodeDescription,
        AC_Code AS ZY_Calc_AC_Code,
		AL_LineAmount AS ZY_Calc_LineAmount,
		AL_LineType AS ZY_Calc_LineType,
		AL_ReverseDate AS ZY_Calc_PostDate,
		AL_PostDate AS ZY_Calc_InvoiceDate,
		'' AS ZY_Calc_TransactionNum,
		AH_FullyPaidDate AS ZY_Calc_FullyPaidDate,
		AL_OH AS ZY_Calc_OH,
		AL_ReverseDate AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		AL_RevRecognitionType AS ZY_Calc_RecognitionType,
		CASE WHEN AL_LineType = @ACR THEN JobConsol.JK_UniqueConsignRef 
             ELSE NULL
		END AS ZY_Calc_ConsolNum,
		SL_EventTime AS ZY_Calc_ReversalDate,
		AL_SystemLastEditTimeUtc AS ZY_Calc_SystemCreateTime,
		AH_GS_NKAuditedBy AS ZY_Calc_AuditedBy,
		APJobCharge.JR_AL_APLine AS ZY_Calc_APLine,
		ARJobCharge.JR_AL_ARLine AS ZY_Calc_ARLine
	FROM
		dbo.AccTransactionLines
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		INNER JOIN dbo.GLBCompany ON GC_PK = AL_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		LEFT OUTER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
		LEFT JOIN dbo.JobHeader ON AL_JH = JH_PK
		LEFT JOIN FilteredJobCharges APJobCharge ON APJobCharge.JR_AL_APLine = AL_PK
		LEFT JOIN FilteredJobCharges ARJobCharge ON ARJobCharge.JR_AL_ARLine = AL_PK
		LEFT JOIN dbo.JobConsolCost ON APJobCharge.JR_E6 = JobConsolCost.E6_PK
		LEFT JOIN dbo.JobConsol ON JobConsol.JK_PK = JobConsolCost.E6_ParentID
		LEFT JOIN dbo.StmALog ON AL_PK = SL_Parent AND SL_SE_NKEvent = 'REV'
	WHERE 
		AL_LineType IN (@WIP, @ACR)
		AND AL_ReverseDate is not null
		AND AL_JH IN (@JobPKs)
		AND @RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'REC')
		[LINECOMPANYWHERECLAUSE]
    ) AS ReversedWipsAccruals
	[COMPANYTOTALS]
	[FILTERWHERECLAUSE]";

				string sQL = @"SELECT
	ZY_Calc_JR,
	ZY_Calc_AH,
	ZY_Calc_AL,
	ZY_Calc_Ledger,
	ZY_Calc_TransactionType,
	ZY_Calc_AC,
	ZY_Calc_JH,
	ZY_Calc_JobLocalReferenceNum,
	ZY_Calc_GB,
	ZY_Calc_GE,
	ZY_Calc_GC,
	ZY_Calc_ChargeCodeDescription,
	ZY_Calc_LineAmount,
	ZY_Calc_LineType,
	ZY_Calc_PostDate,
	ZY_Calc_InvoiceDate,
	ZY_Calc_TransactionNum,
	ZY_Calc_FullyPaidDate,
	ZY_Calc_OH,
	CHARINDEX(ZY_Calc_LineType, @ALLTYPES) AS ZY_Calc_ChargeTypeIndex,
	ZY_Calc_IsReversedWIPAccrual,
	ZY_Calc_RecognizedDate,
	ZY_Calc_LocalCurrency,
	ZY_Calc_RecognitionType,
	ZY_Calc_ConsolNum,
	ZY_Calc_ReversalDate,
	ZY_Calc_SystemCreateTime,
	[COMPANYTOTALSFIELDLIST],
	ZY_Calc_AuditedBy,
	ZY_Calc_APLine,
	ZY_Calc_ARLine
FROM
	(SELECT
		NULL AS ZY_Calc_JR,
		AL_AH AS ZY_Calc_AH,
		AL_PK AS ZY_Calc_AL,
		CASE 
			WHEN AL_AH IS NULL THEN CASE WHEN AL_LineType IN (@ACR, @WIP) THEN /* StringLiteral */ 'JC' ELSE '' END
			ELSE AH_Ledger 
		END AS ZY_Calc_Ledger,
		CASE WHEN AL_AH IS NULL THEN AL_LineType ELSE AH_TransactionType END AS ZY_Calc_TransactionType,
		AL_AC AS ZY_Calc_AC,
		AL_JH AS ZY_Calc_JH,
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		AL_GB AS ZY_Calc_GB,
		AL_GE AS ZY_Calc_GE,
		AL_GC AS ZY_Calc_GC,
		AC_Desc AS ZY_Calc_ChargeCodeDescription,
		AC_Code AS ZY_Calc_AC_Code,
		CASE WHEN AL_LineType IN (@ACR, @WIP) THEN -AL_LineAmount ELSE AL_LineAmount END AS ZY_Calc_LineAmount,
		AL_LineType AS ZY_Calc_LineType,
		CASE WHEN AL_AH IS NULL THEN AL_PostDate ELSE AH_PostDate END AS ZY_Calc_PostDate,
		CASE WHEN AL_AH IS NULL THEN AL_PostDate ELSE AH_InvoiceDate END AS ZY_Calc_InvoiceDate,
		CASE WHEN AL_AH IS NULL THEN '' ELSE AH_TransactionNum END AS ZY_Calc_TransactionNum,
		AH_FullyPaidDate AS ZY_Calc_FullyPaidDate,
		CASE WHEN AL_OH IS NULL THEN AH_OH ELSE AL_OH END AS ZY_Calc_OH,
		CASE WHEN AL_LineType IN (@CST, @REV) THEN AH_IsCancelled ELSE 0 END AS ZY_Calc_IsReversedWIPAccrual,
		CASE WHEN AL_LineType IN (@CST, @REV) THEN AL_ReverseDate ELSE AL_PostDate END AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		AL_RevRecognitionType AS ZY_Calc_RecognitionType,
		CASE WHEN AL_LineType IN (@ACR, @CST) THEN JobConsol.JK_UniqueConsignRef 
             ELSE NULL
		END AS ZY_Calc_ConsolNum,
		SL_EventTime AS ZY_Calc_ReversalDate,
		AL_SystemCreateTimeUtc AS ZY_Calc_SystemCreateTime,
		AH_GS_NKAuditedBy AS ZY_Calc_AuditedBy,
		APJobCharge.JR_AL_APLine AS ZY_Calc_APLine,
		ARJobCharge.JR_AL_ARLine AS ZY_Calc_ARLine
	FROM 
		dbo.AccTransactionLines 
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		INNER JOIN dbo.GLBCompany ON GC_PK = AL_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		LEFT OUTER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
		LEFT JOIN dbo.JobHeader ON AL_JH = JH_PK
		LEFT JOIN FilteredJobCharges APJobCharge ON APJobCharge.JR_AL_APLine = AL_PK
		LEFT JOIN FilteredJobCharges ARJobCharge ON ARJobCharge.JR_AL_ARLine = AL_PK
		LEFT JOIN dbo.JobConsolCost ON APJobCharge.JR_E6 = JobConsolCost.E6_PK
		LEFT JOIN dbo.JobConsol ON JobConsol.JK_PK = JobConsolCost.E6_ParentID
		LEFT JOIN dbo.StmALog ON AL_PK = SL_Parent AND SL_SE_NKEvent = 'REV'
	WHERE
		AL_JH IN (@JobPKs)
		[LINECOMPANYWHERECLAUSE]
		AND 
		(
			(
				AL_LineType IN (@CST, @REV) AND AL_ReverseDate IS NOT NULL
			)
			OR
			(
				AL_LineType IN (@ACR, @WIP)
				AND AL_ReverseDate IS NULL
			)
		)	
		AND @RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'REC')

	UNION ALL

SELECT
		NULL AS ZY_Calc_JR,
		AL_AH AS ZY_Calc_AH,
		AL_PK AS ZY_Calc_AL,
		CASE 
			WHEN AL_AH IS NULL THEN CASE WHEN AL_LineType IN (@ACR, @WIP) THEN /* StringLiteral */ 'JC' ELSE '' END
			ELSE AH_Ledger 
		END AS ZY_Calc_Ledger,
		CASE WHEN AL_AH IS NULL THEN AL_LineType ELSE AH_TransactionType END AS ZY_Calc_TransactionType,
		AL_AC AS ZY_Calc_AC,
		AL_JH AS ZY_Calc_JH,
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		AL_GB AS ZY_Calc_GB,
		AL_GE AS ZY_Calc_GE,
		AL_GC AS ZY_Calc_GC,
		AC_Desc AS ZY_Calc_ChargeCodeDescription,
		AC_Code AS ZY_Calc_AC_Code,
		CASE WHEN AL_LineType IN (@ACR, @WIP) THEN -AL_LineAmount ELSE AL_LineAmount END AS ZY_Calc_LineAmount,
		AL_LineType AS ZY_Calc_LineType,
		CASE WHEN AL_AH IS NULL THEN AL_PostDate ELSE AH_PostDate END AS ZY_Calc_PostDate,
		CASE WHEN AL_AH IS NULL THEN AL_PostDate ELSE AH_InvoiceDate END AS ZY_Calc_InvoiceDate,
		CASE WHEN AL_AH IS NULL THEN '' ELSE AH_TransactionNum END AS ZY_Calc_TransactionNum,
		AH_FullyPaidDate AS ZY_Calc_FullyPaidDate,
		CASE WHEN AL_OH IS NULL THEN AH_OH ELSE AL_OH END AS ZY_Calc_OH,
		CASE WHEN AL_LineType IN (@CST, @REV) THEN AH_IsCancelled ELSE 0 END AS ZY_Calc_IsReversedWIPAccrual,
		CASE WHEN AL_LineType IN (@CST, @REV) THEN AL_ReverseDate ELSE AL_PostDate END AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		AL_RevRecognitionType AS ZY_Calc_RecognitionType,
		CASE WHEN AL_LineType = @CST THEN JobConsol.JK_UniqueConsignRef 
             ELSE NULL
		END AS ZY_Calc_ConsolNum,
		SL_EventTime AS ZY_Calc_ReversalDate,
		AL_SystemCreateTimeUtc AS ZY_Calc_SystemCreateTime,
		AH_GS_NKAuditedBy AS ZY_Calc_AuditedBy,
		APJobCharge.JR_AL_APLine AS ZY_Calc_APLine,
		ARJobCharge.JR_AL_ARLine AS ZY_Calc_ARLine
	FROM 
		dbo.AccTransactionLines 
		INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		INNER JOIN dbo.GLBCompany ON GC_PK = AL_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		INNER JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
		INNER JOIN dbo.JobHeader ON AL_JH = JH_PK
		LEFT JOIN FilteredJobCharges APJobCharge ON APJobCharge.JR_AL_APLine = AL_PK
		LEFT JOIN FilteredJobCharges ARJobCharge ON ARJobCharge.JR_AL_ARLine = AL_PK
		LEFT JOIN dbo.JobConsolCost ON APJobCharge.JR_E6 = JobConsolCost.E6_PK
		LEFT JOIN dbo.JobConsol ON JobConsol.JK_PK = JobConsolCost.E6_ParentID
		LEFT JOIN dbo.StmALog ON AL_PK = SL_Parent AND SL_SE_NKEvent = 'REV'
	WHERE
		AL_JH IN (@JobPKs)
		[LINECOMPANYWHERECLAUSE]
		AND (AL_LineType IN (@CST, @REV) AND AL_ReverseDate IS NULL)	
		AND @RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')

UNION ALL

	SELECT
		JR_PK AS ZY_Calc_JR,
		NULL AS ZY_Calc_AH,
		NULL AS ZY_Calc_AL,
		NULL AS ZY_Calc_Ledger,
		NULL AS ZY_Calc_TransactionType,
		JR_AC AS ZY_Calc_AC,
		JR_JH AS ZY_Calc_JH, 
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		JR_GB AS ZY_Calc_GB, 
		JR_GE AS ZY_Calc_GE,
		JR_GC AS ZY_Calc_GC,
		AC_Desc AS ZY_Calc_ChargeCodeDescription, 
		AC_Code AS ZY_Calc_ChargeCode,
		JR_LocalSellAmt AS ZY_Calc_LineAmount,
		@WIP AS ZY_Calc_LineType,
		NULL AS ZY_Calc_PostDate,
		NULL AS ZY_Calc_InvoiceDate,
		NULL AS ZY_Calc_TransactionNum,
		NULL AS ZY_Calc_FullyPaidDate,
		JR_OH_SellAccount AS ZY_Calc_OH,
		0 AS ZY_Calc_IsReversedWIPAccrual,
		NULL AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		NULL AS ZY_Calc_RecognitionType,
		NULL AS ZY_Calc_ConsolNum,
		NULL AS ZY_Calc_ReversalDate,
		NULL AS ZY_Calc_SystemCreateTime,
		NULL AS ZY_Calc_AuditedBy,
		JR_AL_APLine AS ZY_Calc_APLine,
		JR_AL_ARLine AS ZY_Calc_ARLine
	FROM 
		dbo.JobCharge 
		INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC
		INNER JOIN dbo.GLBCompany ON GC_PK = JR_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		LEFT JOIN dbo.JobHeader ON JR_JH = JH_PK
	WHERE
		@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
		AND (JR_LocalSellAmt != 0 AND JR_AL_ARLine IS NULL)
		AND JR_JH IN (@JobPKs)
		[CHARGECOMPANYWHERECLAUSE]

	UNION ALL

	SELECT
		JR_PK AS ZY_Calc_JR,
		NULL AS ZY_Calc_AH,
		NULL AS ZY_Calc_AL,
		NULL AS ZY_Calc_Ledger,
		NULL AS ZY_Calc_TransactionType,
		JR_AC AS ZY_Calc_AC,
		JR_JH AS ZY_Calc_JH, 
		JH_JobLocalReference  AS ZY_Calc_JobLocalReferenceNum,
		JR_GB AS ZY_Calc_GB, 
		JR_GE AS ZY_Calc_GE,
		JR_GC AS ZY_Calc_GC,
		AC_Desc AS ZY_Calc_ChargeCodeDescription, 
		AC_Code AS ZY_Calc_ChargeCode,
		-JR_LocalCostAmt AS ZY_Calc_LineAmount,
		@ACR AS ZY_Calc_LineType,
		NULL AS ZY_Calc_PostDate,
		NULL AS ZY_Calc_InvoiceDate,
		NULL AS ZY_Calc_TransactionNum,
		NULL AS ZY_Calc_FullyPaidDate,
		JR_OH_CostAccount AS ZY_Calc_OH,
		0 AS ZY_Calc_IsReversedWIPAccrual,
		NULL AS ZY_Calc_RecognizedDate,
		RX_Code AS ZY_Calc_LocalCurrency,
		NULL AS ZY_Calc_RecognitionType,
		JobConsol.JK_UniqueConsignRef AS ZY_Calc_ConsolNum,
		NULL AS ZY_Calc_ReversalDate,
		NULL AS ZY_Calc_SystemCreateTime,
		NULL AS ZY_Calc_AuditedBy,
		JR_AL_APLine AS ZY_Calc_APLine,
		JR_AL_ARLine AS ZY_Calc_ARLine
	FROM 
		dbo.JobCharge
		INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC
		INNER JOIN dbo.GLBCompany ON GC_PK = JR_GC
		INNER JOIN dbo.RefCurrency ON GC_RX_NKLocalCurrency = RX_Code
		LEFT JOIN dbo.JobHeader ON JR_JH = JH_PK
		LEFT JOIN dbo.JobConsolCost ON JobCharge.JR_E6 = JobConsolCost.E6_PK
		LEFT JOIN dbo.JobConsol ON JobConsol.JK_PK = JobConsolCost.E6_ParentID
	WHERE
		@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
		AND (JR_LocalCostAmt != 0 AND JR_AL_APLine IS NULL)
		AND JR_JH IN (@JobPKs)
		[CHARGECOMPANYWHERECLAUSE]
	) AS UnreversedTransactions
	[COMPANYTOTALS]
	[FILTERWHERECLAUSE]
	[REVERSEDTRANS]";

				if (ProfitLossParent.Filter.ShowReversedFilter)
				{
					sQL = sQL.Replace("[REVERSEDTRANS]", "UNION " + reversedTransSQL);
				}
				else
				{
					sQL = sQL.Replace("[REVERSEDTRANS]", "");
				}

				sQL = sQL.Replace("[COMPANYTOTALSFIELDLIST]", CompanyTotalsFieldListSQL);
				sQL = sQL.Replace("[COMPANYTOTALS]", CompanyTotalsSQL);

				sQL = sQL.Replace("[FILTERWHERECLAUSE]", whereClause);

				if (CompanyFilter.IsValid)
				{
					sQL = sQL.Replace("[LINECOMPANYWHERECLAUSE]", @"AND AL_GC = @CompanyPK
						AND AC_GC = @CompanyPK");
					sQL = sQL.Replace("[CHARGECOMPANYWHERECLAUSE]", @"AND JR_GC = @CompanyPK
						AND AC_GC = @CompanyPK");
					@params.Add("@CompanyPK", CompanyFilter, GlbCompanySchema.PK);
				}
				else
				{
					sQL = sQL.Replace("[LINECOMPANYWHERECLAUSE]", String.Empty);
					sQL = sQL.Replace("[CHARGECOMPANYWHERECLAUSE]", String.Empty);
				}

				var orderByClause = @"ORDER BY 
ZY_Calc_AC, 
ZY_Calc_GB, 
ZY_Calc_GE, 
ZY_Calc_ChargeTypeIndex, 
ZY_Calc_LineAmount";

				var taxExpenseSubQuery = @"CROSS APPLY
(SELECT
SUM(ATP_LocalTaxAmount) AS ATP_LocalTaxAmount, ATT_GB, ATT_GE_Department, ATT_PostDate, ATT_RealisationDate
FROM
dbo.AccTaxRecordTransactionLinePivot inner join dbo.AccTaxTransaction on ATP_ATT = ATT_PK
WHERE
ATP_IsTaxExpense = 1
AND ATP_AL_TransactionLine = ZY_Calc_AL
GROUP BY
ATP_AL_TransactionLine, ATT_GB, ATT_GE_Department, ATT_PostDate, ATT_RealisationDate
UNION ALL
SELECT NULL, NULL, NULL, NULL, NULL) TaxExpense";

				sQL = $@"
WITH FilteredJobCharges AS
(
	SELECT JR_AL_APLine,JR_AL_ARLine,JR_E6 FROM dbo.JobCharge WHERE JR_JH IN (@JobPKs)
)

SELECT
ZY_Calc_JR,
ZY_Calc_AH,
ZY_Calc_AL,
ZY_Calc_Ledger,
ZY_Calc_TransactionType,
ZY_Calc_AC,
ZY_Calc_JH,
ZY_Calc_JobLocalReferenceNum,
ISNULL(TaxExpense.ATT_GB, ZY_Calc_GB) AS ZY_Calc_GB,
ISNULL(TaxExpense.ATT_GE_Department, ZY_Calc_GE) AS ZY_Calc_GE,
ZY_Calc_GC,
CASE WHEN TaxExpense.ATP_LocalTaxAmount IS NULL THEN ZY_Calc_ChargeCodeDescription ELSE 'Tax Expense' END AS ZY_Calc_ChargeCodeDescription,
ISNULL(TaxExpense.ATP_LocalTaxAmount, ZY_Calc_LineAmount) AS ZY_Calc_LineAmount,
ZY_Calc_LineType,
CASE WHEN TaxExpense.ATT_PostDate IS NULL THEN ZY_Calc_PostDate ELSE TaxExpense.ATT_PostDate END AS ZY_Calc_PostDate,
ZY_Calc_InvoiceDate,
ZY_Calc_TransactionNum,
ZY_Calc_FullyPaidDate,
ZY_Calc_OH,
ZY_Calc_ChargeTypeIndex,
ZY_Calc_IsReversedWIPAccrual,
CASE WHEN TaxExpense.ATT_RealisationDate IS NULL THEN ZY_Calc_RecognizedDate ELSE TaxExpense.ATT_RealisationDate END AS ZY_Calc_RecognizedDate,
ZY_Calc_LocalCurrency,
ZY_Calc_RecognitionType,
ZY_Calc_ConsolNum,
ZY_Calc_ReversalDate,
ZY_Calc_SystemCreateTime,
{CompanyTotalsFieldListSQL},
ZY_Calc_AuditedBy,
ZY_Calc_APLine,
ZY_Calc_ARLine
FROM ({sQL}) AS ProfitLoss
{taxExpenseSubQuery}
{orderByClause}";

				if (JobPKs.Length == 1)
				{
					@params.Add(ZSqlParameter.New("@JobPKs", JobPKs.First().ToGuid(), AccTransactionLinesSchema.AL_JH));
				}
				else
				{
					@params.Add(ZSqlParameter.New("@JobPKs", JobPKs.Select(x => x.ToGuid()).ToArray(), AccTransactionLinesSchema.AL_JH, true));
				}
				@params.Add("@WIP", "WIP", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@ACR", "ACR", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@REV", "REV", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@CST", "CST", AccTransactionLinesSchema.AL_LineType);
				@params.Add("@ALLTYPES", "REV WIP CST ACR", Schema.GenericStringSchemaColumn);
				@params.Add("@RecognizedCharges", !HasRecognizedChargesFilter || ProfitLossParent.Filter.RecognizedChargesFilter.IsEmpty ? new ZString("ALL") : ProfitLossParent.Filter.RecognizedChargesFilter, AccTransactionLinesSchema.AL_LineType);

				Load(sQL, @params);

				foreach (ProfitLossDetail detail in this)
				{
					detail.GetCFXAmount = delegate(ZGuid chargePK, ZString lineType, ZDateTime recognizedDate)
											{
												ZDecimal result = ZDecimal.Zero;

												if (lineType == TransactionLineTypes.WIP && recognizedDate.IsEmpty)
												{
													var charge = ReadOnlyFactory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.PK, chargePK));
													if (charge != null)
													{
														result = charge.JR_CFXAmtReverseSign;
													}
												}

												return result;
											};
				}
			}

			ProfitLossParent.RefreshBinding();
		}

		public virtual ZGuid[] JobPKs
		{
			get
			{
				ZGuid[] result = null;

				if (JobHeaderParent != null)
				{
					result = new JobInvoicingDataAccessor(Factory).GetJobsFromShipment(new IJobHeaderParent[] { JobHeaderParent }, false);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		protected virtual ZGuid CompanyFilter
		{
			get { return ProfitLossParent.Filter.CompanyFilter; }
		}

		protected virtual string CompanyTotalsSQL
		{
			get
			{
				return @"LEFT JOIN 
	(SELECT
		ZY_Calc_GC AS ZX_Calc_GC,
		SUM(CASE WHEN ZY_Calc_LineType = @REV THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Calc_TaxExpenseRevenue) AS TotalRevenue,
		SUM(CASE WHEN ZY_Calc_LineType = @WIP THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Wip) AS TotalWIP,
		SUM(CASE WHEN ZY_Calc_LineType = @CST THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Calc_TaxExpenseCost) AS TotalCost,
		SUM(CASE WHEN ZY_Calc_LineType = @ACR THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Acr) AS TotalAccrual,
		SUM(ZY_Calc_LineAmount) + SUM(ZY_Wip) + SUM(ZY_Acr) + SUM(ZY_Calc_TaxExpenseRevenue) + SUM(ZY_Calc_TaxExpenseCost) AS TotalLineAmount,

		SUM(CASE WHEN ZY_Calc_LineType = @REV AND ZY_Calc_ReverseDate IS NOT NULL THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Calc_TaxExpenseRevenue) AS TotalRevenueRecognized,
		SUM(CASE WHEN ZY_Calc_LineType = @WIP AND ZY_Calc_ReverseDate IS NULL THEN ZY_Calc_LineAmount ELSE 0 END) AS TotalWIPRecognized,
		SUM(CASE WHEN ZY_Calc_LineType = @CST AND ZY_Calc_ReverseDate IS NOT NULL THEN ZY_Calc_LineAmount ELSE 0 END) + SUM(ZY_Calc_TaxExpenseCost) AS TotalCostRecognized,
		SUM(CASE WHEN ZY_Calc_LineType = @ACR AND ZY_Calc_ReverseDate IS NULL THEN ZY_Calc_LineAmount ELSE 0 END) AS TotalAccrualRecognized,
		
		SUM(CASE WHEN ZY_Calc_LineType = @REV AND ZY_Calc_ReverseDate IS NULL THEN ZY_Calc_LineAmount ELSE 0 END) AS TotalRevenueNotRecognized,
		SUM(ZY_Wip) AS TotalWIPNotRecognized, 
		SUM(CASE WHEN ZY_Calc_LineType = @CST AND ZY_Calc_ReverseDate IS NULL THEN ZY_Calc_LineAmount ELSE 0 END) AS TotalCostNotRecognized,
		SUM(ZY_Acr) AS TotalAccrualNotRecognized,

		SUM(ZY_Calc_TaxExpenseRevenue) AS TotalTaxExpenseRevenue,
		SUM(ZY_Calc_TaxExpenseCost) AS TotalTaxExpenseCost

		FROM
			(SELECT
				AL_JH AS ZY_Calc_JH,
				AL_GB AS ZY_Calc_GB,
				AL_GE AS ZY_Calc_GE,
				AL_GC AS ZY_Calc_GC,
				AC_Code AS ZY_Calc_AC_Code,
				CASE WHEN AL_LineType in (@ACR, @WIP) THEN -AL_LineAmount ELSE AL_LineAmount END AS ZY_Calc_LineAmount,
				AL_LineType AS ZY_Calc_LineType,
				AL_ReverseDate AS ZY_Calc_ReverseDate,
				0 AS ZY_Wip,
				0 AS ZY_Acr,
				CASE WHEN AL_LineType = @REV THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END AS ZY_Calc_TaxExpenseRevenue,
				CASE WHEN AL_LineType = @CST THEN ISNULL(TaxExpense.ATP_LocalTaxAmount, 0) ELSE 0 END AS ZY_Calc_TaxExpenseCost
			FROM 
				dbo.AccTransactionLines 
				INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
				LEFT JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
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
				) TaxExpense ON AL_PK = TaxExpense.ATP_AL_TransactionLine
			WHERE
				AL_JH IN (@JobPKs)
				[LINECOMPANYWHERECLAUSE]
				AND 
				(
					(
						AL_LineType in (@CST, @REV)
					)
					OR
					(
						AL_LineType in (@ACR, @WIP)
						AND AL_ReverseDate IS NULL
					)
				)

			UNION ALL 

			SELECT
				JR_JH AS ZY_Calc_JH, 
				JR_GB AS ZY_Calc_GB, 
				JR_GE AS ZY_Calc_GE,
				JR_GC AS ZY_Calc_GC,
				AC_Code AS ZY_Calc_AC_Code,
				0 AS ZY_Calc_LineAmount, 
				@WIP AS ZY_Calc_LineType,
				NULL AS ZY_Calc_ReverseDate,
				JR_LocalSellAmt AS ZY_Wip,
				0 AS ZY_Acr,
				0 AS ZY_Calc_TaxExpenseRevenue,
				0 AS ZY_Calc_TaxExpenseCost
			FROM 
				dbo.JobCharge 
				INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC
			WHERE
				@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
				AND (JR_LocalSellAmt != 0 AND JR_AL_ARLine IS NULL)
				AND JR_JH IN (@JobPKs)
				[CHARGECOMPANYWHERECLAUSE]

			UNION ALL

			SELECT
				JR_JH AS ZY_Calc_JH, 
				JR_GB AS ZY_Calc_GB, 
				JR_GE AS ZY_Calc_GE,
				JR_GC AS ZY_Calc_GC,
				AC_Code AS ZY_Calc_AC_Code,
				0 AS ZY_Calc_LineAmount,
				@ACR AS ZY_Calc_LineType,
				NULL AS ZY_Calc_ReverseDate,
				0 AS ZY_Wip,
				- JR_LocalCostAmt  AS ZY_Acr,
				0 AS ZY_Calc_TaxExpenseRevenue,
				0 AS ZY_Calc_TaxExpenseCost
			FROM 
				dbo.JobCharge 
				INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC
			WHERE
				@RecognizedCharges IN (/* StringLiteral */ 'ALL', /* StringLiteral */ 'NRC')
				AND (JR_LocalCostAmt != 0 AND JR_AL_APLine IS NULL)
				AND JR_JH IN (@JobPKs)
				[CHARGECOMPANYWHERECLAUSE]
			
	
			) AS CompanyDetails
			[FILTERWHERECLAUSE]
			GROUP BY ZY_Calc_GC) AS CompanyTotals
			ON ZX_Calc_GC = ZY_Calc_GC";
			}
		}

		protected virtual string CompanyTotalsFieldListSQL
		{
			get
			{
				return @"TotalRevenue, 
						TotalWIP, 
						TotalCost, 
						TotalAccrual, 
						TotalLineAmount,
						TotalRevenueRecognized,
						TotalWIPRecognized,
						TotalCostRecognized,
						TotalAccrualRecognized,
						TotalRevenueNotRecognized,
						TotalWIPNotRecognized, 
						TotalCostNotRecognized,
						TotalAccrualNotRecognized,
						TotalTaxExpenseRevenue,
						TotalTaxExpenseCost";
			}
		}

		protected IJobHeaderParent JobHeaderParent { get; set; }

		#endregion

		readonly ReadOnlyBusinessObjectFactory ReadOnlyFactory = new ReadOnlyBusinessObjectFactory();
	}
}
