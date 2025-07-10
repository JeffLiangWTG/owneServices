using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	[Flags]
	public enum PaymentPrintOptions { None = 0, Cheque = 1, PaymentVoucher = 2, RemittanceAdvice = 4, PaymentBatchListing = 8 }
	public abstract class PaymentPrintManagerBase
	{
		protected PaymentPrintOptions GetPaymentPrintOptions(Func<PaymentDocumentsPrintPopup> getNewPaymentDocumentsPrintPopup)
		{
			var showRemittanceForm = AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			var printCheque = AccountingConfigurationRegistry.Instance.PrintCheque.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

			if (showRemittanceForm || printCheque)
			{
				using (var printForm = getNewPaymentDocumentsPrintPopup())
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(printForm) == DialogResult.OK)
					{
						return printForm.GetSelectedOptions();
					}
				}
			}
			else
			{
				try
				{
					return GetDefaultOptionsFromRegistry();
				}
				catch (ReportException ex)
				{
					ErrorReporter.ReportOnce("PaymentPrintManagerBase.GetPaymentPrintOptions", "", ex);
				}
			}

			return default;
		}

		PaymentPrintOptions GetDefaultOptionsFromRegistry()
		{
			PaymentPrintOptions result = default;

			void or(BooleanRegistryItem registry, PaymentPrintOptions option)
			{
				if (registry.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					result |= option;
				}
			}

			or(AccountingConfigurationRegistry.Instance.PrintCheque, PaymentPrintOptions.Cheque);
			or(AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice, PaymentPrintOptions.RemittanceAdvice);
			or(AccountingConfigurationRegistry.Instance.PrintPaymentVoucher, PaymentPrintOptions.PaymentVoucher);
			or(AccountingConfigurationRegistry.Instance.PrintPaymentBatchListing, PaymentPrintOptions.PaymentBatchListing);

			return result;
		}

		#region Payment Properties

		public string PaymentDescription
		{
			get
			{
				ZString description = PaymentTypeCode + " " + PaymentChequeOrReference + " ";

				if (!PaymentOrganisationCode.IsEmpty)
				{
					description += "(" + PaymentOrganisationCode + ")";
				}

				return description;
			}
		}

		protected virtual ZString PaymentOrganisationCode => ZString.Empty;
		protected virtual ZString PaymentTypeCode => ZString.Empty;
		protected virtual ZString PaymentChequeOrReference => ZString.Empty;

		#endregion
	}
}
