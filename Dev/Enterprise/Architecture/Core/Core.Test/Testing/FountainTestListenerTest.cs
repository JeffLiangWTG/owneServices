using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Core.Testing.FountainTestListener;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[UseSnapshotProtection]
	public class FountainTestListenerTest : TestCase
	{
		public void TestAddFountainAccessWrongCall()
		{
			AssertExceptionThrown(
				typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: fountainName",
				() => AddFountainAccess(null, Guid.Empty)
				);
			AssertExceptionThrown(
				typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: fountainName",
				() => new FountainTestListenerForTest().AddFountainAccessCore(null, Guid.Empty)
				);
		}

		public void TestAfterEachTestDoesntClearExistedCache()
		{
			var listener = new FountainTestListenerForTest();

			listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
			AssertNotNull("Existing fouintains were read", listener.PreviouslyExistedFountains);
			var count = listener.PreviouslyExistedFountains.Length;

			listener.AfterEachTest(ZDateTime.Now.ToDateTime());
			AssertNotNull("Existing fouintains were read", listener.PreviouslyExistedFountains);
			AssertEquals("Existing fouintains count", count, listener.PreviouslyExistedFountains.Length);

			listener.AfterEachTest(ZDateTime.Now.ToDateTime());
			AssertNotNull("Existing fouintains were read", listener.PreviouslyExistedFountains);
			AssertEquals("Existing fouintains count", count, listener.PreviouslyExistedFountains.Length);
		}

		public void TestDeleteFountains()
		{
			AssertTestExistingFountains(Array.Empty<int>());

			var existingFountains = CreateExistingFountains().OrderBy(i => i).ToArray();
			AssertTestExistingFountains(existingFountains);

			var accessedFountains = CreateFountainsForAccess().ToArray();
			var accessedFountainsId = accessedFountains.Select(data => data.Id).OrderBy(i => i).ToArray();
			AssertTestAllFountains(existingFountains, accessedFountainsId);

			FountainTestListenerForTest.DeleteFountains(existingFountains, accessedFountains);
			AssertTestExistingFountains(existingFountains);
		}

		public void TestDeleteFountainsWithEmptyExistance()
		{
			AssertTestExistingFountains(Array.Empty<int>());

			var accessedFountains = CreateFountainsForAccess().ToArray();
			var accessedFountainsId = accessedFountains.Select(data => data.Id).OrderBy(i => i).ToArray();
			AssertTestExistingFountains(accessedFountainsId);

			DeleteFountains(null, accessedFountains);
			AssertTestExistingFountains(Array.Empty<int>());

			accessedFountains = CreateFountainsForAccess().ToArray();
			accessedFountainsId = accessedFountains.Select(data => data.Id).OrderBy(i => i).ToArray();
			AssertTestExistingFountains(accessedFountainsId);

			DeleteFountains(Array.Empty<int>(), accessedFountains);
			AssertTestExistingFountains(Array.Empty<int>());
		}

		public void TestFountainTestListenerDeletesUsedFountains()
		{
			var existingFountains = CreateExistingFountains().OrderBy(i => i).ToArray();
			var listener = new FountainTestListenerForTest();

			AssertTestExistingFountains(existingFountains);

			// the first "test"
			listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

			AssertTestExistingFountains(existingFountains);

			var accessedFountains = CreateFountainsForAccess().ToArray();
			var accessedFountainsId = accessedFountains.Select(data => data.Id).OrderBy(i => i).ToArray();

			AssertTestAllFountains(existingFountains, accessedFountainsId);

			// none fountains were marked as Accessed, so all must stay in DB
			listener.AfterEachTest(ZDateTime.Now.ToDateTime());
			AssertTestAllFountains(existingFountains, accessedFountainsId);

			// the second "test"
			listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
			AssertTestAllFountains(existingFountains, accessedFountainsId);

			// access fountains (directly to the instance!)
			foreach (var fountain in accessedFountains)
			{
				listener.AddFountainAccessCore(fountain.Name, fountain.Owner);
			}

			AssertTestAllFountains(existingFountains, accessedFountainsId);
			listener.AfterEachTest(ZDateTime.Now.ToDateTime());
			AssertTestExistingFountains(existingFountains);

			// the third "test"
			listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
			listener.AfterEachTest(ZDateTime.Now.ToDateTime());

			AssertTestExistingFountains(existingFountains);

			// finish...
			listener.EndAllTests(ZDateTime.Now.ToDateTime());

			AssertTestExistingFountains(existingFountains);
		}

		IDisposable AddTemporaryNumberSequence(AdminConnection adminConnection, string name, int start = 1, int inc = 1, int min = 1, int max = 500)
		{
			adminConnection.ExecuteNonQuery($@"CREATE SEQUENCE {name} AS BIGINT   
		START WITH {start}  
		INCREMENT BY {inc}  
		MINVALUE {min}  
		MAXVALUE {max}
");
			return new DisposableAction(() =>
			{
				adminConnection.ExecuteNonQuery($"DROP SEQUENCE {name}");
			});
		}

		public void TestResetLossyFountains()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (AddTemporaryNumberSequence(adminConnection, "MySequence", 1, 25))
			{
				adminConnection.ExecuteNonQuery(@"CREATE SEQUENCE MyDecSequence AS DECIMAL(30,0)
	START WITH 922337203685477580700
");

				var listener = new FountainTestListenerForTest();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				listener.SetLossyFountainAccessCore();
				long FountainValue() => adminConnection.ExecuteScalar<long>("SELECT NEXT VALUE FOR [MySequence]");
				FountainValue();
				var fountainValue = FountainValue();
				AssertEquals(true, fountainValue > 1);

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());

				var valueAfterTest = adminConnection.ExecuteScalar<long>("SELECT current_value FROM sys.sequences WHERE name = 'MySequence'");
				AssertEquals("Current value should be reset after the test", 1, valueAfterTest);

				var valueDecAfterTest = adminConnection.ExecuteScalar<decimal>("SELECT current_value FROM sys.sequences WHERE name = 'MyDecSequence'");
				AssertEquals("Current larger value should be reset after the test", 922337203685477580700M, valueDecAfterTest);
			}
		}

		public void TestResetLossyFountainsWithSingleIncrement()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (AddTemporaryNumberSequence(adminConnection, "SmallSequence", 1, 1))
			{
				var listener = new FountainTestListenerForTest();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				listener.SetLossyFountainAccessCore();
				long FountainValue() => adminConnection.ExecuteScalar<long>("SELECT NEXT VALUE FOR [SmallSequence]");
				var fountainValue = FountainValue();
				AssertEquals("Precondition - fountain starts at 1", 1, fountainValue);

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());

				var valueAfterTest = FountainValue();
				AssertEquals("Sequence should be reset so next value is still 1", 1, valueAfterTest);
			}
		}

		static void AssertTestExistingFountains(IReadOnlyList<int> existingFountains)
		{
			TestFountainsExistance("Existing fountains match", existingFountains);
		}

		static void AssertTestAllFountains(IEnumerable<int> existingFountains, IEnumerable<int> accessedFountains)
		{
			TestFountainsExistance("All fountains match", existingFountains.Concat(accessedFountains).Distinct().ToArray());
		}

		static void TestFountainsExistance(string message, IReadOnlyList<int> expected)
		{
			var ids = new List<int>();
			if (expected.Count > 0)
			{
				using (var cmd = Db.Connection.Command($"SELECT SN_Id FROM [dbo].[StmNums] WHERE SN_Id IN ({string.Join(",", expected)}) ORDER BY 1"))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ids.Add(reader.GetValue<int>("SN_Id"));
						}
					}
				}
			}

			TestArrays(message, expected, ids);
		}

		static void TestArrays(string message, IReadOnlyList<int> expected, IReadOnlyList<int> ids)
		{
			AssertEquals(message, expected.Count, ids.Count);
			for (var i = 0; i < expected.Count; i++)
			{
				AssertEquals(message, expected[i], ids[i]);
			}
		}

		static IEnumerable<FountainData> CreateFountainsForAccess()
		{
			const string sql = @"
DECLARE
	@ids TABLE (id int, name varchar(35), own uniqueidentifier)

INSERT INTO [dbo].[StmNums]
	([SN_Name],[SN_Value],[SN_Owner],[SN_MinimumValue],[SN_MaximumValue],[SN_CanRollover])
OUTPUT
	inserted.SN_Id, inserted.SN_Name, inserted.SN_Owner INTO @ids (id, name, own)
VALUES
	('new123' ,1         ,0x        ,1                ,100              ,0),
	('new234' ,10        ,NEWID()   ,1                ,100              ,0),
	('new345' ,20        ,NEWID()   ,1                ,100              ,0),
	('new456' ,30        ,0x        ,1                ,100              ,0)

INSERT INTO @ids (id, name, own)
SELECT SN_Id, SN_Name, SN_Owner FROM [dbo].[StmNums] WHERE SN_Name IN ('123', '456')

SELECT id, name, own FROM @ids
";
			return CreateFountains(sql);
		}

		static IEnumerable<int> CreateExistingFountains()
		{
			return CreateFountains(@"
INSERT INTO [dbo].[StmNums]
	([SN_Name],[SN_Value],[SN_Owner],[SN_MinimumValue],[SN_MaximumValue],[SN_CanRollover])
OUTPUT
	inserted.SN_Id AS id, inserted.SN_Name AS name, inserted.SN_Owner AS own
VALUES
	('123'    ,1         ,NEWID()   ,1                ,100              ,0),
	('234'    ,10        ,NEWID()   ,1                ,100              ,0),
	('345'    ,20        ,0x        ,1                ,100              ,0),
	('456'    ,30        ,0x        ,1                ,100              ,0)
"
				).Select(data => data.Id);
		}

		static IEnumerable<FountainData> CreateFountains(string sql)
		{
			// real fountains created in new connection!
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (var cmd = connection.Command(sql))
				{
					using (var reader = cmd.ExecuteReader())
					{
						var existingFountains = new List<FountainData>();
						while (reader.Read())
						{
							existingFountains.Add(new FountainData
							{
								Id = reader.GetValue<int>("id"),
								Name = reader.GetValue<string>("name"),
								Owner = reader.GetValue<Guid>("own")
							});
						}
						return existingFountains;
					}
				}
			}
		}

		class FountainTestListenerForTest : FountainTestListener
		{
			new internal void AddFountainAccessCore(string fountainName, Guid ownerPk) => base.AddFountainAccessCore(fountainName, ownerPk);
			new internal void SetLossyFountainAccessCore() => base.SetLossyFountainAccessCore();
		}

		class FountainData : FountainPair
		{
			public int Id { get; set; }
		}
	}
}
