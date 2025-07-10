namespace Enterprise.DataPurge
{
	class CompanySpecificScriptRepository : ScriptRepository
	{
		// Before modifying this array, please read Wiki https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/17711/How-to-register-data-purge-when-add-a-new-Accounting-Data-Table
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[]
				{
					AccGeneralLedgerDataScript,
					AccDraftInvoiceHeaderScript,
					AccTaxGLMovementScript,
					AccTaxTransactionScript,
					GenApprovalRequestScript,
					AccConsolidationBatchScript,
					AccGLAggregateScript,
					AccEPaymentDealScript,
					AccEPaymentQuoteScript,
					AccPaymentApprovalItemScript,
					AccPaymentApprovalScript,
					JobHeaderScript,
					WorkItemsScript,
				};
			}
		}

		#region SuppressResourceStringsCheckRegion

		#region Purge Scripts

		#region AccComplianceSequence

		const string SelectPKFromAccComplianceSequence = @"
SELECT XD_PK FROM dbo.AccComplianceSequence
	WHERE
		XD_GC_Company = {0}
		OR XD_GB_BranchOwner IN (" + SelectPKFromGlbBranch + @")
";

		#endregion

		#region AccGeneralLedgerData

		const string AccGeneralLedgerDataScript = @"
--AccGeneralLedgerData
DELETE dbo.AccGeneralLedgerData
WHERE GLD_GC_Company = {0}
OR GLD_ATM_TaxGLMovement IN (" + SelectPKFromAccTaxGLMovement + @")
OR GLD_YC_CashBasisVAT IN(" + SelectPKFromAccCashBasisVAT + @")
OR GLD_AH_TransactionHeader IN(" + SelectPKFromAccTransactionHeader + @")
OR GLD_AL_TransactionLine IN(" + SelectPKFromAccTransactionLines + @")
OR GLD_GB_Branch IN(" + SelectPKFromGlbBranch + @")
OR GLD_GB_TaxBranch IN(" + SelectPKFromGlbBranch + @")
";

		const string SelectPKFromAccCashBasisVAT = @"
SELECT YC_PK FROM dbo.AccCashBasisVAT 
	WHERE
		YC_GC = {0}
		OR YC_AL_TransactionLine IN (" + SelectPKFromAccTransactionLines + @") 
";

		#endregion

		#region AccDraftInvoiceHeader

		const string AccDraftInvoiceHeaderScript = @"
--AccDraftInvoiceJobReference
DELETE dbo.AccDraftInvoiceJobReference
WHERE AIR_GC_Company = {0}
	OR AIR_AIH_Header IN (" + SelectPKFromAccDraftInvoiceHeader + @")
	OR AIR_AIJ_Job IN (" + SelectPKFromAccDraftInvoiceJob + @")

--AccDraftInvoiceJob
DELETE dbo.AccDraftInvoiceJob
WHERE AIJ_GC_Company = {0}
	OR AIJ_PK IN (" + SelectPKFromAccDraftInvoiceJob + @")

--AccDraftInvoiceJobCluster
DELETE dbo.AccDraftInvoiceJobCluster
WHERE AIC_GC_Company = {0}
	OR AIC_PK IN (" + SelectPKFromAccDraftInvoiceJobCluster + @")

--AccDraftInvoiceProcessingErrorLog
DELETE dbo.AccDraftInvoiceProcessingErrorLog
WHERE AIL_AIH_DraftInvoice IN (" + SelectPKFromAccDraftInvoiceHeader + @")

--AccDraftInvoiceExRate
DELETE dbo.AccDraftInvoiceExRate
WHERE AIE_AIH_Header IN (" + SelectPKFromAccDraftInvoiceHeader + @")

--AccDraftInvoiceHeader
DELETE dbo.AccDraftInvoiceHeader
WHERE AIH_PK IN (" + SelectPKFromAccDraftInvoiceHeader + @")
";

		const string SelectPKFromAccDraftInvoiceHeader = @"
SELECT AIH_PK FROM dbo.AccDraftInvoiceHeader
	WHERE 
		AIH_GC_Company = {0}
		OR AIH_GB_Branch IN (" + SelectPKFromGlbBranch + @")
		OR AIH_AH_PostedTransactionHeader IN (" + SelectPKFromAccTransactionHeader + @")
		OR AIH_AH_OriginalTransaction IN (" + SelectPKFromAccTransactionHeader + @")
