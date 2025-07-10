using System;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class TableInfoProviderTestListener : BaseTestListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			SQLDataObject.TableInfoProvider = OdysseySchemaTableInfoProvider.Instance;
			base.StartTest(test, startTime);
		}

		public override void EndAllTests(DateTime endTime)
		{
			base.EndAllTests(endTime);
			SQLDataObject.TableInfoProvider = null;
		}
	}

	public class TableInfoProviderTestListenerTest : TestCase
	{
		public void TestTableInfoProviderSetup()
		{
			AssertSame("Test listener should setup the table info provider.", OdysseySchemaTableInfoProvider.Instance, SQLDataObject.TableInfoProvider);
		}
	}
}
