using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcessingFees()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			AssertEquals(3, EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.Count);
			AssertEquals(3, billing.Lookups.ProcessingFees.Count);

			AssertEquals(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value[0].Code, billing.Lookups.ProcessingFees[0].Code);
			AssertEquals(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value[1].Code, billing.Lookups.ProcessingFees[1].Code);
			AssertEquals(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value[2].Code, billing.Lookups.ProcessingFees[2].Code);
		}
	}
}