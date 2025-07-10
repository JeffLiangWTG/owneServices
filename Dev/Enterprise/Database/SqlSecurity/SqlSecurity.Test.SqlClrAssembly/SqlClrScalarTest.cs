using Microsoft.SqlServer.Server;
public static class SqlClrScalarTest
{
	[SqlFunction(DataAccess = DataAccessKind.None)]
	public static int ScalarFunction()
	{
		return 0;
	}
}
