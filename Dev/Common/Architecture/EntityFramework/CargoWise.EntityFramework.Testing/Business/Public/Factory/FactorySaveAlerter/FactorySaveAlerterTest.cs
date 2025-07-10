using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FactorySaveAlerterTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestMultipleThreadsCapturedBySaveAlerter()
		{
			var threads = new List<Thread>();

			for (int i = 0; i < 20; ++i)
			{
				threads.Add(new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (int j = 0; j < 10; ++j)
						{
							var factory = new BusinessObjectFactory() { RefreshEnabled = false };
							factory.NewWithValidTestData<DummyBusinessObject>();
							factory.Save();
						}
					}
				}));
			}

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new FactorySaveAlerter(() => "test", "Alerter Test Message"))
			{
				foreach (var thread in threads)
				{
					thread.Start();
				}

				foreach (var thread in threads)
				{
					thread.Join();
				}
			}

			AssertEquals(200, ErrorReporter.TotalErrorCount);
		}

		[UseSnapshotProtection]
		public void TestOnSavingStarted()
		{
			int saves = 0;

			void OnSavingStarted(ITransactionParticipant[] participants) => saves++;

			using (new FactorySaveAlerter(OnSavingStarted))
			{
				var factory = new BusinessObjectFactory();
				factory.Save();
				factory.Save();
			}

			AssertEquals(2, saves);
			AssertEquals("OnSavingStarted handles when to report when this constructor is used", 0, ErrorReporter.TotalErrorCount);
		}

		[UseSnapshotProtection]
		public void TestSaveAlerterOverrideThreadSpecific()
		{
			var threads = new List<Thread>();

			for (int i = 0; i < 19; ++i)
			{
				threads.Add(new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (int j = 0; j < 10; ++j)
						{
							var factory = new BusinessObjectFactory() { RefreshEnabled = false };
							factory.NewWithValidTestData<DummyBusinessObject>();
							factory.Save();
						}
					}
				}));
			}

			threads.Add(new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (FactorySaveAlerter.TemporarilyOverride())
				{
					for (int j = 0; j < 10; ++j)
					{
						var factory = new BusinessObjectFactory() { RefreshEnabled = false };
						factory.NewWithValidTestData<DummyBusinessObject>();
						factory.Save();
					}
				}
			}));

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new FactorySaveAlerter(() => "test", "Alerter Test Message"))
			{
				foreach (var thread in threads)
				{
					thread.Start();
				}

				foreach (var thread in threads)
				{
					thread.Join();
				}
			}

			AssertEquals(190, ErrorReporter.TotalErrorCount);
		}
	}
}
