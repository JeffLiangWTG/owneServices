using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.XmlExport
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public partial class XmlExportGUIWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public XmlExportGUIWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
			SelectedJobs.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
			SelectedBranches.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
			SelectedDepartments.CountChanged += new EventHandler(Collection_CountChanged);
			SelectedOrganisations.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
		}

		public event EventHandler ExistingBatchChanged;

		public virtual string FormCaption
		{
			get { return Res.GetString("Accounting|XmlExportGUIWrapper|FormCaption", "Export Accounting Transactions"); }
		}

		#region Lookups

		#region Organisations Lookup

		public OrganisationsFindBoxCollection OrgHeadersList
		{
			get { return OrgHeadersListCore; }
		}

		protected virtual OrganisationsFindBoxCollection OrgHeadersListCore
		{
			get
			{
				if (fOrgHeadersListCore == null)
				{
					fOrgHeadersListCore = new OrganisationsFindBoxCollection(Factory);
				}

				return fOrgHeadersListCore;
			}
		}

		protected OrganisationsFindBoxCollection fOrgHeadersListCore;

		#endregion

		#region Jobs Lookup

		public JobHeaderCollection JobHeaderList
		{
			get
			{
				if (fJobHeaderList == null)
				{
					ZQuery query = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					fJobHeaderList = new JobHeaderCollection(new BusinessObjectFactory(), query);
				}
				return fJobHeaderList;
			}
		}

		public JobHeaderCollection fJobHeaderList;

		#endregion

		#region Departments Lookup

		public GlbDepartmentCollection DepartmentsList
		{
			get
			{
				if (fDepartmentsList == null)
				{
					fDepartmentsList = new GlbDepartmentCollection(Factory);
				}

				return fDepartmentsList;
			}
		}

		protected GlbDepartmentCollection fDepartmentsList;

		#endregion

		#region Branches Lookup

		public GlbBranchCollection BranchesList
		{
			get
			{
				if (fBranchesList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fBranchesList = new GlbBranchCollection(new BusinessObjectFactory(), filter);
				}
				return fBranchesList;
			}
		}

		public GlbBranchCollection fBranchesList;

		#endregion

		#endregion

		#region Properties

		#region IncludeARInvoices

		public ZBool IncludeARInvoices
		{
			get { return DataExporter.FilterProvider.IncludeARInvoices; }
			set
			{
				DataExporter.FilterProvider.IncludeARInvoices = value;
				IncludeARInvoicesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeARInvoicesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeARInvoices)); }
		}

		#endregion

		#region IncludeARCreditNotes

		public ZBool IncludeARCreditNotes
		{
			get { return DataExporter.FilterProvider.IncludeARCreditNotes; }
			set
			{
				DataExporter.FilterProvider.IncludeARCreditNotes = value;
				IncludeARCreditNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeARCreditNotesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeARCreditNotes)); }
		}

		#endregion

		#region IncludeARAdjustmentNotes

		public ZBool IncludeARAdjustmentNotes
		{
			get { return DataExporter.FilterProvider.IncludeARAdjustmentNotes; }
			set
			{
				DataExporter.FilterProvider.IncludeARAdjustmentNotes = value;
				IncludeARAdjustmentNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeARAdjustmentNotesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeARAdjustmentNotes)); }
		}

		#endregion

		#region IncludeAPInvoices

		public ZBool IncludeAPInvoices
		{
			get { return DataExporter.FilterProvider.IncludeAPInvoices; }
			set
			{
				DataExporter.FilterProvider.IncludeAPInvoices = value;
				IncludeAPInvoicesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeAPInvoicesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAPInvoices)); }
		}

		#endregion

		#region IncludeAPCreditNotes

		public ZBool IncludeAPCreditNotes
		{
			get { return DataExporter.FilterProvider.IncludeAPCreditNotes; }
			set
			{
				DataExporter.FilterProvider.IncludeAPCreditNotes = value;
				IncludeAPCreditNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeAPCreditNotesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAPCreditNotes)); }
		}

		#endregion

		#region IncludeAPAdjustmentNotes

		public ZBool IncludeAPAdjustmentNotes
		{
			get { return DataExporter.FilterProvider.IncludeAPAdjustmentNotes; }
			set
			{
				DataExporter.FilterProvider.IncludeAPAdjustmentNotes = value;
				IncludeAPAdjustmentNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeAPAdjustmentNotesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAPAdjustmentNotes)); }
		}

		#endregion

		#region IncludeWipsPosting

		public ZBool IncludeWipsPosting
		{
			get { return DataExporter.FilterProvider.IncludeWIPsPosting; }
			set
			{
				DataExporter.FilterProvider.IncludeWIPsPosting = value;
				IncludeWipsPostingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeWipsPostingInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeWipsPosting)); }
		}

		#endregion

		#region IncludeWipsReversing

		public ZBool IncludeWipsReversing
		{
			get { return DataExporter.FilterProvider.IncludeWIPsReversing; }
			set
			{
				DataExporter.FilterProvider.IncludeWIPsReversing = value;
				IncludeWipsReversingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeWipsReversingInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeWipsReversing)); }
		}

		#endregion

		#region IncludeAccrualsPosting

		public ZBool IncludeAccrualsPosting
		{
			get { return DataExporter.FilterProvider.IncludeAccrualsPosting; }
			set
			{
				DataExporter.FilterProvider.IncludeAccrualsPosting = value;
				IncludeAccrualsPostingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeAccrualsPostingInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAccrualsPosting)); }
		}

		#endregion

		#region IncludeAccrualsReversing

		public ZBool IncludeAccrualsReversing
		{
			get { return DataExporter.FilterProvider.IncludeAccrualsReversing; }
			set
			{
				DataExporter.FilterProvider.IncludeAccrualsReversing = value;
				IncludeAccrualsReversingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeAccrualsReversingInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAccrualsReversing)); }
		}

		#endregion

		#region Include Unallocated AP Invoices

		public ZBool IncludeUnallocatedAPInvoices
		{
			get { return DataExporter.FilterProvider.IncludeUnallocatedAPInvoices; }
			set
			{
				DataExporter.FilterProvider.IncludeUnallocatedAPInvoices = value;
				IncludeUnallocatedAPInvoicesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeUnallocatedAPInvoicesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeUnallocatedAPInvoices)); }
		}

		#endregion

		#region Include Unallocated AP CreditNotes

		public ZBool IncludeUnallocatedAPCreditNotes
		{
			get { return DataExporter.FilterProvider.IncludeUnallocatedAPCreditNotes; }
			set
			{
				DataExporter.FilterProvider.IncludeUnallocatedAPCreditNotes = value;
				IncludeUnallocatedAPCreditNotesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeUnallocatedAPCreditNotesInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeUnallocatedAPCreditNotes)); }
		}

		#endregion

		#region ExcludeJobRelatedTransactionsForAR

		public ZBool ExcludeJobRelatedTransactionsForAR
		{
			get { return DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAR; }
			set
			{
				DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAR = value;
				ExcludeJobRelatedTransactionsForARInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExcludeJobRelatedTransactionsForARInfo
		{
			get { return GetZPropertyInfo(nameof(ExcludeJobRelatedTransactionsForAR)); }
		}

		#endregion

		#region ExcludeNonJobRelatedTransactionsForAR

		public ZBool ExcludeNonJobRelatedTransactionsForAR
		{
			get { return DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR; }
			set
			{
				DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR = value;
				ExcludeNonJobRelatedTransactionsForARInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExcludeNonJobRelatedTransactionsForARInfo
		{
			get { return GetZPropertyInfo(nameof(ExcludeNonJobRelatedTransactionsForAR)); }
		}

		#endregion

		#region ExcludeJobRelatedTransactionsForAP

		public ZBool ExcludeJobRelatedTransactionsForAP
		{
			get { return DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAP; }
			set
			{
				DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAP = value;
				ExcludeJobRelatedTransactionsForAPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExcludeJobRelatedTransactionsForAPInfo
		{
			get { return GetZPropertyInfo(nameof(ExcludeJobRelatedTransactionsForAP)); }
		}

		#endregion

		#region ExcludeNonJobRelatedTransactionsForAP

		public ZBool ExcludeNonJobRelatedTransactionsForAP
		{
			get { return DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP; }
			set
			{
				DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP = value;
				ExcludeNonJobRelatedTransactionsForAPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExcludeNonJobRelatedTransactionsForAPInfo
		{
			get { return GetZPropertyInfo(nameof(ExcludeNonJobRelatedTransactionsForAP)); }
		}

		#endregion

		#region DateFrom

		public ZDateTime DateFrom
		{
			get { return DataExporter.FilterProvider.DateFrom; }
			set
			{
				DataExporter.FilterProvider.DateFrom = value;
				DateFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateFromInfo
		{
			get { return GetZPropertyInfo(nameof(DateFrom)); }
		}

		#endregion

		#region DateTo

		public virtual ZDateTime DateTo
		{
			get { return DataExporter.FilterProvider.DateTo; }
			set
			{
				DataExporter.FilterProvider.DateTo = value;
				DateToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateToInfo
		{
			get { return GetZPropertyInfo(nameof(DateTo)); }
		}

		#endregion

		#region PeriodFrom

		public ZInt PeriodFrom
		{
			get { return DataExporter.FilterProvider.PeriodFrom; }
			set
			{
				DataExporter.FilterProvider.PeriodFrom = value;
				PeriodFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PeriodFromInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodFrom)); }
		}

		#endregion

		#region PeriodTo

		public ZInt PeriodTo
		{
			get { return DataExporter.FilterProvider.PeriodTo; }
			set
			{
				DataExporter.FilterProvider.PeriodTo = value;
				PeriodToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PeriodToInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodTo)); }
		}

		#endregion

		#region TransactionNumberFrom

		[MaxLength(AccTransactionHeader.Schema.AH_TransactionNumMaxLength)]
		public ZString TransactionNumberFrom
		{
			get { return DataExporter.FilterProvider.TransactionNumberFrom; }
			set
			{
				if (DataExporter.FilterProvider.TransactionNumberFrom != value)
				{
					CheckMaximumLength(TransactionNumberFromInfo, value);
					DataExporter.FilterProvider.TransactionNumberFrom = value;
				}
				TransactionNumberFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransactionNumberFromInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumberFrom)); }
		}

		protected bool TransactionNumberFrom_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region TransactionNumberTo

		[MaxLength(AccTransactionHeader.Schema.AH_TransactionNumMaxLength)]
		public ZString TransactionNumberTo
		{
			get { return DataExporter.FilterProvider.TransactionNumberTo; }
			set
			{
				if (DataExporter.FilterProvider.TransactionNumberTo != value)
				{
					CheckMaximumLength(TransactionNumberToInfo, value);
					DataExporter.FilterProvider.TransactionNumberTo = value;
				}
				TransactionNumberToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransactionNumberToInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumberTo)); }
		}

		protected bool TransactionNumberTo_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region High Water Mark

		public ZString HighWaterMarkMessage
		{
			get
			{
				var result = ZString.Empty;
				if (DataExporter.FilterProvider.UsingHighWaterMark)
				{
					result = Res.GetString("2E9A4576-DE64-40D4-8188-AB916036C0DE",
						@"You have selected the same transaction types as nominated in the registry ('{0}/{1}').
To aid performance of the export, the system will only search for un-batched transactions that were created or edited since {2}.
If you need to export transactions from before this date, please use the date filtering and this will search for all un-batched transactions.",
						DataExporter.FilterProvider.AccountingTransactionTypesRegistry.Category,
						DataExporter.FilterProvider.AccountingTransactionTypesRegistry.Caption, DataExporter.FilterProvider.HighWaterMark);
				}
				return result;
			}
		}

		public ZPropertyInfo HighWaterMarkMessageInfo
		{
			get { return GetZPropertyInfo(nameof(HighWaterMarkMessage)); }
		}

		void Collection_CountChanged(object sender, EventArgs e)
		{
			HighWaterMarkMessageInfo.RefreshBinding();
		}

		#endregion

		public JobHeaderCollection SelectedJobs
		{
			get { return DataExporter.FilterProvider.Jobs; }
		}

		public GlbBranchCollection SelectedBranches
		{
			get { return DataExporter.FilterProvider.Branches; }
		}

		public GlbDepartmentCollection SelectedDepartments
		{
			get { return DataExporter.FilterProvider.Departments; }
		}

		public OrgHeaderCollection SelectedOrganisations
		{
			get { return DataExporter.FilterProvider.Organisations; }
		}

		#region Existing Batch Number to ReExport

		public ZInt ExistingBatchNumberToExport
		{
			get { return DataExporter.FilterProvider.CurrentBatchNo; }
			set
			{
				DataExporter.FilterProvider.CurrentBatchNo = value;

				if (DataExporter.FilterProvider.CurrentBatchNo != 0)
				{
					IncludeAllTransactionTypes();
				}

				RaiseExistingBatchNumberToExportEvent();
				ExistingBatchNumberToExportInfo.RefreshBinding();
			}
		}

		void RaiseExistingBatchNumberToExportEvent()
		{
			if (ExistingBatchChanged != null)
			{
				ExistingBatchChanged(this, EventArgs.Empty);
			}
		}

		public ZPropertyInfo ExistingBatchNumberToExportInfo
		{
			get { return GetZPropertyInfo(nameof(ExistingBatchNumberToExport)); }
		}

		void IncludeAllTransactionTypes()
		{
			IncludeARInvoices = true;
			IncludeARCreditNotes = true;
			IncludeARAdjustmentNotes = true;
			IncludeAPInvoices = true;
			IncludeAPCreditNotes = true;
			IncludeAPAdjustmentNotes = true;
			IncludeAccrualsPosting = true;
			IncludeAccrualsReversing = true;
			IncludeWipsPosting = true;
			IncludeWipsReversing = true;
			IncludeUnallocatedAPInvoices = true;
			IncludeUnallocatedAPCreditNotes = true;
		}

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			var result = false;
			var properties = new List<ZString> {
				IncludeARInvoicesInfo.Name,
				IncludeARCreditNotesInfo.Name,
				IncludeARAdjustmentNotesInfo.Name,
				IncludeAPInvoicesInfo.Name,
				IncludeAPCreditNotesInfo.Name,
				IncludeAPAdjustmentNotesInfo.Name,
				IncludeWipsPostingInfo.Name,
				IncludeWipsReversingInfo.Name,
				IncludeAccrualsPostingInfo.Name,
				IncludeAccrualsReversingInfo.Name,
				ExcludeJobRelatedTransactionsForARInfo.Name,
				ExcludeNonJobRelatedTransactionsForARInfo.Name,
				ExcludeJobRelatedTransactionsForAPInfo.Name,
				ExcludeNonJobRelatedTransactionsForAPInfo.Name,
				DateFromInfo.Name,
				DateToInfo.Name,
				PeriodFromInfo.Name,
				PeriodToInfo.Name
			};
			if (properties.Contains(property.Name) && property.HasSetter())
			{
				result = ExistingBatchNumberToExport != 0;
			}
			return result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#endregion

		#region Validation

		#region Dates

		public void ValidateDateFrom()
		{
			DateFromInfo.ClearAllNotifications();

			TypeValidation.CheckValidSmallDateTime(DateFromInfo);
			TypeValidation.CheckValidZDateTimeRange(DateFromInfo);

			if (DateFrom.IsValid && !DateFromInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DateFrom);
				BusinessObject periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter);

				if (periodManagement == null)
				{
					DateFromInfo.AddError(Res.GetString("de52780f-9216-4658-b33e-bfb056c5d0df", "Please select a Date that falls within a valid period."));
				}
			}
			else if (DateFrom != ZDateTime.Empty)
			{
				DateFromInfo.AddError(Res.GetString("2c0e7d8b-44cd-40d4-bb21-d888482fb35f", "Please select a valid Date."));
			}

			ValidateDatesTogether();
		}

		public void ValidateDateTo()
		{
			DateToInfo.ClearAllNotifications();

			TypeValidation.CheckValidSmallDateTime(DateToInfo);
			TypeValidation.CheckValidZDateTimeRange(DateToInfo);

			if (DateTo.IsValid && !DateToInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DateTo);
				BusinessObject periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter);

				if (periodManagement == null)
				{
					DateToInfo.AddError(Res.GetString("de52780f-9216-4658-b33e-bfb056c5d0df", "Please select a Date that falls within a valid period."));
				}
			}
			else if (DateTo != ZDateTime.Empty)
			{
				DateToInfo.AddError(Res.GetString("2c0e7d8b-44cd-40d4-bb21-d888482fb35f", "Please select a valid Date."));
			}

			ValidateDatesTogether();
		}

		void ValidateDatesTogether()
		{
			if (!DateFrom.IsEmpty && DateTo.IsEmpty)
			{
				DateToInfo.AddError(Res.GetString("b43e8b40-743a-4dff-b1e3-0cb09f177cc0", "Please select a valid TO Date."));
			}
			else if (DateFrom.IsEmpty && !DateTo.IsEmpty)
			{
				DateFromInfo.AddError(Res.GetString("4b1a50da-eb27-4b11-94cd-3ef59e5e18f3", "Please select a valid FROM Date."));
			}
			else if (!DateFrom.IsEmpty && !DateTo.IsEmpty && DateFrom.Date > DateTo.Date)
			{
				DateToInfo.AddError(Res.GetString("c30e78d7-06d8-40ec-8439-102e9d4e8c7b",
					"FROM Date must be the same as or before TO Date."));
			}
			else if (DateFrom.IsEmpty && DateTo.IsEmpty)
			{
				DateFromInfo.ClearAllNotifications();
				DateToInfo.ClearAllNotifications();
			}
		}

		protected bool DatesHaveNoErrors
		{
			get
			{
				return
					!(DateFromInfo.HasErrors() || DateToInfo.HasErrors() || PeriodFromInfo.HasErrors() || PeriodToInfo.HasErrors() ||
					  TransactionNumberFromInfo.HasErrors() || TransactionNumberToInfo.HasErrors());
			}
		}

		#endregion

		#region Periods

		public void ValidatePeriodFrom()
		{
			if (!PeriodFrom.IsEmpty)
			{
				PeriodFromInfo.ClearAllNotifications();

				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccPeriodManagementSchema.AM_Period, PeriodFrom);
				BusinessObject periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter);

				if (periodManagement == null)
				{
					PeriodFromInfo.AddError(Res.GetString("69424a4d-cfbe-4c4c-879d-b292c0d6ecda", "Please select a valid period."));
				}
			}

			ValidatePeriodsTogether();
		}

		public void ValidatePeriodTo()
		{
			if (!PeriodTo.IsEmpty)
			{
				PeriodToInfo.ClearAllNotifications();

				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccPeriodManagementSchema.AM_Period, PeriodTo);
				BusinessObject periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter);

				if (periodManagement == null)
				{
					PeriodToInfo.AddError(Res.GetString("69424a4d-cfbe-4c4c-879d-b292c0d6ecda", "Please select a valid period."));
				}
			}

			ValidatePeriodsTogether();
		}

		void ValidatePeriodsTogether()
		{
			if (!PeriodFrom.IsEmpty && PeriodTo.IsEmpty)
			{
				PeriodToInfo.AddError(Res.GetString("dda2d8e5-df46-4708-9b51-0d8b19e390ee", "Please select a valid TO period."));
			}
			else if (PeriodFrom.IsEmpty && !PeriodTo.IsEmpty)
			{
				PeriodFromInfo.AddError(Res.GetString("8798381b-d087-4d40-a1be-fc14218136f1", "Please select a valid FROM period."));
			}
			else if (!PeriodFrom.IsEmpty && !PeriodTo.IsEmpty && PeriodFrom > PeriodTo)
			{
				PeriodToInfo.AddError(Res.GetString("4c9366ec-ac0a-4b60-ae79-d68f0c074f6b",
					"FROM period must be the same as or before TO period."));
			}
			else if (PeriodFrom.IsEmpty && PeriodTo.IsEmpty)
			{
				PeriodFromInfo.ClearAllNotifications();
				PeriodToInfo.ClearAllNotifications();
			}
		}

		#endregion

		#region TransactionNumbers

		public void ValidateTransactionNumberFrom()
		{
			ValidateTransactionNumbersTogether();
		}

		public void ValidateTransactionNumberTo()
		{
			ValidateTransactionNumbersTogether();
		}

		void ValidateTransactionNumbersTogether()
		{
			TransactionNumberFromInfo.ClearAllNotifications();
			TransactionNumberToInfo.ClearAllNotifications();
			if (!TransactionNumberFrom.IsEmpty && !TransactionNumberTo.IsEmpty &&
				(String.Compare(TransactionNumberFrom.ToString(), TransactionNumberTo.ToString()) > 0))
			{
				TransactionNumberFromInfo.AddError(Res.GetString("ceabbd62-0812-47d1-a44c-89fa22a1eb78",
					"FROM Transaction Number must be the same as or before TO Transaction Number."));
			}
		}

		#endregion

		#endregion

		#region Exporting

		public void Export()
		{
			Export(null);
		}

		public void Export(Form parentForm)
		{
			this.ParentForm = parentForm;
			try
			{
				DataExporter.ProcessingProgressed += new EventHandler(UpdateStatusAndPercentageComplete);
				if (DatesHaveNoErrors)
				{
					using (new AccountingUtils.CommandTimeoutInitializer(1800))
					{
						DoExport();
					}
				}
			}
			finally
			{
				DataExporter.ProcessingProgressed -= new EventHandler(UpdateStatusAndPercentageComplete);
			}
		}

		protected void DoExport()
		{
			if (IsOKToExport())
			{
				LoadFormAndExport();
				if (ExistingBatchNumberToExport == 0)
				{
					RaiseExistingBatchNumberToExportEvent();
				}
			}
		}

		protected virtual bool IsOKToExport()
		{
			bool result = true;

			if (ExistingBatchNumberToExport == 0)
			{
				ZInt numberOfAccrualPostingsSatisfyingFilterParams = DataExporter.FilterProvider.IncludeAccrualsPosting
					? DataExporter.NumberOfAccrualPostingsInBatch
					: (ZInt)0;
				ZInt numberOfAccrualReversalsSatisfyingFilterParams = DataExporter.FilterProvider.IncludeAccrualsReversing
					? DataExporter.NumberOfAccrualReversalsInBatch
					: (ZInt)0;
				ZInt numberOfWIPsPostingsSatisfyingFilterParams = DataExporter.FilterProvider.IncludeWIPsPosting
					? DataExporter.NumberOfWipPostingsInBatch
					: (ZInt)0;
				ZInt numberOfWIPsReversalsSatisfyingFilterParams = DataExporter.FilterProvider.IncludeWIPsReversing
					? DataExporter.NumberOfWipReversalsInBatch
					: (ZInt)0;

				if (!DataExporter.IsTransactionsExistInBatch)
				{
					Globals.Message.Show(
						Res.GetString("93174a56-5d2b-48d4-8e84-d73545f0314b",
							"Based on the criteria provided, there are currently no transactions to export"),
						Res.GetString("3d6ddfc2-b8f9-4712-90b9-1478def15c5a", "Accounting XML Export"),
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					result = false;
				}
				else if ((numberOfAccrualPostingsSatisfyingFilterParams +
						  // check to see how many WIP and Accrual lines will be generated and ensure that it is less than 32767
						  numberOfAccrualReversalsSatisfyingFilterParams +
						  numberOfWIPsPostingsSatisfyingFilterParams +
						  numberOfWIPsReversalsSatisfyingFilterParams) > short.MaxValue)
				{
					Globals.Message.Show(
						Res.GetString("7e5d1c98-bfcd-444b-8525-e90d97454c4b",
							"You cannot export more than 32767 WIP and Accrual transactions in a single batch.\r\nYou must modify the filter criteria to reduce the number of WIPs and Accruals being exported."),
						Res.GetString("3d6ddfc2-b8f9-4712-90b9-1478def15c5a", "Accounting XML Export"),
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					result = false;
				}
			}
			else if (!DataExporter.IsTransactionsExistInBatch)
			{
				Globals.Message.Show(
					Res.GetString("26f9d7b1-69ff-4ce9-bbfe-060cccbd83c6", "Unable to find any transactions in Batch Number {0}",
						ExistingBatchNumberToExport.ToString()),
					Res.GetString("3d6ddfc2-b8f9-4712-90b9-1478def15c5a", "Accounting XML Export"),
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				result = false;
			}
			return result;
		}

		protected virtual void LoadFormAndExport()
		{
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.DefaultExt = FileExtention;
				dialog.FileName = FileName;
				dialog.AddExtension = true;
				dialog.Filter = DialogFilter;
				dialog.InitialDirectory = InitialDirectory;

				if (ShowDialog(dialog) == DialogResult.OK)
				{
					using (var stream = dialog.OpenFile())
					{
						CreateFileAndExport(stream);
					}
				}
				else
				{
					Globals.Message.ShowInformation(
						Res.GetString("ede89500-8424-44bd-9187-10f7ac2036b5", "No Transactions were exported."),
						Res.GetString("a27c9812-1c32-4940-9a29-91a69f218ec5", "There are currently no transactions to export"));
				}
			}
		}

		protected void ExportAndShowResultsToUser(Stream stream)
		{
			try
			{
				DataExporter.Export(stream);
				Globals.Message.ShowInformation(DataExporter.GetMessageToDisplayWhenExportIsFinished(),
					Res.GetString("817c62ac-1def-4378-87d4-936921cbedb3", "Financial Transaction Export"));
			}
			catch (System.Data.Common.DbException ex)
			{
				if (ex.Message.Contains((NoResString)"Could not obtain lock - process is already running -1")) // handle specific exception
				{
					Globals.Message.ShowError(Res.GetString("08344b98-08a9-45a2-9fcb-11d750765b50", "Another user is performing an export. Please wait for sometime and try again. If the problem persists then please contact support."), Res.GetString("817c62ac-1def-4378-87d4-936921cbedb3", "Financial Transaction Export")); // Exception Messag
				}
				else
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("817c62ac-1def-4378-87d4-936921cbedb3", "Financial Transaction Export"));
				}
			}
		}

		protected virtual DialogResult ShowDialog(IFileDialog dialog)
		{
			return ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
		}

		protected virtual void CreateFileAndExport(Stream stream)
		{
			using (ProgressForm = new ProgressForm())
			{
				ShowProgressForm();
				ExportAndShowResultsToUser(stream);
			}
		}

		protected virtual void ShowProgressForm()
		{
			if (ProgressForm != null)
			{
				ProgressForm.ShowCancelButton = false;
				ProgressForm.CaptionResourceString = Res.GetData("Accounting|XmlExportGUIWrapper|TransactionsProcessedCaption",
					"Export Accounting Transactions");
				if (ParentForm != null)
				{
					ZFormModaliser.Show(ProgressForm, ParentForm);
				}
				else
				{
					ProgressForm.Show();
				}
			}
		}

		protected virtual string DialogFilter
		{
			get { return (NoResString)"XML Files | *.xml"; } // File extension filter
		}

		protected virtual string FileExtention
		{
			get { return ".xml"; }
		}

		protected virtual string InitialDirectory
		{
			get { return "\\"; }
		}

		protected virtual string FileName
		{
			get { return ZDateTime.Now.ToString("yyyy-MM-dd_HHmm"); }
		}

#if DEBUG
		internal
#endif
		protected virtual AccountingTransactionsDataExporter DataExporter
		{
			get
			{
				if (fDataExporter == null)
				{
					fDataExporter = new XmlAccountingTransactionExporter(Factory);
				}

				return fDataExporter;
			}
		}

		protected AccountingTransactionsDataExporter fDataExporter;

		protected ProgressForm ProgressForm;
		Form ParentForm;

		protected void UpdateStatusAndPercentageComplete(object sender, EventArgs e)
		{
			AccountingTransactionsDataExporter dataExporter = (AccountingTransactionsDataExporter)sender;
			if (ProgressForm != null)
			{
				string status = Res.GetString("Accounting|XmlExportGUIWrapper|TransactionsProcessedProgress",
					"{0} of {1} Transactions Processed", dataExporter.NumberOfTransactionsProcessed,
					dataExporter.LastBatchNumberOfTransactions);
				ProgressForm.SetStatusAndPercentComplete(status, dataExporter.PercentageComplete);
			}
		}

		#endregion
	}
}
