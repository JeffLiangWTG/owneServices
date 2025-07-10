using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneOffsetCacheTest : TransactionedTestCase
	{
		public void TestTimeZoneOffsetCacheRetrievesCorrectDistinctValuesFromDatabase()
		{
			var previousCache = new TimeZoneOffsetCache();
			var previousCacheOffsetCount = previousCache.TimeZoneOffsets.Count;

			var sqlInsertionText = @"
				INSERT dbo.RefTimeZone (R2_PK, R2_OffsetMinutesFromUTC)
				VALUES
					(@ZonePk1, @Offset),
					(@ZonePk2, @Offset)
				";

			using (DbCommand command = Db.Connection.Command(sqlInsertionText))
			{
				command.AddParameter("@ZonePk1", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@ZonePk2", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Offset", SqlDbType.SmallInt, -780);
				command.ExecuteNonQuery();
			}

			var newOffsets = new TimeZoneOffsetCache().TimeZoneOffsets;

			AssertNotNull("The cache should have a value assigned in initialisation.", newOffsets);
			AssertEquals("The new cache should have an additional distinct value compared to the previous cache.", newOffsets.Count, previousCacheOffsetCount + 1);
		}

		public void TestTimeZoneOffsetCacheHasExpectedValuesUponInitializationAndHitsDatabaseOnce()
		{
			var testCacheOne = new TimeZoneOffsetCache();
			var cacheCountOne = testCacheOne.TimeZoneOffsets?.Count ?? 0;

			// Act
			string sqlTextInsertion = @"
					INSERT dbo.RefTimeZone (R2_PK, R2_OffsetMinutesFromUTC)
					VALUES
						(@ZonePk1, @Offset1),
						(@ZonePk2, @Offset2),
						(@ZonePk3, @Offset3)
				";

			using (var command = Db.Connection.Command(sqlTextInsertion))
			{
				command.AddParameter("@ZonePk1", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@ZonePk2", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@ZonePk3", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Offset1", SqlDbType.SmallInt, -960);
				command.AddParameter("@Offset2", SqlDbType.SmallInt, -1020);
				command.AddParameter("@Offset3", SqlDbType.SmallInt, -1080);
				command.ExecuteNonQuery();
			}

			var testCacheTwo = new TimeZoneOffsetCache();
			var cacheCountTwo = testCacheTwo.TimeZoneOffsets.Count;

			AssertNotNull("The cache should not be null and a value should have been assigned in initialisation.", testCacheOne.TimeZoneOffsets);
			Assert("The cache should have at least 1 value stored.", cacheCountOne > 0);
			AssertEquals("The cache should not have updated.", cacheCountOne, testCacheOne.TimeZoneOffsets.Count);
			AssertNotNull("The new cache should not be null and a value should have been assigned in initialisation.", testCacheTwo.TimeZoneOffsets);
			AssertEquals("The new cache should not have the same amount of values as the previous.", cacheCountTwo, cacheCountOne + 3);
		}

		public void TestTimeZoneOffsetCacheHasSortedValues()
		{
			var sqlTextInsertion = @"
					INSERT dbo.RefTimeZone (R2_PK, R2_OffsetMinutesFromUTC)
					VALUES
						(@ZonePk, @Offset)";

			var random = new Random();
			for (var index = 0; index < 1000; index++)
			{
				using (var command = Db.Connection.Command(sqlTextInsertion))
				{
					command.AddParameter("@ZonePk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
					command.AddParameter("@Offset", SqlDbType.SmallInt, random.Next(-1000, 1000));
					command.ExecuteNonQuery();
				}
			}

			var cache = new TimeZoneOffsetCache();
			var previousItem = cache.TimeZoneOffsets.First();
			foreach (var currentItem in cache.TimeZoneOffsets.Skip(1))
			{
				AssertGreaterThan("TimeZoneOffsetCache Should Be Ascendingly Sorted", currentItem, previousItem);
				previousItem = currentItem;
			}
		}
	}
}
