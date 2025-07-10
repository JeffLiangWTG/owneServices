using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Preconditions.Testing
{
	sealed class SqlServerDependentPreconditionTest : TestCase
	{
		[DatCapabilityRequirement("SQL2019")]
		public void TestSqlServiceName_2019()
		{
			AssertEquals("MSSQLSERVER", SqlServerTools.SqlServerServiceName);
		}

		[DatCapabilityRequirement("SQL2022")]
		public void TestSqlServiceName_2022()
		{
			AssertEquals("MSSQL$MSSQLSERVER22", SqlServerTools.SqlServerServiceName);
		}

		[DatCapabilityRequirement("SQL2022+")]
		public void TestSqlServiceName_2022Plus()
		{
			AssertEquals("MSSQL$MSSQLSERVER22", SqlServerTools.SqlServerServiceName);
		}
	}
}
