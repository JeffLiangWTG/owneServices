using CargoWise.EntityFramework;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.GUI.Testing
{
	[TestedType(typeof(DocumentSigningServicePartnerCredentialsControl))]
	sealed class DocumentSigningServicePartnerCredentialsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DocumentSigningServicePartnerCredentials();
		}

		#endregion
	}
}