";

		const string SelectPKFromAccDraftInvoiceJobCluster = @"
SELECT AIC_PK FROM dbo.AccDraftInvoiceJobCluster
	WHERE 
		AIC_GC_Company = {0}
		OR AIC_AIH_Header IN (" + SelectPKFromAccDraftInvoiceHeader + @")
";

		const string SelectPKFromAccDraftInvoiceJob = @"
SELECT AIJ_PK FROM dbo.AccDraftInvoiceJob
	WHERE 
		AIJ_GC_Company = {0}
		OR AIJ_AIC_Cluster IN (" + SelectPKFromAccDraftInvoiceJobCluster + @")
";

		#endregion

		#region AccTaxGLMovement

		const string AccTaxGLMovementScript = @"
--AccTaxGLMovementQueue
DELETE dbo.AccTaxGLMovementQueue
WHERE ATQ_ATM IN (" + SelectPKFromAccTaxGLMovement + @")

--AccTaxGLMovement
DELETE dbo.AccTaxGLMovement
WHERE ATM_PK IN (" + SelectPKFromAccTaxGLMovement + @")
";

		const string SelectPKFromAccTaxGLMovement = @"
SELECT ATM_PK FROM dbo.AccTaxGLMovement 
	WHERE
		ATM_ATT_TaxTransaction IN (" + SelectPKFromAccTaxTransaction + @")
";

		#endregion

		#region AccTaxTransaction

		const string AccTaxTransactionScript = @"
--AccTaxRecordTransactionLinePivot
DELETE dbo.AccTaxRecordTransactionLinePivot
WHERE ATP_AL_TransactionLine IN (" + SelectPKFromAccTransactionLines + @")
OR	ATP_ATT IN (" + SelectPKFromAccTaxTransaction + @")

--AccTaxTransaction
DELETE dbo.AccTaxTransaction
WHERE ATT_PK IN (" + SelectPKFromAccTaxTransaction + @")";

		const string SelectPKFromAccTaxTransaction = @"
SELECT ATT_PK FROM dbo.AccTaxTransaction 
	WHERE
		ATT_AH IN (" + SelectPKFromAccTransactionHeader + @")
		OR ATT_AH_MatchTransaction IN (" + SelectPKFromAccTransactionHeader + @")
		OR ATT_GC = {0}
";

		#endregion

		#region GenApprovalRequest

		const string GenApprovalRequestScript = @"
--GenApprovalRequest
DELETE dbo.GenApprovalRequest
WHERE XP_GB_RequestingBranch IN (" + SelectPKFromGlbBranch + ")";

		#endregion

		#region AccConsolidationBatch

		const string AccConsolidationBatchScript = @"
--AccConsolidationBatch
DELETE dbo.AccConsolidationBatch
  WHERE YB_GC_Company = {0}
				OR YB_AH_EliminationJournal IN (" + SelectPKFromAccTransactionHeader + ")";

		#endregion

		#region AccGLAggregate

		const string AccGLAggregateScript = @"
--AccGLAggregate
DELETE dbo.AccGLAggregate
  WHERE AA_GB in (" + SelectPKFromGlbBranch + @")";

		#endregion

		#region AccEPaymentDeal

		const string AccEPaymentDealScript = @"
--AccEPaymentDeal
DELETE dbo.AccEPaymentDeal WHERE AED_GC_Company = {0}";

		#endregion

		#region AccEPaymentQuote

		const string AccEPaymentQuoteScript = @"
--AccEPaymentQuote
DELETE dbo.AccEPaymentDeal
  WHERE AED_QU_Quote IN (SELECT QU_PK
							FROM dbo.AccEPaymentQuote WHERE QU_GC = {0})

DELETE dbo.AccEPaymentQuote WHERE QU_GC = {0}";

		#endregion

		#region AccPaymentApprovalItem

		const string AccPaymentApprovalItemScript = @"
--AccPaymentApprovalItem
DELETE dbo.AccPaymentApprovalItem
  WHERE A2_AV in (SELECT AV_PK FROM dbo.AccPaymentApproval 
                            WHERE AV_GB IN (" + SelectPKFromGlbBranch + @"))
";

		#endregion

		#region AccPaymentApproval

		const string AccPaymentApprovalScript = @"
--AccPaymentApproval
DELETE dbo.AccEPaymentDeal
  WHERE AED_QU_Quote IN (SELECT QU_PK
							FROM dbo.AccEPaymentQuote
							WHERE QU_AV IN (SELECT AV_PK
												FROM dbo.AccPaymentApproval
												WHERE AV_GC = {0}))

