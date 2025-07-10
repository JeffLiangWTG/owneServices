using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty("Z0_Code")]
	[DescriptionProperty("Z0_Description")]
	public class DummyBusinessObjectWithCodeDescriptionProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithCodeDescriptionProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
