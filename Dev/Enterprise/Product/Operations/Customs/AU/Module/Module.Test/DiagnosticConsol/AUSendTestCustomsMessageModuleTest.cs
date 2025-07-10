using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUSendTestCustomsMessageModule))]
	sealed class AUSendTestCustomsMessageModuleTest : Customs.Module.Testing.SendDiagnosticMessageModuleTest<AUSendTestCustomsMessageModule, AUSendTestCustomsMessageController>
	{
	}
}
