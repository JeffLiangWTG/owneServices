using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	abstract class TelDeviceMarksForEntityTestBase : TelDeviceTestCase
	{
		public void TestEntity0()
		{
			TestEntity(Entities[0], new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000007")),
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009"))
			});
		}

		public void TestEntity1()
		{
			TestEntity(Entities[1], new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000009"))
			});
		}

		public void TestEntity2()
		{
			TestEntity(Entities[2], new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000007")),
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009"))
			});
		}

		public void TestEntity3()
		{
			TestEntity(Entities[3], new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000007")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000009")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000009"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingEntity0()
		{
			TestBlockedRowsReading(Entities[0], new[]
			{
				new Guid("00000001-0000-0000-0000-000000000007"),
				new Guid("00000001-0000-0000-0000-000000000008")
			}, new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingEntity1()
		{
			TestBlockedRowsReading(Entities[1], new[]
			{
				new Guid("00000002-0000-0000-0000-000000000008")
			}, new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000009"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingEntity2()
		{
			TestBlockedRowsReading(Entities[2], new[]
			{
				new Guid("00000003-0000-0000-0000-000000000007"),
				new Guid("00000003-0000-0000-0000-000000000008")
			}, new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingEntity3()
		{
			TestBlockedRowsReading(Entities[3], new[]
			{
				new Guid("00000002-0000-0000-0000-000000000007"),
				new Guid("00000004-0000-0000-0000-000000000009"),
				new Guid("00000005-0000-0000-0000-000000000009")
			}, new[]
			{
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000006")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000008")),
				Tuple.Create("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000008"))
			});
		}

		void TestBlockedRowsReading(Guid entityPk, IEnumerable<Guid> pksToBlock, Tuple<string, Guid, Guid, Guid>[] expectedPks)
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.DefaultCommandTimeOutInSeconds = 5;
				CreateData(connection);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				{
					using (blockingConnection.BeginTransactionWithManager())
					{
						using (var command = blockingConnection.Command(BlockingQueryPattern(pksToBlock)))
						{
							command.ExecuteNonQuery();
						}

						// Act
						var result = DataUtils.GetDataTableFromQuery(connection, ActQuery(entityPk)).Rows.Cast<DataRow>();

						// Assert
						Assert(result, expectedPks);
					}
				}
			}
		}

		void TestEntity(Guid entityPk, Tuple<string, Guid, Guid, Guid>[] expectedPks)
		{
			// Arrange
			CreateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, ActQuery(entityPk)).Rows.Cast<DataRow>();

			// Assert
			Assert(result, expectedPks);
		}

		protected abstract string ActQuery(Guid entityPk);
		protected abstract void Assert(IEnumerable<DataRow> result, Tuple<string, Guid, Guid, Guid>[] expectedPks);
		protected abstract string BlockingQueryPattern(IEnumerable<Guid> pksToBlock);
	}
}
