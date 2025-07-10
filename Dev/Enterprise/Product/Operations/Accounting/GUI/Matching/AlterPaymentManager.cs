using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Manages displaying the AlterPaymentForm.
	/// </summary>
	public partial class AlterPaymentManager
	{
		public AlterPaymentManager(PaymentApprovalMatchingBase matchingBizO)
		{
			fMatchingBase = matchingBizO;
		}

		readonly PaymentApprovalMatchingBase fMatchingBase;

		public void ShowForm(IZForm parentForm)
		{
			PaymentApprovalBase paymentToAlter = PreparePayment();
			AlterPaymentApprovalForm alterPayForm = new AlterPaymentApprovalForm(paymentToAlter);
			alterPayForm.Closed += new EventHandler(AlterPaymentFormClosed);
			if (alterPayForm.ControllerID == null)
			{
				alterPayForm.ControllerID = AccountingControllerCreator.GetNewController(TransactionTypes.Payment, fMatchingBase.LedgerType).ID;
			}
			ZFormModaliser.Show(alterPayForm, (Form)parentForm);
		}

		void AlterPaymentFormClosed(object sender, EventArgs e)
		{
			fMatchingBase.UpdateAndValidateBalance();
			fMatchingBase.UpdatePaymentOSPartialPaidAmount();
		}

		PaymentApprovalBase PreparePayment()
		{
			PaymentApprovalMatchingBase approvalMatching = fMatchingBase;
			PaymentApprovalBase approval = null;

			if (approvalMatching != null && approvalMatching.PaymentApproval != null)
			{
				approval = approvalMatching.PaymentApproval;

				if (approvalMatching.IsAllPaidInTheSameCurrency(approval.AV_RX_NKPaymentCurrency))
				{
					ZDecimal oSAmountInSpecificCurrency = approvalMatching.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly(approval.AV_RX_NKPaymentCurrency, approval.PK);
					if (!oSAmountInSpecificCurrency.IsEmpty)
					{
						approval.AV_Amount = oSAmountInSpecificCurrency * -1;
						((IMatching)approval).OSPartialPaymentAmount = approval.AV_Amount;
					}
				}
				else
				{
					approval.AV_Amount = approvalMatching.CalculateOSBalanceExcludingPayment();
					((IMatching)approval).OSPartialPaymentAmount = approval.AV_Amount;
				}
			}

			return approval;
		}
	}
}
