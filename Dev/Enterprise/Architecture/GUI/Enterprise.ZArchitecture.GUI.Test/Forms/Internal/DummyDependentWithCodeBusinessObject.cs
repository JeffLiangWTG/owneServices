using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class DummyDependentWithCodeBusinessObject : AutoDummyDependentBizo, IDummyDependant
	{
		public DummyDependentWithCodeBusinessObject(BusinessObjectFactory factory, DataRow row)
	: base(factory, row)
		{
		}
	}
}
