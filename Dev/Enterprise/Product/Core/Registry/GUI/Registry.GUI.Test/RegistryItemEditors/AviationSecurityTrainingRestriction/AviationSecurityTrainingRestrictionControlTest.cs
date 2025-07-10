using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(AviationSecurityTrainingRestrictionControl))]
	sealed class AviationSecurityTrainingRestrictionControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new AviationSecurityTrainingRestriction();
		}
	}
}
