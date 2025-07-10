using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(PopulateActiveDirectorySidAndSamAccountFullName))]
	public class PopulateActiveDirectorySidAndSamAccountFullNameTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			// I need to revert this transform, but was told to retain OnlinePostUpgradeTransform otherwise ODT Service task could fail.
			// So I make it as a no-op transform as suggested, when it is needed in furture, it should be re-added as version 2

			return new PopulateActiveDirectorySidAndSamAccountFullName();
		}
	}
}
