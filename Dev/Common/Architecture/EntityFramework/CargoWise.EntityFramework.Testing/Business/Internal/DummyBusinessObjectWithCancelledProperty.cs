using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	[IsCancelledProperty("Z0_Bool")]
	internal class DummyBusinessObjectWithCancelledProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithCancelledProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
