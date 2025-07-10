using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DummyBusinessObjectForTesting : DummyBusinessObject, ICADeclarationProvider
	{
		public DummyBusinessObjectForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZBool ICADeclarationProvider.IsValidationEnabled => true;

		BaseJobDeclaration IDeclarationProvider.Declaration => null;

		BusinessObjectFactory IDeclarationProvider.Factory => Factory;
	}
}
