using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface ITransaction : IBusiness
	{
		ZGuid PK { get; }
		ZString Ledger { get; }
		ZPropertyInfo LedgerInfo { get; }
		ZString TransactionType { get; }
		ZPropertyInfo TransactionTypeInfo { get; }
		ZString TransactionNumber { get; set; }
		ZPropertyInfo TransactionNumberInfo { get; }
		ZDateTime TransactionDate { get; set; }
		ZPropertyInfo TransactionDateInfo { get; }
		ZDateTime PostDate { get; set; }
		ZPropertyInfo PostDateInfo { get; }
		ZString CurrencyCode { get; }
		ZPropertyInfo CurrencyCodeInfo { get; }
		ZDecimal OverseasTotalAmount { get; }
		ZPropertyInfo OverseasTotalAmountInfo { get; }
		[List("Headers")]
		ZGuid Organization { get; set; }
		ZPropertyInfo OrganizationInfo { get; }
		OrgHeaderCollection Headers { get; }
		bool UserAllowedToBackPost { get; }
		ZDateTime UnmatchDate { get; set; }
		ZPropertyInfo UnmatchDateInfo { get; }
		bool UnmatchDate_ReadOnly { get; }
		ZString OriginalTransactionNumber { get; }
		ZPropertyInfo OriginalTransactionNumberInfo { get; }
		bool OriginalTransactionNumber_ReadOnly { get; }
		ZString OriginalTransactionType { get; }
		ZPropertyInfo OriginalTransactionTypeInfo { get; }
		bool OriginalTransactionType_ReadOnly { get; }
		ZString SupportingDocumentNumber { get; set; }
		ZPropertyInfo SupportingDocumentNumberInfo { get; }
		[List("ReversalStatusCodeList")]
		ZString ReversalStatusCode { get; set; }
		ZPropertyInfo ReversalStatusCodeInfo { get; }
		bool ReversalStatusCode_ReadOnly { get; }
		ReadOnlyCodeDescriptionPairList ReversalStatusCodeList { get; }
	}
}

