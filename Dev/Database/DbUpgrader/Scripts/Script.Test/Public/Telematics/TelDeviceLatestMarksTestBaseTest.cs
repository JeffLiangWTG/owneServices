using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	abstract class TelDeviceLatestMarksTestBase : TelDeviceTestCase
	{
		public void TestTime1()
		{
			TestEntity("2017-01-15", new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000005"))
			});
		}

		public void TestTime2()
		{
			TestEntity("2017-02-15", new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000009"))
			});
		}

		public void TestTime3()
		{
			TestEntity("2017-06-15", new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000F"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingTime1()
		{
			TestBlockedRowsReading("2017-01-15", new[]
			{
				Devices[1],
				Devices[3],
			}, new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000005"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingTime2()
		{
			TestBlockedRowsReading("2017-02-15", new[]
			{
				Devices[1],
				Devices[4],
			}, new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000009"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReadingTime3()
		{
			TestBlockedRowsReading("2017-06-15", new[]
			{
				Devices[1],
				Devices[2],
				Devices[4],
			}, new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000F"))
			});
		}

		void TestBlockedRowsReading(string dateTimeOffsetString, IEnumerable<Guid> pksToBlock, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
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
						var result = DataUtils.GetDataTableFromQuery(connection, ActQuery(dateTimeOffsetString)).Rows.Cast<DataRow>();

						// Assert
						Assert(result, expectedPks);
					}
				}
			}
		}

		void TestEntity(string dateTimeOffsetString, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			// Arrange
			CreateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, ActQuery(dateTimeOffsetString)).Rows.Cast<DataRow>();

			// Assert
			Assert(result, expectedPks);
		}

		protected abstract string ActQuery(string entityPk);
		protected abstract void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks);
		protected abstract string BlockingQueryPattern(IEnumerable<Guid> pksToBlock);
	}
}
