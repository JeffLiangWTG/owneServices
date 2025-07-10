using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class InstallCurrentVersionTaskTest : AbstractApplicationStartupTaskTest<InstallCurrentVersionTask>
	{
		public void TestShouldExecute()
		{
			InstallCurrentVersionTask task = new InstallCurrentVersionTask();
			Assert(!task.ShouldExecute(new ApplicationArguments(System.Array.Empty<string>())));
			Assert(task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-SkipVersionCheck" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader", "-SkipVersionCheck" })));
			Assert(!task.ShouldExecute(new ApplicationArguments(new string[] { "-IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader", "-Upgrade:ABC-123" })));
		}

#if !WINZOR

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestExecuteWithStmDataSchemaChange()
		{
			Db.Connection.ExecuteNonQuery("alter table " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " drop constraint DF_StmData_SD_PreserveTestValue");
			Db.Connection.ExecuteNonQuery("alter table " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " drop column " + StmData.Schema.SD_PreserveTestValue);

			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
			new InstallCurrentVersionTask().Execute(new ApplicationArguments(System.Array.Empty<string>()));
		}

#endif

		public override int DefaultErrorExitCode => ExitCodes.InstallCurrentVersionTaskExit;
	}
}
