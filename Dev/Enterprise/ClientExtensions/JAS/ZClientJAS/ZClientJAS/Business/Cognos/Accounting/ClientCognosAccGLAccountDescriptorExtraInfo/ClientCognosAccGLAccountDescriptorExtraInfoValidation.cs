//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCognosAccGLAccountDescriptorExtraInfoValidation
//
//    This class should be used for overriding validation in AutoClientCognosAccGLAccountDescriptorExtraInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ClientCognosAccGLAccountDescriptorExtraInfoValidation : AutoClientCognosAccGLAccountDescriptorExtraInfoValidation
	{
		public ClientCognosAccGLAccountDescriptorExtraInfoValidation(AutoClientCognosAccGLAccountDescriptorExtraInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateIsSubClassifiedByDebtor();
			ValidateIsSubClassifiedByCreditor();
		}

		public void ValidateIsSubClassifiedByDebtor()
		{
			ValidateCalculatedProperty(Parent.IsSubClassifiedByDebtorInfo);
		}

		public void ValidateIsSubClassifiedByCreditor()
		{
			ValidateCalculatedProperty(Parent.IsSubClassifiedByCreditorInfo);
		}

		protected void CheckIsSubClassifiedByDebtor()
		{
			if (Parent.IsSubClassificationAccount && Parent.IsSubClassifiedByDebtor && Parent.MappedDebtorGroups.Count < 1)
			{
				Parent.IsSubClassifiedByDebtorInfo.AddError("Should map at least one Debtor Group when the Account is Sub Classified by Debtor");
			}
		}

		protected void CheckIsSubClassifiedByCreditor()
		{
			if (Parent.IsSubClassificationAccount && Parent.IsSubClassifiedByCreditor && Parent.MappedCreditorGroups.Count < 1)
			{
				Parent.IsSubClassifiedByCreditorInfo.AddError("Should map at least one Creditor Group when the Account is Sub Classified by Creditor");
			}
		}

		protected override void CheckT9_AJ_AccountToBeSubClassified()
		{
			base.CheckT9_AJ_AccountToBeSubClassified();

			if (Parent.IsSubClassificationAccount)
			{
				MandatoryValidation.CheckEntered(Parent.T9_AJ_AccountToBeSubClassifiedInfo);
				ListValidation.ErrorIfInvalidPK(Parent.T9_AJ_AccountToBeSubClassifiedInfo, Parent.Lookups.CognosAccounts);
				if (Parent.AccountToBeSubClassified != null)
				{
					CognosAccGLAccountDescriptor accountToBeSubClassified = Parent.AccountToBeSubClassified;
					if (accountToBeSubClassified.HasExtraInfoBeenCreated && accountToBeSubClassified.ExtraInfo.IsSubClassifyingAnotherSubClassificationAccount)
					{
						Parent.T9_AJ_AccountToBeSubClassifiedInfo.AddError("Cannot select an Account that is sub-classifying another Sub-Classification Account. The Sub-Classification hierarchy only goes to two levels down");
					}
				}
			}
		}

		protected override void CheckT9_ReconciliationTotalAccount()
		{
			base.CheckT9_ReconciliationTotalAccount();

			if (Parent.ShouldReconciliateTotal)
			{
				MandatoryValidation.CheckEntered(Parent.T9_ReconciliationTotalAccountInfo);
				CheckForDuplicateReconciliationTotalAccount();
			}
		}

		protected override void CheckT9_AccountAge()
		{
			base.CheckT9_AccountAge();

			if (Parent.IsSubClassificationAccount && Parent.IsSubClassifiedByAge)
			{
				MandatoryValidation.CheckEntered(Parent.T9_AccountAgeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.T9_AccountAgeInfo, Parent.Lookups.AccountAgeList);
			}
		}

		#region Implementation

		void CheckForDuplicateReconciliationTotalAccount()
		{
			if (!Parent.T9_ReconciliationTotalAccount.IsEmpty)
			{
				CognosAccGLAccountDescriptor otherAccount = GetOtherAccountWithTheSameAccountNumber();
				if (otherAccount != null)
				{
					string errorMessage = string.Format("Account Number '{0}' has already been used", Parent.T9_ReconciliationTotalAccount);
					Parent.T9_ReconciliationTotalAccountInfo.AddError(errorMessage);
				}
			}
		}

		CognosAccGLAccountDescriptor GetOtherAccountWithTheSameAccountNumber()
		{
			CognosAccGLAccountDescriptor result = null;

			ZQuery filter = new ZQuery(ClientCognosAccGLAccountDescriptorExtraInfoSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			filter.AddToFilter(ClientCognosAccGLAccountDescriptorExtraInfoSchema.T9_ReconciliationTotalAccount, Parent.T9_ReconciliationTotalAccount);
			CognosAccGLAccountDescriptorExtraInfo otherExtraInfo = Parent.Factory.LoadTop1<CognosAccGLAccountDescriptorExtraInfo>(filter);
			if (otherExtraInfo != null)
			{
				result = (CognosAccGLAccountDescriptor)otherExtraInfo.GLAccountDescriptor;
			}
			else
			{
				filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, Parent.T9_ReconciliationTotalAccount);
				filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger);
				result = Parent.Factory.LoadTop1<CognosAccGLAccountDescriptor>(filter);
			}

			return result;
		}

		new CognosAccGLAccountDescriptorExtraInfo Parent
		{
			get { return (CognosAccGLAccountDescriptorExtraInfo)base.Parent; }
		}

		#endregion
	}
}
