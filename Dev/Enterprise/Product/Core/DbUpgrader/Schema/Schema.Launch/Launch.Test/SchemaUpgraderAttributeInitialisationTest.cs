using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformations;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class SchemaUpgraderAttributeInitialisationTest : TestCase
	{
		public void TestPreAndPostTransformationDirectorIsLazyInstantiated()
		{
			var testUpgrader = new UpgraderForAttributeInitialisationTest();
			Assert("[PRE-CONDITION] field should be null", testUpgrader.IsTransformationDirectorAttributeNull);

			var director = testUpgrader.TransformationDirector_Exposed;
			Assert("[PRE-CONDITION] field should NOT be null", !testUpgrader.IsTransformationDirectorAttributeNull);
		}

		class UpgraderForAttributeInitialisationTest : Schema.SchemaUpgrader
		{
			public UpgraderForAttributeInitialisationTest()
				: base(new DummyUpgradeManager(), upgConnection: null, auditConnection: null, dataWarehouseConnection: null)
			{
			}

			public TransformationDirector TransformationDirector_Exposed
			{
				get { return TransformationDirector; }
			}

			public bool IsTransformationDirectorAttributeNull
			{
				get { return (fTransformationDirector == null); }
			}
		}
	}
}
