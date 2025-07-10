using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class BaseModuleIDsTest : TestCase
	{
		protected abstract IEnumerable<ModuleIdentifier> GetModuleIDs();
		public abstract void TestNestedModules();
	}
}
