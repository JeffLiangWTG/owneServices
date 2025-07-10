using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Tools.Testing
{
	public class SpellCheckerSingletonTest : TestCaseWithFactory
	{
		public void TestRemoveSpellCheckerInstanceFromDictionary()
		{
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				SpellChecker.InitialiseSpellcheck(textBox1, "Test1");
				SpellChecker.InitialiseSpellcheck(textBox2, "Test1");
				AssertEquals(1, SpellCheckerStatusSingleton.Instance.SpellCheckerInstancesDictionary.Count);
				Assert(SpellCheckerStatusSingleton.Instance.SpellCheckerInstancesDictionary.TryGetValue("SpellCheckerKey|Test1", out var instancesCollection));
				AssertNotNull(instancesCollection);
				AssertEquals(2, instancesCollection.Count);
			}

			AssertEquals(0, SpellCheckerStatusSingleton.Instance.SpellCheckerInstancesDictionary.Count);
			Assert(!SpellCheckerStatusSingleton.Instance.SpellCheckerInstancesDictionary.ContainsKey("SpellCheckerKey|Test1"));
		}

		public void TestSyncroniseSpellCheckerStatus()
		{
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				var spellChecker1 = SpellChecker.InitialiseSpellcheck(textBox1, "Test2");
				var spellChecker2 = SpellChecker.InitialiseSpellcheck(textBox2, "Test2");
				Assert(spellChecker1.SpellCheckerEnabled_Exposed);
				Assert(spellChecker2.SpellCheckerEnabled_Exposed);
				SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus("SpellCheckerKey|Test2", false);
				Assert(!spellChecker1.SpellCheckerEnabled_Exposed);
				Assert(!spellChecker2.SpellCheckerEnabled_Exposed);
				SpellCheckerStatusSingleton.Instance.SyncroniseSpellCheckerStatus("SpellCheckerKey|Test2", true);
				Assert(spellChecker1.SpellCheckerEnabled_Exposed);
				Assert(spellChecker2.SpellCheckerEnabled_Exposed);
			}
		}

		public void TestDbHits()
		{
			using (var textBox1 = new ZTextBox())
			using (Db.Connection.TrackExecutedCommands())
			{
				// Act
				var spellChecker1 = SpellChecker.InitialiseSpellcheck(textBox1, "TestDbHits");
				Factory.Save();
				// Assert
				var executedCommand = Db.Connection.ExecutedCommands.Count(c => c.Contains("SpellCheckerKey|"));
				AssertEquals("Should only hit the db once because of the registry cache", 1, executedCommand);
			}
		}
	}
}