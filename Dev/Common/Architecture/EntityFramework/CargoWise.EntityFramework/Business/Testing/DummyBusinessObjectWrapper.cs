#if DEBUG

namespace CargoWise.EntityFramework.Testing
{
	[BusinessObjectTestExclude]
	public class DummyBusinessObjectWrapper : BusinessObjectWrapper
	{
		protected DummyBusinessObjectWrapper(DummyBusinessObject bizO) : base(bizO)
		{
		}

		public static DummyBusinessObjectWrapper Load(DummyBusinessObject bizO)
		{
			return (DummyBusinessObjectWrapper)Load(typeof(DummyBusinessObjectWrapper), bizO);
		}
	}
}

#endif
