using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FetchHintTest : TestCaseWithFactory
	{
		public void TestComplexFetchHints_DoNotRunTogether()
		{
			var dummies = new List<DummyBusinessObject>();
			for (int i = 0; i < 10; i++)
			{
				dummies.Add(Factory.New<DummyBusinessObject>());
				var dum = Factory.New<DummyBusinessObject>();
				dum.Z0_Guid = ZGuid.NewZGuid();
				dummies.Add(dum);
			}
			Factory.Save();
			var factory = new BusinessObjectFactory();
			factory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.PK, dummies.Where(s => s.Z0_Guid.IsEmpty).Select(s => s.PK).ToArray()));
			factory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Guid, dummies.Where(s => !s.Z0_Guid.IsEmpty).Select(s => s.Z0_Guid).ToArray()));
			var connection = ((IDbConnected)factory).Connection;

			using (var settings = TestEntityFrameworkSettings.Get())
			using (connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				factory.ExecuteAllFetchHints();

				AssertEquals("Split the queries for different columns", 2, connection.ExecutedCommandsAndQueryPlans.Count(l => l.Item1.ToLower().Contains("from dbo.dummybizo")));
			}

			foreach (var dum in dummies)
			{
				AssertNotNull(factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, dum.PK) { FetchOnlyFromLocalCache = true }));
			}
		}

		public void TestManyFetchHints_UnionRatherThanOr()
		{
			// In this test we load a bunch of dummy bizos via two different types of fetch hint.
			// Then assert that the generated query uses UNION rather than OR filters.
			// Then check that all of the things were fetched into the cache.
			var dummies = new List<DummyBusinessObject>();
			for (int i = 0; i < 10; i++)
			{
				dummies.Add(Factory.New<DummyBusinessObject>());
				var dum = Factory.New<DummyBusinessObject>();
				dum.Z0_Guid = ZGuid.NewZGuid();
				dummies.Add(dum);
			}
			Factory.Save();
			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 10; i++)
			{
				factory.AddFetchHint(DummyBizoSchema.PK, dummies[i * 2].PK);
				factory.AddFetchHint(DummyBizoSchema.Z0_Guid, dummies[i * 2 + 1].Z0_Guid);
			}
			var connection = ((IDbConnected)factory).Connection;

			using (var settings = TestEntityFrameworkSettings.Get())
			using (connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				settings.ConcatenateMultipleFetchHintTypes = true; // The magic setting!
				factory.ExecuteAllFetchHints();

				var lastQuery = connection.ExecutedCommandsAndQueryPlans.Single(l => l.Item1.ToLower().Contains("from dbo.dummybizo")).Item1.ToLower();
				AssertEquals(true, lastQuery.Contains("union all"));
			}

			foreach (var dum in dummies)
			{
				AssertNotNull(factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, dum.PK) { FetchOnlyFromLocalCache = true }));
			}
		}

		public void TestTableNameReturnsDatabaseWhenNonStandardTable()
		{
			FetchHint hint = new FetchHint(CMRRefundReasonSchema.CR_RefundReasonDescription, (ZString)"Hello");
			AssertEquals(CMRRefundReasonSchema.Constants.TableName, hint.TableName);
		}

		public void TestSimpleOrIsKeptSeparate()
		{
			var f = Factory;
			for (int i = 0; i < 200; i++)
			{
				f.AddFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
			}
			f.Load<DummyBusinessObject>(ZGuid.NewZGuid());
			var hit = f.TableSelects[0];
			// default is 64 parameters max, so 4 should do it (4*64 >= 200), but the final PK load is another hit
			AssertEquals(5, hit.Value);
		}

		public void TestIsNeededForPK()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			DataRow row = factory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row.Table.Rows.Add(row);

			IFetchHint hint = new FetchHint(DummyBizoSchema.PK, ZGuid.NewZGuid());
			AssertEquals(true, hint.IsNeeded(historyProvider));

			IFetchHint hint2 = new FetchHint(DummyBizoSchema.PK, new ZGuid(row[0]));
			AssertEquals(false, hint2.IsNeeded(historyProvider));
		}

		public void TestIsNeededForNonPKColumn()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			IFetchHint hint = new FetchHint(DummyBizoSchema.Z0_Code, (ZString)"Code");
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.AddFetchHint(hint);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.ExecuteAllFetchHints();
			AssertEquals(false, hint.IsNeeded(historyProvider));
		}

		public void TestConstructor()
		{
			ZString code = "123";
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.Z0_Code, code);
			AssertEquals("DummyBizo", fetchHint.TableName);
			AssertEquals(DummyBizoSchema.Z0_Code, fetchHint.Column);
			AssertEquals(code, fetchHint.Value);
		}

		public void TestGetHashStringIncludingLoadWithBlobs()
		{
			ZString code = "123";
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.Z0_Code, code);
			AssertEquals("DummyBizoZ0_Code123", fetchHint.GetHashKeyObject().ToString());
		}

		public void TestIsDataHintLoaded()
		{
			FetchHint fetchHint = new FetchHint(DummyBizoSchema.Z0_Code, ZString.Empty);
			AssertEquals(false, fetchHint.IsDataHintLoaded);
			fetchHint.IsDataHintLoaded = true;
			AssertEquals(true, fetchHint.IsDataHintLoaded);
		}

		public void TestLoadWithBlobs()
		{
			FetchHint fetchHintWithBlob = new FetchHint(DummyBizoSchema.Z0_Code, ZString.Empty, DummyBizoSchema.Z0_VarCharMax);
			AssertEquals(true, new HashSet<SchemaColumn>(fetchHintWithBlob.LoadWithBlobs).Contains(DummyBizoSchema.Z0_VarCharMax));
			FetchHint fetchHintWithoutBlob = new FetchHint(DummyBizoSchema.Z0_Code, ZString.Empty);
			AssertEquals(0, new HashSet<SchemaColumn>(fetchHintWithoutBlob.LoadWithBlobs).Count);
		}
	}
}