DELETE dbo.AccEPaymentQuote
  WHERE QU_AV IN (SELECT AV_PK
					FROM dbo.AccPaymentApproval
					WHERE AV_GC = {0})

DELETE dbo.AccPaymentApproval WHERE AV_GC = {0}";

		#endregion

		#region JobHeader

		#region Select PK From Tables Related to JobHeader

		const string SelectPKFromJobHeader = @"
SELECT JH_PK FROM dbo.JobHeader 
	WHERE
		JH_GC = {0}
		OR JH_GB IN (" + SelectPKFromGlbBranch + @")
";

		const string SelectPKFromJobConsolCost = @"
SELECT E6_PK FROM dbo.JobConsolCost
	WHERE
		E6_GC = {0}
		OR E6_AH_APInvoice IN (" + SelectPKFromAccTransactionHeader + @")
		OR E6_AH_ARInvoice IN (" + SelectPKFromAccTransactionHeader + @")
";

		const string SelectPKFromAccTransactionLines = @"
SELECT AL_PK FROM dbo.AccTransactionLines 
	WHERE
		AL_GC = {0}
		OR AL_JH IN (" + SelectPKFromJobHeader + @")
		OR AL_GB IN (" + SelectPKFromGlbBranch + @")
		OR AL_AH IN (" + SelectPKFromAccTransactionHeader + @")
		OR AL_JBB IN (" + SelectPKFromDsbJobCloseBatch + @")";

		const string SelectPKFromAccTransactionHeader = @"
SELECT AH_PK FROM dbo.AccTransactionHeader 
	WHERE
		AH_GC = {0}
		OR AH_JH IN (" + SelectPKFromJobHeader + @")
		OR AH_GB IN (" + SelectPKFromGlbBranch + @")
		OR AH_GB_TaxBranch IN (" + SelectPKFromGlbBranch + @")
		OR AH_CAH_CashAdvanceRequestHeader IN (" + SelectPkFromAccCashAdvanceRequestHeader + @")
		OR AH_XD_ComplianceBook IN (" + SelectPKFromAccComplianceSequence + @")
";

		const string SelectPKFromGlbBranch = @"
SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = {0}
";

		const string SelectPkFromAccCashAdvanceRequestHeader = @"
SELECT CAH_PK FROM dbo.AccCashAdvanceRequestHeader
	WHERE
		CAH_GC_Company = {0}
		OR CAH_JH_Job IN (" + SelectPKFromJobHeader + @")
";

		const string SelectPKFromAccCashAdvanceRequestLine = @"
SELECT CAL_PK FROM dbo.AccCashAdvanceRequestLine
	WHERE
		CAL_GC_Company = {0}
		OR CAL_CAH_RequestHeader IN (" + SelectPkFromAccCashAdvanceRequestHeader + @")
";

		const string SelectPKFromAccWithholding = @"
SELECT AW_PK FROM dbo.AccWithholding
	WHERE
		AW_GC = {0}
";

		const string SelectPKFromAccChargeCode = @"
SELECT AC_PK FROM dbo.AccChargeCode
	WHERE
		AC_GC = {0}
		OR AC_AW_WithholdingTaxRate IN (" + SelectPKFromAccWithholding + @")
";

		const string SelectPKFromJobCharge = @"
SELECT JR_PK FROM dbo.JobCharge 
	WHERE
		JR_GC = {0}
		OR JR_GB IN (" + SelectPKFromGlbBranch + @")
		OR JR_GB_InternalBranch IN (" + SelectPKFromGlbBranch + @")
		OR JR_JH IN (" + SelectPKFromJobHeader + @")
		OR JR_JH_InternalJob IN (" + SelectPKFromJobHeader + @")
		OR JR_AL_CFXLine IN (" + SelectPKFromAccTransactionLines + @")
		OR JR_AL_APLine IN (" + SelectPKFromAccTransactionLines + @")
		OR JR_AL_ARLine IN (" + SelectPKFromAccTransactionLines + @")
		OR JR_E6 IN (" + SelectPKFromJobConsolCost + @")
		OR JR_E6_GatewaySellHeader IN (" + SelectPKFromJobConsolCost + @")
		OR JR_CAL_ARLine IN (" + SelectPKFromAccCashAdvanceRequestLine + @")
		OR JR_CAL_APLine IN (" + SelectPKFromAccCashAdvanceRequestLine + @")
