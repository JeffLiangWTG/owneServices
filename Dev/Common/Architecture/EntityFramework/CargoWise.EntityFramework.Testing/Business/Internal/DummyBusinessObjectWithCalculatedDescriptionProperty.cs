using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty("Z0_Code")]
	[DescriptionProperty("TestDescriptionProperty")]
	public class DummyBusinessObjectWithCalculatedDescriptionProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithCalculatedDescriptionProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString TestDescriptionProperty { get; set; }
	}
}
