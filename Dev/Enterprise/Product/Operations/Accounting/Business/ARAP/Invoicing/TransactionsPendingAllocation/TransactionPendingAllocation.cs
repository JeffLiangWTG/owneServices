using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class TransactionPendingAllocation : InvoicingBase, IDocManagerSupport, IEDocsParsingSupport
	{
		public TransactionPendingAllocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			BlockAmountRecalculationFromLines();

			LocalForeignDataEntryCalculator = new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, AH_LocalExTaxAmountInfo, AH_OSExTaxAmountInfo, ExchangeRate as ZAccExchangeRate, false, true);
			LocalForeignTaxDataEntryCalculator = new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, AH_LocalTaxAmountInfo, AH_OSTaxAmountInfo, ExchangeRate as ZAccExchangeRate, true, true);
		}

		public void InitializeApprovalRequestData(string sourceXMLToStoreInRequest, bool isCrossLedgerImport)
		{
			request_sourceXMLToStoreInRequest = sourceXMLToStoreInRequest;
			request_isCrossLedgerImport = isCrossLedgerImport;
		}
		string request_sourceXMLToStoreInRequest = "";
		bool request_isCrossLedgerImport;

		void BlockAmountRecalculationFromLines()
		{
			if (lineAmountRecalculationBlocker == null)
			{
				lineAmountRecalculationBlocker = GetHeaderAmountsUpdateSuspender();
			}
		}

		IDisposable lineAmountRecalculationBlocker;

		public void SetParentCollection(TransactionPendingAllocationCollection collection)
		{
			this.ParentCollection = collection;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("7c670fc7-fab9-45b7-a27a-0f5408a6699b", "Unallocated Transaction"); }
		}

		protected override void SetDefaultValues()
		{
			BlockAmountRecalculationFromLines();

			base.SetDefaultValues();
			AH_TransactionCategory = "STD";
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected TransactionPendingAllocationCollection ParentCollection;

		public bool DoDuplicatesNumbersExistInParentCollection()
		{
			bool result = false;
			if (ParentCollection != null)
			{
				foreach (TransactionPendingAllocation transaction in ParentCollection)
				{
					if (transaction.PK != PK &&
						AH_OH.IsValid &&
						transaction.TransactionType == this.TransactionType &&
						transaction.AH_OH == this.AH_OH &&
						transaction.AH_TransactionNum == this.AH_TransactionNum)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString TransactionType
		{
			get
			{
				return AH_OSTotalAmount < 0 ?
						TransactionTypes.CreditNotePendingAllocation :
						TransactionTypes.InvoicePendingAllocation;
			}
		}

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				if (!IsInDatabase)
				{
					return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}|{1}|Created", AH_Ledger, AH_TransactionType);
				}

				return base.CustomLogReferenceSuffix;
			}
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.TransactionsPendingAllocation; }
		}

		protected override bool IsTransactionInDatabaseReadOnlyCore
		{
			get { return false; }
		}

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				ZGuid oldValue = base.AH_OHCore;
				if (oldValue != value)
				{
					base.AH_OHCore = value;
					if (Header != null)
					{
						if (!Header.CompanyData.IsAPTaxApplicable)
						{
							AH_OSTaxAmount = 0m;
						}
					}
				}
			}
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new TransactionPendingAllocationValidation(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Called indirectly through ValueChanged events from ZPropertyInfos which it attaches to during construction")]
		readonly LocalForeignDataEntry LocalForeignDataEntryCalculator;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Called indirectly through ValueChanged events from ZPropertyInfos which it attaches to during construction")]
		readonly LocalForeignDataEntry LocalForeignTaxDataEntryCalculator;

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		[ReadOnlyMember(nameof(AH_OSTaxAmount_ReadOnly))]
		public override ZDecimal AH_OSTaxAmount
		{
			get
			{
				return base.AH_OSTaxAmount;
			}
			set
			{
				ZDecimal oldValue = base.AH_OSTaxAmount;
				if (oldValue != value)
				{
					base.AH_OSTaxAmount = value;
					if (ParentCollection != null)
					{
						ParentCollection.Parent.TotalsDirty();
					}
				}
			}
		}

		public ZBool AH_OSTaxAmount_ReadOnly
		{
			get
			{
				return !(GlbCompany.CurrentCompany.GC_IsGSTRegistered &&
					Header != null && Header.CompanyData.IsAPTaxApplicable);
			}
		}

		protected override bool AllowDelete
		{
			get { return true; }
		}

		public override ZDecimal AH_OSExTaxAmount
		{
			get
			{
				return base.AH_OSExTaxAmount;
			}
			set
			{
				ZDecimal oldValue = base.AH_OSExTaxAmount;
				if (oldValue != value)
				{
					base.AH_OSExTaxAmount = value;
					if (value < 0)
					{
						AH_TransactionType = TransactionType;
					}
					if (ParentCollection != null)
					{
						ParentCollection.Parent.TotalsDirty();
					}
				}
			}
		}

		[List("Branches")]
		public override ZGuid AH_GB
		{
			get { return base.AH_GB; }
			set { base.AH_GB = value; }
		}

		[ReadOnly(false)]
		public override ZString AH_PlaceOfSupply
		{
			get => base.AH_PlaceOfSupply;
			set => base.AH_PlaceOfSupply = value;
		}

		[ReadOnly(true)]
		public override ZDecimal AH_LocalTaxAmount
		{
			get { return base.AH_LocalTaxAmount; }
			set { base.AH_LocalTaxAmount = value; }
		}

		protected override bool AH_LocalExTaxAmount_ReadOnly
		{
			get { return true; }
			set { }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase)
			{
				if (TransactionApprovalRequest == null && approvalRequestForNewTransaction == null)
				{
					approvalRequestForNewTransaction = Factory.New<TransactionPendingAllocationApprovalRequest>();
				}
				if (approvalRequestForNewTransaction != null)
				{
					approvalRequestForNewTransaction.Initialize(this, request_sourceXMLToStoreInRequest, request_isCrossLedgerImport);
					if (HasErrors)
					{
						approvalRequestForNewTransaction.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
					}
				}
			}
		}

		TransactionPendingAllocationApprovalRequest approvalRequestForNewTransaction;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && IsInDatabase)
			{
				approvalRequestForNewTransaction = null;
			}
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			bool exchangeRateAccurateEnough = Env.CurrentCompany.ExchangeRate.LocalToForeign(AH_LocalTaxAmount, AH_ExchangeRate, AH_RX_NKTransactionCurrency) == AH_OSTaxAmount;
			if (!exchangeRateAccurateEnough)
			{
				AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(AH_LocalTotalAmount, AH_OSTotalAmount);
			}
		}

		protected override TransactionHeader CopyTransaction()
		{
			TransactionHeader result = base.CopyTransaction();
			result.AH_OH = AH_OH;
			result.AH_Desc = AH_Desc;
			result.AH_GB = AH_GB;
			result.AH_GE = AH_GE;
			result.AH_OA_InvoiceAddressOverride = AH_OA_InvoiceAddressOverride;
			result.AH_OC_InvoiceContactOverride = AH_OC_InvoiceContactOverride;

			return result;
		}

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry => throw new NotSupportedException();

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode) => throw new NotSupportedException();

		public override Type DependentTransactionLineType
		{
			get { return typeof(InvoicingLineBase); }
		}

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new TransactionPendingAllocationWithNoLinesCollection(this);
		}

		[BusinessObjectTestExclude]
		public override ZBool AH_PostedToEFT
		{
			get { return base.AH_PostedToEFT; }
			set
			{
				//do nothing
			}
		}

		protected override bool IsUseJobExchangeRateApplicable => false;

		protected override void DeleteCore()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();

			base.DeleteCore();

			var transactionRelatedApprovalRequests = GetTransactionRelatedApprovalRequests(Factory);
			if (transactionRelatedApprovalRequests != null && transactionRelatedApprovalRequests.Length > 1)
			{
				ErrorReporter.ReportOnce("TransactionPendingAllocation_ApprovalRequest_OnDelete", "System allowed to delete the Transaction Pending Allocation which has more than one Approval Request. TPA should be allowed to delete when it has only one approval request.");
			}

			TransactionRelatedApprovalRequest?.Delete();
		}

		#region Approval Request

		public override bool HasApprovalRequest
		{
			get { return TransactionRelatedApprovalRequest != null; }
		}

		ZQuery GetTransactionRelatedApprovalRequestsQuery(BusinessObjectFactory factory)
		{
			return new ZQuery(new TransactionPendingAllocationApprovalRequestCollection(factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK)).CompleteFilter);
		}

		GenApprovalRequest[] GetTransactionRelatedApprovalRequests(BusinessObjectFactory factory)
		{
			var query = GetTransactionRelatedApprovalRequestsQuery(factory);

			return factory.Load<TransactionPendingAllocationApprovalRequest>(query);
		}

		protected override GenApprovalRequest GetLatestTransactionRelatedApprovalRequest(BusinessObjectFactory factory, bool alwaysCheckInDb = false)
		{
			var query = GetTransactionRelatedApprovalRequestsQuery(factory);
			query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";
			query.FetchOnlyFromLocalCache = !IsInDatabase && !alwaysCheckInDb;

			return factory.LoadTop1<TransactionPendingAllocationApprovalRequest>(query);
		}

		public TransactionPendingAllocationApprovalRequest TransactionApprovalRequest
		{
			get { return (TransactionPendingAllocationApprovalRequest)TransactionRelatedApprovalRequest; }
		}

		public override bool IsImportedFromUniversalXML => TransactionApprovalRequest != null && TransactionApprovalRequest.HasUniversalTransaction;

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return new InvoicingDocManagerInfo(this, Constants.DocManagerCodes.PayableTransactionPendingAllocation);
		}

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		public bool IsAllocated
		{
			get { return IsInstanceLedgerModified(); }
		}

		#region ICanDelete
		public bool TransactionHasBeenEdited()
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);

			return Factory.Exists(typeof(StmALog), query);
		}
		public override bool CanDelete
		{
			get
			{
				return (ExportedBatchSequence == null && !IsAllocated)
					&& ((!HasApprovalRequest)
						|| ((TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested
							|| TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Error)
							&& !TransactionHasBeenEdited()));
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason = (NoResString)string.Empty;
				if (ExportedBatchSequence != null)
				{
					reason = ResString.GetMultilingualString("178969a2-3fdc-49c1-b68e-c335a782792e", "This transaction has been exported and cannot be deleted.");
				}
				else if (IsAllocated)
				{
					reason = ResString.GetMultilingualString("528109C5-30D2-4735-8F9E-339A54678492", "This transaction has been allocated and cannot be deleted.");
				}
				else if (HasApprovalRequest)
				{
					if (TransactionApprovalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested && TransactionHasBeenEdited())
					{
						reason = ResString.GetMultilingualString("a0459550-15ef-4c12-b094-fbd8f0b27fe6", "This transaction cannot be deleted as it has been edited, you can Cancel the corresponding approval request.");
					}
					else
					{
						reason = ResString.GetMultilingualString("adcf13f6-357f-442c-aca2-2cb23f8a5ad6", "This transaction cannot be deleted as it is linked to approval request that has the status of APP, REJ or CAN.");
					}
				}
				return reason;
			}
		}

		#endregion

		protected override ZString GetPostedByCore()
		{
			return ZString.Empty;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new TransactionPendingAllocationFetchStrategy(this);

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;

				fNoteTypes.Add(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.DataImportLogNote);

				return fNoteTypes;
			}
		}

		public override bool ShouldSetExchangeRateWhenSetInvoiceDate => true;
	}
}
