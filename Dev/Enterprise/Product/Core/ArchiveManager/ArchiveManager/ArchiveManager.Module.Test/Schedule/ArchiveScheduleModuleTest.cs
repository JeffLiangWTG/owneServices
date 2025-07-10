using Enterprise.ArchiveManager.Module.Schedule;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleModule))]
	class ArchiveScheduleModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
			=> ModuleIDs.ArchiveSchedule;
	}
}
