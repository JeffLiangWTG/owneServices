using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(DrawbackEntryLineModule))]
	sealed class DrawbackEntryLineModuleTest : Customs.Module.Testing.EntryLineModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.DrawbackEntryLine;
	}
}
