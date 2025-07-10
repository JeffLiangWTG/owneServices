using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDocImportedOrderLineCollection))]
	class WowDocImportedOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WowDocImportedOrderLineCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(WowDocImportedOrderLineCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			return WowDocImportOrderLine.New(orderLine, Factory, true);
		}

		protected override WowDocImportedOrderLineCollection GetCollectionToTest()
		{
			return new WowDocImportedOrderLineCollection(Factory);
		}
	}
}
