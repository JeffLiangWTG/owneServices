using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMREstablishmentCodesModule))]
	sealed class CMREstablishmentCodesModuleTest : CMRSearchOnlyModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.CMREstablishmentCodes;
	}
}
