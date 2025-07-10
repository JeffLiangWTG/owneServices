using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	public class AccBillingEventCollectorTest : TestCaseWithFactory
	{
		public void TestAddAndGetEvent()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var bizOPK1 = new ZGuid(Guid.NewGuid());
			var bizOPK2 = new ZGuid(Guid.NewGuid());
			var bizOPK3 = new ZGuid(Guid.NewGuid());

			AccBillingEventCollector.GetInstance(factory1).AddEvent("ABC", bizOPK1, "JK", "CST");
			AccBillingEventCollector.GetInstance(factory1).AddEvent("ABC", bizOPK1, "JK", "REV");
			AccBillingEventCollector.GetInstance(factory1).AddEvent("XYZ", bizOPK1, "JK", "PER");
			AccBillingEventCollector.GetInstance(factory2).AddEvent("XYZ", bizOPK1, "JK", "LAT");
			AccBillingEventCollector.GetInstance(factory1).AddEvent("ABC", bizOPK2, "JK", "NOR");
			AccBillingEventCollector.GetInstance(factory1).AddEvent("ABC", bizOPK3, "JK", "TST");

			var events = AccBillingEventCollector.GetInstance(factory1).GetEvents("ABC");
			AssertEquals(3, events.Count());
			AssertEquals("PST", events.First(e => e.BilledBizOPK == bizOPK1).EventCode);
			AssertEquals("NOR", events.First(e => e.BilledBizOPK == bizOPK2).EventCode);
			AssertEquals("TST", events.First(e => e.BilledBizOPK == bizOPK3).EventCode);

			events = AccBillingEventCollector.GetInstance(factory1).GetEvents("XYZ");
			AssertEquals(1, events.Count());
			AssertEquals("PER", events.First(e => e.BilledBizOPK == bizOPK1).EventCode);

			events = AccBillingEventCollector.GetInstance(factory2).GetEvents("XYZ");
			AssertEquals(1, events.Count());
			AssertEquals("LAT", events.First(e => e.BilledBizOPK == bizOPK1).EventCode);
		}
	}
}
