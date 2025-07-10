using Enterprise.ZArchitecture.Core.Test;
using NUnit.Framework;

namespace Enterprise.StabilityChecker.Test
{
	[TestedType(typeof(StabilityCheckerAttribute))]
	sealed class StabilityCheckerAttributeTest : AssemblyMetaDataAttributeTestCase<StabilityCheckerAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.Description = "Description";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Description = "Description";
			Assert(attribute1.Equals(attribute2));

			attribute1.Category = "Category";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Category = "Category";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
