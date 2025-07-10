using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientFaxPrice))]
	public class ClientFaxPriceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertiesReadOnly()
		{
			ClientFaxPrice price = Factory.New<ClientFaxPrice>();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals(false, price.CFP_RX_NKCurrencyCodeInfo.ReadOnly);
			AssertEquals(false, price.CFP_PageRateInfo.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			AssertEquals(true, price.CFP_RX_NKCurrencyCodeInfo.ReadOnly);
			AssertEquals(true, price.CFP_PageRateInfo.ReadOnly);
		}
	}
}
