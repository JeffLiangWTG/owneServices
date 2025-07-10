using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace CargoWise.ProductRegistration.Service.Test
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class WebConfigTest : BaseWebConfigTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "ClientExtensions", "EDI", "ProductRegistration", "Service", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
