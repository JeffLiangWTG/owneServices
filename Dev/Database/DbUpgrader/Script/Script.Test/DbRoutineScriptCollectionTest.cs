using System.Linq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class DbRoutineScriptCollectionTest : TestCase
	{
		public void TestAddClientSpecific()
		{
			var script1 = new DbScript("b", "btext", "");
			var script2 = new DbScript("a", "atext", "");
			var scriptClientNew = new DbScript("c", "ctext", "");
			var scriptClientOverride = new DbScript("a", "aclient", "");
			var collection = new DbRoutineScriptCollection();
			collection.Add(script1);
			collection.Add(script2);
			collection.AddClientSpecific(scriptClientNew);
			collection.AddClientSpecific(scriptClientOverride);
			AssertEquals(3, collection.Count());

			var item0 = collection.ElementAt(0);
			AssertEquals("b", item0.Name);
			AssertEquals("btext", item0.Text);

			var item1 = collection.ElementAt(1);
			AssertEquals("a", item1.Name);
			AssertEquals("aclient", item1.Text);

			var item2 = collection.ElementAt(2);
			AssertEquals("c", item2.Name);
			AssertEquals("ctext", item2.Text);
		}
	}
}
