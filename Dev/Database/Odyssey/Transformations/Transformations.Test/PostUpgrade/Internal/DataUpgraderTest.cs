using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Test;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	public class DataUpgraderTest : BaseUpgraderTestCase
	{
		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			return new DataTransformationUpgraderForTesting(dummyUpgradeManager, Db.Connection);
		}

		public class DataTransformationUpgraderForTesting : DataTransformationUpgrader
		{
			public DataTransformationUpgraderForTesting(IUpgradeManager manager, DbConnection upgConnection)
					: base(manager, new TransformationDirectorTest.MyDirector(manager), upgConnection)
			{
			}
		}
	}
}
