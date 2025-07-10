using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class IReversingImplicitlyImplementedWrapperForBinding : NonPersistentBusinessObject, IReversing, IObsoleteValidation
	{
		public IReversingImplicitlyImplementedWrapperForBinding(IReversing businessEntity)
			: base(businessEntity.Factory)
		{
			WrappedBusinessEntity = businessEntity;
			RegisterEditableChildObject(WrappedBusinessEntity);
		}

		public readonly IReversing WrappedBusinessEntity;
		
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			WrappedBusinessEntity.RunPreSaveValidation();

			if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled && WrappedBusinessEntity is ARCreditNote arCreditNote)
			{
				var errorMessage = AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber;
				arCreditNote.ClearRowNotificationsContaining(errorMessage);

				if (string.IsNullOrEmpty(arCreditNote.SupportingDocumentNumber) && arCreditNote.OriginalTransaction?.EInvoicingTransactionPivotSubmitted != null)
				{
					arCreditNote.AddRowError(errorMessage);
				}
			}
		}

		#region IReversing Members

		public bool IsReversing
		{
			get { return WrappedBusinessEntity.IsReversing; }
		}

		public bool IsReversed
		{
			get { return WrappedBusinessEntity.IsReversed; }
		}

		public bool IsReverseTransaction
		{
			get { return WrappedBusinessEntity.IsReverseTransaction; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			WrappedBusinessEntity.GenerateReverseTransaction(mustTransform);
		}

		public IReversing ReverseTransaction
		{
			get { return WrappedBusinessEntity.ReverseTransaction; }
		}

		public void SetCancellationFlag(bool cancel)
		{
			WrappedBusinessEntity.SetCancellationFlag(cancel);
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
			WrappedBusinessEntity.SetTransactionBelongsToGroupField(groupingGuidValue);
		}

		public void SetDescription(ZString descriptionToSet)
		{
			WrappedBusinessEntity.SetDescription(descriptionToSet);
		}
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			WrappedBusinessEntity.SetNumberOfSupportingDocuments(numberOfSupportingDocumentsToSet);
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
			WrappedBusinessEntity.ApplyWorkflowTemplatesOnReverseTransaction();
		}

		public ZString ReversingReason
		{
			get
			{
				return WrappedBusinessEntity.ReversingReason;
			}
			set
			{
				WrappedBusinessEntity.ReversingReason = value;
			}
		}

		public ZString ReversingCode
		{
			get
			{
				return WrappedBusinessEntity.ReversingCode;
			}
			set
			{
				WrappedBusinessEntity.ReversingCode = value;
			}
		}

		public bool IsClearedInCashbook
		{
			get { return WrappedBusinessEntity.IsClearedInCashbook; }
		}

		public string[] MultipleReversingErrors
		{
			get
			{
				return WrappedBusinessEntity.MultipleReversingErrors;
			}
			set
			{
				WrappedBusinessEntity.MultipleReversingErrors = value;
			}
		}

		#endregion

		#region ITransaction Members

		ZGuid ITransaction.PK
		{
			get { return WrappedBusinessEntity.PK; }
		}

		public ZString Ledger
		{
			get { return WrappedBusinessEntity.Ledger; }
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return WrappedBusinessEntity == null ? null : WrappedBusinessEntity.LedgerInfo; }
		}

		public ZString TransactionType
		{
			get { return WrappedBusinessEntity.TransactionType; }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return WrappedBusinessEntity == null ? null : WrappedBusinessEntity.TransactionTypeInfo; }
		}

		public ZString TransactionNumber
		{
			get
			{
				return WrappedBusinessEntity.TransactionNumber;
			}
			set
			{
				WrappedBusinessEntity.TransactionNumber = value;
			}
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(TransactionNumber), x => WrappedBusinessEntity.TransactionNumberInfo); }
		}

		public ZDateTime TransactionDate
		{
			get { return WrappedBusinessEntity.TransactionDate; }
			set { WrappedBusinessEntity.TransactionDate = value; }
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(TransactionDate), x => WrappedBusinessEntity.TransactionDateInfo); }
		}

		public ZDateTime PostDate
		{
			get
			{
				return WrappedBusinessEntity.PostDate;
			}
			set
			{
				WrappedBusinessEntity.PostDate = value;
			}
		}

		public ZPropertyInfo PostDateInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(PostDate), x => WrappedBusinessEntity.PostDateInfo); }
		}

		public ZString CurrencyCode
		{
			get { return WrappedBusinessEntity.CurrencyCode; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(CurrencyCode), x => WrappedBusinessEntity.CurrencyCodeInfo); }
		}

		public int OSDecimals => RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCode)?.Decimals ?? LocalDecimals;
		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OverseasTotalAmount
		{
			get { return WrappedBusinessEntity.OverseasTotalAmount; }
		}

		public ZPropertyInfo OverseasTotalAmountInfo
		{
			get { return WrappedBusinessEntity == null ? null : WrappedBusinessEntity.OverseasTotalAmountInfo; }
		}

		[List(nameof(Headers))]
		public ZGuid Organization
		{
			get
			{
				return WrappedBusinessEntity.Organization;
			}
			set
			{
				WrappedBusinessEntity.Organization = value;
			}
		}

		public ZPropertyInfo OrganizationInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(Organization), x => WrappedBusinessEntity.OrganizationInfo); }
		}

		public ZString OriginalTransactionType
		{
			get { return WrappedBusinessEntity.OriginalTransactionType; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return WrappedBusinessEntity == null ? null : WrappedBusinessEntity.OriginalTransactionTypeInfo; }
		}

		public bool OriginalTransactionType_ReadOnly { get { return WrappedBusinessEntity.OriginalTransactionType_ReadOnly; } }

		public ZString SupportingDocumentNumber
		{
			get
			{
				return WrappedBusinessEntity.SupportingDocumentNumber;
			}
			set
			{
				WrappedBusinessEntity.SupportingDocumentNumber = value;
			}
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(SupportingDocumentNumber), x => WrappedBusinessEntity.SupportingDocumentNumberInfo); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return WrappedBusinessEntity.OriginalTransactionNumber; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return WrappedBusinessEntity == null ? null : WrappedBusinessEntity.OriginalTransactionNumberInfo; }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return WrappedBusinessEntity.OriginalTransactionNumber_ReadOnly; } }

		public OrgHeaderCollection Headers
		{
			get { return WrappedBusinessEntity.Headers; }
		}

		public bool UserAllowedToBackPost
		{
			get { return WrappedBusinessEntity.UserAllowedToBackPost; }
		}

		public ZDateTime UnmatchDate
		{
			get
			{
				return WrappedBusinessEntity.UnmatchDate;
			}
			set
			{
				WrappedBusinessEntity.UnmatchDate = value;
			}
		}

		public bool UnmatchDate_ReadOnly
		{
			get { return WrappedBusinessEntity?.UnmatchDate_ReadOnly ?? true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return WrappedBusinessEntity == null ? null : GetWrappedZPropertyInfo(nameof(UnmatchDate), x => WrappedBusinessEntity.UnmatchDateInfo); }
		}

		#region Reversal Status Code

		[List(nameof(ReversalStatusCodeList))]
		public ZString ReversalStatusCode
		{
			get { return WrappedBusinessEntity.ReversalStatusCode; }
			set { WrappedBusinessEntity.ReversalStatusCode = value; }
		}

		public ZPropertyInfo ReversalStatusCodeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ReversalStatusCode), x => WrappedBusinessEntity.ReversalStatusCodeInfo); }
		}

		public bool ReversalStatusCode_ReadOnly
		{
			get { return WrappedBusinessEntity.ReversalStatusCode_ReadOnly; }
		}

		public ReadOnlyCodeDescriptionPairList ReversalStatusCodeList
		{
			get { return WrappedBusinessEntity.ReversalStatusCodeList; }
		}

		#endregion

		[List(nameof(AmendStatusCodeList))]
		public ZString AmendStatusCode
		{
			get
			{
				return InvoicingBase?.AH_Calc_AmendStatusCode ?? ZString.Empty;
			}
			set
			{
				if (InvoicingBase != null)
				{
					InvoicingBase.AH_Calc_AmendStatusCode = value;
				}
			}
		}

		public ZPropertyInfo AmendStatusCodeInfo => GetWrappedZPropertyInfo(nameof(AmendStatusCode), x => InvoicingBase?.AH_Calc_AmendStatusCodeInfo);

		public bool AmendStatusCode_ReadOnly
		{
			get
			{
				if (WrappedBusinessEntity is BusinessObject bizO && bizO.HasRowErrors)
				{
					return true;
				}

				var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
				var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
				return !(amendStatusCodeProvider?.IsSupportAmendStatusCode(WrappedBusinessEntity as BusinessObject) ?? false);
			}
		}

		public ReadOnlyCodeDescriptionPairList AmendStatusCodeList
		{
			get
			{
				if (amendStatusCodeList == null)
				{
					var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
					var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
					amendStatusCodeList = amendStatusCodeProvider?.AmendStatusCodeList ?? new CodeDescriptionPairList();
				}
				return amendStatusCodeList;
			}
		}

		ReadOnlyCodeDescriptionPairList amendStatusCodeList;

		#endregion

		InvoicingBase InvoicingBase
		{
			get
			{
				if (invoicingBase == null)
				{
					invoicingBase = WrappedBusinessEntity as InvoicingBase;
				}

				return invoicingBase;
			}
		}

		InvoicingBase invoicingBase;
	}
}
