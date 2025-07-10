using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.BillingPrices;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(BillingPricesCollection))]
	public class BillingPricesCollectionTest : ActiveBusinessObjectCollectionTestCase<BillingPricesCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(BillingPricesCollection);
		}
	}
}
