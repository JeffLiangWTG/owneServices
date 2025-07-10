using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class MultipleReversingProviderForHeader : MultipleReversingProviderBase, IReversing, IBadDebtWritingOff, ISecurityOverrideProviderSource
	{
		public MultipleReversingProviderForHeader() : base()
		{
			TransactionsWithLevelAuthorizationProblems = new HashSet<ZGuid>();
			ShouldSaveApprovalFactory = false;
			ApprovalFactory = null;
		}

		public HashSet<ZGuid> TransactionsWithLevelAuthorizationProblems { get; }
		public bool ShouldSaveApprovalFactory;
		public BusinessObjectFactory ApprovalFactory;

		public override BusinessObjectCollection BizObjectsForReversing => transactionsForReversing ?? (transactionsForReversing = new TransactionHeaderCollection(Factory));
		TransactionHeaderCollection transactionsForReversing;

		public IReversingCollection TransactionsAlreadyReversed
		{
			get
			{
				if (TransactionsAlreadyReversed_innerValue == null)
				{
					TransactionsAlreadyReversed_innerValue = new IReversingCollection(Factory);
					TransactionsAlreadyReversed_innerValue.CountChanged += new CollectionCountChangedEventHandler(TransactionsAlreadyReversed_innerValue_CountChanged);
					RegisterEditableChildObject(TransactionsAlreadyReversed_innerValue);

					TransactionsAlreadyReversedWithoutWrappersForTransactionNumberValidation = new IReversingCollection(Factory);
				}
				return TransactionsAlreadyReversed_innerValue;
			}
		}
		IReversingCollection TransactionsAlreadyReversed_innerValue;

		public ITransactionCollection<IReversingImplicitlyImplementedWrapperForBinding> TransactionsAlreadyReversedAsITransactionCollection => TransactionsAlreadyReversed;

		public override BusinessObjectCollection BizObjectsAlreadyReversed => TransactionsAlreadyReversed;

		public override BusinessObject GetWrappedBusinessEntity(BusinessObject bizObjectAlreadyReversed) => (BusinessObject)((IReversingImplicitlyImplementedWrapperForBinding)bizObjectAlreadyReversed).WrappedBusinessEntity;

		public SecurityCertificate PaidRelatedInvoicesSecurityCertificate
		{
			get;
			set;
		}

		public SecurityCertificate PaidRelatedSelfBilledInvoicesSecurityCertificate
		{
			get;
			set;
		}

		void TransactionsAlreadyReversed_innerValue_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is IReversingImplicitlyImplementedWrapperForBinding iReversingWrapper)
			{
				if (e.ItemAdded)
				{
					TransactionsAlreadyReversedWithoutWrappersForTransactionNumberValidation.Add((BusinessObject)iReversingWrapper.WrappedBusinessEntity);
				}
				if (e.ItemRemoved)
				{
					TransactionsAlreadyReversedWithoutWrappersForTransactionNumberValidation.Remove((BusinessObject)iReversingWrapper.WrappedBusinessEntity);
				}
			}
		}

		protected IReversingCollection TransactionsAlreadyReversedWithoutWrappersForTransactionNumberValidation;

		#region IReversing Members

		public ZString ReversingReason
		{
			get => TransactionsAlreadyReversed.Any() ? TransactionsAlreadyReversed[0].ReversingReason : ZString.Empty;
			set
			{
				foreach (IReversing transaction in TransactionsAlreadyReversed)
				{
					transaction.ReversingReason = value;
				}
			}
		}

		public ZString ReversingCode
		{
			get => TransactionsAlreadyReversed.Any() ? TransactionsAlreadyReversed[0].ReversingCode : ZString.Empty;
			set
			{
				foreach (IReversing transaction in TransactionsAlreadyReversed)
				{
					transaction.ReversingCode = value;
				}
			}
		}

		#region Not Used Members

		public bool IsReversing
		{
			get { throw new NotSupportedException(); }
		}

		public bool IsReversed
		{
			get { throw new NotSupportedException(); }
		}

		public bool IsReverseTransaction
		{
			get { throw new NotSupportedException(); }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			throw new NotImplementedException();
		}

		public IReversing ReverseTransaction
		{
			get { throw new NotSupportedException(); }
		}

		public void SetCancellationFlag(bool cancel)
		{
			throw new NotImplementedException();
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
			throw new NotImplementedException();
		}

		public void SetDescription(ZString descriptionToSet)
		{
			throw new NotImplementedException();
		}
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			throw new NotImplementedException();
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
			throw new NotImplementedException();
		}

		public bool IsClearedInCashbook
		{
			get { throw new NotSupportedException(); }
		}

		public string[] MultipleReversingErrors
		{
			get
			{
				return Array.Empty<string>();
			}
			set
			{
			}
		}

		#endregion

		#endregion

		#region ITransaction Members

		public bool UserAllowedToBackPost => false;

		#region Not Used Members

		public ZString Ledger
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(Ledger)); }
		}

		public ZString TransactionType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		[MaxLength(1)]
		public ZString TransactionNumber
		{
			get { return TransactionNumber_innerValue; }
			set
			{
				if (TransactionNumber_innerValue != value)
				{
					CheckMaximumLength(TransactionNumberInfo, value);
					SetNonPersistentPropertyValue(TransactionNumberInfo, ref TransactionNumber_innerValue, value);
				}
			}
		}
		ZString TransactionNumber_innerValue;

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumber)); }
		}

		[BusinessObjectTestExclude]
		public ZDateTime TransactionDate
		{
			get { return ZDateTime.Empty; }
			set { }
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionDate)); }
		}

		public ZDateTime PostDate
		{
			get { return PostDate_innerValue; }
			set { SetNonPersistentPropertyValue(PostDateInfo, ref PostDate_innerValue, value); }
		}
		ZDateTime PostDate_innerValue;

		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostDate)); }
		}

		public ZString SupportingDocumentNumber
		{
			get { return SupportingDocumentNumber_innerValue; }
			set { SetNonPersistentPropertyValue(SupportingDocumentNumberInfo, ref SupportingDocumentNumber_innerValue, value); }
		}
		ZString SupportingDocumentNumber_innerValue;

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		public ZString CurrencyCode
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		public ZDecimal OverseasTotalAmount
		{
			get { return ZDecimal.Zero; }
		}

		public ZPropertyInfo OverseasTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OverseasTotalAmount)); }
		}

		public ZGuid Organization
		{
			get { return Organization_innerValue; }
			set { SetNonPersistentPropertyValue(OrganizationInfo, ref Organization_innerValue, value); }
		}
		ZGuid Organization_innerValue;

		public ZPropertyInfo OrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(Organization)); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString OriginalTransactionType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		public OrgHeaderCollection Headers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set { SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value); }
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(UnmatchDate)); }
		}

		#region ReversalStatusCode

		ZString ITransaction.ReversalStatusCode
		{
			get => ZString.Empty;
			set { }
		}

		ZPropertyInfo ITransaction.ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ITransaction.ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion ReversalStatusCode

		#endregion

		#endregion

		#region IBadDebtWritingOff Members

		public bool IsWritingOff
		{
			get
			{
				bool result = false;
				if (BizObjectsForReversing.Any() &&
					BizObjectsForReversing.ElementAt(0) is IBadDebtWritingOff transactionAsIBadDebtWritingOff)
				{
					result = transactionAsIBadDebtWritingOff.IsWritingOff;
				}

				return result;
			}
			set
			{
				foreach (TransactionHeader transaction in BizObjectsForReversing)
				{
					if (transaction is IBadDebtWritingOff transactionAsIBadDebtWritingOff)
					{
						transactionAsIBadDebtWritingOff.IsWritingOff = value;
					}
				}
			}
		}

		#endregion

		#region ISecurityOverrideProviderSource Members

		public ISecurityOverrideProvider GetSecurityOverrideProvider(ISecurityOverrideProvider proposedProvider)
		{
			return SecurityOverrideProvider ?? (SecurityOverrideProvider = proposedProvider);
		}

		ISecurityOverrideProvider SecurityOverrideProvider;

		ISecurityOverrideProvider ISecurityOverrideProviderSource.Provider
		{
			get => SecurityOverrideProvider;
			set => SecurityOverrideProvider = GetSecurityOverrideProvider(value);
		}

		#endregion

		public bool CheckHasComplianceSubTypeAndNumberingErrors()
		{
			bool result = false;
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				foreach (var invoice in GetInvoicingBase())
				{
					if (invoice != null
						&& invoice.IsComplianceNumberAllocationMandatory
						&& invoice.ShouldAllocateComplianceNumberOnPosting())
					{
						var complianceError = invoice.AssignComplianceSubTypeAndCheckComplianceErrors();
						if (!complianceError.IsEmpty)
						{
							invoice.AddRowError(complianceError);
							result = true;
						}
					}
				}
			}
			return result;
		}

		public bool HasClosedJob => GetInvoicingBase().Any(x => x.HasClosedJob);

		public IEnumerable<Job> GetAllJobs() => GetInvoicingBase().Where(x => x.HasClosedJob).SelectMany(x => x.RelatedJobsForReversing);

		public bool ReOpenClosedJob()
		{
			var invoices = GetInvoicingBase().Where(x => x.HasClosedJob);
			bool result = invoices.Any();

			foreach (InvoicingBase invoice in invoices)
			{
				result &= invoice.ReOpenClosedJob();
			}

			return result;
		}

		IEnumerable<InvoicingBase> GetInvoicingBase()
		{
			return TransactionsAlreadyReversedAsITransactionCollection.Cast<IReversingImplicitlyImplementedWrapperForBinding>()
					.Where(x => x.WrappedBusinessEntity is InvoicingBase)
					.Select(x => x.WrappedBusinessEntity as InvoicingBase);
		}
	}
}
