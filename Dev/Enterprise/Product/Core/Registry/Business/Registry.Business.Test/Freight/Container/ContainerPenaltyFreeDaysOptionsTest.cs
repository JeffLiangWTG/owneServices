using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContainerPenaltyFreeDaysOptions))]
	sealed class ContainerPenaltyFreeDaysOptionsTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ContainerPenaltyFreeDaysOptions();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new ContainerPenaltyFreeDaysOptions();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
