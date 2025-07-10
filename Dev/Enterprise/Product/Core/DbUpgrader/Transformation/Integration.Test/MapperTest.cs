using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.DbUpgrader.Transformation.Integration.Test
{
	class MapperTest : TestCase
	{
		public void TestNoTransformationsMappedToNonZeroMinorVersionOnAlphaRelease()
		{
			if (ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP)
			{
				var allMappings = Mapper.GetAllMappings();

				foreach (var map in allMappings)
				{
					if (map.MappedVersion.Major != 210 && map.MappedVersion.Major != 1000)
					{
						AssertEquals($"Transformation [{map.TransformationType.FullName}] mapped to a minor version != 0", 0, map.MappedVersion.Minor);
					}
				}
			}
			else
			{
				Assert("This test should run on Alpha (Ring 0) release only", true);
			}
		}

		public void TestMinorVersionMustBeZeroOnAlphaRelease()
		{
			if (ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP)
			{
				AssertEquals("DataTransformation Minor Version MUST be 0 (zero) on Alpha release:", 0, TransformationVersion.ApplicationNumber.Minor);
			}
			else
			{
				Assert("This test should run on Alpha (Ring 0) release only", true);
			}
		}
	}
}
