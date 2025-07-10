using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DummyDependentBusinessObject : AutoDummyDependentBizo, IDummyDependant
	{
		public DummyDependentBusinessObject(BusinessObjectFactory factory, DataRow row)
	: base(factory, row)
		{
		}
	}
}
