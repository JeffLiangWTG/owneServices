using Enterprise.DataPurge.Test.Utility;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataPurge
{
	public class AccountingRelationshipStageForTest : BusinessRelationshipStageForTest
	{
		public override void SetupRelationships()
		{
			#region Mock SchemaColumn for columns whose table doesn't have a Schema in code

			var columnAPQ_ParentID = SchemaUtility.CreateGuidColumnForTest(tableName: "AccTransactionPostingToGLDQueue", columnName: "APQ_ParentID");

			#endregion

			#region Add Relationships

			AddRelationship(AccCashBasisVATSchema.PK, columnAPQ_ParentID,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(AccChargeCodeSchema.PK, AccChargeSupplyTypeOverrideSchema.ACS_ParentID);
			AddRelationship(AccChargeCodeSchema.PK, AccJobConfigPivotSchema.JCT_ParentId,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(AccCommissionHeaderSchema.PK, AccCommissionLineSchema.CL0_ParentID);

			AddRelationship(AccCommissionLineGroupSchema.PK, AccCommissionLineSchema.CL0_ParentID);

			AddRelationship(AccComplianceDocumentHeaderSchema.PK, AccEInvoicingTransactionPivotSchema.AIP_ParentID);

			AddRelationship(AccGroupsSchema.PK, AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId);
			AddRelationship(AccGroupsSchema.PK, AccTransactionLineSubAccountSchema.AL1_SubClassParentId);

			AddRelationship(AccTaxGLMovementSchema.PK, columnAPQ_ParentID,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(AccTransactionHeaderSchema.PK, AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			AddRelationship(AccTransactionHeaderSchema.PK, AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId);
			AddRelationship(AccTransactionHeaderSchema.PK, columnAPQ_ParentID,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(AccTransactionLinesSchema.PK, columnAPQ_ParentID,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(CarrierShipmentHeaderSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusCAeMHMasterSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusHAWBSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusInBondHeaderSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusISFHeaderSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusMAWBSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusPermitHeaderSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CusUnderbondSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CYDReceiveAdviceSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CYDReleaseAdviceSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(CYDTransportationUnitSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(DtbAgentBookingSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(DtbBookingConsolidationSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(DtbBookingSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(DtbConsignmentRunSheetSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(DtbConsignmentSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(ExportCustomsManifestLinesSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(GlbBranchSchema.PK, AccTaxConfigurationSchema.ETC_ParentId,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(GlbCompanySchema.PK, AccTaxConfigurationSchema.ETC_ParentId,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");

			AddRelationship(GlbGroupSchema.PK, AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId);
			AddRelationship(GlbGroupSchema.PK, AccTransactionLineSubAccountSchema.AL1_SubClassParentId);

			AddRelationship(GlbStaffSchema.PK, AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId);
			AddRelationship(GlbStaffSchema.PK, AccTransactionLineSubAccountSchema.AL1_SubClassParentId);

			AddRelationship(JobCartageRunSheetSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobCartageSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobConsolSchema.JK_UniqueConsignRef, AccBillingHeaderSchema.ABH_ParentReferenceNumber,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");
			AddRelationship(JobConsolSchema.PK, AccBillingHeaderSchema.ABH_ParentId,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");
			AddRelationship(JobConsolSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobContainerDetentionSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobContainerSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobDeclarationSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobMawbSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobShipmentSchema.JS_UniqueConsignRef, AccBillingItemSchema.ABI_ParentReferenceNumber,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");
			AddRelationship(JobShipmentSchema.PK, AccBillingItemSchema.ABI_ParentId,
				justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords: "This needs an investigation to decide to keep it in the whitelist or to add related purging SQL scripts. We should give a more reasonable justification here if finally we keep it.");
			AddRelationship(JobShipmentSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobStorageSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobSundryChargesSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(JobVoyAccountSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(OrgHeaderSchema.PK, AccTransactionHeaderSubAccountSchema.AHS_SubClassParentId);
			AddRelationship(OrgHeaderSchema.PK, AccTransactionLineSubAccountSchema.AL1_SubClassParentId);

			AddRelationship(WhsAdHocServiceJobSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsDocketSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsItemDispatchConsignmentSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsItemDispatchLoadListSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsItemDispatchTransportationUnitSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsItemReceiveConsignmentSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsItemReceiveTransportationUnitSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsStocktakeSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WhsVASOrderSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WorkItemSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WorkProjectSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			AddRelationship(WorkRequestSchema.PK, AccDraftInvoiceJobSchema.AIJ_ParentID);

			#endregion
		}
	}
}
