using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FindWindowQueryCosts))]
	sealed class FindWindowQueryCostsTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new FindWindowQueryCosts();
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
