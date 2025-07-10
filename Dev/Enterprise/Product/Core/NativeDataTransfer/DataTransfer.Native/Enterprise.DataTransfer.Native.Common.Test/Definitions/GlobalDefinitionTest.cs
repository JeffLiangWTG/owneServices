using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public class GlobalDefinitionTest : TestCase
	{
		public void TestInit()
		{
			var globalDefinition = GlobalDefinition.Instance;
			AssertEquals("No Exception should throw", true, true);
			AssertNotNull("EDICodeMapping should not be null", globalDefinition.CodeMappings);
		}

		public void TestDummyBizoIsNotInMapping()
		{
			var globalDefinition = GlobalDefinition.Instance;
			AssertCollectionNotContains("DummyBizo should not be part of the Global Definitions", "DummyBizo", globalDefinition.TableMapping.Keys);
		}
	}
}
