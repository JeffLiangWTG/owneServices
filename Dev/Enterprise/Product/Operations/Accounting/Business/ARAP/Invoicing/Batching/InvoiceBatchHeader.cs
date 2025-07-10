using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeader : TransactionHeader, IInvoiceTerms
	{
		public InvoiceBatchHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
			((ModuleNkFilter)Filter["Currency"]).Property = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#region Initialization

		public void Initialization(InvoiceBatchHeader batchCopy)
		{
			Initialization(batchCopy, batchCopy.AH_OH);
		}

		public void Initialization(InvoiceBatchHeader batchCopy, ZGuid debtorPK)
		{
			AH_OH = debtorPK;
			SetJobTypeList(batchCopy.JobTypeList);
			AH_RX_NKTransactionCurrency = batchCopy.AH_RX_NKTransactionCurrency;
			AH_InvoiceDate = batchCopy.AH_InvoiceDate;
			if (!batchCopy.AH_InvoiceTerm.IsEmpty)
			{
				AH_InvoiceTerm = batchCopy.AH_InvoiceTerm;
			}
			if (!batchCopy.AH_InvoiceTermDays.IsEmpty)
			{
				AH_InvoiceTermDays = batchCopy.AH_InvoiceTermDays;
			}
			if (batchCopy.UpdateInvoiceBulkBatchTotals != null)
			{
				UpdateInvoiceBulkBatchTotals = batchCopy.UpdateInvoiceBulkBatchTotals;
			}
		}

		#endregion

		#region Callback Methods
		public Action<InvoicingBase> UpdateInvoiceBulkBatchTotals
		{
			get; set;
		}

		#endregion

		#region Lines & Related Method

		[ChildEditable(false)]
		public InvoiceBatchLineCollection Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new InvoiceBatchLineCollection(Factory, this);
					if (IsInDatabase)
					{
						fLine.Load();
						//fLine.SuspendValidation(); it hasn't appropriate ResumeValidation. It's cause of exception during canceling Invoice Batches
						HasChanges = false;
					}
					RegisterEditableChildObject(fLine);
				}

				return fLine;
			}
		}
		InvoiceBatchLineCollection fLine;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				Line.SetReadOnlyIncludingChildren(true);
			}
		}

		public void ClearLines()
		{
			Line.RemoveAll();
			SetAmountToZero();
		}

		public void SetAmountToZero()
		{
			AH_OutstandingAmount = 0m;
			AH_GSTAmount = 0m;
			AH_OSTotal = 0m;
			AH_InvoiceAmount = 0m;
			AH_WithholdingTax = 0m;
		}

		internal void UpdateSelectedTotal(InvoicingBase invoice)
		{
			var multiplier = invoice.IncludeInTheBatch ? 1.0m : -1.0m;

			AH_OutstandingAmount += invoice.AH_OutstandingAmount * multiplier;
			AH_GSTAmount += invoice.AH_GSTAmount * multiplier;
			AH_OSTotal += invoice.AH_OSTotal * multiplier;
			AH_InvoiceAmount += invoice.AH_InvoiceAmount * multiplier;
			AH_WithholdingTax += invoice.AH_WithholdingTax * multiplier;

			if (UpdateInvoiceBulkBatchTotals != null)
			{
				UpdateInvoiceBulkBatchTotals(invoice);
			}
		}

		#endregion

		#region Lookups

		#region Invoice Terms

		public CodeDescriptionPairList InvoiceTerms_List
		{
			get { return FindboxLookupCollections.GetARInvoiceTermsList(Factory); }
		}

		#endregion

		#region Invoice Types

		public CodeDescriptionPairList GetInvoiceTypeModuleList()
		{
			return GetInvoiceTypeModuleList(AH_OH);
		}

		public CodeDescriptionPairList GetInvoiceTypeModuleList(ZGuid orgHeaderPK)
		{
			CodeDescriptionPairList listToReturn = new CodeDescriptionPairList();
			if (!IsInDatabase)
			{
				OrgHeader selectedOrgHeader = Factory.Load<OrgHeader>(orgHeaderPK);
				if (selectedOrgHeader != null)
				{
					foreach (OrgInvoiceType invoiceType in selectedOrgHeader.CompanyData.InvoiceTypes)
					{
						listToReturn.AddPair(invoiceType.PI_Module, InvoiceTypeLookUp.GetDescriptionFromCode(invoiceType.PI_Module));
					}
				}
			}
			else
			{
				listToReturn = InvoiceTypeLookUp;
			}
			return listToReturn;
		}

		InvoiceTypeModuleList fInvoiceTypeLookUp;
		InvoiceTypeModuleList InvoiceTypeLookUp
		{
			get
			{
				if (fInvoiceTypeLookUp == null)
				{
					fInvoiceTypeLookUp = new InvoiceTypeModuleList();
					fInvoiceTypeLookUp.RemoveCode(InvoiceTypeModuleList.Codes.TCN);
					if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.UnitedStates)
					{
						fInvoiceTypeLookUp.RemoveCode(InvoiceTypeModuleList.Codes.ISF);
					}
				}
				return fInvoiceTypeLookUp;
			}
		}

		#endregion

		#endregion

		#region Filter Business Object

		protected InvoiceBatchHeaderFilterBusinessObject fFilter;
		public virtual InvoiceBatchHeaderFilterBusinessObject Filter
		{
			get
			{
				if (fFilter == null)
				{
					fFilter = new InvoiceBatchHeaderFilterBusinessObject();
					fFilter.SetParent(this);
				}
				return fFilter;
			}
		}

		#endregion

		#region Base Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("cbbb5bb3-25a0-4380-9ce6-9e130f11a0e8", "Invoice Batch"); }
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase)
			{
				foreach (InvoicingBase batchLine in Line)
				{
					if (batchLine.IncludeInTheBatch)
					{
						batchLine.AH_AH_InvoiceStatement = PK;
						SetReceiptBatchNo(batchLine);

						using (batchLine.SetExchangeRateSuspender.GetSuspender())
						{
							batchLine.AH_InvoiceDate = AH_InvoiceDate;
							batchLine.AH_DueDate = AH_DueDate;
						}
					}
				}

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.InvoiceBatchOnSavingDetailInfo,
					() => {
						var linesLog = new StringBuilder();
						linesLog.AppendLine(FormattableString.Invariant($"batch original AH_ExchangeRate: {AH_ExchangeRate}"));
						linesLog.AppendLine((NoResString)"batch lines info:");
						foreach (InvoicingBase batchLine in Line)
						{
							linesLog.AppendLine(FormattableString.Invariant(
								$"IncludeInTheBatch:{batchLine.IncludeInTheBatch}, Cur:{batchLine.AH_RX_NKTransactionCurrency}, ExtRate:{batchLine.AH_ExchangeRate}, OsAmount:{batchLine.AH_OSTotal}, LocalAmount:{batchLine.AH_LocalTotal}, Outstanding:{batchLine.AH_OutstandingAmount}, TaxAmount:{batchLine.AH_GSTAmount}"
							));
						}
						return linesLog.ToString();
					},
					CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE
				);

				AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(AH_LocalTotal, AH_OSTotal);
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "AH_TransactionNum is currently 8 characters for batching")]
		void SetReceiptBatchNo(TransactionHeader batchLine)
		{
			batchLine.AH_ReceiptBatchNo = AH_TransactionNum;
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.InvoiceBatch; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.InvoiceBatchNo; }
		}

		protected override void SetTransactionBelongsToGroupFieldCore(ZGuid groupingGuidValue)
		{
			// DO nothing.
		}

		#endregion

		#region Property Info Override

		protected bool AH_InvoiceAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSTotal_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_GSTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_DueDate_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_InvoiceTermDays_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) || AH_InvoiceTerm == Constants.InvoiceTerms.CashOnDelivery; }
		}

		protected bool AH_InvoiceTerm_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		#endregion

		#region Property Override

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get
			{
				return base.AH_OHCore;
			}
			set
			{
				if (base.AH_OHCore != value)
				{
					ClearExistingLines();

					// Clear Dependent Fields
					AH_RX_NKTransactionCurrency = ZString.Empty;
					ResetJobTypeList();
					base.AH_OHCore = value;

					SetCurrencyBasedOnDebtor();
					SetJobTypeBasedOnDebtor();

					if (Filter != null)
					{
						((ModuleGuidFilter)Filter["Organisation"]).Property = value;
					}

					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
				}
			}
		}

		void SetJobTypeBasedOnDebtor()
		{
			if (JobTypeList.Count == 1)
			{
				JobTypeList[0].Value = true;
			}
		}

		void SetCurrencyBasedOnDebtor()
		{
			if (Header != null && Header.CompanyData != null && Header.CompanyData.ARDDefltCurrency != null)
			{
				AH_RX_NKTransactionCurrency = Header.CompanyData.ARDDefltCurrency.RX_Code;
			}
			else
			{
				AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		public override ZString AH_RX_NKTransactionCurrency
		{
			get
			{
				return base.AH_RX_NKTransactionCurrency;
			}
			set
			{
				if (base.AH_RX_NKTransactionCurrency != value)
				{
					ClearExistingLines();
					base.AH_RX_NKTransactionCurrency = value;
					if (Filter != null)
					{
						((ModuleNkFilter)Filter["Currency"]).Property = value;
					}
				}
			}
		}

		#endregion

		#region AH_InvoiceDate

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}

		#endregion

		#region AH_InvoiceTerm

		[List("InvoiceTerms_List")]
		public override ZString AH_InvoiceTerm
		{
			get { return base.AH_InvoiceTerm; }
			set
			{
				base.AH_InvoiceTerm = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
				if (value == Constants.InvoiceTerms.CashOnDelivery)
				{
					AH_InvoiceTermDays = ZByte.Zero;
				}
			}
		}

		#endregion

		#region AH_InvoiceTermDays

		public override ZByte AH_InvoiceTermDays
		{
			get { return base.AH_InvoiceTermDays; }
			set
			{
				base.AH_InvoiceTermDays = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}

		internal TermsAndDueDateCalculationProvider TermsAndDueDateCalculationProvider
		{
			get
			{
				fTermsAndDueDateCalculationProvider = new ARTermsAndDueDateCalculationProvider(this);
				return fTermsAndDueDateCalculationProvider;
			}
		}

		TermsAndDueDateCalculationProvider fTermsAndDueDateCalculationProvider;

		#endregion

		#region Job Type List

		public ZBoolDescriptionPairList JobTypeList
		{
			get
			{
				if (fJobTypeList == null)
				{
					List<ZString> moduleList = BatchInvoiceModuleList;
					fJobTypeList = new ZBoolDescriptionPairList();
					if (!JobTypeListLoaded)
					{
						JobTypeListLoaded = true;
						SetJobTypeList(moduleList);
					}
					foreach (CodeDescriptionPair pair in InvoiceTypeLookUp)
					{
						fJobTypeList.AddNew(GetDescriptionForJobTypeList(pair.Code), false);
					}
					fJobTypeList.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(fJobTypeList_OnPairChanged);
				}
				return fJobTypeList;
			}
		}
		ZBoolDescriptionPairList fJobTypeList;

		bool JobTypeListLoaded;

		void fJobTypeList_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			Validation.ValidateAH_OH();
		}

		public List<ZString> SelectedJobTypeCodes
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (ZBoolDescriptionPair jobType in JobTypeList)
				{
					if (jobType.Value)
					{
						result.Add(GetJobTypeCodeFromJobTypeListDescription(jobType.Description));
					}
				}
				return result;
			}
		}

		void SetJobTypeList(List<ZString> sourceList)
		{
			for (int i = 0; i < sourceList.Count; i++)
			{
				ZString desc = GetDescriptionForJobTypeList(sourceList[i]);
				if (JobTypeList[desc] != null)
				{
					JobTypeList[desc].Value = true;
				}
			}
		}

		void SetJobTypeList(ZBoolDescriptionPairList sourceList)
		{
			for (int i = 0; i < sourceList.Count; i++)
			{
				ZString desc = sourceList[i].Description;
				if (JobTypeList[desc] != null)
				{
					JobTypeList[desc].Value = sourceList[i].Value;
				}
			}
		}

		public string GetDescriptionForJobTypeList(string jobTypeCode)
		{
			return InvoiceTypeLookUp.GetDescriptionFromCode(jobTypeCode);
		}

		public string GetJobTypeCodeFromJobTypeListDescription(string description)
		{
			return InvoiceTypeLookUp.GetCodeFromDescription(description);
		}

		void ResetJobTypeList()
		{
			foreach (ZBoolDescriptionPair jobType in JobTypeList)
			{
				jobType.Value = false;
			}
		}

		#endregion

		#region Batch Invoice Module List

		public List<ZString> BatchInvoiceModuleList
		{
			get
			{
				List<ZString> result = new List<ZString>();
				if (Line != null && Line.Count > 0)
				{
					foreach (InvoicingBase invoice in Line)
					{
						ZString moduleCode = GetInvoiceModuleCode(invoice);
						if (!moduleCode.IsEmpty && !result.Contains(moduleCode))
						{
							result.Add(moduleCode);
						}
					}
				}

				return result;
			}
		}

		public ZString BathInvoiceModuleListDescription
		{
			get
			{
				StringBuilder result = new StringBuilder();

				List<ZString> jobTypeList = BatchInvoiceModuleList;
				foreach (ZString jobType in jobTypeList)
				{
					if (InvoiceTypeLookUp.ContainsCode(jobType))
					{
						result.Append(InvoiceTypeLookUp.GetDescriptionFromCode(jobType) + ", ");
					}
				}
				return result.Length > 2 ? (ZString)result.Remove(result.Length - 2, 2).ToString() : ZString.Empty;
			}
		}

		public ZString GetInvoiceModuleCode(InvoicingBase invoice)
		{
			return invoice == null ? ZString.Empty : (invoice.InvoicingJob == null ? (ZString)InvoiceTypeModuleList.Codes.MSC : invoice.InvoicingJob.GetInvoiceTypeModule());
		}

		#endregion

		void ClearExistingLines()
		{
			//AH_OSTotalInfo.ClearAllNotifications();
			using (GetValidationSuspender())
			{
				if (Line.Count > 0)
				{
					Line.ClearAllLines();
				}
			}
		}

		#endregion

		#region Validation Object

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new InvoiceBatchHeaderValidation(this);
		}

		#endregion

		#region IReversing Members

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			fIsReversing = true;

			((IReversing)this).SetCancellationFlag(true);

			foreach (InvoicingBase batchLine in Line)
			{
				batchLine.AH_ReceiptBatchNo = "";
				batchLine.AH_AH_InvoiceStatement = ZGuid.Empty;
			}

			Line.SetIncludeBatchFlags(false);

			AH_OSExTaxAmount = 0;
			AH_LocalExTaxAmount = 0;
			AH_LocalTaxAmount = 0;
			AH_OSTaxAmount = 0;
			AH_OutstandingAmount = 0;
			AH_WithholdingTax = 0;
			AH_ExchangeRate = 1;

			fReverseTransaction = this;
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;
		}

		#endregion

		#region IInvoiceTerms members

		JobInvoicingConsumerType IInvoiceTerms.JobType
		{
			get { return null; }
		}

		ZString IInvoiceTerms.Direction
		{
			get { return ZString.Empty; }
		}

		ZString IInvoiceTerms.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZGuid IInvoiceTerms.AH_GB
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IInvoiceTerms.AH_GE
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return InvoiceBatchHeaderDocumentSupporter.New(this); }
		}

		#endregion
	}

	#region DocumentSupporter

	public class InvoiceBatchHeaderDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		protected InvoiceBatchHeaderDocumentSupporter(InvoiceBatchHeader invoiceBatchHeader)
				: base(invoiceBatchHeader)
		{
		}

		public static InvoiceBatchHeaderDocumentSupporter New(InvoiceBatchHeader invoiceBatchHeader)
		{
			InvoiceBatchHeaderDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoiceBatchHeader);
			}
			else if (invoiceBatchHeader != null)
			{
				result = new InvoiceBatchHeaderDocumentSupporter(invoiceBatchHeader);
			}

			return result;
		}

		protected delegate InvoiceBatchHeaderDocumentSupporter NewDelegate(InvoiceBatchHeader invoiceBatchHeader);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Constants.DataContext.ARBatchInvoice)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ARBatchInvoice, TransactionHeader) };
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				BusinessContext context = BusinessContext.ARBatchInvoice;

				if (BatchHeader.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					context = BusinessContext.INVALID;
				}

				return context;
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.ARBatchInvoice };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return new OrgHeaderContact(BatchHeader.Header, null);
		}

		protected InvoiceBatchHeader BatchHeader
		{
			get { return (InvoiceBatchHeader)BusinessObject; }
		}
	}

	#endregion
}
