using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.DataTransfer.Native.Service
{
	sealed class WebConfigTest : BaseWebConfigTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Core", "NativeDataTransfer", "DataTransfer.Native", "Enterprise.DataTransfer.Native.Service", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
