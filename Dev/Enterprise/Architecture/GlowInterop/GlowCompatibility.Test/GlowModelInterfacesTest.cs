using System.Data;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Glow.Model.CW1.Validator;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowCompatibility.Test
{
	class GlowModelInterfacesTest : TestCase
	{
		[FrequentlyFailing]
		public void TestEntityInterfacesCompatibleWithDatabase()
		{
			var cdcTableConfig = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Select(t =>
				new
				{
					TableName = t.SourceTable,
					Columns = t.GetCdcColumnConfigRows().Where(c => c.CdcEnabled).Select(c => c.SourceColumn),
				}).ToDictionary(t => t.TableName);

			var result = CW1ModelTestHelperHost.Instance.AreEntityInterfacesCompatibleWithDatabase(
				Connection,
				(tableName) => cdcTableConfig.ContainsKey(tableName),
				out var message);
			Assert(message, result);
		}

		[FrequentlyFailing]
		public void TestStoredProcedureInterfaceCompatibleWithDatabase()
		{
			var result = CW1ModelTestHelperHost.Instance.IsStoredProcedureInterfaceCompatibleWithDatabase(Connection, out var message);
			Assert(message, result);
		}

		[FrequentlyFailing]
		public void TestEntitiesOptimisticConcurrencyReady()
		{
			var result = CW1ModelTestHelperHost.Instance.AreEntitiesOptimisticConcurrencyReady(Connection, out var message);
			Assert(message, result);
		}

		[FrequentlyFailing]
		public void TestStoredProceduresInSync_CW1()
		{
			var result = CW1ModelTestHelperHost.Instance.AreStoredProceduresInSync(Connection, out var message);
			Assert(message, result);
		}

		[FrequentlyFailing]
		public void TestViewsInSync_CW1()
		{
			var result = CW1ModelTestHelperHost.Instance.AreViewsInSync(Connection, out var message);
			Assert(message, result);
		}

		IDbConnection Connection
		{
			get
			{
				Db.Connection.EnsureIsOpen();
				return ((IDbConnectionInternals)Db.Connection).ADOConnection;
			}
		}
	}
}