";

		const string SelectPKFromDsbJobCloseBatch = @"
SELECT JBB_PK
FROM  dbo.DsbJobCloseBatch
WHERE
	JBB_AH_Journal IN (" + SelectPKFromAccTransactionHeader + @")
	OR JBB_GC = {0}
";

		#endregion

		const string JobHeaderScript = @"
--JobHeader
UPDATE dbo.JobCharge SET JR_JR_RevenueLine = null
	WHERE JR_JR_RevenueLine IN (" + SelectPKFromJobCharge + @")

DELETE Q
	FROM dbo.AccCashBasisVATQueue Q
	JOIN dbo.AccCashBasisVAT ON Q.YCC_YC = YC_PK
WHERE YC_AL_TransactionLine IN (" + SelectPKFromAccTransactionLines + @")

DELETE dbo.AccCashBasisVAT
WHERE YC_AL_TransactionLine IN (" + SelectPKFromAccTransactionLines + @")

DELETE dbo.JobPaymentBasis
	WHERE PBS_JR IN (" + SelectPKFromJobCharge + @")

DELETE dbo.JobChargePostingQueue
	WHERE JPQ_JR IN (" + SelectPKFromJobCharge + @")

DELETE dbo.JobCharge
	WHERE JR_PK IN (" + SelectPKFromJobCharge + @")

DELETE dbo.AccOrgBalance WHERE Y3_GC = {0}
DELETE dbo.AccOrgBalanceChanges WHERE Y2_GC = {0}

DELETE dbo.JobPaymentBasis
	WHERE PBS_E6 IN (" + SelectPKFromJobConsolCost + @")

DELETE dbo.JobConsolCost
	WHERE E6_PK IN (" + SelectPKFromJobConsolCost + @")

