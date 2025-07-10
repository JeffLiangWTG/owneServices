using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	public class IsraelEInvoicingCredentialSettings : GlobalEInvoicingCredentialSettings, IEInvoicingCredentialXUEBehaviorProvider
	{
		protected override bool IsCompanyCredentialsRequired => true;
	}
}
