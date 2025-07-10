

using System;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class GlowIndexQueryResultCollectionTest : TestCase
	{
		public void TestLastUpdateTime()
		{
			var collection = new GlowIndexQueryResultCollection();

			AssertEquals(0, collection.TablesUpdateTime.Count);
			AssertEquals("if no table, will returns min value", DateTimeOffset.MinValue, collection.LastUpdateTime);
			AssertEquals(true, collection.IsOutOfDate);

			var now = DateTimeOffset.Now;
			collection.TablesUpdateTime.Add("table1", now.AddMinutes(-30));
			collection.TablesUpdateTime.Add("table2", now.AddMinutes(-180));

			AssertEquals(2, collection.TablesUpdateTime.Count);
			AssertEquals(now.AddMinutes(-30), collection.LastUpdateTime);
			AssertEquals(true, collection.IsOutOfDate);

			collection.TablesUpdateTime["table1"] = now;

			AssertEquals(2, collection.TablesUpdateTime.Count);
			AssertEquals(now, collection.LastUpdateTime);
			AssertEquals(false, collection.IsOutOfDate);
		}
	}
}
