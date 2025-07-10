using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Germany
{
	public class GermanyPreferredPaymentMethod : IPreferredPaymentMethod
	{
		public string GetPreferredPaymentMethodWarning(string orgCategory, string paymentMethod)
		{
			return orgCategory == OrgConstants.Category.Government && paymentMethod != OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer
				? Res.GetString("1ebfa926-0396-4797-839f-088bba933375", "You are saving a German organization with the organization category set to Government. It is recommended to set all payment methods to TRF (Transfer) as you otherwise risk getting electronic invoices rejected.")
				: null;
		}
	}
}
