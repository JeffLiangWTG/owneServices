using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferValidation : ZValidation
	{
		public BankTransferValidation(BankTransfer parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		static string FinanceChargeAccountMissing
		{
			get { return Res.GetString("2d4439f5-43be-4031-8adb-96d4f8aa998f", "Finance charge general ledger account is missing in the the registry. Finance Charge Account registry item is located in Accounting -> General Ledger Defaults -> Link Account in the Registry."); }
		}

		public override void ValidateAll()
		{
			ValidateDescription();
			ValidateReference();
			ValidateEnableFinanceCharge();
		}

		public override Type AutoValidationType
		{
			get { return typeof(BankTransferValidation); }
		}

		public void ValidateAH_PostDate()
		{
			ValidateCalculatedProperty(Parent.AH_PostDateInfo);
		}

		protected virtual void CheckAH_PostDate()
		{
			if (Parent.TransferRowFrom.AH_PostDateInfo.HasNotifications())
			{
				Parent.AH_PostDateInfo.AddAllNotificationsFrom(Parent.TransferRowFrom.AH_PostDateInfo);
			}
		}

		public void ValidateDescription()
		{
			ValidateCalculatedProperty(Parent.DescriptionInfo);
		}

		protected virtual void CheckDescription()
		{
			if (!Parent.IsValidationSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.DescriptionInfo);
			}
		}

		public void ValidateReference()
		{
			ValidateCalculatedProperty(Parent.ReferenceInfo);
		}

		protected virtual void CheckReference()
		{
			if (!Parent.IsValidationSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.ReferenceInfo);
			}
		}

		public void ValidateEnableFinanceCharge()
		{
			ValidateCalculatedProperty(Parent.EnableFinanceChargeInfo);
		}

		protected virtual void CheckEnableFinanceCharge()
		{
			if (!Parent.IsValidationSuspended)
			{
				if (Parent.EnableFinanceCharge && (((Guid)AccountingConfigurationRegistry.Instance.FinanceChargesAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)) == Guid.Empty))
				{
					Parent.EnableFinanceChargeInfo.AddError(FinanceChargeAccountMissing);
				}
			}
		}

		readonly BankTransfer Parent;
	}
}