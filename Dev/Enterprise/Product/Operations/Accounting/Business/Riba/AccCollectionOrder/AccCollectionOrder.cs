using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Riba
{
	[CodeProperty(Schema.ACO_OrderNumber)]
	[DescriptionProperty("Description")]
	[PreventDelete(false)]
	public class AccCollectionOrder : AutoAccCollectionOrder, IDocumentSupportable, IWorkflowProvider, IDocManagerSupport, IEDocsParsingSupport
	{
		public AccCollectionOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		AccCollectionOrderLineCollection fCollectionOrderLines;
		public AccCollectionOrderLineCollection CollectionOrderLines
		{
			get
			{
				if (fCollectionOrderLines == null)
				{
					var query = new ZQuery(AccCollectionOrderLineSchema.AOL_ACO, PK);
					fCollectionOrderLines = new AccCollectionOrderLineCollection(Factory, query);
					foreach (var line in fCollectionOrderLines)
					{
						line.SetIncludeInOrderWithoutRecalculateOrderAmount(ZBool.True);
					}
				}
				return fCollectionOrderLines;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("810C56DA-ED84-4055-8EED-EAA02BBDA98D", "Collection Order");

		public ZBool IsMatchedWithReceipt
		{
			get
			{
				ZBool result = false;
				if (CollectionOrderLines != null && CollectionOrderLines.Count > 0)
				{
					return CollectionOrderLines.Any(x => x.IsMatchedWithReceipt);
				}
				return result;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[RelatedBusinessObject("CollectionBatch")]
		public override ZGuid ACO_ACB
		{
			get { return base.ACO_ACB; }
			set { base.ACO_ACB = value; }
		}

		protected virtual bool ACO_CollectionDate_ReadOnly => Order_ReadOnly;

		public AccCollectionBatch CollectionBatch => Factory.Load<AccCollectionBatch>(ACO_ACB);

		public ZString Description => Res.GetString("9371EF81-3A15-44BD-B9B9-03BB25867E05", "Collection Order");

		[ReadOnly(true)]
		[RelatedBusinessObject("Currency")]
		[List("Lookups.Currencies")]
		public ZString ACO_RX_NKCurrency => CollectionBatch?.ACB_RX_NKCurrency ?? ZString.Empty;

		public ZString ACO_BatchNumber => CollectionBatch?.ACB_BatchNumber ?? ZString.Empty;

		[List("Lookups.BankAccounts")]
		public ZGuid BankAccount => CollectionBatch?.ACB_AB ?? ZGuid.Empty;

		public ZString ACO_Calc_AB_Code => CollectionBatch?.BankAccount?.AB_Code ?? ZString.Empty;

		public ZString ACO_BatchType => CollectionBatch?.ACB_Type ?? ZString.Empty;

		public ZString CollectionRequestBankName => CollectionRequestBankAccountDetail?.A1_BankName ?? ZString.Empty;

		public ZString CollectionRequestBankBranchName => CollectionRequestBankAccountDetail?.A1_BankBranchName ?? ZString.Empty;

		public ZBool CollectionRequestBankIsDefault => CollectionRequestBankAccountDetail?.A1_IsDefaultAccount ?? ZBool.False;

		public ZString CollectionRequestBankCountry => CollectionRequestBankAccountDetail?.Country?.RN_DescMultilingual ?? ZString.Empty;

		public ZString CollectionRequestIBANNumber => CollectionRequestBankAccountDetail?.A1_IBANNumber ?? ZString.Empty;

		public ZString CollectionRequestUMRReference => UMRReferenceDocument?.EQ_DocNumber ?? ZString.Empty;

		public ZDateTime CollectionRequestUMRSignedDate => UMRReferenceDocument?.EQ_DateReceived.ToZDateTime() ?? ZDateTime.Empty;

		JobRequiredDocument UMRReferenceDocument
		{
			get
			{
				JobRequiredDocument result = null;
				if (Debtor != null)
				{
					result = Debtor.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(
						x => x.EQ_DocType == "UMR" &&
						x.EQ_DocUsage == "DBT" &&
						x.EQ_DocPeriod == "PER" &&
						ZDateTime.Now <= x.EQ_ValidToDate);
				}
				return result;
			}
		}

		public ZString CollectionRequestAccountCurrency => CollectionRequestBankAccountDetail?.A1_RX_NKAccountCurrency ?? ZString.Empty;

		public ZString CollectionRequestBankBsb => CollectionRequestBankAccountDetail?.A1_BankBsb ?? ZString.Empty;

		public ZString CollectionRequestAccountNumber => CollectionRequestBankAccountDetail?.A1_BankAccount ?? ZString.Empty;

		public ZString CollectionRequestAccountName => CollectionRequestBankAccountDetail?.A1_AccountName ?? ZString.Empty;

		public ZString CollectionRequestBankSwift => CollectionRequestBankAccountDetail?.A1_BankSwift ?? ZString.Empty;

		public AccARAccountDetails CollectionRequestBankAccountDetail
		{
			get
			{
				if (collectionRequestBankAccountDetail == null && Debtor?.CompanyData != null)
				{
					collectionRequestBankAccountDetail = Debtor.CompanyData.ARAccountDetailsCollection.Cast<AccARAccountDetails>().
															FirstOrDefault(x => x.A1_PaymentMethod == AccARAccountDetails.ARCollectionRequest
																		&& x.A1_IsDefaultAccount == ZBool.True
																		&& x.A1_RX_NKAccountCurrency == ACO_RX_NKCurrency);
				}
				return collectionRequestBankAccountDetail;
			}
		}
		AccARAccountDetails collectionRequestBankAccountDetail;

		[ReadOnly(true)]
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal ACO_Amount
		{
			get
			{
				return base.ACO_Amount;
			}
			set
			{
				if (IncludeInBatch && !IsCancelled && (ACO_Amount != value))
				{
					CollectionBatch.ACB_TotalAmount = CollectionBatch.ACB_TotalAmount - ACO_Amount + value;
				}
				base.ACO_Amount = value;
			}
		}

		RefCurrency Currency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ACO_RX_NKCurrency);

		public int CurrencyDecimals => Currency?.Decimals ?? LocalDecimals;

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public DebtorValidation DebtorValidationType { get; set; }

		[RelatedBusinessObject("Debtor")]
		[List("Lookups.Debtors")]
		public override ZGuid ACO_OH_Debtor
		{
			get { return base.ACO_OH_Debtor; }
			set { base.ACO_OH_Debtor = value; }
		}

		[ReadOnlyMember(nameof(Order_ReadOnly))]
		public ZBool IncludeInBatch
		{
			get
			{
				return fIncludeInBatch;
			}
			set
			{
				bool hasChanges = fIncludeInBatch != value;
				fIncludeInBatch = value;
				IncludeInBatchInfo.RefreshBinding();
				if (hasChanges && !IsCancelled && CollectionBatch != null)
				{
					if (value)
					{
						CollectionBatch.ACB_TotalAmount += ACO_Amount;
					}
					else
					{
						CollectionBatch.ACB_TotalAmount -= ACO_Amount;
					}
				}
			}
		}
		ZBool fIncludeInBatch = ZBool.False;

		public override void OnSaving()
		{
			base.OnSaving();

			DeleteNotIncludedLines();
		}

		internal void DeleteNotIncludedLines()
		{
			foreach (var line in CollectionOrderLines.Where(line => !line.IncludeInOrder).ToArray())
			{
				line.Delete();
			}
		}

		public ZPropertyInfo IncludeInBatchInfo => GetZPropertyInfo(nameof(IncludeInBatch));

		public bool Order_ReadOnly => IsCancelled || IsMatchedWithReceipt || ACO_DepositedDate.IsValid;

		public void SetIncludeInBatchWithoutRecalculateBatchAmount(ZBool isIncluded)
		{
			fIncludeInBatch = isIncluded;
		}

		public DocumentSupporter DocumentSupporter => new AccCollectionOrderDocumentSupporter(this);

		#region IWorkflowSupporter

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccCollectionOrderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CollectionOrderCode; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CollectionOrder);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();

			if (IncludeInBatch)
			{
				CollectionBatch.ACB_TotalAmount -= ACO_Amount;
			}
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public void Reject(ZString rejectionReason)
		{
			ACO_CancelledReason = rejectionReason;
			foreach (var line in CollectionOrderLines)
			{
				line.IsCancelled = true;
			}
			IsCancelled = true;
			ZDecimal newBatchTotal = CollectionBatch.ACB_TotalAmount - ACO_Amount;
			if (newBatchTotal == 0)
			{
				CollectionBatch.IsCancelled = true;
			}
			CollectionBatch.ACB_TotalAmount = newBatchTotal;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			CollectionBatch.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Order Number {0} rejected.", ACO_OrderNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

#if DEBUG
		public bool SimulateExceptionCondition
		{
			get;
			set;
		}
#endif

		public ARReceipt CreateReceiptsAndDepositBatch(BusinessObjectFactory newFactory, ZDateTime backPostDate, ZDateTime backInvoiceDate, bool skipCreateDepositBatch, bool doNotSaveFactoryOnMatching = false)
		{
#if DEBUG
			if (Globals.IsTest && SimulateExceptionCondition)
			{
				throw new ReceiptMatchingProcessFailedException(ACO_OrderNumber);
			}
#endif
			var shouldUpdateDepositedDate = ValidateDepositedDate(out string log);
			if (!shouldUpdateDepositedDate)
			{
				backPostDate = ACO_DepositedDate;
				backInvoiceDate = ACO_DepositedDate;
			}
			ARReceipt receipt = null;
			var transactionsAffected = new List<ZGuid>();
			foreach (var line in CollectionOrderLines)
			{
				if (line.IncludeInOrder && !line.IsCancelled && !line.IsMatchedWithReceipt)
				{
					transactionsAffected.Add(line.AOL_AH);
				}
			}
			if (transactionsAffected.Count == 0)
			{
				throw new OrderIsAlreadyCancelledOrPaidException(ACO_OrderNumber);
			}
			else
			{
				var transactions = newFactory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, transactionsAffected.ToArray()));
				receipt = newFactory.New<ARReceipt>();
				receipt.AH_PostDate = backPostDate;
				receipt.AH_InvoiceDate = backInvoiceDate;
				receipt.AH_OH = ACO_OH_Debtor;
				receipt.AH_AB = CollectionBatch.ACB_AB;
				receipt.AH_RX_NKTransactionCurrency = ACO_RX_NKCurrency;
				receipt.AH_ReceiptType = ReceiptTypes.DirectCredit;				// always DCR receipt?
				receipt.SkipCreateDepositBatch = skipCreateDepositBatch;
				receipt.AH_ChequeOrReference = ZString.Format("{0}/{1}", CollectionBatch.ACB_BatchNumber, ACO_OrderNumber);
				receipt.AH_OSExTaxAmount = ACO_Amount;

				ZDecimal totalLocalOutstandingAmount = transactions.Sum(x => x.AH_OutstandingAmount);
				receipt.AH_InvoiceAmount = ACO_Amount * -1;
				receipt.AH_OutstandingAmount = ACO_Amount * -1;

				bool receiptIsInForeignCurrency = ACO_RX_NKCurrency != Env.CurrentCompany.LocalCurrency.Code;
				if (receiptIsInForeignCurrency)
				{
					receipt.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(totalLocalOutstandingAmount, ACO_Amount, 6);
				}
				else
				{
					receipt.AH_ExchangeRate = 1m;
				}

				receipt.AH_LocalExTaxAmount = totalLocalOutstandingAmount;  // fix decimal number roundings when the exchange rate is not accruate enough

				foreach (var t in transactions)
				{
					((IMatching)t).OSPartialPaymentAmount = t.AH_OSOutstandingAmountWithoutMultiplier;
				}

				if (receipt != null && shouldUpdateDepositedDate)
				{
					UpdateDepositedDate(backPostDate.Date, log);
				}

				var matching = new ARMatchingBase(newFactory, receipt, false);
				matching.MatchedTransactions.AddRange(transactions);
				matching.DoNotSaveFactoryOnMatching = doNotSaveFactoryOnMatching;
				if (!matching.MatchAndClearTransactionsWithSaveErrorHandling())		// DCR receipt will create a deposit batch (unless SkipCreateDepositBatch is set)
				{
					throw new ReceiptMatchingProcessFailedException(ACO_OrderNumber, matching.Corrupted);
				}
				else if (!skipCreateDepositBatch)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					CollectionBatch.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Receipt and Deposit Batch created for Order Number {0}.", ACO_OrderNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					CollectionBatch.Logs.Factory.Save();
				}
			}
			return receipt;
		}

		void UpdateDepositedDate(ZDate backPostDate, string log)
		{
			ACO_DepositedDate = backPostDate;

			if (!string.IsNullOrEmpty(log))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(AutoEvents.EditedARecord, log);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "log reference information")]
		ZBool ValidateDepositedDate(out string log)
		{
			log = string.Empty;

			if (ACO_DepositedDate.IsEmpty)
			{
				return true;
			}
			else
			{
				if (ACO_DepositedDate > ZDate.Today)
				{
					log = FormattableString.Invariant($"Deposited Date of {ACO_DepositedDate.ToString("dd-MMM-yy", CultureInfo.InvariantCulture)} was in the future and discarded.");
					return true;
				}
				else
				{
					var period = PeriodCalculator.GetPeriodFromDate(ACO_DepositedDate);
					var isInClosedPeriod = PeriodCalculator.IsPeriodSubLedgerClosed(period);
					if (isInClosedPeriod)
					{
						log = FormattableString.Invariant($"Deposited Date of {ACO_DepositedDate.ToString("dd-MMM-yy", CultureInfo.InvariantCulture)} falls in closed sub ledger period and discarded.");
					}
					return isInClosedPeriod;
				}
			}
		}

		public enum DebtorValidation
		{
			NoDebtorValidation,
			DebtorIsRequired,
			DebtorShouldBeEmpty
		}

		AccountingPeriodCalculator PeriodCalculator => periodCalculator ?? (periodCalculator = new AccountingPeriodCalculator(Factory));
		AccountingPeriodCalculator periodCalculator;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			ACO_CollectionDate = ZDateTime.Today.Date;
			ACO_Amount = 50m;
			CollectionBatch.ACB_TotalAmount = 50m;
		}
