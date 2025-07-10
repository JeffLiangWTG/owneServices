using CargoWise.Data;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.AuditLogs
{
	public class AuditLoggerTest : TransactionedTestCase
	{
		public void TestLogAction_DataImport()
		{
			var sql = $"SELECT count(*) FROM dbo.StmALog where SL_Parent = '{entity.InternalPK}' and SL_Table = '{entity.Definition.TableName}' and SL_SE_NKEvent = 'DIM' and SL_Reference = '{entity.Action.Code()}'";
			var beforeLogCount = (int)connection.ExecuteScalar(sql);

			logger.LogAction(entity, Events.DataImportCode);

			var afterLogCount = (int)connection.ExecuteScalar(sql);
			AssertEquals(beforeLogCount, afterLogCount);

			rowRepository.Save();

			afterLogCount = (int)connection.ExecuteScalar(sql);
			AssertEquals(beforeLogCount + 1, afterLogCount);
		}

		public void TestNonUserLoggedin()
		{
			EnvProvider providerToSave = Env.GetCurrentProvider();
			var nullEnvironment = new NullEnvProvider();
			try
			{
				nullEnvironment.Enable();
				AssertEquals("Precondition: Env.CurrentUser", null, Env.CurrentUser);
				AssertNoExceptionThrown(() => logger.LogAction(entity, Events.DataImportCode));
			}
			finally
			{
				providerToSave.Enable();
				nullEnvironment.Dispose();
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			sessionServices = new AncillaryImportServices();
			entity = TestUtil.PrepareDummyBizoEntity(sessionServices);

			connection = TestUtil.Connection;

			rowRepository = new RowRepository(connection);
			logger = new AuditLogger(rowRepository);
		}

		AncillaryImportServices sessionServices;
		Entity entity;
		RowRepository rowRepository;
		IAuditLogger logger;
		DbConnection connection;

		#endregion
	}
}
