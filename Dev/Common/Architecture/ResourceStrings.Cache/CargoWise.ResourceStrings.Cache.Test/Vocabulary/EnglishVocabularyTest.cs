using NUnit.Framework;

namespace CargoWise.ResourceStrings.Vocabulary.Testing
{
	class EnglishVocabularyTest : TestCase
	{
		public void TestPluralize()
		{
			var vocabulary = EnglishVocabulary.Instance;
			CombineAssertions(() =>
			{
				AssertEquals("friends", vocabulary.ApplyRules("friend"));
				AssertEquals("styles", vocabulary.ApplyRules("style"));
				AssertEquals("boys", vocabulary.ApplyRules("boy"));

				AssertEquals("bellows", vocabulary.ApplyRules("bellows"));
				AssertEquals("shears", vocabulary.ApplyRules("shears"));

				AssertEquals("axes", vocabulary.ApplyRules("axis"));
				AssertEquals("quizzes", vocabulary.ApplyRules("quiz"));

				AssertEquals("buses", vocabulary.ApplyRules("bus"));
				AssertEquals("foxes", vocabulary.ApplyRules("fox"));
				AssertEquals("Inches", vocabulary.ApplyRules("Inch"));
				AssertEquals("focuses", vocabulary.ApplyRules("focus"));
				AssertEquals("funguses", vocabulary.ApplyRules("fungus"));
				AssertEquals("biases", vocabulary.ApplyRules("bias"));
				AssertEquals("viruses", vocabulary.ApplyRules("virus"));
				AssertEquals("aliases", vocabulary.ApplyRules("alias"));

				AssertEquals("forums", vocabulary.ApplyRules("forum"));
				AssertEquals("stadiums", vocabulary.ApplyRules("stadium"));
				AssertEquals("data", vocabulary.ApplyRules("datum"));
				AssertEquals("drums", vocabulary.ApplyRules("drum"));
				AssertEquals("curricula", vocabulary.ApplyRules("curriculum"));
				AssertEquals("millennia", vocabulary.ApplyRules("millennium"));

				AssertEquals("mediae", vocabulary.ApplyRules("media"));

				AssertEquals("knives", vocabulary.ApplyRules("knife"));
				AssertEquals("leaves", vocabulary.ApplyRules("leaf"));
				AssertEquals("roofs", vocabulary.ApplyRules("roof"));

				AssertEquals("matrices", vocabulary.ApplyRules("matrix"));
				AssertEquals("Affixes", vocabulary.ApplyRules("Affix"));

				AssertEquals("candies", vocabulary.ApplyRules("candy"));

				AssertEquals("potatoes", vocabulary.ApplyRules("potato"));
				AssertEquals("photos", vocabulary.ApplyRules("photo"));
				AssertEquals("silos", vocabulary.ApplyRules("silo"));
				AssertEquals("macros", vocabulary.ApplyRules("macro"));
				AssertEquals("pianos", vocabulary.ApplyRules("piano"));

				AssertEquals("mice", vocabulary.ApplyRules("mouse"));
				AssertEquals("lice", vocabulary.ApplyRules("louse"));

				AssertNotEquals("children", vocabulary.ApplyRules("Child"));
				AssertEquals("Children", vocabulary.ApplyRules("Child"));
				AssertEquals("children", vocabulary.ApplyRules("child"));
				AssertEquals("people", vocabulary.ApplyRules("person"));
				AssertEquals("women", vocabulary.ApplyRules("woman"));
				AssertEquals("feet", vocabulary.ApplyRules("foot"));
				AssertEquals("geese", vocabulary.ApplyRules("goose"));
				AssertEquals("brethren", vocabulary.ApplyRules("brother"));
				AssertEquals("Oxen", vocabulary.ApplyRules("Ox"));

				AssertEquals("troy", vocabulary.ApplyRules("troy"));
				AssertEquals("fish", vocabulary.ApplyRules("fish"));
				AssertNotEquals("fish", vocabulary.ApplyRules("Fish"));
				AssertEquals("Fish", vocabulary.ApplyRules("Fish"));
				AssertEquals("sheep", vocabulary.ApplyRules("sheep"));
				AssertEquals("cattle", vocabulary.ApplyRules("cattle"));
				AssertEquals("deer", vocabulary.ApplyRules("deer"));
				AssertEquals("salmon", vocabulary.ApplyRules("salmon"));
				AssertEquals("trout", vocabulary.ApplyRules("trout"));
				AssertEquals("fix", vocabulary.ApplyRules("fix"));

				AssertEquals("BOXES", vocabulary.ApplyRules("BOX"));
				AssertEquals("Boxes", vocabulary.ApplyRules("Box"));
				AssertEquals("boxes", vocabulary.ApplyRules("box"));

				AssertEquals("Bizos", vocabulary.ApplyRules("Bizo"));
				AssertEquals("Dozen", vocabulary.ApplyRules("Dozen"));

				AssertEquals("Bales, Compressed", vocabulary.ApplyRules("Bale, Compressed"));
				AssertEquals("Bales, Uncompressed", vocabulary.ApplyRules("Bale, Uncompressed"));
				AssertEquals("Break Bulk", vocabulary.ApplyRules("Break Bulk"));
				AssertEquals("Tariffs", vocabulary.ApplyRules("Tariff"));
				AssertEquals("tariffs", vocabulary.ApplyRules("tariff"));
				AssertNotEquals("tariffs", vocabulary.ApplyRules("Tariff"));
			});
		}
	}
}
