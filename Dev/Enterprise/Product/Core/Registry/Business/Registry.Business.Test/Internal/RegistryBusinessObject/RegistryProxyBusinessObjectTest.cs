using System;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryProxyBusinessObjectTest<T> : RegistryBusinessObjectTemplateTestCase<T>
			where T : RegistryProxyBusinessObject, new()
	{
		#region TestCloneProxyPK

		public void TestCloneProxyPK()
		{
			var proxy = new T();
			proxy.ProxyPK = ZGuid.NewZGuid();

			var clone = (T)proxy.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals("Guid should be cloned.", clone.ProxyPK, proxy.ProxyPK);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var proxy = new T();
			var collection = GetNewCollection();
			collection.Add(proxy);
			AssertContainsExactElementsInAnyOrder("Precondition: Element is in collection.", new[] { proxy }, collection);

			proxy.Delete();
			AssertEquals("Element is removed from the collection on delete.", 0, collection.Count);
		}

		protected abstract RegistryProxyBusinessObjectCollection<T> GetNewCollection();

		#endregion

		#region TestPK

		public void TestPK()
		{
			var proxy = new T();
			AssertEquals("PK property should use the Proxy PK.", ZGuid.Empty, proxy.PK);

			proxy.ProxyPK = ZGuid.NewZGuid();
			AssertEquals("PK property should use the Proxy PK.", proxy.ProxyPK, proxy.PK);
		}

		#endregion

		#region TestSerialiseEmptyProxy

		public void TestSerialiseEmptyProxy()
		{
			var proxy = new T();
			var typeName = typeof(T).Name;
			var serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(proxy);
			AssertEquals(string.Format(@"<?xml version=""1.0"" encoding=""utf-16""?><{0}><ProxyPK>00000000-0000-0000-0000-000000000000</ProxyPK></{0}>", typeName),
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserialisedProxy = RegistryBusinessObjectTemplateTestCase.Deserialize<T>(serializedValue);
			AssertEquals(ZGuid.Empty, deserialisedProxy.ProxyPK);
		}

		#endregion

		#region TestSerialiseFullyPopulatedProxy

		public void TestSerialiseFullyPopulatedProxy()
		{
			var proxy = new T();
			proxy.ProxyPK = ZGuid.NewZGuid();

			var typeName = typeof(T).Name;
			var serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(proxy);
			AssertEquals(string.Format(@"<?xml version=""1.0"" encoding=""utf-16""?><{0}><ProxyPK>{1}</ProxyPK></{0}>", typeName, proxy.ProxyPK),
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserialisedProxy = RegistryBusinessObjectTemplateTestCase.Deserialize<T>(serializedValue);
			AssertEquals(proxy.ProxyPK, deserialisedProxy.ProxyPK);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override T GetBusinessObjectToClone()
		{
			return new T();
		}

		protected override T GetBusinessObjectToSerialise()
		{
			return new T();
		}

		#endregion
	}
}
