using CargoWise.EntityFramework;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NotificationFrequencyControl))]
	sealed class NotificationFrequencyControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new NotificationFrequency();
		}
	}
}
