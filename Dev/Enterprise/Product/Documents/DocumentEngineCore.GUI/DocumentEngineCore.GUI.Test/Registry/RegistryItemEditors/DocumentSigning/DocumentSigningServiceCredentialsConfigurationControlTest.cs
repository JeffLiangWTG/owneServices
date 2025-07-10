using CargoWise.EntityFramework;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.GUI.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsConfigurationControl))]
	sealed class DocumentSigningServiceCredentialsConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DocumentSigningServiceCredentialsConfiguration();
		}

		#endregion
	}
}
