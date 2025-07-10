using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer
{
	public class TransactionExportFilterProvider : Accounting.Integration.ITransactionExportFilterProvider
	{
		public TransactionExportFilterProvider(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region Filtering

		#region Transaction Types

		#region IncludeARInvoices

		public ZBool IncludeARInvoices
		{
			get { return fIncludeARInvoices; }
			set { fIncludeARInvoices = value; }
		}

		ZBool fIncludeARInvoices;

		#endregion

		#region IncludeARCreditNotes

		public ZBool IncludeARCreditNotes
		{
			get { return fIncludeARCreditNotes; }
			set { fIncludeARCreditNotes = value; }
		}

		ZBool fIncludeARCreditNotes;

		#endregion

		#region IncludeARAdjustmentNotes

		public ZBool IncludeARAdjustmentNotes
		{
			get { return fIncludeARAdjustmentNotes; }
			set { fIncludeARAdjustmentNotes = value; }
		}

		ZBool fIncludeARAdjustmentNotes;

		#endregion

		#region IncludeAPInvoices

		public ZBool IncludeAPInvoices
		{
			get { return fIncludeAPInvoices; }
			set { fIncludeAPInvoices = value; }
		}

		ZBool fIncludeAPInvoices;

		#endregion

		#region IncludeAPCreditNotes

		public ZBool IncludeAPCreditNotes
		{
			get { return fIncludeAPCreditNotes; }
			set { fIncludeAPCreditNotes = value; }
		}

		ZBool fIncludeAPCreditNotes;

		#endregion

		#region IncludeAPAdjustmentNotes

		public ZBool IncludeAPAdjustmentNotes
		{
			get { return fIncludeAPAdjustmentNotes; }
			set { fIncludeAPAdjustmentNotes = value; }
		}

		ZBool fIncludeAPAdjustmentNotes;

		#endregion

		#region AtLeastOneTypeOfARIsSelected

		public bool AtLeastOneTypeOfARIsSelected
		{
			get
			{
				return IncludeARInvoices || IncludeARCreditNotes || IncludeARAdjustmentNotes;
			}
		}

		#endregion

		#region AtLeastOneTypeOfAPIsSelected

		public bool AtLeastOneTypeOfAPIsSelected
		{
			get
			{
				return IncludeAPInvoices || IncludeAPCreditNotes || IncludeAPAdjustmentNotes;
			}
		}

		#endregion

		#region AtLeastOneTypeOfInvoiceIsSelected

		public bool AtLeastOneTypeOfInvoiceIsSelected
		{
			get
			{
				return AtLeastOneTypeOfARIsSelected || AtLeastOneTypeOfAPIsSelected;
			}
		}

		#endregion

		#region Include WIPs Posting

		public ZBool IncludeWIPsPosting
		{
			get { return fIncludeWIPsPosting; }
			set { fIncludeWIPsPosting = value; }
		}

		ZBool fIncludeWIPsPosting;

		#endregion

		#region Include WIPs Reversing

		public ZBool IncludeWIPsReversing
		{
			get { return fIncludeWIPsReversing; }
			set { fIncludeWIPsReversing = value; }
		}

		ZBool fIncludeWIPsReversing;

		#endregion

		#region Include Accruals Posting

		public ZBool IncludeAccrualsPosting
		{
			get { return fIncludeAccrualsPosting; }
			set { fIncludeAccrualsPosting = value; }
		}

		ZBool fIncludeAccrualsPosting;

		#endregion

		#region Include Accruals Reversing

		public ZBool IncludeAccrualsReversing
		{
			get { return fIncludeAccrualsReversing; }
			set { fIncludeAccrualsReversing = value; }
		}

		ZBool fIncludeAccrualsReversing;

		#endregion

		public ZBool IncludeUnallocatedAPInvoices
		{
			get;
			set;
		}

		public ZBool IncludeUnallocatedAPCreditNotes
		{
			get;
			set;
		}

		#region AtLeastOneTypeOfWIPAccrualPostingIsSelected

		public bool AtLeastOneTypeOfWIPAccrualPostingIsSelected
		{
			get { return IncludeWIPsPosting || IncludeAccrualsPosting; }
		}

		#endregion

		#region AtLeastOneTypeOfWIPAccrualReversingIsSelected

		public bool AtLeastOneTypeOfWIPAccrualReversingIsSelected
		{
			get { return IncludeWIPsReversing || IncludeAccrualsReversing; }
		}

		#endregion

		#endregion

		#region Job Related / Non Job Related Transactions

		#region Jobs

		public JobHeaderCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = new JobHeaderCollection(Factory);
				}

				return fJobs;
			}
		}

		JobHeaderCollection fJobs;

		#endregion

		#region Exclude Job Related Transactions For AR

		public ZBool ExcludeJobRelatedTransactionsForAR
		{
			get { return fExcludeJobRelatedTransactionsForAR; }
			set { fExcludeJobRelatedTransactionsForAR = value; }
		}

		ZBool fExcludeJobRelatedTransactionsForAR;

		#endregion

		#region Exclude Non Job Related Transactions For AR

		public ZBool ExcludeNonJobRelatedTransactionsForAR
		{
			get { return fExcludeNonJobRelatedTransactionsForAR; }
			set { fExcludeNonJobRelatedTransactionsForAR = value; }
		}

		ZBool fExcludeNonJobRelatedTransactionsForAR;

		#endregion

		#region Exclude Job Related Transactions For AP

		public ZBool ExcludeJobRelatedTransactionsForAP
		{
			get { return fExcludeJobRelatedTransactionsForAP; }
			set { fExcludeJobRelatedTransactionsForAP = value; }
		}

		ZBool fExcludeJobRelatedTransactionsForAP;

		#endregion

		#region Exclude Non Job Related Transactions For AP

		public ZBool ExcludeNonJobRelatedTransactionsForAP
		{
			get { return fExcludeNonJobRelatedTransactionsForAP; }
			set { fExcludeNonJobRelatedTransactionsForAP = value; }
		}

		ZBool fExcludeNonJobRelatedTransactionsForAP;

		#endregion

		#endregion

		#region Dates

		#region Date From

		public ZDateTime DateFrom
		{
			get { return fDateFrom; }
			set { fDateFrom = value; }
		}

		ZDateTime fDateFrom;

		#endregion

		#region Date To

		public ZDateTime DateTo
		{
			get { return fDateTo; }
			set { fDateTo = value; }
		}

		ZDateTime fDateTo;

		#endregion

		#endregion

		#region Accounting Periods

		#region Period From

		public ZInt PeriodFrom
		{
			get { return fPeriodFrom; }
			set { fPeriodFrom = value; }
		}

		ZInt fPeriodFrom;

		#endregion

		#region Period To

		public ZInt PeriodTo
		{
			get { return fPeriodTo; }
			set { fPeriodTo = value; }
		}

		ZInt fPeriodTo;

		#endregion

		#endregion

		#region Transaction Numbers

		#region Transaction Number From

		public ZString  TransactionNumberFrom
		{
			get { return fTransactionNumberFrom ; }
			set { fTransactionNumberFrom  = value; }
		}

		ZString fTransactionNumberFrom;

		#endregion

		#region Transaction Numbers To

		public ZString TransactionNumberTo
		{
			get { return fTransactionNumberTo  ; }
			set { fTransactionNumberTo  = value; }
		}

		ZString fTransactionNumberTo;

		#endregion

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}
		OrgHeaderCollection fOrganisations;

		#endregion

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}
		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory, new AdhocCollectionRelationship(typeof(GlbDepartment)));
				}
				return fDepartments;
			}
		}
		GlbDepartmentCollection fDepartments;

		#endregion

		#region Current Batch No

		public ZInt CurrentBatchNo
		{
			get { return fCurrentBatchNo; }
			set { fCurrentBatchNo = value; }
		}

		ZInt fCurrentBatchNo;

		#endregion

		#region AccountGroup

		public ZGuid AccountGroup
		{
			get { return fAccountGroup; }
			set { fAccountGroup = value; }
		}
		ZGuid fAccountGroup;

		#endregion

		#endregion

		#region HighWaterMark

		public ISupportHighWaterMark Exporter
		{
			get;
			set;
		}

		public bool UsingHighWaterMark
		{
			get
			{
				return Exporter != null && Exporter.HighWaterMarkRegistry.Value != DateTime.MinValue && CanSaveHighWaterMark;
			}
		}

		public bool CanSaveHighWaterMark
		{
			get
			{
				return Exporter != null && Exporter.IsHighWaterMarkEnabled && CurrentBatchNo == 0 && !HasRestrictiveFilters && FilterMatchesAccountingTransactionTypesRegistry;
			}
		}

		public CodeDescriptionBoolRegistryItem AccountingTransactionTypesRegistry
		{
			get
			{
				return SystemDataRegistry.Instance.AccountingTransactionTypes;
			}
		}

		public DateTime HighWaterMark
		{
			get
			{
				var result = DateTime.MinValue;
				if (Exporter != null)
				{
					result = Exporter.HighWaterMarkRegistry.Value;
				}
				return result;
			}
		}

		public bool HasRestrictiveFilters
		{
			get
			{
				return
					!TransactionNumberFrom.IsEmpty || !TransactionNumberTo.IsEmpty ||
					ExcludeJobRelatedTransactionsForAR || ExcludeNonJobRelatedTransactionsForAR || ExcludeJobRelatedTransactionsForAP || ExcludeNonJobRelatedTransactionsForAP ||
					!DateFrom.IsEmpty || !DateTo.IsEmpty ||
					PeriodFrom > 0 || PeriodTo > 0 ||
					Branches.Count > 0 ||
					Departments.Count > 0 ||
					Jobs.Count > 0 ||
					Organisations.Count > 0;
			}
		}

		bool FilterMatchesAccountingTransactionTypesRegistry
		{
			get
			{
				return AccountingTransactionTypesRegistry.Value.Cast<CodeDescriptionBool>().Any(code => code.Bool == ZBool.True) &&
					AccountingTransactionTypesRegistryMapper.FilterMatchesRegistryValues(this);
			}
		}

		public void SaveHighWaterMarkRegistry(ZDateTime batchCreateDate)
		{
			using (Exporter.HighWaterMarkRegistry.DataType.SuspendValidation())
			{
				Exporter.HighWaterMarkRegistry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, batchCreateDate.ToDateTime().Subtract(HighWaterMarkBuffer));
			}
		}

		readonly TimeSpan HighWaterMarkBuffer = new TimeSpan(48, 0, 0);

		#endregion

		#region AccountingTransactionTypesRegistryMapper

		public void CopyRegistryValuesToFilter()
		{
			AccountingTransactionTypesRegistryMapper.CopyRegistryValuesToFilter(this);
		}

