using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingLookups : AutoClientLicenceBillingLookups
	{
		public ClientLicenceBillingLookups(AutoClientLicenceBilling parent)
			: base(parent)
		{
		}

		protected new ClientLicenceBilling Parent
		{
			get { return (ClientLicenceBilling)base.Parent; }
		}

		public CodeDescriptionBoolCollection ProcessingFees
		{
			get { return EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value; }
		}
	}
}

