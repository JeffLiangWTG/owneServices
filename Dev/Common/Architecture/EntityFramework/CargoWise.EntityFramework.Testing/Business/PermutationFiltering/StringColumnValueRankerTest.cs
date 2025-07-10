using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class StringColumnValueRankerTest : ColumnValueRankerBaseTest
	{
		public void TestEmptyRanker()
		{
			AssertEquals(null, new StringColumnValueRanker().GetBestMatch(collection));
		}

		public void TestGetBestMatches()
		{
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, SpecificDescription, SpecificDate, true, 1));

			var ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, SpecificDescription, SpecificDate, true, 1));
			var items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 2, items.Length);
			AssertEquals("Number of Matched Items", 1, items[0].Number);
			AssertEquals("Number of Matched Items", 1, items[1].Number);
		}

		public void TestCollectionRanking()
		{
			var ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, SpecificDescription, SpecificDate, true, 1));
			var items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Number of Matched Items", 1, items[0].Number);

			ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, "A Description", SpecificDate, true, 1));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Number of Matched Items", 3, items[0].Number);

			ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, SpecificDescription, ZDateTime.Today.AddDays(-2), true, 1));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Number of Matched Items", 4, items[0].Number);

			ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), "COD99", SpecificDescription, SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Row Number", 2, items[0].Number);

			ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), "COD99", "Long Description", SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Row Number", 3, items[0].Number);

			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "ALL", "Long Description", SpecificDate, false, 8));

			ranker = GetRanker(SetupDummyBizO(new DummyNonPersistentBizO(), "COD99", "Long Description", SpecificDate, true, 2));
			items = ranker.GetBestMatch(collection).ToArray();
			AssertEquals("Count of matched Items", 1, items.Length);
			AssertEquals("Row Number", 8, items[0].Number);
		}

		public void TestCollectionRanking_NoMatch()
		{
			var ranker = new StringColumnValueRanker();
			ranker.Add(DummyNonPersistentBizO.Schema.Code, new ZString("COD"));
			ranker.Add(DummyNonPersistentBizO.Schema.Desc, new ZString("Long Description"));
			ranker.Add(DummyNonPersistentBizO.Schema.Date, ZDateTime.BrettsBirthday);

			var items = ranker.GetBestMatch(collection);
			AssertNull(items);
		}

		protected override void SetUp()
		{
			base.SetUp();

			collection = new List<DummyNonPersistentBizO>();
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), SpecificCode, SpecificDescription, SpecificDate, true, 1));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "ALL", SpecificDescription, SpecificDate, true, 2));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "ALL", "Any Description", SpecificDate, false, 3));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "ALL", "Any Description", ZDateTime.BrettsBirthday, false, 4));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "", "Any Description", ZDateTime.BrettsBirthday, false, 5));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "COD2", SpecificDescription, ZDateTime.Today.AddDays(-2), true, 6));
			collection.Add(SetupDummyBizO(new DummyNonPersistentBizO(), "COD2", ZString.Empty, ZDateTime.Empty, true, 7));

			Factory.Save();
		}

		DummyNonPersistentBizO SetupDummyBizO(DummyNonPersistentBizO bizo, ZString code, ZString desc, ZDateTime date, ZBool boolean, ZInt number)
		{
			bizo.Code = code;
			bizo.Desc = desc;
			bizo.Date = date;
			bizo.Bool = boolean;
			bizo.Number = number;
			return bizo;
		}

		StringColumnValueRanker GetRanker(DummyNonPersistentBizO bizo)
		{
			var ranker = new StringColumnValueRanker();
			ranker.Add(DummyNonPersistentBizO.Schema.Code, bizo.Code, new ZString("COD"), new ZString("ALL"), ZString.Empty);
			ranker.Add(DummyNonPersistentBizO.Schema.Desc, bizo.Desc, new ZString("Long Description"), new ZString("Any Description"), ZString.Empty);
			ranker.Add(DummyNonPersistentBizO.Schema.Date, bizo.Date, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			return ranker;
		}

		List<DummyNonPersistentBizO> collection;

		public class DummyNonPersistentBizO : NonPersistentBusinessObject
		{
			public abstract class Schema
			{
				public const string Code = "Code";
				public const string Desc = "Desc";
				public const string Date = "Date";
				public const string Bool = "Bool";
				public const string Number = "Number";
			}

			public ZString Code { get; set; }
			public ZString Desc { get; set; }
			public ZDateTime Date { get; set; }
			public ZBool Bool { get; set; }
			public ZInt Number { get; set; }
		}
	}
}