DELETE dbo.AccHotCheque
  WHERE AQ_JH in (" + SelectPKFromJobHeader + @")
  OR    AQ_AH in (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccQueryClaim
  WHERE AY_AH in (" + SelectPKFromAccTransactionHeader + @")

IF OBJECT_ID('tempdb..#tempComplianceDocumentLine') IS NOT NULL
DROP TABLE #tempComplianceDocumentLine

SELECT ADL_PK, ADL_ADH INTO #tempComplianceDocumentLine FROM dbo.AccComplianceDocumentLine
	WHERE
	ADL_PK IN
	(
		SELECT ADP_ADL FROM dbo.AccComplianceDocumentPivot
			WHERE
			ADP_AL IN (" + SelectPKFromAccTransactionLines + @")
	)

DELETE dbo.AccComplianceDocumentPivot
	WHERE ADP_AL IN (" + SelectPKFromAccTransactionLines + @")
	OR ADP_ADL IN (SELECT ADL_PK FROM #tempComplianceDocumentLine)
	OR ADP_ADL IN
	(
		SELECT ADL_PK FROM dbo.AccComplianceDocumentLine 
		WHERE
		ADL_ADH IN
		(
			SELECT DISTINCT ADL_ADH FROM #tempComplianceDocumentLine
		)
	)

DELETE dbo.AccComplianceDocumentLine
	WHERE
	ADL_PK IN
	(
		SELECT ADL_PK FROM #tempComplianceDocumentLine
	)
	OR
	ADL_ADH IN
	(
		SELECT DISTINCT ADL_ADH FROM #tempComplianceDocumentLine
	)

DELETE dbo.AccComplianceDocumentHeader
	WHERE
	ADH_PK IN
	(
		SELECT DISTINCT ADL_ADH FROM #tempComplianceDocumentLine
	)

DROP TABLE #tempComplianceDocumentLine

DELETE dbo.AccTransactionLineSubAccount
	WHERE AL1_AL IN  (" + SelectPKFromAccTransactionLines + @")

DELETE dbo.AccTransactionLineDissectionAttribute
  WHERE ALD_AL_TransactionLine IN (" + SelectPKFromAccTransactionLines + @")

DELETE dbo.AccTransactionLines
  WHERE AL_PK IN (" + SelectPKFromAccTransactionLines + @")

DELETE dbo.AccTransactionMatchLink
  WHERE AP_AH IN (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccEPaymentDeal
  WHERE AED_QU_Quote in (SELECT QU_PK FROM dbo.AccEPaymentQuote
						 WHERE QU_AV IN (SELECT AV_PK from dbo.AccPaymentApproval 
										 WHERE AV_AH IN (" + SelectPKFromAccTransactionHeader + @"))
	)

DELETE dbo.AccEPaymentQuote
  WHERE QU_AV IN (
		SELECT AV_PK from dbo.AccPaymentApproval 
		WHERE AV_AH IN (" + SelectPKFromAccTransactionHeader + @")
	)

DELETE dbo.AccPaymentApprovalItem
  WHERE A2_AV IN (
		SELECT AV_PK from dbo.AccPaymentApproval 
		WHERE AV_AH IN (" + SelectPKFromAccTransactionHeader + @")
	)

DELETE dbo.AccPaymentApproval
  WHERE AV_AH IN (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccTransactionHeaderReference
	WHERE AH1_AH IN  (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccTransactionHeaderNettingLink
	WHERE AH2_AH IN  (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccTransactionHeaderSubAccount
	WHERE AHS_AH IN  (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccPayableOrderLine
	WHERE
	APL_APH in (SELECT APH_PK FROM dbo.AccPayableOrderHeader
					WHERE APH_AH in (" + SelectPKFromAccTransactionHeader + @")
					OR APH_GC = {0}
				)
	OR APL_GB in (" + SelectPKFromGlbBranch + @")
	OR APL_AC in (" + SelectPKFromAccChargeCode + @")
	OR APL_GC = {0}

DELETE dbo.AccPayableOrderHeader
	WHERE
	APH_AH in (" + SelectPKFromAccTransactionHeader + @")
	OR APH_GC = {0}

DELETE dbo.AccCollectionOrderLine
	WHERE AOL_AH in (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccCommissionHeader
	WHERE
	CH0_AH_Source in (" + SelectPKFromAccTransactionHeader + @")
	OR CH0_GC = {0}

DELETE dbo.OrgCommissionCalculationQueue
	WHERE CAQ_JH in (" + SelectPKFromJobHeader + @")
	OR    CAQ_AH in (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.DsbJobCloseBatch
	WHERE JBB_PK IN (" + SelectPKFromDsbJobCloseBatch + @")

UPDATE dbo.AccTransactionHeader SET AH_AH_InvoiceStatement = null
	WHERE AH_AH_InvoiceStatement in (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccTransactionHeaderFiscalization
	WHERE AF_AH_TransactionHeader IN (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccTransactionHeader
	WHERE AH_PK IN (" + SelectPKFromAccTransactionHeader + @")

DELETE dbo.AccCashAdvanceRequestLine
	WHERE
		CAL_PK IN (" + SelectPKFromAccCashAdvanceRequestLine + @")

DELETE dbo.AccCashAdvanceRequestHeader
	WHERE
		CAH_PK IN (" + SelectPkFromAccCashAdvanceRequestHeader + @")

DELETE dbo.JobExRate
	WHERE JF_JH in (" + SelectPKFromJobHeader + @")

DELETE FROM dbo.JobToCloseQueue
	WHERE JHC_JH in (" + SelectPKFromJobHeader + @")

UPDATE dbo.JobHeader SET JH_JH_ParentJob = null
	WHERE JH_JH_ParentJob in (" + SelectPKFromJobHeader + @")

DELETE FROM dbo.CYDYardStorageLines WHERE
    YSL_ET_JobStorage in (
		SELECT JH_ParentID
		FROM dbo.JobHeader
		WHERE JH_GC = {0})
	OR
	YSL_ET_JobStorage NOT in (
		SELECT JH_ParentID
		FROM dbo.JobHeader)

DELETE FROM dbo.JobStorage WHERE
	ET_PK in (
		SELECT JH_ParentID
		FROM dbo.JobHeader
		WHERE JH_GC = {0}) 
	OR
	ET_PK NOT in (
		SELECT JH_ParentID
		FROM dbo.JobHeader)

DELETE S
	FROM dbo.ShipmentProfitShares S
	JOIN dbo.JobHeader ON S.PSS_JH_ShipmentJob = JH_PK
WHERE JH_GC = {0}

DELETE dbo.JobHeader WHERE JH_GC = {0}";

		#endregion

		#region Work Items

		const string WorkItemsScript = OperationalScriptRepository.WorkItemsScript + @"
WHERE WKI_GC_AssignedCompany = {0}
";

		#endregion

		#endregion

		#endregion
	}
}
