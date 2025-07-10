using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsConfidenceThresholdsUserControl))]
	sealed class DpsConfidenceThresholdsUserControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((DpsConfidenceThresholdsUserControl)control).ReadOnly;

		protected override IBusiness GetNewBusinessEntity() => new DpsConfidenceThresholdsBusinessObject();
	}
}
