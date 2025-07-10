using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class TransformationHelper
	{
		public static readonly string DataCopyDbName = UpgUtils.UpgraderPrefix + "DataCopyDb_" + Db.DatabaseName;
	}
}