#endif
	}

	[Serializable]
	public abstract class CollectionBatchProcessException : Exception
	{
		public CollectionBatchProcessException()
		{ }

#if NETFRAMEWORK
		protected CollectionBatchProcessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public abstract ZString UserFriendlyMessage
		{
			get;
		}
	}

	[Serializable]
	public class OrderIsAlreadyCancelledOrPaidException : CollectionBatchProcessException
	{
		public OrderIsAlreadyCancelledOrPaidException()
		{ }

		public OrderIsAlreadyCancelledOrPaidException(ZString orderNumber)
		{
			this.orderNumber = orderNumber;
		}

		readonly ZString orderNumber;

#if NETFRAMEWORK
		protected OrderIsAlreadyCancelledOrPaidException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage => RibaProcessErrorMessages.OrderIsAlreadyCancelledOrPaidExceptionMessage(orderNumber);
	}

	[Serializable]
	public class ReceiptMatchingProcessFailedException : CollectionBatchProcessException
	{
		public ReceiptMatchingProcessFailedException()
		{ }

		public ReceiptMatchingProcessFailedException(ZString orderNumber, bool saveFailed = false)
		{
			this.orderNumber = orderNumber;
			this.saveFailed = saveFailed;
		}

		readonly ZString orderNumber;
		readonly ZBool saveFailed;

#if NETFRAMEWORK
		protected ReceiptMatchingProcessFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override ZString UserFriendlyMessage
		{
			get
			{
				if (saveFailed)
				{
					return RibaProcessErrorMessages.ReceiptMatchingProcessFailedDueToSaveException(orderNumber);
				}
				else
				{
					return RibaProcessErrorMessages.ReceiptMatchingProcessFailedException(orderNumber);
				}
			}
		}
	}

	public static class RibaProcessErrorMessages
	{
		public static string OrderIsAlreadyCancelledOrPaidExceptionMessage(ZString orderNumber)
		{
			return Res.GetString("7cf28690-61e9-43f6-9741-87a7161d3d62", "Order {0} is already canceled or fully paid.", orderNumber);
		}

		public static string ReceiptMatchingProcessFailedException(ZString orderNumber)
		{
			return Res.GetString("32ad651a-7366-4a45-a880-16cd7f17475a", "Receipt matching process failed for the transactions in the order {0}.", orderNumber);
		}

		public static string ReceiptMatchingProcessFailedDueToSaveException(ZString orderNumber)
		{
			return Res.GetString("7a8629cc-8834-4def-aacd-12d9dd034dc3", "Receipt matching process failed for the transactions in the order {0} due to another user accessing the transactions at the same time. Please close and re-open the Collection Batch before trying again.", orderNumber);
		}
	}
}

