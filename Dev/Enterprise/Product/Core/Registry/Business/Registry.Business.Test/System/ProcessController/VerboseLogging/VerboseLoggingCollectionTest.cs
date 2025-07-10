using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(VerboseLoggingCollection))]
	class VerboseLoggingCollectionTest : RegistryBusinessObjectCollectionTestCase<VerboseLoggingCollection>
	{
		public void TestCodeMaxLength()
		{
			// Arrange
			var verboseLoggingCollection = new VerboseLoggingCollection();

			// Act
			var result = verboseLoggingCollection.CodeMaxLength;

			// Assert
			AssertEquals(4, result);
		}

		public class VerboseLoggingByCodeTest : TestCase
		{
			public void TestNoCodeReturnsFalse()
			{
				Test("TS1");
				Test("TS2");

				void Test(string code)
				{
					// Arrange
					var collection = new VerboseLoggingCollection();
					AddItem(collection, "TST", ZDateTime.Empty);

					// Act
					var result = collection.VerboseLoggingByCode(code);

					// Assert
					AssertEquals(false, result);
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 20)]
			public void TestObsoleteDateReturnsFalse()
			{
				Test("TS1");
				Test("TS2");

				void Test(string code)
				{
					// Arrange
					var collection = new VerboseLoggingCollection();
					AddItem(collection, "TST", ZDateTime.UtcNow.AddDays(-1));
					AddItem(collection, "TS1", ZDateTime.UtcNow.AddDays(-1));
					AddItem(collection, "TS2", ZDateTime.UtcNow.AddDays(-1));

					// Act
					var result = collection.VerboseLoggingByCode(code);

					// Assert
					AssertEquals(false, result);
				}
			}

			[TestDate(2022, 7, 21, 8, 30, 20)]
			public void TestFutureDateReturnsTrue()
			{
				Test("TS1");
				Test("TS2");

				void Test(string code)
				{
					// Arrange
					var collection = new VerboseLoggingCollection();
					AddItem(collection, "TST", ZDateTime.UtcNow.AddDays(1));
					AddItem(collection, "TS1", ZDateTime.UtcNow.AddDays(1));
					AddItem(collection, "TS2", ZDateTime.UtcNow.AddDays(1));

					// Act
					var result = collection.VerboseLoggingByCode(code);

					// Assert
					AssertEquals(true, result);
				}
			}

			[TestDate(2006, 12, 26, 13, 0, 0)]
			[TestUtcOffset(11, 0, 0)]
			public void TestTimeInUtc()
			{
				Test("TS1", false);
				Test("TS2", false);
				Test("TS3", true);

				void Test(string code, bool expected)
				{
					// Arrange
					var collection = new VerboseLoggingCollection();
					AddItem(collection, "TST", ZDateTime.UtcNow);
					AddItem(collection, "TS1", ZDateTime.UtcNow.AddSeconds(-1));
					AddItem(collection, "TS2", ZDateTime.UtcNow);
					AddItem(collection, "TS3", ZDateTime.UtcNow.AddSeconds(1));

					// Act
					var result = collection.VerboseLoggingByCode(code);

					// Assert
					AssertEquals(expected, result);
				}
			}

			static void AddItem(VerboseLoggingCollection collection, string code, ZDateTime dateTime)
			{
				var item = collection.AddNew();
				item.Code = code;
				item.Value = dateTime;
			}
		}

		protected override VerboseLoggingCollection GetCollectionToTest()
		{
			return new VerboseLoggingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VerboseLoggingBusinessObject();
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
	}
}
