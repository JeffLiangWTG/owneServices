using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Glow.Model.CW1.Validator;
using CargoWise.Glow.Model.Validator;

namespace GlowModelTestRunner
{
	static class GlowModelTestRunner
	{
		[STAThread]
		static int Main(string[] args)
		{
			int result = 0;

			try
			{
				Db.InitializeDatabaseDetails(Environment.MachineName, AutoRegenDb);
				result = RunGlowModelTests();
			}
			catch (Exception ex)
			{
				// Running these tests during regen is for convenience only, dont stop autocheckin if something is broken
				Console.WriteLine("Exception occured while running GLOW Model Tests: {0}", ex.Message);
			}

			return result;
		}

		const string AutoRegenDb = "OdysseyAutoRegen";

		#region RunGlowModelTests

		static int RunGlowModelTests()
		{
			Db.Connection.EnsureIsOpen();
			return
				RunGlowModelTests(CW1ModelTestHelperHost.Instance, "CW1") ? 0 : 1;
		}

		static bool RunGlowModelTests(ModelTestHelper helper, string product)
		{
			return
				RunGlowModelTest(helper.AreEntityInterfacesCompatibleWithDatabase, product) &
				RunGlowModelTest(helper.IsStoredProcedureInterfaceCompatibleWithDatabase, product) &
				RunGlowModelTest(helper.AreEntitiesOptimisticConcurrencyReady, product) &
				RunGlowModelTest(helper.AreTriggersInSync, product) &
				RunGlowModelTest(helper.AreViewsInSync, product) &
				RunGlowModelTest(helper.AreStoredProceduresInSync, product);
		}

		static bool RunGlowModelTest(GlowModelTest testHelper, string product)
		{
			var adoConnection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			var result = testHelper(adoConnection, out var message);

			if (!string.IsNullOrEmpty(message))
			{
				Console.WriteLine("GLOW Model Test Error ({0}): {1}", product, message);
			}

			return result;
		}

		delegate bool GlowModelTest(IDbConnection connection, out string message);

		#endregion
	}
}
