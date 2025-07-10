using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	sealed class ModuleRegistrationTest : TestCase
	{
		public void TestModuleRegistrationWhenNotAllowingMissingTypes()
		{
			ModuleIDs.ResetModuleIDs();
			ModuleListingSubsetRegister.ThrowWhenSubsetTypeUnavailableOverridable.Value = true;
			ModuleListingSubsetRegister.ResetRegistrations();
			AssertNoExceptionThrown($"All types specified within {nameof(ModuleListingSubsetRegister)} should be resolvable", () =>
			{
				_ = ModuleIDs.AllIncludingClientModules;
				_ = new ModuleList().All;
			});
		}
	}
}
