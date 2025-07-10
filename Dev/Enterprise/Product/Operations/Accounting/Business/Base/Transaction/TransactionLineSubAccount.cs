using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineSubAccount : AccTransactionLineSubAccount, ISupportSubAccount
	{
		public TransactionLineSubAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : AccTransactionLineSubAccount.Schema
		{
			public const string AL1_Calc_Sequence = "AL1_Calc_Sequence";
		}

		#endregion

		#region Properties

		#region Parent

		DependentTransactionLine fParent;
		public DependentTransactionLine Parent
		{
			get
			{
				if (fParent == null && AL1_AL.IsValid)
				{
					foreach (var parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						var transactionLineSubAccountCollection = parentCollection as TransactionLineSubAccountCollection;
						if (transactionLineSubAccountCollection != null)
						{
							fParent = transactionLineSubAccountCollection.Master;
							break;
						}
					}
				}
				return fParent;
			}
		}

		#endregion

		#region AL1_SubClassParentId

		[List("Lookups.SubAccountList")]
		public override ZGuid AL1_SubClassParentId
		{
			get => base.AL1_SubClassParentId;
			set
			{
				base.AL1_SubClassParentId = value;
				ValidateSubAccount();
			}
		}

		public bool AL1_SubClassParentId_ReadOnly => SubAccountHelper.IsSubAccountReadOnly(this);

		#endregion

		#region AL1_SubClassParentTableCode

		public override ZString AL1_SubClassParentTableCode
		{
			get => base.AL1_SubClassParentTableCode;
			set
			{
				var oldValue = AL1_SubClassParentTableCode;
				base.AL1_SubClassParentTableCode = value;
				if (oldValue != value)
				{
					AL1_SubClassParentId = ZGuid.Empty;
					Parent?.SubAccounts?.UpdateSubAccountCacheKey();
				}
				ValidateSubAccount();
			}
		}

		#endregion

		#region AL1_Calc_SubClassParent

		public ZString AL1_Calc_SubClassParent
		{
			get
			{
				var subAccountTypeDisplayCode = SubAccountTypeDisplayCode;
				return Lookups.SubAccountTypeList.GetDescriptionFromCode(subAccountTypeDisplayCode) ?? subAccountTypeDisplayCode;
			}
		}

		public ZPropertyInfo AL1_Calc_SubClassParentInfo => GetZPropertyInfo(nameof(AL1_Calc_SubClassParent));

		#endregion

		#region AL1_Calc_SubAccountDescription

		public ZString AL1_Calc_SubAccountDescription => SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, SubAccountTypeDisplayCode, AL1_SubClassParentId, SubAccountHelper.SubAccountValueType.Description);

		public ZPropertyInfo AL1_Calc_SubAccountDescriptionInfo => GetZPropertyInfo(nameof(AL1_Calc_SubAccountDescription));

		#endregion

		#region AL1_Calc_Sequence

		public ZInt AL1_Calc_Sequence
		{
			get
			{
				var sequence = Lookups.SubAccountTypeList.IndexOfCode(SubAccountTypeDisplayCode);
				if (sequence == -1)
				{
					sequence = Lookups.SubAccountTypeList.Count + 1;
				}
				else
				{
					sequence++;
				}
				return sequence;
			}
		}

		public ZPropertyInfo AL1_Calc_SequenceInfo => GetZPropertyInfo(nameof(AL1_Calc_Sequence));

		#endregion

		#endregion

		#region Lookups

		protected override AccTransactionLineSubAccountLookups GetNewLookups()
		{
			return new TransactionLineSubAccountLookups(this);
		}

		public new TransactionLineSubAccountLookups Lookups => (TransactionLineSubAccountLookups)base.Lookups;

		#endregion

		#region Validation

		protected override AccTransactionLineSubAccountValidation GetNewValidation()
		{
			return new TransactionLineSubAccountValidation(this);
		}

		public new TransactionLineSubAccountValidation Validation => (TransactionLineSubAccountValidation)base.Validation;

		#endregion

		#region Impementation

		public override bool IsInDatabase => base.IsInDatabase || ((Parent?.IsInDatabase ?? false) && !HasChanges);

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (Parent == null || (Parent != null && !Parent.ReadOnly));
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (AL1_SubClassParentId.IsEmpty || AL1_SubClassParentTableCode.IsEmpty)
			{
				if (Parent != null)
				{
					Parent.SubAccounts.RemoveAndDelete(this);
				}
				else
				{
					Delete();
				}
			}
		}

		#endregion

		#region ISupportMutilSubAccounts Impementation 

		public ZString SubAccountTypeParentTableCode
		{
			get => IsDeleted ? ZString.Empty : AL1_SubClassParentTableCode;
			set
			{
				if (!IsDeleted)
				{
					AL1_SubClassParentTableCode = value;
				}
			}
		}

		public ZGuid SubAccountParentId
		{
			get => IsDeleted ? ZGuid.Empty : AL1_SubClassParentId;
			set
			{
				if (!IsDeleted)
				{
					AL1_SubClassParentId = value;
				}
			}
		}

		public ZPropertyInfo SubAccountParentIdInfo => GetWrappedZPropertyInfo(nameof(SubAccountParentId), x => AL1_SubClassParentIdInfo);

		[MaxLength(3)]
		public ZString SubAccountTypeDisplayCode
		{
			get => SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(AL1_SubClassParentTableCode);
			set
			{
				AL1_SubClassParentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(value);
				SubAccountTypeDisplayCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SubAccountTypeDisplayCodeInfo => GetZPropertyInfo(nameof(SubAccountTypeDisplayCode));

		public ISupportMultiSubAccounts SubAccountParent => Parent;

		public void ValidateSubAccount()
		{
			if (!IsValidationSuspended && Parent != null)
			{
				if (Parent.SubAccounts.FirstSubAccount == this)
				{
					Parent.AL_Calc_FirstSubClassParentIdInfo.RefreshBinding();
					(Parent.Validation as DependentTransactionLineValidation)?.ValidateAL_Calc_FirstSubClassParentId();
				}
				else if (Parent.SubAccounts.SecondSubAccount == this)
				{
					Parent.AL_Calc_SecondSubClassParentIdInfo.RefreshBinding();
					(Parent.Validation as DependentTransactionLineValidation)?.ValidateAL_Calc_SecondSubClassParentId();
				}
			}
		}

		#endregion
	}
}
