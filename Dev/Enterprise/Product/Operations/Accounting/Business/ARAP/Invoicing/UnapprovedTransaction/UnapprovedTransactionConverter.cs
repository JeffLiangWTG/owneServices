using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionConverter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UnapprovedTransactionConverter(BusinessObjectFactory factory)
			: base(factory)
		{
			this.Notify = new NotificationBuffer(new NotificationBuffer());
		}

		public UnapprovedTransactionConverter(BusinessObjectFactory factory, ZQuery additionalFilters)
			: this(factory)
		{
			this.AdditionalFilters = additionalFilters;
		}
		readonly ZQuery AdditionalFilters;

		#region Public Members

		public InvoicingBase ConvertToAP(InvoicingBase transaction, bool releaseJobHeaderMutex, bool revalidateLines = false, bool generateInvoicePdf = true, bool isAutoImport = false)
		{
			BusinessObjectFactory conversionFactory = new BusinessObjectFactory();
			UnapprovedTransactionCandidateCollection conversionCollection = new UnapprovedTransactionCandidateCollection(conversionFactory);

			var newTransaction = conversionFactory.Load<InvoicingBase>(transaction.PK);
			newTransaction.ShowError = transaction.ShowError;

			InvoicingBase aPInvoicingBase = null;
			using (conversionFactory.SetTempContext(BusinessContext.ShouldTraceExchangeRateError))
			{
				aPInvoicingBase = ConvertToAPUnsafe(newTransaction, conversionFactory, releaseJobHeaderMutex, revalidateLines, generateInvoicePdf, isAutoImport);
			}

			conversionCollection.Add(aPInvoicingBase); // To provide the proper Context	
			return aPInvoicingBase;
		}

		public InvoicingBase ConvertToAPUnsafe(InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject, bool releaseJobHeaderMutex, bool revalidateLines = false, bool generateInvoicePdf = true, bool isAutoImport = false)
		{
			InvoicingBase apInvoicingBase = null;
			var approvalEventReference = string.Empty;

			if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				(apInvoicingBase, approvalEventReference) = ConvertToAPUnsafeFromARTransaction(transaction, factoryForConvertedObject, releaseJobHeaderMutex, revalidateLines, generateInvoicePdf, isAutoImport);
			}
			else if (transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				(apInvoicingBase, approvalEventReference) = ConvertToAPUnsafeFromUATransaction(transaction);
			}
			else if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				var log = transaction.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code)).First();
				var otherUser = log != null ? log.User.GS_FullName : ZString.Empty;
				throw new JobCreationException(Res.GetString("1d13d026-4e1a-4ed3-a785-2acd7c8fbb60", "Invoice {0} has already been approved by {1}", transaction.AH_TransactionNum, otherUser));
			}
			else
			{
				throw new NotSupportedException("Only AR and UA Ledger Types are supported.");
			}

			apInvoicingBase.SubmittedFromInvoicingForm = true;
			apInvoicingBase.Logs.AddNew(Events.TransactionApprovalActioned, approvalEventReference);

			return apInvoicingBase;
		}

		public InvoicingBase ConvertToUA(InvoicingBase transaction)
		{
			transaction.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			if (transaction.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote)
			{
				transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.UACreditNote;
			}
			else if (transaction.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice)
			{
				transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.UAInvoice;
			}
			else
			{
				throw new NotSupportedException();
			}
			foreach (InvoicingLineBase line in transaction.Lines)
			{
				line.AL_LineType = TransactionLineTypes.UnapprovedCost;
			}

			if (!transaction.IsInDatabase)
			{
				foreach (InvoicingLineBase line in transaction.Lines)
				{
					line.OnLoaded();
				}
				transaction.OnLoaded();
			}
			return transaction;
		}

		#endregion

		#region Private Members

		(InvoicingBase convertedAPTransaction, ZString approvalEventReference) ConvertToAPUnsafeFromARTransaction(InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject, bool releaseJobHeaderMutex, bool revalidateLines, bool generateInvoicePdf, bool isAutoImport)
		{
			if (transaction != null && transaction.AH_Ledger != LedgerTypes.AccountsReceivable)
			{
				throw new ArgumentException("transaction must be not null and AR", nameof(transaction));
			}

			var arToAPConverter = GetARToAPConverter(transaction);
			var convertedAPTransactionAndEventReference = ConvertToAPUnsafeFromARTransactionCore(arToAPConverter, transaction, factoryForConvertedObject, releaseJobHeaderMutex, revalidateLines, generateInvoicePdf, isAutoImport);
			return convertedAPTransactionAndEventReference;
		}

		ARTransactionToAPTransactionConverterBase GetARToAPConverter(InvoicingBase transaction)
		{
			var consol = transaction.Consol;
			var isConsolInvoice = transaction.IsConsolInvoice;
			var isGatewayConsolInCurrentCompany = consol != null && IsGatewayConsolWithGatewayBillingEnabled(consol, GlbCompany.CurrentCompany);
			var forwardingConsolToGatewayConsol = isConsolInvoice && isGatewayConsolInCurrentCompany;
			var forwardingConsolToForwardingConsol = isConsolInvoice && !isGatewayConsolInCurrentCompany;
			var linesAssociatedWithGatewayJob = IntercompanyGatewayARTransactionLineFinder.FindInvoiceLinesAssociatedWithGatewayJobInSisterCompany(transaction);

			ARTransactionToAPTransactionConverterBase arToAPConverter;
			if (linesAssociatedWithGatewayJob.Any())
			{
				arToAPConverter = new GatewayARTransactionToAPTransactionConverter(Notify, linesAssociatedWithGatewayJob);
			}
			else if (forwardingConsolToForwardingConsol)
			{
				arToAPConverter = new ForwardingARTransactionToForwardingAPTransactionConverter(Notify);
			}
			else if (forwardingConsolToGatewayConsol)
			{
				arToAPConverter = new ForwardingARTransactionToGatewayAPTransactionConverter(Notify);
			}
			else
			{
				arToAPConverter = new ARTransactionToAPTransactionConverterBase(Notify);
			}

			return arToAPConverter;
		}

		protected virtual (InvoicingBase convertedAPTransaction, ZString approvalEventReference) ConvertToAPUnsafeFromARTransactionCore(ARTransactionToAPTransactionConverterBase arToAPConverter, InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject, bool releaseJobHeaderMutex, bool revalidateLines, bool generateInvoicePdf, bool isAutoImport)
		{
			return arToAPConverter.ConvertToAPTransactionFromARTransaction(transaction, factoryForConvertedObject, releaseJobHeaderMutex, revalidateLines, generateInvoicePdf, isAutoImport);
		}

		bool IsGatewayConsolWithGatewayBillingEnabled(IJobCostingPlugIn consol, ICompany company)
		{
			return consol is IGateway gateway && gateway.GatewayBillingSupporter.IsGatewayBillingEnabled(company);
		}

		(InvoicingBase convertedAPTransaction, ZString approvalEventReference) ConvertToAPUnsafeFromUATransaction(InvoicingBase transaction)
		{
			if (transaction != null && transaction.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions)
			{
				throw new ArgumentException("transaction must be not null and UA", nameof(transaction));
			}

			var apInvoicingBase = transaction;
			apInvoicingBase.AH_Ledger = LedgerTypes.AccountsPayable;

			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			if (periodCalculator.IsPostDateValid(apInvoicingBase.AH_PostDate) == PostDateValidationResult.SubLedgerPeriodClosed)
			{
				apInvoicingBase.AH_PostDate = ZDateTime.Today;
			}

			if (apInvoicingBase.AH_TransactionType == TransactionTypes.UACreditNote)
			{
				apInvoicingBase.AH_TransactionType = TransactionTypes.CreditNote;
				apInvoicingBase.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsPayable + TransactionTypes.CreditNote,
							LedgerTypes.AccountsPayable + " " +
							new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.CreditNote));
			}
			else if (apInvoicingBase.AH_TransactionType == TransactionTypes.UAInvoice)
			{
				apInvoicingBase.AH_TransactionType = TransactionTypes.Invoice;
				apInvoicingBase.AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Invoice,
							LedgerTypes.AccountsPayable + " " +
							new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.Invoice));
			}
			else
			{
				throw new NotSupportedException();
			}
			foreach (InvoicingLineBase line in apInvoicingBase.Lines)
			{
				line.AL_LineType = TransactionLineTypes.Cost;
			}
			var approvalEventReference = (NoResString)"AP Invoice Approved";

			return (apInvoicingBase, approvalEventReference);
		}

		#endregion

		#region GUIBindable Members

		public UnapprovedTransactionCandidateCollection Candidates
		{
			get
			{
				if (fCandidates == null)
				{
					fCandidates = new UnapprovedTransactionCandidateCollection(Factory, AdditionalFilters);
					fCandidates.SetReadOnlyIncludingChildren(true);
					if (IsUnapprovedTransactionCandidateCollectionValidationSuspended)
					{
						fCandidates.SuspendValidation();
					}
					fCandidates.Load();
				}
				return fCandidates;
			}
		}
		UnapprovedTransactionCandidateCollection fCandidates;

		class UnapprovedTransactionCandidateCollectionValidationSuspender : IDisposable
		{
			internal UnapprovedTransactionCandidateCollectionValidationSuspender(UnapprovedTransactionConverter parent)
			{
				this.parent = parent;
				this.parent.unapprovedTransactionCandidateCollectionValidationSuspendCount++;
			}

			void IDisposable.Dispose()
			{
				parent.unapprovedTransactionCandidateCollectionValidationSuspendCount--;
			}

			readonly UnapprovedTransactionConverter parent;
		}

		int unapprovedTransactionCandidateCollectionValidationSuspendCount;

		public IDisposable GetUnapprovedTransactionCandidateCollectionValidationSuspender()
		{
			return new UnapprovedTransactionCandidateCollectionValidationSuspender(this);
		}

		internal bool IsUnapprovedTransactionCandidateCollectionValidationSuspended
		{
			get { return unapprovedTransactionCandidateCollectionValidationSuspendCount > 0; }
		}

		#endregion

		#region Implementation

		readonly NotificationBuffer Notify;

		#endregion
	}
}
