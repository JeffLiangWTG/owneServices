using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Dash.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.APAutomation.AccountingParseImportServiceTask
{
	public static class DraftInvoiceImporter
	{
		public static void ImportToDraftInvoice(BusinessObjectFactory factory, DashAPInvoice dashInvoice, AccDraftInvoiceHeader draftInvoice, IJobReferenceMatcher jobReferenceMatcher = null)
		{
			jobReferenceMatcher ??= new JobReferenceMatcher();
			PopulateDraftInvoiceHeader(dashInvoice, draftInvoice);
			PopulateDraftInvoiceJobReferences(factory, dashInvoice, draftInvoice);
			MatchJobReferencesInInvoice(factory, jobReferenceMatcher, draftInvoice);
		}

		static void PopulateDraftInvoiceHeader(DashAPInvoice dashInvoice, AccDraftInvoiceHeader draftInvoice)
		{
			if (dashInvoice.MatchedIssuerID != null)
			{
				draftInvoice.AIH_OH_Creditor = dashInvoice.MatchedIssuerID.PK;
				if (dashInvoice.MatchedIssuerAddressID == null)
				{
					draftInvoice.AIH_OA_CreditorAddress = dashInvoice.MatchedIssuerID.AddressForSendingAPDocuments.PK;
				}
				else
				{
					draftInvoice.AIH_OA_CreditorAddress = dashInvoice.MatchedIssuerAddressID.PK;
				}
			}

			if (!dashInvoice.DPI_InvoiceNumber.IsEmpty)
			{
				draftInvoice.AIH_TransactionNumber = dashInvoice.DPI_InvoiceNumber;
			}

			if (dashInvoice.DPI_InvoiceDate.IsValid)
			{
				draftInvoice.AIH_TransactionDate = dashInvoice.DPI_InvoiceDate;
			}

			if (!dashInvoice.DPI_RX_NKInvoiceCurrency.IsEmpty)
			{
				draftInvoice.AIH_RX_NKTransactionCurrency = dashInvoice.DPI_RX_NKInvoiceCurrency;
			}

			var isCreditNote = dashInvoice.DPI_GrossTotal < 0;

			if (isCreditNote)
			{
				draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
				draftInvoice.AIH_Description = APCreditNoteDescription;
			}

			draftInvoice.AIH_ExpectedOSExTaxAmount = Math.Abs(dashInvoice.DPI_NetTotal);

			draftInvoice.AIH_ExpectedOSTaxAmount = Math.Abs(dashInvoice.DPI_VatTotal);

			draftInvoice.AIH_ExpectedOSTotalAmount = Math.Abs(dashInvoice.DPI_GrossTotal);
		}
		static readonly string APCreditNoteDescription = (NoResString)"AP Credit Note";

		static void PopulateDraftInvoiceJobReferences(BusinessObjectFactory factory, DashAPInvoice dashInvoice, AccDraftInvoiceHeader draftInvoice)
		{
			dashInvoice.DashAPInvoiceRefs.OfType<DashAPInvoiceRef>()
				.Select(jobRef => new {
					SanitizedJobReference = jobRef.DPR_Reference
						.Trim()
						.ToUpperInvariant()
						.Substring(0, AccDraftInvoiceJobReferenceSchema.AIR_Reference.MaxLength),
					SanitizedJobType = jobRef.DPR_ReferenceType.ToUpperInvariant()
				})
				.GroupBy(s => s.SanitizedJobReference)
				.Select(g => g.First())
				.ForEach(sanitizedJobRef =>
					{
						var newJobRef = factory.New<AccDraftInvoiceJobReference>();
						newJobRef.AIR_Reference = sanitizedJobRef.SanitizedJobReference;
						newJobRef.AIR_Type = sanitizedJobRef.SanitizedJobType;
						newJobRef.AIR_AIH_Header = draftInvoice.PK;
						newJobRef.AIR_GC_Company = draftInvoice.AIH_GC_Company;
						draftInvoice.JobReferences.Add(newJobRef);
					}
				);
		}

		static void MatchJobReferencesInInvoice(BusinessObjectFactory factory, IJobReferenceMatcher jobReferenceMatcher, AccDraftInvoiceHeader draftInvoice)
		{
			if (draftInvoice.JobReferences.Count == 0)
			{
				return;
			}

			var existingJobsByParentPk = new Dictionary<ZGuid, ZGuid>();

			foreach (var jobRef in draftInvoice.JobReferences.Cast<AccDraftInvoiceJobReference>())
			{
				var searchFields = JobTypeToSearchFields[jobRef.AIR_Type];
				var (jobParentPk, jobParentTablecode) = jobReferenceMatcher.FindBestMatching(jobRef.AIR_Reference, searchFields);

				if (jobParentPk == ZGuid.Invalid)
				{
					(jobParentPk, jobParentTablecode) = jobReferenceMatcher.FindBestMatching(jobRef.AIR_Reference);
				}

				if (jobParentPk != ZGuid.Invalid)
				{
					if (existingJobsByParentPk.TryGetValue(jobParentPk, out var existingJobPk))
					{
						jobRef.AIR_AIJ_Job = existingJobPk;
					}
					else
					{
						var cluster = factory.New<AccDraftInvoiceJobCluster>();
						cluster.AIC_AIH_Header = draftInvoice.PK;
						cluster.AIC_GC_Company = draftInvoice.AIH_GC_Company;
						cluster.AIC_RX_NKCurrency = draftInvoice.AIH_RX_NKTransactionCurrency;

						var newJob = factory.New<AccDraftInvoiceJob>();
						newJob.AIJ_ParentID = jobParentPk;
						newJob.AIJ_ParentTableCode = jobParentTablecode;
						newJob.AIJ_GC_Company = jobRef.AIR_GC_Company;
						newJob.AIJ_AIC_Cluster = cluster.PK;

						jobRef.AIR_AIJ_Job = newJob.PK;
						existingJobsByParentPk[jobParentPk] = newJob.PK;
					}
				}
			}
		}

		[ThreadSafe]
		readonly static internal Dictionary<string, string[]> JobTypeToSearchFields = new()
		{
			{ JobTypesForSearching.JOB, [JobReferenceMatcher.SearchFieldsForCategoryAPA.JobNumber] },
			{ JobTypesForSearching.BOL, [JobReferenceMatcher.SearchFieldsForCategoryAPA.MasterBillNumber, JobReferenceMatcher.SearchFieldsForCategoryAPA.HouseBillNumber] },
			{ JobTypesForSearching.CNT, [JobReferenceMatcher.SearchFieldsForCategoryAPA.ContainerNumber] },
		};

		static class JobTypesForSearching
		{
			public const string JOB = "JOB";
			public const string BOL = "BOL";
			public const string CNT = "CNT";
		}
	}
}
