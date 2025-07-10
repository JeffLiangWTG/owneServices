using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AQISProducerCodeModule))]
	sealed class AQISProducerCodeModuleTest : CMRSearchOnlyModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.AQISProducerCode;
	}
}
