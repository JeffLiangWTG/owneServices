using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleIDLoader_Test : TestCase
	{
		public void TestGetModuleIDs()
		{
			Array arrModuleIDs = ModuleIDLoader.GetModuleIDs(typeof(ModuleIDs), typeof(ModuleIdentifier));
			AssertEquals(typeof(ModuleIdentifier[]), arrModuleIDs.GetType());
		}
	}
}
