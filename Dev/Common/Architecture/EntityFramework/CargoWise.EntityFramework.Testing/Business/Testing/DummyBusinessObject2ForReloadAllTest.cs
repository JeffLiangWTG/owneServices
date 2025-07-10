using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyBusinessObject2ForReloadAllTest : DummyBusinessObject
	{
		public DummyBusinessObject2ForReloadAllTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
