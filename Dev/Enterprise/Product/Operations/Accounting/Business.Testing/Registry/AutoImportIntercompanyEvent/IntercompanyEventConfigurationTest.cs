using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyEventConfiguration))]
	public class IntercompanyEventConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new IntercompanyEventConfiguration();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
