using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(KafkaSecurityControl))]
	sealed class KafkaSecurityControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new Business.KafkaSecurity();
	}
}
