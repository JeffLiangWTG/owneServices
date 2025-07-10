using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade
{
	public interface IPopulateAuditTimeAndUserInfo
	{
		ITableSchema TableSchema { get; }
		SchemaDateTimeColumn CreateTime { get; }
		SchemaStringColumn CreateUser { get; }
		SchemaDateTimeColumn LastEditTime { get; }
		SchemaStringColumn LastEditUser { get; }
	}
}