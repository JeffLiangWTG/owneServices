using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccGLAccountDescriptorExtraInfo : AutoClientCognosAccGLAccountDescriptorExtraInfo
	{
		public CognosAccGLAccountDescriptorExtraInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString T9_SubClassificationCode
		{
			get { return base.T9_SubClassificationCode; }
			set
			{
				if (base.T9_SubClassificationCode != value)
				{
					base.T9_SubClassificationCode = value;

					if (value != SubClassificationCodes.SubClassifiedByAge && value != SubClassificationCodes.SubClassifiedByCreditor && value != SubClassificationCodes.SubClassifiedByDebtor)
					{
						string error = string.Format("Invalid sub classification code specified. This should either be '{0}' or '{1}' or '{2}'", SubClassificationCodes.SubClassifiedByAge, SubClassificationCodes.SubClassifiedByCreditor, SubClassificationCodes.SubClassifiedByDebtor);
						ErrorReporter.ReportOnce("CognosAccGLAccountDescriptorExtraInfo_SubClassificationCode", error);
					}
				}
			}
		}

		public override ZPropertyInfo T9_ReconciliationTotalAccountInfo
		{
			get
			{
				ZPropertyInfo result = base.T9_ReconciliationTotalAccountInfo;
				((IZPropertyInfoObsolete)result).ReadOnly = !ShouldReconciliateTotal;
				return result;
			}
		}

		public ZBool ShouldReconciliateTotal
		{
			get
			{
				if (fShouldReconciliateTotal == null)
				{
					fShouldReconciliateTotal = !T9_ReconciliationTotalAccount.IsEmpty;
				}
				return fShouldReconciliateTotal.Value;
			}
			set
			{
				if (fShouldReconciliateTotal != value)
				{
					fShouldReconciliateTotal = value;
					ShouldReconciliateTotalInfo.RefreshBinding();
					HasChanges = true;
					T9_ReconciliationTotalAccountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ShouldReconciliateTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldReconciliateTotal)); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsSubClassifiedByAge)
			{
				MappedCreditorGroups.RemoveAll();
				MappedDebtorGroups.RemoveAll();
			}
			else
			{
				T9_AccountAge = "";
				if (IsSubClassifiedByDebtor)
				{
					MappedCreditorGroups.RemoveAll();
				}
				else if (IsSubClassifiedByCreditor)
				{
					MappedDebtorGroups.RemoveAll();
				}
			}
		}

		public override void Delete()
		{
			MappedDebtorGroups.RemoveAll();
			MappedCreditorGroups.RemoveAll();
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			T9_IsPublished = true;
			T9_SubClassificationCode = SubClassificationCodes.SubClassifiedByDebtor;
		}

		#region Sub Classification

		public bool IsSubClassificationAccount
		{
			get
			{
				CognosAccGLAccountDescriptor accountDescriptor = GLAccountDescriptor as CognosAccGLAccountDescriptor;
				return accountDescriptor != null && accountDescriptor.IsCognosSubClassificationAccount;
			}
		}

		public bool IsSubClassifyingAnotherSubClassificationAccount
		{
			get { return AccountToBeSubClassified != null && AccountToBeSubClassified.IsCognosSubClassificationAccount; }
		}

		public new CognosAccGLAccountDescriptor AccountToBeSubClassified
		{
			get { return (CognosAccGLAccountDescriptor)base.AccountToBeSubClassified; }
		}

		#region Debtor

		public ZBool IsSubClassifiedByDebtor
		{
			get { return T9_SubClassificationCode == SubClassificationCodes.SubClassifiedByDebtor; }
			set
			{
				T9_SubClassificationCode = (value) ? SubClassificationCodes.SubClassifiedByDebtor : SubClassificationCodes.SubClassifiedByCreditor;
				Validation.ValidateIsSubClassifiedByDebtor();
				IsSubClassifiedByDebtorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSubClassifiedByDebtorInfo
		{
			get { return GetZPropertyInfo(nameof(IsSubClassifiedByDebtor)); }
		}

		[ChildEditable(false)]
		public ManyToManyCognosDebtorCollection MappedDebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new ManyToManyCognosDebtorCollection(this);
					fDebtorGroups.Load();
					fDebtorGroups.CountChanged += delegate
					{
						Validation.ValidateIsSubClassifiedByDebtor();
					};
					RegisterEditableChildObject(fDebtorGroups);
				}
				return fDebtorGroups;
			}
		}

		ManyToManyCognosDebtorCollection fDebtorGroups;

		#endregion

		#region Creditor

		public ZBool IsSubClassifiedByCreditor
		{
			get { return T9_SubClassificationCode == SubClassificationCodes.SubClassifiedByCreditor; }
			set
			{
				T9_SubClassificationCode = (value) ? SubClassificationCodes.SubClassifiedByCreditor : SubClassificationCodes.SubClassifiedByDebtor;
				Validation.ValidateIsSubClassifiedByCreditor();
				IsSubClassifiedByCreditorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSubClassifiedByCreditorInfo
		{
			get { return GetZPropertyInfo(nameof(IsSubClassifiedByCreditor)); }
		}

		[ChildEditable(false)]
		public ManyToManyCognosCreditorCollection MappedCreditorGroups
		{
			get
			{
				if (fCreditorGroups == null)
				{
					fCreditorGroups = new ManyToManyCognosCreditorCollection(this);
					fCreditorGroups.Load();
					fCreditorGroups.CountChanged += delegate
					{
						Validation.ValidateIsSubClassifiedByCreditor();
					};
					RegisterEditableChildObject(fCreditorGroups);
				}
				return fCreditorGroups;
			}
		}

		ManyToManyCognosCreditorCollection fCreditorGroups;

		#endregion

		#region Account Aging

		[BusinessObjectTestExclude]
		public ZBool IsSubClassifiedByAge
		{
			get { return T9_SubClassificationCode == SubClassificationCodes.SubClassifiedByAge; }
			set
			{
				if (value)
				{
					T9_SubClassificationCode = SubClassificationCodes.SubClassifiedByAge;
					IsSubClassifiedByAgeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsSubClassifiedByAgeInfo
		{
			get { return GetZPropertyInfo(nameof(IsSubClassifiedByAge)); }
		}

		#endregion

		#region Codes

		public static class SubClassificationCodes
		{
			public const string SubClassifiedByDebtor = "DEB";
			public const string SubClassifiedByCreditor = "CRD";
			public const string SubClassifiedByAge = "AGE";
		}

		#endregion

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ExtraInfoUniqueIndexFailureHandler(this); }
		}

		class ExtraInfoUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ExtraInfoUniqueIndexFailureHandler(CognosAccGLAccountDescriptorExtraInfo extraInfo)
			{
				this.ExtraInfo = extraInfo;
			}

			readonly CognosAccGLAccountDescriptorExtraInfo ExtraInfo;
			const string ConstraintName = "NR_UX__T9_AJ";

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ConstraintName; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(string.Format(CultureInfo.InvariantCulture,
					"The Cognos Account settings for Account '{0}' has already been configured by another user. Please close and re-open the form.",
					ExtraInfo.GLAccountDescriptor.AJ_LocalAccountNumber), "Error");
			}

			#endregion
		}

		#endregion

		ZBool? fShouldReconciliateTotal;
	}
}

#region Implementation
#endregion
