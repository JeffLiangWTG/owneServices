using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class JournalValidation : TransactionHeaderValidation
	{
		public JournalValidation(Journal parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAH_Calc_FirstSubClassParentId();
			ValidateAH_Calc_SecondSubClassParentId();
		}

		Journal Journal
		{
			get { return (Journal)Parent; }
		}

		#region CheckAH_DueDate

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();
			MandatoryValidation.CheckEntered(Parent.AH_DueDateInfo);
		}

		#endregion

		#region CheckAH_OH

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			if (!Parent.AH_OHInfo.HasErrors() && Parent.Header != null)
			{
				if (Parent.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					if (!Parent.Header.OH_IsCreditor)
					{
						Parent.AH_OHInfo.AddError(Res.GetString("6aa0634a-1817-4e7e-a2a7-bb0dbf0bc6d2", "The Organization must have an Organization Type of Payables selected."));
					}
				}
				else if (Parent.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					if (!Parent.Header.OH_IsDebtor)
					{
						Parent.AH_OHInfo.AddError(Res.GetString("2339958d-48d4-452a-9a38-6315d89daa8b", "The Organization must have an Organization Type of Receivables selected."));
					}
				}
			}

			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
		}

		#endregion

		#region CheckAH_Desc

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		#endregion

		#region CheckAH_AG

		protected override void CheckAH_AG()
		{
			base.CheckAH_AG();

			if (Parent.GLHeader != null)
			{
				if (Parent.GLHeader.AG_DisallowDirectPosting)
				{
					Parent.AH_AGInfo.AddError(Res.GetString("4e040a20-b82c-4368-9781-9ed0a0789b4a", "This GL Account is flagged to prevent direct posting. Please select a different GL Account."));
				}
				else if (!Parent.GLHeader.AG_IsGlobal && !Parent.GLHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == GlbCompany.CurrentCompany.PK))
				{
					Parent.AH_AGInfo.AddError(Res.GetString("959eaf60-b1d2-4eff-838e-11b84d205133", "This GL Account cannot be used."));
				}

				if (Journal.IsMultiSubAccountsSupported && Journal.EnableCheckSubAccountsForGLHeader)
				{
					if (Journal.SubAccounts?.Count > 2)
					{
						if (Journal.SubAccounts.Cast<JournalSubAccount>().All(x => !x.IsSubClassValidationRuleMandatory))
						{
							Parent.AH_AGInfo.AddWarning(Res.GetString("0b301fc7-b69b-4725-ae91-da3f53f3a544", "This GL account has {0} sub account types. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu.", Journal.SubAccounts.Count));
						}
						else if (Journal.SubAccounts.Cast<JournalSubAccount>().Any(x => x.IsSubClassValidationRuleMandatory && x.SubAccountParentId.IsEmpty))
						{
							Journal.AH_AGInfo.AddError(Res.GetString("0b301fc7-b69b-4725-ae91-da3f53f3a545", "This GL account has {0} sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu.", Journal.SubAccounts.Count));
						}
					}
				}
			}
		}

		protected override void CheckIsAH_AGEmpty()
		{
			if (Parent is Journal journal &&
					journal.IsCashAdvanceJournal &&
					journal.AH_AG.IsEmpty)
			{
				Parent.AH_AGInfo.AddError(Res.GetString("ddaec8c3-bf21-4ae9-a605-698121456d80", "This Advance Payment can't be matched as there is no Advance Payment Clearing Account recorded in the {0} registry. Please ensure this registry has a Advance Payment Clearing account recorded and then try the match again.", AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.HumanReadableRegistryPath()));
			}
			else
			{
				base.CheckIsAH_AGEmpty();
			}
		}

		#endregion

		protected override void CheckAH_TransactionCategory()
		{
			base.CheckAH_TransactionCategory();
			MandatoryValidation.CheckEntered(Parent.AH_TransactionCategoryInfo);
			if (!(Parent.AH_Ledger == LedgerTypes.AccountsPayable && Journal.IsPaymentBasisWithholdingJournal || Journal.IsMultipleInstallmentsJournal))
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_TransactionCategoryInfo);
			}
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.AH_RX_NKTransactionCurrencyInfo);
		}

		protected override void CheckAH_GB()
		{
			base.CheckAH_GB();
			ListValidation.ErrorIfInvalidPK(Parent.AH_GBInfo, Parent.Lookups.Branches);
		}

		protected override void CheckAH_GE()
		{
			base.CheckAH_GE();
			ListValidation.ErrorIfInvalidPK(Parent.AH_GEInfo, Parent.Lookups.Departments);
		}

		public void ValidateDebitCreditSign()
		{
			ValidateCalculatedProperty(Journal.DebitCreditSignInfo);
		}

		protected void CheckDebitCreditSign()
		{
			ListValidation.ErrorIfInvalidCode(Journal.DebitCreditSignInfo, Journal.DebitCreditSign_List);
			MandatoryValidation.CheckEntered(Journal.DebitCreditSignInfo);
		}

		#region Calculated Properties Validation

		protected override void CheckAH_LocalExTaxAmount()
		{
			base.CheckAH_LocalExTaxAmount();
			MandatoryValidation.CheckEntered(Parent.AH_LocalExTaxAmountInfo);
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			MandatoryValidation.CheckEntered(Parent.AH_OSExTaxAmountInfo);
			if (!Parent.AH_OSExTaxAmountInfo.HasErrors())
			{
				if (Parent.AH_OSExTaxAmount < 0)
				{
					Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("0b301fc7-b69b-4725-ae91-da3f53f3a535", "must be non negative"));
				}
			}
		}

		#endregion

		#region AH_Calc_FirstSubClassParentId

		public void ValidateAH_Calc_FirstSubClassParentId()
		{
			ValidateCalculatedProperty(Journal.AH_Calc_FirstSubClassParentIdInfo);
		}

		protected void CheckAH_Calc_FirstSubClassParentId()
		{
			if (Journal.IsMultiSubAccountsSupported && Journal.EnableCheckSubAccountsForGLHeader)
			{
				SubAccountHelper.ValidateSubClassParentId(Journal.AH_Calc_FirstSubClassParentIdInfo, Journal.SubAccounts.FirstSubAccount?.AHS_SubClassParentTableCode ?? ZString.Empty, Journal);
			}
		}

		#endregion

		#region AH_Calc_SecondSubClassParentId

		public void ValidateAH_Calc_SecondSubClassParentId()
		{
			ValidateCalculatedProperty(Journal.AH_Calc_SecondSubClassParentIdInfo);
		}

		protected void CheckAH_Calc_SecondSubClassParentId()
		{
			if (Journal.IsMultiSubAccountsSupported && Journal.EnableCheckSubAccountsForGLHeader)
			{
				SubAccountHelper.ValidateSubClassParentId(Journal.AH_Calc_SecondSubClassParentIdInfo, Journal.SubAccounts.SecondSubAccount?.AHS_SubClassParentTableCode ?? ZString.Empty, Journal);
			}
		}

		#endregion
	}
}
