using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class ARComplianceDocumentFilterStripBusinessObject : ComplianceDocumentFilterStripBusinessObject
	{
		public ARComplianceDocumentFilterStripBusinessObject()
		{ }

		public override ZString LedgerType => LedgerTypes.AccountsReceivable;

		protected override void AddStatusesFilter(ModuleFilterCollection filters)
		{
			base.AddStatusesFilter(filters);

			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
			{
				var eInvoicingPivotStatusFilter = filters.AddTextFilter("EInvoicing Pivot Status", AIPStatusQuery, AccEInvoicingTransactionPivotLookups.PivotStatusList);
				eInvoicingPivotStatusFilter.Category = FilterCategories.StatusAndFlags;
				eInvoicingPivotStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ComplianceDocumentFilter|EInvoicingPivotStatus", "E-Reporting Pivot Status");

				var eInvoicingBatchStatusFilter = filters.AddTextFilter("EInvoicing Batch Status", AIBStatusQuery, AccEInvoicingBatch.AIB_Status_List);
				eInvoicingBatchStatusFilter.Category = FilterCategories.StatusAndFlags;
				eInvoicingBatchStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ComplianceDocumentFilter|EInvoicingBatchStatus", "E-Reporting Batch Status");
			}
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);

			var approvalNumberFilter = filters.AddNumberFilter("Void Approval Number", AccComplianceDocumentHeaderSchema.ADH_ApprovalNumber);
			approvalNumberFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_ApprovalNumber.MaxLength;
			approvalNumberFilter.MultilingualDescription = ResString.GetMultilingualString("763DE183-2C95-4EDE-9C2C-C8AB29BE6FFA", "Void Approval Number");
		}

		protected override void AddTextFilters(ModuleFilterCollection filters)
		{
			base.AddTextFilters(filters);

			var voidReasonFilter = filters.AddTextFilter("Void Reason", AccComplianceDocumentHeaderSchema.ADH_VoidingReason);
			voidReasonFilter.Category = FilterCategories.TextSearch;
			voidReasonFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_VoidingReason.MaxLength;
			voidReasonFilter.MultilingualDescription = ResString.GetMultilingualString("30C0F728-78A7-4E99-9436-D9B6E0E2F89E", "Void Reason");
		}

		ZQuery AIPStatusQuery(ZString aipStatus)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, aipStatus);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery AIBStatusQuery(ZString aibStatus)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccComplianceDocumentHeaderSchema.Constants.Prefix);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK);
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);
			subQuery1.AddToFilter(AccEInvoicingBatchSchema.AIB_Status, aibStatus);
			subQuery1.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, subQuery1, JoinCondition.And);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		public override CodeDescriptionPairList ComplianceStatusList
		{
			get
			{
				var fComplianceStatusList = base.ComplianceStatusList;
				fComplianceStatusList.AddPair(Constants.ComplianceDocumentStatus.FinalisedSpecialVoided, ResString.GetMultilingualString("ComplianceDocumentStatus|FinalisedSpecialVoided", "Document Record Special Voided After Finalized"));
				return fComplianceStatusList;
			}
		}
	}
}