#if DEBUG
		internal
#endif
		static class AccountingTransactionTypesRegistryMapper
		{
			class Schema
			{
				public const string IncludeAPAdjustmentNotes = "IncludeAPAdjustmentNotes";
				public const string IncludeAPCreditNotes = "IncludeAPCreditNotes";
				public const string IncludeAPInvoices = "IncludeAPInvoices";
				public const string IncludeARAdjustmentNotes = "IncludeARAdjustmentNotes";
				public const string IncludeARCreditNotes = "IncludeARCreditNotes";
				public const string IncludeARInvoices = "IncludeARInvoices";
				public const string IncludeWIPsPosting = "IncludeWIPsPosting";
				public const string IncludeWIPsReversing = "IncludeWIPsReversing";
				public const string IncludeAccrualsPosting = "IncludeAccrualsPosting";
				public const string IncludeAccrualsReversing = "IncludeAccrualsReversing";
				public const string IncludeUnallocatedAPCreditNotes = "IncludeUnallocatedAPCreditNotes";
				public const string IncludeUnallocatedAPInvoices = "IncludeUnallocatedAPInvoices";
			}

			public static void CopyRegistryValuesToFilter(TransactionExportFilterProvider filterProvider)
			{
				foreach (CodeDescriptionBool item in AccountingTransactionTypesRegistry.Value)
				{
					Map[item.Code].SetValue(filterProvider, item.Bool, null);
				}
			}

			public static bool FilterMatchesRegistryValues(TransactionExportFilterProvider filterProvider)
			{
				foreach (var item in Map)
				{
					if ((ZBool)item.Value.GetValue(filterProvider, null) != AccountingTransactionTypesRegistry.Value.GetBoolFromCode(item.Key))
					{
						return false;
					}
				}
				return true;
			}

#if DEBUG
			internal
#endif
			readonly static ImmutableDictionary<string, PropertyInfo> Map = new Dictionary<string, PropertyInfo>()
				{
					{ ExportTransactionTypes.APAdjustmentNote, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeAPAdjustmentNotes) },
					{ ExportTransactionTypes.APCreditNote, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeAPCreditNotes) },
					{ ExportTransactionTypes.APInvoice, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeAPInvoices) },
					{ ExportTransactionTypes.ARAdjustmentNote, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeARAdjustmentNotes) },
					{ ExportTransactionTypes.ARCreditNote, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeARCreditNotes) },
					{ ExportTransactionTypes.ARInvoice, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeARInvoices) },
					{ ExportTransactionTypes.WIPPosting, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeWIPsPosting) },
					{ ExportTransactionTypes.WIPReversal, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeWIPsReversing) },
					{ ExportTransactionTypes.AccrualPosting, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeAccrualsPosting) },
					{ ExportTransactionTypes.AccrualReversal, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeAccrualsReversing) },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeUnallocatedAPCreditNotes) },
					{ ExportTransactionTypes.UnallocatedAPInvoices, typeof(TransactionExportFilterProvider).GetProperty(Schema.IncludeUnallocatedAPInvoices) }
				}.ToImmutableDictionary();

			static CodeDescriptionBoolRegistryItem AccountingTransactionTypesRegistry
			{
				get
				{
					return SystemDataRegistry.Instance.AccountingTransactionTypes;
				}
			}
		}
		#endregion

		readonly BusinessObjectFactory Factory;
	}
}
