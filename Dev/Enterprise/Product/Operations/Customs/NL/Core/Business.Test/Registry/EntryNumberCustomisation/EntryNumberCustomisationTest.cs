using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(EntryNumberCustomisation))]
class EntryNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<EntryNumberCustomisation>
{
	protected override bool RequiresFactory => false;

	protected override bool RequiresFallbackLevel => false;

	protected override EntryNumberCustomisation GetBusinessObjectToClone()
	{
		return new EntryNumberCustomisation();
	}

	protected override EntryNumberCustomisation GetBusinessObjectToSerialise()
	{
		return new EntryNumberCustomisation();
	}
}