#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public abstract class TestITransaction : ITransactionForTests
	{
		readonly ZGuid fPK = ZGuid.NewZGuid();
		public ZGuid PK
		{
			get { return fPK; }
		}

		#region ITransaction Members

		public ZString OriginalTransactionType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo { get { return null; } }

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		public ZString SupportingDocumentNumber
		{
			get { return ZString.Empty; }
			set { fSupportingDocumentNumber = value; }
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return null; }
		}

		public ZString OriginalTransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo { get { return null; } }

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString Ledger
		{
			get { return fLedger; }
			set { fLedger = value; }
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return null; }
		}

		public ZString CurrencyCode
		{
			get { return fCurrencyCode; }
			set { fCurrencyCode = value; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return null; }
		}

		public ZDecimal OverseasTotalAmount
		{
			get { return fOverseasTotalAmount; }
			set { fOverseasTotalAmount = value; }
		}

		public ZPropertyInfo OverseasTotalAmountInfo
		{
			get { return null; }
		}

		public ZDateTime PostDate
		{
			get { return fPostDate; }
			set { fPostDate = value; }
		}

		public ZPropertyInfo PostDateInfo
		{
			get { return null; }
		}

		public ZDateTime FullyPaidDate
		{
			get { return fFullyPaidDate; }
			set { fFullyPaidDate = value; }
		}

		public ZDateTime TransactionDate
		{
			get { return fTransactionDate; }
			set { fTransactionDate = value; }
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return null; }
		}

		public ZString TransactionNumber
		{
			get { return fTransactionNumber; }
			set { fTransactionNumber = value; }
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return null; }
		}

		public ZString TransactionType
		{
			get { return fTransactionType; }
			set { fTransactionType = value; }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return null; }
		}

		[List("Headers")]
		public ZGuid Organization
		{
			get { return fOrganization; }
			set { fOrganization = value; }
		}

		public ZPropertyInfo OrganizationInfo
		{
			get { return null; }
		}

		public OrgHeaderCollection Headers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public bool UserAllowedToBackPost
		{
			get { return fUserAllowedToBackPost; }
		}

		protected ZString fLedger;
		protected ZString fCurrencyCode;
		protected ZDecimal fOverseasTotalAmount;
		protected ZDateTime fPostDate;
		protected ZDateTime fFullyPaidDate;
		protected ZDateTime fTransactionDate;
		protected ZString fTransactionNumber;
		protected ZString fTransactionType;
		protected ZGuid fOrganization;
		protected bool fUserAllowedToBackPost;
		protected ZString fSupportingDocumentNumber;

		public ZString TransactionReason
		{ get; set; }

		public ZString TransactionReasonCode
		{ get; set; }

		public ZDateTime UnmatchDate
		{
			get;
			set;
		}

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return null; }
		}

		ZString ITransaction.ReversalStatusCode { get; set; }

		ZPropertyInfo ITransaction.ReversalStatusCodeInfo { get; }

		bool ITransaction.ReversalStatusCode_ReadOnly { get; }

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList { get; }

		#endregion

		#region IBusiness Members

		public bool HasChangesNotIncludingChildren
		{
			get { return false; }
		}

		public bool IsValidationSuspended
		{
			get { return false; }
		}

		public uint LastChangeNumber
		{
			get { return 0; }
		}

		public void SuspendValidation()
		{
		}

		public void MarkAsNeedingValidationIncludingChildren()
		{
		}

		public void ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
		}

		public void ResumeValidation()
		{
		}

		bool IBusiness.IgnoreValidationSuspended
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void SetFactory(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		BusinessObjectFactory fFactory;
		public BusinessObjectFactory Factory
		{
			get { return fFactory; }
		}

		public ZString HumanReadableName
		{
			get { return ""; }
		}

		public string TableName
		{
			get { return null; }
		}

		public void Delete()
		{
		}

		bool IBusiness.CanContinueWithSave
		{
			get { return true; }
		}

		class EmptyFetchStrategy : IFetchStrategy
		{
			#region IFetchStrategy Members

			public void FetchForBind()
			{
				// TODO:  Add EmptyFetchStrategy.FetchForBind implementation
			}

			#endregion
		}
		public IFetchStrategy FetchStrategy
		{
			get { return new EmptyFetchStrategy(); }
		}

		void IBusiness.RunPreSaveValidationFetch(bool executeHints)
		{
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
		}

		public void RunPreSaveValidation()
		{
		}

		bool IBusiness.CanDeleteForDataRefresh => false;

		void IBusiness.DeleteForDataRefresh()
		{
		}

		#endregion

		#region IBusinessObjectState Members

#pragma warning disable
		public event System.ComponentModel.ListChangedEventHandler ListChanged;
		public event System.EventHandler<HasChangesChangedEventArgs> HasChangesChanged;
		public event System.EventHandler<NotificationsChangedEventArgs> NotificationsChanged;
		public event System.EventHandler UpdatedByDataRefreshIncludingChildren;
#pragma warning restore

		public void IncrementReadOnlyIncludingChildren()
		{
		}

		public void DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
		}

		public void ClearHasChangesIncludingChildren()
		{
		}

		public void RefreshBindingIncludingChildren()
		{
		}

		public bool HasChanges
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.HasChanges getter implementation
				return false;
			}
			set
			{
				// TODO:  Add TestIReverseTransaction.HasChanges setter implementation
			}
		}

		public bool IsInDatabase => false;

		public bool IsInDatabaseIncludingChildren
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.IsInDatabaseIncludingChildren getter implementation
				return false;
			}
		}

		#endregion

		#region IBindingList Members

		public void AddIndex(System.ComponentModel.PropertyDescriptor property)
		{
			// TODO:  Add TestIReverseTransaction.AddIndex implementation
		}

		public bool AllowNew
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.AllowNew getter implementation
				return false;
			}
		}

		public void ApplySort(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
		{
			// TODO:  Add TestIReverseTransaction.ApplySort implementation
		}

		public System.ComponentModel.PropertyDescriptor SortProperty
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SortProperty getter implementation
				return null;
			}
		}

		public int Find(System.ComponentModel.PropertyDescriptor property, object key)
		{
			// TODO:  Add TestIReverseTransaction.Find implementation
			return 0;
		}

		public bool SupportsSorting
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SupportsSorting getter implementation
				return false;
			}
		}

		public bool IsSorted
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.IsSorted getter implementation
				return false;
			}
		}

		public bool AllowRemove
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.AllowRemove getter implementation
				return false;
			}
		}

		public bool SupportsSearching
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SupportsSearching getter implementation
				return false;
			}
		}

		public System.ComponentModel.ListSortDirection SortDirection
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SortDirection getter implementation
				return new System.ComponentModel.ListSortDirection();
			}
		}

		public bool SupportsChangeNotification
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SupportsChangeNotification getter implementation
				return false;
			}
		}

		public void RemoveSort()
		{
			// TODO:  Add TestIReverseTransaction.RemoveSort implementation
		}

		public object AddNew()
		{
			// TODO:  Add TestIReverseTransaction.AddNew implementation
			return null;
		}

		public bool AllowEdit
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.AllowEdit getter implementation
				return false;
			}
		}

		public void RemoveIndex(System.ComponentModel.PropertyDescriptor property)
		{
			// TODO:  Add TestIReverseTransaction.RemoveIndex implementation
		}

		#endregion

		#region IList Members

		public bool IsReadOnly
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.IsReadOnly getter implementation
				return false;
			}
		}

		public object this[int index]
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.this getter implementation
				return null;
			}
			set
			{
				// TODO:  Add TestIReverseTransaction.this setter implementation
			}
		}

		public void RemoveAt(int index)
		{
			// TODO:  Add TestIReverseTransaction.RemoveAt implementation
		}

		public void Insert(int index, object value)
		{
			// TODO:  Add TestIReverseTransaction.Insert implementation
		}

		public void Remove(object value)
		{
			// TODO:  Add TestIReverseTransaction.Remove implementation
		}

		public bool Contains(object value)
		{
			// TODO:  Add TestIReverseTransaction.Contains implementation
			return false;
		}

		public void Clear()
		{
			// TODO:  Add TestIReverseTransaction.Clear implementation
		}

		public int IndexOf(object value)
		{
			// TODO:  Add TestIReverseTransaction.IndexOf implementation
			return 0;
		}

		public int Add(object value)
		{
			// TODO:  Add TestIReverseTransaction.Add implementation
			return 0;
		}

		public bool IsFixedSize
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.IsFixedSize getter implementation
				return false;
			}
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.IsSynchronized getter implementation
				return false;
			}
		}

		public int Count
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.Count getter implementation
				return 0;
			}
		}

		public void CopyTo(Array array, int index)
		{
			// TODO:  Add TestIReverseTransaction.CopyTo implementation
		}

		public object SyncRoot
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.SyncRoot getter implementation
				return null;
			}
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			// TODO:  Add TestIReverseTransaction.GetEnumerator implementation
			return null;
		}

		#endregion

		#region INotificationProvider Members

		IEnumerable<INotification> INotificationProvider.Notifications
		{
			get { yield break; }
		}

		bool INotificationProvider.HasNotifications()
		{
			return false;
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return false;
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return null;
		}

		#endregion

		#region INeedDebugMethods Members

		public IBusiness[] Children
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.Children getter implementation
				return null;
			}
		}

		public string[] ErrorsIncludingChildren
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.ErrorsIncludingChildren getter implementation
				return null;
			}
		}

		#endregion

		#region IIdentified Members

		public ZGuid Identifier
		{
			get
			{
				// TODO:  Add TestIReverseTransaction.Identifier getter implementation
				return new ZGuid();
			}
		}

		#endregion
	}

	public interface ITransactionForTests : ITransaction
	{
		void SetFactory(BusinessObjectFactory factory);
		ZDateTime FullyPaidDate { get; set; }
	}
}
#endif
