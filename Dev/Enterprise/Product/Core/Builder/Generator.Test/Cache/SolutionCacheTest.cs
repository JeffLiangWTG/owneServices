using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Cache.Testing
{
	sealed class SolutionCacheTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSolution()
		{
			//Arrange
			var sampleFileToCheck = BuildConstants.LocalEnterprisePath + @"Enterprise\Product\Core\Builder\Builder.sln";

			//Act
			var result = SolutionCache.Instance.GetSolution(sampleFileToCheck);

			//Assert
			AssertEquals(sampleFileToCheck, result.SolutionFileName);
		}
	}
}
