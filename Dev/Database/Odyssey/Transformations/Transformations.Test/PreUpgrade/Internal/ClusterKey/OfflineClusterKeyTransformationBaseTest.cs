using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	abstract class OfflineClusterKeyTransformationBaseTest : TransactionedTestCase
	{
		public void TestOfflineClusterKeyTransformation()
		{
			PrepareTestData();

			var transformation = GetTransformation();
			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertTransformationResult();
		}

		protected abstract void PrepareTestData();
		protected abstract void AssertTransformationResult();
		protected abstract OfflineClusterKeyTransformationBase GetTransformation();
	}
}
