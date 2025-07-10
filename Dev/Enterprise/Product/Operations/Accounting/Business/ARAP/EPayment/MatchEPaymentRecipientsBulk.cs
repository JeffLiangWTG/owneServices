using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class MatchEPaymentRecipientsBulk : MatchEPaymentRecipients
	{
		public MatchEPaymentRecipientsBulk(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		public AccEPaymentBeneficiary CurrentBeneficiary { get; set; }

		#region CreditorPK

		[RelatedBusinessObject("Creditor")]
		[List("Creditors")]
		public ZGuid CreditorPK
		{
			get
			{
				return creditorPK_internal;
			}
			set
			{
				if (CreditorPK != value)
				{
					SetNonPersistentPropertyValue(CreditorPKInfo, ref creditorPK_internal, value);
					if (CreditorPK.IsEmpty)
					{
						DefaultBankAccountPK = ZGuid.Empty;
						AgreedPaymentMethod = ZString.Empty;
						DefaultPaymentReason = ZString.Empty;
						AllowOverrideDefault = false;
						PaymentReferenceType = ZString.Empty;
						PaymentReference = ZString.Empty;
					}
					else
					{
						UpdateBankAccountAndPaymentMethodWhenCreditorChanged();
						DefaultPaymentReason = EPaymentPropertyHelper.GetDefaultPaymentReasonForEPaymentMethod(Env.CurrentCompanyPK, Env.CurrentBranchPK, EPaymentMethods.EPaymentViaOFX);

						var defaultPaymentRef = EPaymentPropertyHelper.GetDefaultEPaymentReferenceForEPaymentMethod(EPaymentMethods.EPaymentViaOFX);
						PaymentReferenceType = defaultPaymentRef?.ReferenceType ?? ZString.Empty;
						PaymentReference = defaultPaymentRef?.Reference ?? ZString.Empty;
					}
					if (!IsValidationSuspended)
					{
						ValidateCreditorPK();
					}
				}
			}
		}
		public ZPropertyInfo CreditorPKInfo => GetZPropertyInfo(nameof(CreditorPK));
		ZGuid creditorPK_internal;

		public bool CreditorPK_ReadOnly => CurrentBeneficiary?.IsMatchedWithOrg ?? true;

		public OrgHeader Creditor
		{
			get { return Factory.Load<OrgHeader>(CreditorPK); }
		}

		public CreditorCollection Creditors
		{
			get { return FindboxLookupCollections.GetCreditorCollection(Factory); }
		}

		void UpdateBankAccountAndPaymentMethodWhenCreditorChanged()
		{
			if (CreditorPK.IsValid)
			{
				var org = Factory.Load<OrgHeader>(CreditorPK);
				if (org != null)
				{
					DefaultBankAccountPK = org.MiscServ.OM_AB_APDefaultBankAccount;
					AgreedPaymentMethod = org.CompanyData.OB_APCreditAgreedPaymentMethod;
				}
			}
		}

		#endregion

		#region DefaultBankAccountPK

		[RelatedBusinessObject("DefaultBankAccount")]
		[List("DefaultBankAccounts")]
		public ZGuid DefaultBankAccountPK
		{
			get
			{
				return defaultBankAccountPK_internal;
			}
			set
			{
				if (DefaultBankAccountPK != value)
				{
					SetNonPersistentPropertyValue(DefaultBankAccountPKInfo, ref defaultBankAccountPK_internal, value);
					if (!IsValidationSuspended)
					{
						ValidateDefaultBankAccountPK();
					}
				}
			}
		}
		public ZPropertyInfo DefaultBankAccountPKInfo => GetZPropertyInfo(nameof(DefaultBankAccountPK));
		ZGuid defaultBankAccountPK_internal;

		public bool DefaultBankAccountPK_ReadOnly => CreditorPK.IsEmpty || !AllowOverrideDefault;

		public AccBankAccount DefaultBankAccount
		{
			get { return Factory.Load<AccBankAccount>(DefaultBankAccountPK); }
		}

		public AccBankAccountCollection DefaultBankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					fBankAccounts = AccountingUtils.GetAccBankAccountCollection(Factory);
				}
				return fBankAccounts;
			}
		}
		AccBankAccountCollection fBankAccounts;

		#endregion

		#region AgreedPaymentMethod

		[List("APCreditAgreedPaymentMethodList")]
		public ZString AgreedPaymentMethod
		{
			get
			{
				return agreedPaymentMethod_internal;
			}
			set
			{
				if (AgreedPaymentMethod != value)
				{
					SetNonPersistentPropertyValue(AgreedPaymentMethodInfo, ref agreedPaymentMethod_internal, value);
					if (!IsValidationSuspended)
					{
						ValidateAgreedPaymentMethod();
					}
				}
			}
		}
		public ZPropertyInfo AgreedPaymentMethodInfo => GetZPropertyInfo(nameof(AgreedPaymentMethod));
		ZString agreedPaymentMethod_internal;

		public bool AgreedPaymentMethod_ReadOnly => CreditorPK.IsEmpty || !AllowOverrideDefault;

		public ReadOnlyCodeDescriptionPairList APCreditAgreedPaymentMethodList
		{
			get { return Env.Registry.PayablesCreditAgreedPaymentMethodsList; }
		}

		#endregion

		#region PaymentReferenceType

		[List("PaymentReferenceTypeList")]
		public ZString PaymentReferenceType
		{
			get
			{
				return paymentReferenceType_internal;
			}
			set
			{
				if (PaymentReferenceType != value)
				{
					SetNonPersistentPropertyValue(PaymentReferenceTypeInfo, ref paymentReferenceType_internal, value);
					OnPaymentReferenceTypeChanged?.Invoke(this, EventArgs.Empty);
					if (!IsValidationSuspended)
					{
						ValidatePaymentReferenceType();
					}
					PaymentReference = ZString.Empty;
				}
			}
		}
		ZString paymentReferenceType_internal;

		public ZPropertyInfo PaymentReferenceTypeInfo => GetZPropertyInfo(nameof(PaymentReferenceType));

		public bool PaymentReferenceType_ReadOnly => (CurrentBeneficiary?.IsMatchedWithOrg ?? false) || CreditorPK.IsEmpty;

		public ReadOnlyCodeDescriptionPairList PaymentReferenceTypeList => EPaymentReferenceTypes.CodesList;

		public EventHandler OnPaymentReferenceTypeChanged;

		#endregion

		#region PaymentReference

		[MaxLength(30)]
		public ZString PaymentReference
		{
			get
			{
				return paymentReference_internal;
			}
			set
			{
				if (PaymentReference != value)
				{
					SetNonPersistentPropertyValue(PaymentReferenceInfo, ref paymentReference_internal, value);
					if (!IsValidationSuspended)
					{
						ValidatePaymentReference();
					}
				}
			}
		}
		ZString paymentReference_internal;

		public ZPropertyInfo PaymentReferenceInfo => GetZPropertyInfo(nameof(PaymentReference));

		public bool PaymentReference_ReadOnly => CreditorPK.IsEmpty
			|| (CurrentBeneficiary?.IsMatchedWithOrg ?? false)
			|| PaymentReferenceType != EPaymentReferenceTypes.FreeText;

		#endregion

		#region AllowOverrideDefault

		public ZBool AllowOverrideDefault
		{
			get
			{
				return allowOverrideDefault_internal;
			}
			set
			{
				if (AllowOverrideDefault != value)
				{
					SetNonPersistentPropertyValue(AllowOverrideDefaultInfo, ref allowOverrideDefault_internal, value);
					if (!AllowOverrideDefault)
					{
						//restore the value in DB
						UpdateBankAccountAndPaymentMethodWhenCreditorChanged();
					}
				}
			}
		}
		public ZPropertyInfo AllowOverrideDefaultInfo => GetZPropertyInfo(nameof(AllowOverrideDefault));
		ZBool allowOverrideDefault_internal;

		public bool AllowOverrideDefault_ReadOnly
		{
			get
			{
				var result = true;
				if (CurrentBeneficiary != null && !CurrentBeneficiary.IsMatchedWithOrg && CreditorPK.IsValid)
				{
					result = false;
				}
				return result;
			}
		}

		#endregion

		#region Default Payment Reason

		[List("PaymentReasons")]
		public ZString DefaultPaymentReason
		{
			get
			{
				return defaultPaymentReason_internal;
			}
			set
			{
				if (DefaultPaymentReason != value)
				{
					SetNonPersistentPropertyValue(DefaultPaymentReasonInfo, ref defaultPaymentReason_internal, value);
					if (!IsValidationSuspended)
					{
						ValidateDefaultPaymentReason();
					}
				}
			}
		}
		public ZPropertyInfo DefaultPaymentReasonInfo => GetZPropertyInfo(nameof(DefaultPaymentReason));
		ZString defaultPaymentReason_internal;

		public bool DefaultPaymentReason_ReadOnly => (CurrentBeneficiary?.IsMatchedWithOrg ?? false) || CreditorPK.IsEmpty;

		public ReadOnlyCodeDescriptionPairList PaymentReasons => paymentReasons ?? (paymentReasons = EPaymentPropertyHelper.GetPaymentReasonsForEPaymentMethod(Env.CurrentCompanyPK, EPaymentMethods.EPaymentViaOFX));
		ReadOnlyCodeDescriptionPairList paymentReasons;

		#endregion

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateCreditorPK();
			ValidateDefaultBankAccountPK();
			ValidateAgreedPaymentMethod();
			ValidateDefaultPaymentReason();
			ValidatePaymentReferenceType();
			ValidatePaymentReference();
		}

		void ValidateCreditorPK()
		{
			CreditorPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CreditorPKInfo, Creditors);
		}

		void ValidateDefaultBankAccountPK()
		{
			DefaultBankAccountPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(DefaultBankAccountPKInfo, DefaultBankAccounts);
		}

		void ValidateAgreedPaymentMethod()
		{
			AgreedPaymentMethodInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AgreedPaymentMethodInfo, APCreditAgreedPaymentMethodList);
		}

		void ValidateDefaultPaymentReason()
		{
			DefaultPaymentReasonInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DefaultPaymentReasonInfo, PaymentReasons);
		}

		void ValidatePaymentReferenceType()
		{
			PaymentReferenceTypeInfo.ClearAllNotifications();

			if (CreditorPK.IsValid && PaymentReferenceTypeInfo.Value.IsEmpty)
			{
				PaymentReferenceTypeInfo.AddError(AccountingMasterFilesConstants.ValidationErrorMessages.EPaymentReferenceTypeCannotBeEmptyErrorMessage);
			}
			ListValidation.ErrorIfInvalidCode(PaymentReferenceTypeInfo, PaymentReferenceTypeList);
		}

		void ValidatePaymentReference()
		{
			PaymentReferenceInfo.ClearAllNotifications();

			if (PaymentReferenceType == EPaymentReferenceTypes.FreeText)
			{
				MandatoryValidation.CheckEntered(PaymentReferenceInfo);
			}
		}

		#endregion
	}
}
