using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[AllowAllObjectsToBeLoaded]
	[CodeProperty(Schema.ZD1_Code)]
	public sealed class DummyDependentWithCodeBusinessObject : AutoDummyDependentBizo, IDummyDependant
	{
		public DummyDependentWithCodeBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
