using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Client.___.Testing
{
	class ClientDummyBusinessObject : DummyEnterpriseBusinessObject
	{
		public ClientDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
