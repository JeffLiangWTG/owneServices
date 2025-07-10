using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class BusinessObjectRegistryItemTest : TransactionedTestCase
	{
		public void TestBusinessObjectRegistryItem()
		{
			var item = new BusinessObjectRegistryItem<DummyBusinessObject>("Rego", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			var factory = new BusinessObjectFactory();
			AssertNull(item.Value.BusinessObject(factory));
			var dummy = factory.New<DummyBusinessObject>();
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dummy);
			AssertEquals(dummy, item.Value.BusinessObject(factory));

			var factory2 = new BusinessObjectFactory();
			AssertNull(item.Value.BusinessObject(factory2));

			factory.Save();
			AssertEquals(dummy.PK, item.Value.BusinessObject(factory2).PK);
			AssertEquals(dummy.PK, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).BusinessObject(factory2).PK);
		}
	}
}
