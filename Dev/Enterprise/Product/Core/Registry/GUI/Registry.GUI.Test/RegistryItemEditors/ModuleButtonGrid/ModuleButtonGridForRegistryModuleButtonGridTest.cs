#if !WINZOR
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ModuleButtonGridForRegistry<RecordAttacherForRegistryTest.DummyProxy>))]
	sealed class ModuleButtonGridForRegistryModuleButtonGridTest : Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
#endif
