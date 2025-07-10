using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Manages displaying the AlterReceiptForm.
	/// </summary>
	public partial class AlterReceiptManager
	{
		public AlterReceiptManager(MatchingBase matchingBizO)
		{
			fMatchingBase = matchingBizO;
		}

		MatchingBase fMatchingBase;

		public void ShowForm(IZForm parentForm)
		{
			Receipt receiptToAlter = PrepareReceipt();
			if (receiptToAlter == null)
			{
				Globals.Message.Show(Res.GetString("25bfd480-9a84-4cea-a2a8-62c464639f17", "There is no receipt linked."));
				return;
			}
			AlterReceiptForm alterRecForm = new AlterReceiptForm(receiptToAlter);
			alterRecForm.Closed += new EventHandler(AlterReceiptFormClosed);
			if (alterRecForm.ControllerID == null)
			{
				alterRecForm.ControllerID = AccountingControllerCreator.GetNewController(TransactionTypes.Receipt, fMatchingBase.LedgerType).ID;
			}
			ZFormModaliser.Show(alterRecForm, (Form)parentForm);
		}

		void AlterReceiptFormClosed(object sender, EventArgs e)
		{
			fMatchingBase.ReceiptDetail.UseReceiptValidation = false;
			fMatchingBase.UpdateAndValidateBalance();
			fMatchingBase.UpdatePaymentOSPartialPaidAmount();
		}

		Receipt PrepareReceipt()
		{
			Receipt receipt = null;

			if (fMatchingBase != null && fMatchingBase.ReceiptDetail != null)
			{
				receipt = fMatchingBase.ReceiptDetail;
				receipt.UseReceiptValidation = true;

				if (fMatchingBase.IsAllPaidInTheSameCurrency(receipt.AH_RX_NKTransactionCurrency))
				{
					ZDecimal oSAmountInSpecificCurrency = fMatchingBase.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly(receipt.AH_RX_NKTransactionCurrency, receipt.PK);
					if (!oSAmountInSpecificCurrency.IsEmpty)
					{
						receipt.AH_OSExTaxAmount = oSAmountInSpecificCurrency;
						((IMatching)receipt).OSPartialPaymentAmount = receipt.AH_OSExTaxAmount;

						if (AccountingConfigurationRegistry.Instance.UseInvoiceExchangeRateWhenChangingReceiptAmount.Value)
						{
							receipt.AH_LocalExTaxAmount = fMatchingBase.CalculateLocalBalanceExcludingReceipt();
						}
					}
				}
				else
				{
					receipt.AH_OSExTaxAmount = fMatchingBase.CalculateOSBalanceExcludingReceipt();
					((IMatching)receipt).OSPartialPaymentAmount = receipt.AH_OSExTaxAmount;
				}
			}

			return receipt;
		}
	}
}
