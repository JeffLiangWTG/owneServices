using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryProxyBusinessObjectMasterTest<T> : RegistryBusinessObjectTemplateTestCase<RegistryProxyBusinessObjectMaster<T>>
			where T : RegistryProxyBusinessObject, new()
	{
		#region TestCloneItems

		public void TestCloneItems()
		{
			var proxyMaster = GetBusinessObjectToClone();
			var proxy = new T();
			proxy.ProxyPK = ZGuid.NewZGuid();
			proxyMaster.Items.Add(proxy);

			var clone = (RegistryProxyBusinessObjectMaster<T>)proxyMaster.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertNotEquals("Items were cloned.", proxyMaster.Items, clone.Items);
			AssertEquals("Items were cloned.", 1, clone.Items.Count);

			var clonedItem = clone.Items[0];
			AssertNotEquals("Items were cloned.", proxy, clonedItem);
			AssertEquals("Items were cloned.", proxy.ProxyPK, clonedItem.ProxyPK);
		}

		#endregion

		#region TestItems

		public void TestItems()
		{
			var proxyMaster = GetBusinessObjectToClone();
			AssertNotNull(proxyMaster.Items);
			AssertEquals("Proxy Collection should be cached.", proxyMaster.Items, proxyMaster.Items);
			AssertNoExceptionThrown(() => proxyMaster.Items.Add(new T()));
		}

		#endregion

		#region TestFindBoxCollection

		public void TestFindBoxCollection()
		{
			var proxyMaster = GetBusinessObjectToClone();
			AssertNotNull(proxyMaster.FindBoxCollection);
			AssertEquals("FindBox Collection should be cached.", proxyMaster.FindBoxCollection, proxyMaster.FindBoxCollection);
			AssertEquals("FindBox Collection type should be correct.", ExpectedFindBoxCollectionType, proxyMaster.FindBoxCollection.GetType());
		}

		protected abstract Type ExpectedFindBoxCollectionType { get; }

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
