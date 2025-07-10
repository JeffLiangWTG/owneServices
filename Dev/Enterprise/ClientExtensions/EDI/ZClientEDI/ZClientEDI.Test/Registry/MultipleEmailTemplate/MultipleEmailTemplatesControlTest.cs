using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI
{
	[TestedType(typeof(MultipleEmailTemplatesControl))]
	class MultipleEmailTemplatesControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionEmailTemplateCollection(typeof(DocSupportIncident));
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			MultipleEmailTemplatesControl control = (MultipleEmailTemplatesControl)control1;
			return control.CategoryGrid.ReadOnly && control.notificationEmailTemplateRegistryControl1.ReadOnly;
		}
	}
}
