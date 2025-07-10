using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BiReportCredentialControl))]
	sealed class BiReportCredentialControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (BiReportCredentialControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BiReportCredential();
		}
	}
}
