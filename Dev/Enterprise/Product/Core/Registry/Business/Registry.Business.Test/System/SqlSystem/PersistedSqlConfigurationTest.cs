using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersistedSqlConfiguration))]
	sealed class PersistedSqlConfigurationTest : RegistryBusinessObjectTestCaseBase
	{
		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetNewBusinessObject() as RegistryBusinessObjectTemplate;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetNewBusinessObject() as RegistryBusinessObjectTemplate;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PersistedSqlConfiguration { ConfigurationId = 1592 };
		}
	}
}
