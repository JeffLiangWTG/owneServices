using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferReversalValidation : BankTransferValidation
	{
		public BankTransferReversalValidation(BankTransfer parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAH_PostDate();
			ValidateFinanceChargeOSTotal();
		}

		public override Type AutoValidationType
		{
			get { return typeof(BankTransferReversalValidation); }
		}

		protected override void CheckAH_PostDate()
		{
			MandatoryValidation.CheckEntered(Parent.AH_PostDateInfo);
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				BankTransfer originalTransaction = Parent.OriginalTransaction;
				if (originalTransaction != null && Parent.AH_PostDate.Date < originalTransaction.AH_PostDate.Date)
				{
					Parent.AH_PostDateInfo.AddError(Res.GetString("5f5067b2-5a8d-451d-94af-14770928f08f", "Reversing post date cannot be before the original post date of '{0}'.", originalTransaction.AH_PostDate.ToShortDateString()));
				}

				if (!Parent.AH_PostDateInfo.HasErrors())
				{
					base.CheckAH_PostDate();
				}
			}
		}

		public void ValidateFinanceChargeOSTotal()
		{
			ValidateCalculatedProperty(Parent.FinanceChargeOSTotalInfo);
		}

		protected virtual void CheckFinanceChargeOSTotal()
		{
		}

		protected override void CheckDescription()
		{
		}

		protected override void CheckReference()
		{
		}

		protected override void CheckEnableFinanceCharge()
		{
		}

		readonly BankTransfer Parent;
	}
}