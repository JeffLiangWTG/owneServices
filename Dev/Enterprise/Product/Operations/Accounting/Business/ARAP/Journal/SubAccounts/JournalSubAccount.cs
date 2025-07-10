using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class JournalSubAccount : AccTransactionHeaderSubAccount, ISupportSubAccount
	{
		public JournalSubAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : AccTransactionHeaderSubAccount.Schema
		{
			public const string AHS_Calc_Sequence = "AHS_Calc_Sequence";
		}

		#endregion

		#region Properties

		#region IsSubClassValidationRuleMandatory

		public bool IsSubClassValidationRuleMandatory => Parent.GLHeader?.GetIsSubClassValidationRuleMandatory(SubAccountTypeParentTableCode) ?? false;

		#endregion

		#region Parent

		Journal fParent;
		public Journal Parent
		{
			get
			{
				if (fParent == null && AHS_AH.IsValid)
				{
					foreach (var parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						var transactionLineSubAccountCollection = parentCollection as JournalSubAccountCollection;
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

		#region AHS_SubClassParentId

		[List("Lookups.SubAccountList")]
		public override ZGuid AHS_SubClassParentId
		{
			get => base.AHS_SubClassParentId;
			set
			{
				base.AHS_SubClassParentId = value;
				if (Parent?.RelatedJournal?.SubAccounts?.Count > 0)
				{
					var relatedJournalSubAccount = Parent.RelatedJournal.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x.SubAccountTypeParentTableCode.Equals(SubAccountTypeParentTableCode) && x.SubAccountParentId != SubAccountParentId);
					if (relatedJournalSubAccount != null)
					{
						relatedJournalSubAccount.SubAccountParentId = SubAccountParentId;
					}
				}
			}
		}

		public bool AHS_SubClassParentId_ReadOnly => SubAccountHelper.IsSubAccountReadOnly(this);

		#endregion

		#region AHS_SubClassParentTableCode

		public override ZString AHS_SubClassParentTableCode
		{
			get => base.AHS_SubClassParentTableCode;
			set
			{
				var oldValue = AHS_SubClassParentTableCode;
				base.AHS_SubClassParentTableCode = value;
				if (oldValue != value)
				{
					AHS_SubClassParentId = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region AHS_Calc_SubClassParent

		public ZString AHS_Calc_SubClassParent
		{
			get
			{
				var subAccountTypeDisplayCode = SubAccountTypeDisplayCode;
				return Lookups.SubAccountTypeList.GetDescriptionFromCode(subAccountTypeDisplayCode) ?? subAccountTypeDisplayCode;
			}
		}

		public ZPropertyInfo AHS_Calc_SubClassParentInfo => GetZPropertyInfo(nameof(AHS_Calc_SubClassParent));

		#endregion

		#region AHS_Calc_SubAccountDescription

		public ZString AHS_Calc_SubAccountDescription => SubAccountHelper.GetSubAccountValueFromSubAccountId(Factory, SubAccountTypeDisplayCode, AHS_SubClassParentId, SubAccountHelper.SubAccountValueType.Description);

		public ZPropertyInfo AHS_Calc_SubAccountDescriptionInfo => GetZPropertyInfo(nameof(AHS_Calc_SubAccountDescription));

		#endregion

		#region AHS_Calc_Sequence

		public ZInt AHS_Calc_Sequence
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

		public ZPropertyInfo AHS_Calc_SequenceInfo => GetZPropertyInfo(nameof(AHS_Calc_Sequence));

		#endregion

		#endregion

		#region Lookups

		protected override AccTransactionHeaderSubAccountLookups GetNewLookups()
		{
			return new JournalSubAccountLookups(this);
		}

		public new JournalSubAccountLookups Lookups => (JournalSubAccountLookups)base.Lookups;

		#endregion

		#region Validation

		protected override AccTransactionHeaderSubAccountValidation GetNewValidation()
		{
			return new JournalSubAccountValidation(this);
		}

		public new JournalSubAccountValidation Validation => (JournalSubAccountValidation)base.Validation;

		#endregion

		#region Impementation

		public override void OnSaving()
		{
			base.OnSaving();
			if (AHS_SubClassParentId.IsEmpty || AHS_SubClassParentTableCode.IsEmpty)
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
			get => AHS_SubClassParentTableCode;
			set => AHS_SubClassParentTableCode = value;
		}

		public ZGuid SubAccountParentId
		{
			get => AHS_SubClassParentId;
			set => AHS_SubClassParentId = value;
		}

		public ZPropertyInfo SubAccountParentIdInfo => GetWrappedZPropertyInfo(nameof(SubAccountParentId), x => AHS_SubClassParentIdInfo);

		[MaxLength(3)]
		public ZString SubAccountTypeDisplayCode
		{
			get => SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(AHS_SubClassParentTableCode);
			set
			{
				AHS_SubClassParentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(value);
				SubAccountTypeDisplayCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SubAccountTypeDisplayCodeInfo => GetZPropertyInfo(nameof(SubAccountTypeDisplayCode));

		public ISupportMultiSubAccounts SubAccountParent => Parent;

		#endregion
	}
}
