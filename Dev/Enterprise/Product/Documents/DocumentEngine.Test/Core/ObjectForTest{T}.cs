using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Testing
{
	class ObjectForTest<T> : NonPersistentBusinessObject
	{
		public T Property { get; set; }
	}
}
