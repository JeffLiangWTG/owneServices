using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BranchLevelPostingConfiguration))]
	public class BranchLevelPostingConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new BranchLevelPostingConfiguration();
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
