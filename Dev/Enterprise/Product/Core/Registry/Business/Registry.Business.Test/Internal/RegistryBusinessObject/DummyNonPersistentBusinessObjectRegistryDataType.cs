using System;

namespace Enterprise.Registry.Business.Testing
{
	[Testing.ExcludeFromRegistryDataTypeTest]
	public sealed class DummyNonPersistentBusinessObjectRegistryDataType : WeaklyTypedNonPersistentBusinessObjectRegistryDataType
	{
		public DummyNonPersistentBusinessObjectRegistryDataType(Type dataType)
			: base(dataType)
		{
		}
	}
}
