using CargoWise.Data;

namespace Enterprise.Integration;
public interface IAuditTsqlScriptRunnerHelperForTest
{
	void RunAETWithAdminDbConnection(AdminConnection adminConnection);
	void EnableCdc(AdminConnection connection, string testSchemaName, string testTableName);
}