using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocAccountMovement : DocTransactionHeader
	{
		protected DocAccountMovement(AccountMovement transactionHeader, BusinessObjectFactory factoryToWrap)
			: base(transactionHeader, factoryToWrap)
		{
		}

		public static DocTransactionHeader New(AccountMovement transactionHeader, BusinessObjectFactory factoryToWrap)
		{
			DocTransactionHeader result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(transactionHeader, factoryToWrap);
			}
			else if (transactionHeader != null)
			{
				result = new DocAccountMovement(transactionHeader, factoryToWrap);
			}

			return result;
		}

		public override ZDecimal GSTAmount
		{
			get
			{
				if (AccountMovementTransaction.IsMultipleCurrency)
				{
					return base.GSTAmount;
				}
				else
				{
					return OSGstAmount;
				}
			}
		}

		public override ZDecimal InvoiceAmountWithGST
		{
			get
			{
				return GetInvoiceAmountWithGST(true);
			}
		}

		public override ZDecimal AccountMovementBalance
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (AccountMovementTransaction.IsFirstTransaction)
				{
					result = GetInvoiceAmountWithGST(false) + AccountMovementTransaction.OpeningBalance;
				}
				else
				{
					result = GetInvoiceAmountWithGST(false);
				}

				return result;
			}
		}

		ZDecimal GetInvoiceAmountWithGST(bool applyMultiplier)
		{
			if (AccountMovementTransaction.IsMultipleCurrency)
			{
				ZDecimal result = 0M;

				if (TransactionHeader != null)
				{
					DataRow row = ((INeedRow)TransactionHeader).Row;

					if (row != null)
					{
						result = new ZDecimal(TransactionHeader[AccTransactionHeader.Schema.AH_LocalTotal]);
					}
				}
				return result * (applyMultiplier ? AmountMultiplierForARCreditNote : 1);
			}
			else
			{
				return base.InvoiceAmountWithGST * (applyMultiplier ? 1 : AmountMultiplierForARCreditNote); //we have to undo the effect of applying multiplier (done in the setter of base.InvoiceAmountWithGST) when applyMultiplier = false
			}
		}

		protected override bool CalculateBalanceInLocalCurrency
		{
			get { return AccountMovementTransaction.IsMultipleCurrency; }
		}

		protected override void SplitAmountByChargeCode()
		{
			fAmountSplittedByChargeCode = new DocAmountByChargeCodeCollection(Factory);
			if (AccountMovementTransaction.TransactionLines != null)
			{
				foreach (TransactionLine line in AccountMovementTransaction.TransactionLines)
				{
					if (line.ChargeCode != null)
					{
						var amntChargeCode = new AmountByChargeCode(line.ChargeCode.AC_Code, AccountMovementTransaction.IsMultipleCurrency ? line.AL_LocalTotalAmount : line.AL_OSAmount, AccountMovementTransaction.IsMultipleCurrency ? line.AL_LocalExTaxAmount : line.AL_OSExTaxAmount);
						fAmountSplittedByChargeCode.Add(DocAmountByChargeCode.New(amntChargeCode, Factory));
					}
				}
			}
		}

		AccountMovement AccountMovementTransaction
		{
			get { return base.TransactionHeader as AccountMovement; }
		}
	}
}
