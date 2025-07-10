using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty("TestCodeProperty")]
	[DescriptionProperty("TestCodeProperty")]
	public class DummyBusinessObjectWithCalculatedCodeProperty : DummyBusinessObject
	{
		public DummyBusinessObjectWithCalculatedCodeProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString TestCodeProperty
		{
			get
			{
				return fTestCodeProperty;
			}
			set
			{
				fTestCodeProperty = value;
			}
		}
		ZString fTestCodeProperty;
	}
}
