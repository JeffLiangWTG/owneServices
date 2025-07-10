using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.Wow.Business.Testing
{
	class WoolworthsImportedOrderTest : TestCaseWithFactory
	{
		public void TestAdditionalProperties()
		{
			WoolworthsImportedOrder order = Factory.NewWithValidTestData<WoolworthsImportedOrder>();
			order.IsUpdated = false;
			order.OrderLineStates = new Dictionary<ZGuid, bool>();
			ZGuid dummyPK = ZGuid.NewZGuid();
			order.OrderLineStates[dummyPK] = true;
			AssertEquals("order 's line state", true, order.OrderLineStates[dummyPK]);
			AssertEquals("order is updated", false, order.IsUpdated);
		}
	}
}
