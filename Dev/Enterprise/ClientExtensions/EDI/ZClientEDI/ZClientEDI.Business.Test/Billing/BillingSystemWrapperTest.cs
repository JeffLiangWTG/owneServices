using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BillingSystemWrapper))]
	public class BillingSystemWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var system = new AirlineMessagingBillingSystem();
			var wrapper = new BillingSystemWrapper(system);
			AssertEquals(wrapper.IsEnabled, system.IsEnabled);
			AssertEquals(wrapper.SystemCode, system.SystemCode);
			AssertEquals(wrapper.SystemDescription, system.SystemDescription);

			foreach (var boolValue in new[] { true, false })
			{
				wrapper.IsEnabled = boolValue;
				AssertEquals(boolValue, system.IsEnabled);
				AssertEquals(wrapper.IsEnabled, system.IsEnabled);

				system.IsEnabled = !boolValue;
				AssertEquals(!boolValue, wrapper.IsEnabled);
				AssertEquals(wrapper.IsEnabled, system.IsEnabled);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BillingSystemWrapper(new AirlineMessagingBillingSystem());
		}
	}

	[TestedType(typeof(BillingSystemWrapperCollection))]
	public class BillingSystemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillingSystemWrapperCollection>
	{
		public void TestConstructor()
		{
			var systems = new BillingSystemList();
			var collection = new BillingSystemWrapperCollection(systems);
			AssertEquals(systems.Count, collection.Count);
			foreach (var system in systems)
			{
				AssertEquals(true, collection.OfType<BillingSystemWrapper>().Any(x => x.SystemCode == system.SystemCode));
			}
		}

		protected override BillingSystemWrapperCollection GetCollectionToTest()
		{
			return new BillingSystemWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillingSystemWrapper(new AirlineMessagingBillingSystem());
		}
	}
}
