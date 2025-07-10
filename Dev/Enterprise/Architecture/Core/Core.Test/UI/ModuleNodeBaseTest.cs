using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Core.Modules.Testing
{
	sealed class ModuleNodeBaseTest : TestCase
	{
		public void TestDisplayTextWithoutAmpersand()
		{
			ModuleNodeBase node = new ModuleNodeBase("A", (NoResString)"Something", null);
			AssertEquals("Something", node.DisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Some&thing", null);
			AssertEquals("Something", node.DisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Some&&thing", null);
			AssertEquals("Some&thing", node.DisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Something && Nothing", null);
			AssertEquals("Something & Nothing", node.DisplayTextWithoutAmpersand);
		}

		public void TestUnresolvedDisplayTextWithoutAmpersand()
		{
			ModuleNodeBase node = new ModuleNodeBase("A", (NoResString)"Something", null);
			AssertEquals("Something", node.UnresolvedDisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Some&thing", null);
			AssertEquals("Something", node.UnresolvedDisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Some&&thing", null);
			AssertEquals("Some&thing", node.UnresolvedDisplayTextWithoutAmpersand);

			node = new ModuleNodeBase("A", (NoResString)"Something && Nothing", null);
			AssertEquals("Something & Nothing", node.UnresolvedDisplayTextWithoutAmpersand);
		}
	}
}
