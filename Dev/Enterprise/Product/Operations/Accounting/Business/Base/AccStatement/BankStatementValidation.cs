using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class BankStatementValidation : AccBankAccountValidation
	{
		public BankStatementValidation(BankStatement parent)
			: base(parent)
		{
		}

		new BankStatement Parent
		{
			get { return (BankStatement)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateStatementDateFilter();
			ValidateDebitCreditFilter();
			ValidateTypeFilter();
			ValidateBalanceAmount();
		}

		public void ValidateStatementDateFilter()
		{
			ValidateCalculatedProperty(Parent.StatementDateFilterInfo);
		}

		protected void CheckStatementDateFilter()
		{
			MandatoryValidation.CheckEntered(Parent.StatementDateFilterInfo);
			if (!Parent.StatementDateFilterInfo.HasErrors())
			{
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.StatementDateFilterInfo);
			}
		}

		public void ValidateDebitCreditFilter()
		{
			ValidateCalculatedProperty(Parent.DebitCreditFilterInfo);
		}

		protected void CheckDebitCreditFilter()
		{
			ListValidation.ErrorIfInvalidCode(Parent.DebitCreditFilterInfo, Parent.AB_DebitCredit_List);
		}

		public void ValidateTypeFilter()
		{
			ValidateCalculatedProperty(Parent.TypeFilterInfo);
		}

		protected void CheckTypeFilter()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TypeFilterInfo, Parent.AB_Type_List);
		}

		public void ValidateBalanceAmount()
		{
			ValidateCalculatedProperty(Parent.BalanceAmountInfo);
		}

		protected void CheckBalanceAmount()
		{
			if (Parent.BalanceAmount != 0m)
			{
				Parent.BalanceAmountInfo.AddWarning(Res.GetString("eaf3081d-9fa6-4d48-b033-869f2fb3fbeb", "If you are entering a new statement, there is an error in the statement entry."));
			}
		}
	}
}
