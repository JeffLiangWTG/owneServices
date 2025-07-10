using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	[IsActiveProperty("Z0_Bool")]
	internal class DummyBusinessObjectWithActiveProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithActiveProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
