using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferChargeLine : DirectPayment.DirectPaymentLine
	{
		public BankTransferChargeLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AL_AG = (Guid)AccountingConfigurationRegistry.Instance.FinanceChargesAccount.Value;
		}

		protected override bool SupportsInputVatRecoverableCore
		{
			get { return false; }
		}

		protected override Security.SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.None; }
		}

		public BankTransferCharge BankTransferCharge => MasterTransactionHeader as BankTransferCharge;

		public override AccTransactionHeader TransactionHeader => BankTransferCharge ?? base.TransactionHeader;

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (IsInDatabase || !EnableFinanceCharge)
			{
				return new BankTransferChargeLineEmptyValidation(this);
			}
			else
			{
				return base.GetNewValidationCore();
			}
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (EnableFinanceCharge && !IsInDatabase); }
		}

		public bool EnableFinanceCharge
		{
			get { return BankTransferCharge?.EnableFinanceCharge ?? false; }
		}

		protected override bool IsMultiSubAccountsSupportedCore => false;
	}
}
