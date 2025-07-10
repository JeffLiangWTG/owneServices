using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(IncidentEmailTemplatePairRegistryControl))]
	sealed class IncidentEmailTemplatePairRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new IncidentEmailTemplatePair(typeof(DocSupportIncident));
		}
	}
}
