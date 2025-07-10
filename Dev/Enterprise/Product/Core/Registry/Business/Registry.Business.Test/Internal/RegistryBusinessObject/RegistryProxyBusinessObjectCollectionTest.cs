using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business.Testing
{
	abstract class RegistryProxyBusinessObjectCollectionTest<T, U> : RegistryBusinessObjectCollectionTemplateTestCase<T>
			where T : RegistryProxyBusinessObjectCollection<U> where U : RegistryProxyBusinessObject, new()
	{
		#region TestAddNewThrowsException

		public void TestAddNewThrowsException()
		{
			AssertExceptionThrown<NotImplementedException>(() => GetCollectionToTest().AddNew());
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		#endregion

		#region TestCollectionHasModuleId

		public void TestCollectionHasModuleId()
		{
			var collection = GetCollectionToTest();
			var moduleIdentifier = ZMetaData.GetModuleId(collection);
			AssertNotEquals("Collection should have a Module Id for it to work on a Module Button grid.", ModuleId.NotAssigned, moduleIdentifier.ID);
		}

		#endregion

		#region Implementation

		protected override bool SupportsAddNew
		{
			get { return false; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new U { ProxyPK = ZGuid.NewZGuid() };
		}

		#endregion
	}
}
