using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Billing.ClientLicencePriceHeader;

namespace ZClientEDI.Test.Billing.Module.BillingPrices;

[TestedType(typeof(ClientLicencePriceHeaderFilterBusinessObject))]
public class ClientLicencePriceHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new ClientLicencePriceHeaderFilterBusinessObject();
	}
}
