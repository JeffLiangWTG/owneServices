using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ZeroBalanceValidationHelper
	{
		public static bool IsZeroOSTotalAmountInvalid(InvoicingBase invoicingBase, out string errorMessage)
		{
			errorMessage = null;

			var result = invoicingBase.Lines.Count < 2
			   && !(invoicingBase.Lines.Count == 1
					&& invoicingBase.Lines[0].ChargeCode != null
					&& invoicingBase.Lines[0].ChargeCode.HighestChargeType == Core.Constants.ChargeType.Comment);

			if (result)
			{
				errorMessage = Res.GetString("f9fd39f1-8b2e-425d-bf03-b24a572723e1", "The sum of the transaction lines should not equal zero.");
			}
			else if (invoicingBase.IsARInvoiceOrCreditNoteOrAdjustmentNote && !AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value)
			{
				result = true;
				errorMessage = Res.GetString("00C7F509-F22C-46ED-83D7-F8AE8175969E", "Transaction Total cannot be zero. This is controlled by the registry: {0}", AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.HumanReadableRegistryPath());
			}
			
			return result;
		}

		public static bool IsZeroOSTotalAmountInvalidForPeriodicInvoice(out string errorMessage)
		{
			errorMessage = null;
			if (!AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.Value)
			{
				errorMessage = Res.GetString("5bd023fe-a333-42dd-bea4-07e1bff73423", "Total Amount cannot be 0. Please select transactions in order to generate Periodic Invoice. This is controlled by the registry: {0}.", AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.HumanReadableRegistryPath());
				return true;
			}

			return false;
		}
	}
}
