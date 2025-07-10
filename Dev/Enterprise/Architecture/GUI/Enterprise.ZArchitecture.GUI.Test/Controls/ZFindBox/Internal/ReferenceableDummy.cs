using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[CodeProperty("Z0_Code"), DescriptionProperty("Z0_Description", CanBeReferencedBy = true)]
	public class ReferenceableDummy : DummyBusinessObject
	{
		public ReferenceableDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
	}
}
