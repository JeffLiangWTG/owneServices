using System;
using CargoWise.Common;
using Enterprise.Accounting.APAutomation.APReconciliation.Helpers;
using Enterprise.Accounting.Business.APReconciliation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public static class APReconciliationNodeProvider
	{
		public static APReconciliationNode GetAPReconciliationNode(AccDraftInvoiceJob job, AccDraftInvoiceHeader draftInvoice)
		{
			Argument.NotNull(draftInvoice, nameof(draftInvoice));
			var cache = APReconciliationAccrualSourceCache.Get<DraftInvoiceHeaderBasedAPReconciliationAccrualSource>(draftInvoice);
			var accrualSource = GetAccrualSourceType(job.AIJ_ParentTableCode);
			switch (accrualSource)
			{
				case AccrualSourceTypes.Consol:
					var consol = cache.GetConsol(job.AIJ_ParentID);
					if (consol != null)
					{
						return new APReconciliationNode(consol.JK_UniqueConsignRef, new ConsolCostRelatedAPReconciliationLineProvider(job.AIJ_ParentID, draftInvoice));
					}
					return null;
				case AccrualSourceTypes.Job:
					var invoicingJob = cache.GetJob(job.AIJ_ParentID);
					if (invoicingJob != null)
					{
						return new APReconciliationNode(invoicingJob.JH_JobNum, new JobChargeRelatedAPReconciliationLineProvider(job.AIJ_ParentID, draftInvoice));
					}
					return null;
			}
			return null;
		}

		public static AccrualSourceTypes GetAccrualSourceType(string parentTableCode)
		{
			Argument.NotNullOrEmpty(parentTableCode, nameof(parentTableCode));
			switch (parentTableCode)
			{
				case JobConsolSchema.Constants.Prefix:
				case DtbBookingConsolidationSchema.Constants.Prefix:
				case DtbBookingSchema.Constants.Prefix:
				case DtbConsignmentRunSheetSchema.Constants.Prefix:
				case DtbLinehaulManifestSchema.Constants.Prefix:
				case JobCartageRunSheetSchema.Constants.Prefix:
				case WhsItemReceiveTransportationUnitSchema.Constants.Prefix:
				case WhsItemDispatchTransportationUnitSchema.Constants.Prefix:
				case WhsItemDispatchLoadListSchema.Constants.Prefix:
					{
						return AccrualSourceTypes.Consol;
					}
				case JobShipmentSchema.Constants.Prefix:
				case JobDeclarationSchema.Constants.Prefix:
				case CYDTransportationUnitSchema.Constants.Prefix:
				case CYDReleaseAdviceSchema.Constants.Prefix:
				case CYDReceiveAdviceSchema.Constants.Prefix:
				case WhsVASOrderSchema.Constants.Prefix:
				case WhsAdHocServiceJobSchema.Constants.Prefix:
				case WhsItemReceiveConsignmentSchema.Constants.Prefix:
				case WhsStocktakeSchema.Constants.Prefix:
				case WorkRequestSchema.Constants.Prefix:
				case WorkProjectSchema.Constants.Prefix:
				case WorkItemSchema.Constants.Prefix:
				case WhsItemDispatchConsignmentSchema.Constants.Prefix:
				case WhsDocketSchema.Constants.Prefix:
				case JobContainerDetentionSchema.Constants.Prefix:
				case JobVoyAccountSchema.Constants.Prefix:
				case DtbConsignmentSchema.Constants.Prefix:
				case DtbAgentBookingSchema.Constants.Prefix:
				case JobMawbSchema.Constants.Prefix:
				case JobCartageSchema.Constants.Prefix:
				case JobContainerSchema.Constants.Prefix:
				case JobStorageSchema.Constants.Prefix:
				case ExportCustomsManifestLinesSchema.Constants.Prefix:
				case JobSundryChargesSchema.Constants.Prefix:
				case CusHAWBSchema.Constants.Prefix:
				case CusPermitHeaderSchema.Constants.Prefix:
				case CusMAWBSchema.Constants.Prefix:
				case CusUnderbondSchema.Constants.Prefix:
				case CusISFHeaderSchema.Constants.Prefix:
				case CusInBondHeaderSchema.Constants.Prefix:
				case CusCAeMHMasterSchema.Constants.Prefix:
				case CarrierShipmentHeaderSchema.Constants.Prefix:
					{
						return AccrualSourceTypes.Job;
					}
				default:
					{
						throw new ArgumentOutOfRangeException(nameof(parentTableCode), parentTableCode, "unrecognised value");
					}
			}
		}
	}
}
