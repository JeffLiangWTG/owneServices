using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs.Testing
{
	abstract class BaseMoveLeafTableToAddinfoTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			SetUpLeafTableIfNotExists();
		}

		internal abstract void SetUpLeafTableIfNotExists();
	}
}
