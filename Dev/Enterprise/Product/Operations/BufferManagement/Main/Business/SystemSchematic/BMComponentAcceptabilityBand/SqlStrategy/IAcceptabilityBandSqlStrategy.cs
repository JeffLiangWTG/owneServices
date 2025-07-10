using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public interface IAcceptabilityBandSqlStrategy
	{
		(string, ZSqlParameter[]) GetAcceptabilityBandSql(bool createInsertQuery = false);
		(string, ZSqlParameter[]) GetMatchingWorkflowCountSql();
		(string, ZSqlParameter[]) GetMatchingWorkflowsSql();
	}
}
